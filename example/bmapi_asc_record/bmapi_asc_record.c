/* Minimal multi-channel CAN/CAN FD ASC recorder. See README.md before running. */
#include "bmapi_asc_common.h"
#ifdef _WIN32
#include <io.h>
#include <fcntl.h>
#include <sys/stat.h>
#endif

/* v140_xp's CRT rejects fopen mode "wx". Preserve atomic no-overwrite creation. */
static FILE* create_capture(const char* path)
{
#ifdef _WIN32
    int fd = _open(path, _O_CREAT | _O_EXCL | _O_WRONLY | _O_TEXT, _S_IREAD | _S_IWRITE);
    FILE* file;
    if (fd < 0) return NULL;
    file = _fdopen(fd, "w");
    if (!file) _close(fd);
    return file;
#else
    return fopen(path, "wx");
#endif
}

typedef struct {
    uint32_t sampled;
    int64_t elapsed;
} CaptureClock;

/* Latch the first observed stop; signal handlers never call BMAPI or file I/O. */
static int check_stop(uint64_t start, uint64_t duration, uint64_t* cutoff, uint64_t* drain_deadline)
{
    uint64_t now = asc_now_us();
    if (!*drain_deadline && (asc_stopping || now - start >= duration)) {
        *cutoff = now - start < duration ? now - start : duration;
        *drain_deadline = now + 2000000ULL;
    }
    if (*drain_deadline && now >= *drain_deadline) {
        fprintf(stderr, "Receive queues did not drain within 2 seconds; capture is incomplete.\n");
        return 0;
    }
    return 1;
}

/* Write only data frames. Unsupported input is an explicit incomplete capture. */
static int write_frame(FILE* file, unsigned channel, uint64_t time_us, const BM_CanMessageTypeDef* msg)
{
    unsigned i, dlc = msg->ctrl.rx.DLC;
    unsigned length = asc_dlc_lengths[dlc];
    unsigned id = msg->ctrl.rx.IDE ? BM_GET_EXT_MSG_ID(msg->id) : BM_GET_STD_MSG_ID(msg->id);
    if (msg->ctrl.rx.RTR || (!msg->ctrl.rx.FDF && dlc > 8)) {
        fprintf(stderr, "Unsupported RTR frame or invalid Classic CAN DLC on channel %u.\n", channel);
        return 0;
    }
    if (msg->ctrl.rx.FDF) {
        fprintf(file, "%.6f CANFD %u Rx %X%s %u %u %X %u",
            time_us / 1000000.0, channel, id, msg->ctrl.rx.IDE ? "x" : "",
            msg->ctrl.rx.BRS, msg->ctrl.rx.ESI, dlc, length);
    } else {
        fprintf(file, "%.6f %u %X%s Rx d %u", time_us / 1000000.0,
            channel, id, msg->ctrl.rx.IDE ? "x" : "", length);
    }
    for (i = 0; i < length; ++i) fprintf(file, " %02X", msg->payload[i]);
    if (msg->ctrl.rx.FDF) fprintf(file, " 0 0 0 0 0 0 0 0"); /* optional Vector metadata */
    fputc('\n', file);
    return !ferror(file);
}

int main(int argc, char* argv[])
{
    BM_ChannelHandle channels[ASC_MAX_CHANNELS] = {0};
    BM_NotificationHandle notifications[ASC_MAX_CHANNELS] = {0};
    CaptureClock clocks[ASC_MAX_CHANNELS];
    FILE* file = NULL;
    unsigned first, count = 0, seconds, i;
    uint64_t start = 0, last_sample = 0, written = 0;
    uint64_t duration, cutoff, drain_deadline = 0;
    int initialized = 0, result = 1;
    time_t date;
    struct tm* local;
    char date_text[80];
    if (argc == 2 && !strcmp(argv[1], "--list")) return asc_list_channels();
    if (argc == 2 && !strcmp(argv[1], "--help")) {
        puts("Usage: bmapi_asc_record output.asc first-enumeration-index channel-count seconds\n"
             "       bmapi_asc_record --list\n"
             "Configure bitrate/resistor in bmapi_asc_common.h first. Ctrl+C stops capture.");
        return 0;
    }
    if (argc != 5 || !asc_unsigned(argv[2], ASC_ENUM_CAPACITY - 1, &first, 10) ||
        !asc_unsigned(argv[3], ASC_MAX_CHANNELS, &count, 10) || count == 0 ||
        !asc_unsigned(argv[4], UINT_MAX / 1000, &seconds, 10) || seconds == 0) {
        fprintf(stderr, "Invalid arguments; use --help. No hardware was opened.\n");
        return 2;
    }
    /* Never silently truncate an existing capture. */
    file = fopen(argv[1], "rb");
    if (file) { fclose(file); fprintf(stderr, "Output already exists.\n"); return 2; }
    file = create_capture(argv[1]);
    if (!file) { perror("Create ASC file"); return 2; }
    if (!asc_api_ok("BM_Init", BM_Init())) goto cleanup;
    initialized = 1;
    if (!asc_open_channels(channels, first, count)) goto cleanup;
    start = asc_now_us();
    for (i = 0; i < count; ++i) {
        uint64_t before, after;
        if (!asc_api_ok("BM_GetNotification", BM_GetNotification(channels[i], &notifications[i]))) goto cleanup;
        before = asc_now_us();
        if (!asc_api_ok("BM_GetTimestamp", BM_GetTimestamp(channels[i], &clocks[i].sampled))) goto cleanup;
        after = asc_now_us();
        clocks[i].elapsed = (int64_t)((before - start) + (after - before) / 2);
    }
    last_sample = asc_now_us();
    date = time(NULL);
    local = localtime(&date);
    if (!local || !strftime(date_text, sizeof(date_text), "%a %b %d %H:%M:%S %Y", local)) goto cleanup;
    fprintf(file, "date %s\nbase hex  timestamps absolute\ninternal events logged\n", date_text);
    if (ferror(file)) goto cleanup;
    signal(SIGINT, asc_stop);
    duration = cutoff = (uint64_t)seconds * 1000000ULL;
    printf("Recording for %u seconds; default buffers, no device logging configuration.\n", seconds);
    for (;;) {
        int all_empty = 1, stopping_at_start;
        /* Sample each running device counter, including during idle periods, to extend wrap. */
        uint64_t now = asc_now_us();
        if (!check_stop(start, duration, &cutoff, &drain_deadline)) goto cleanup;
        stopping_at_start = drain_deadline != 0;
        if (now - last_sample >= 1000000ULL) {
            for (i = 0; i < count; ++i) {
                uint32_t sampled;
                if (!asc_api_ok("BM_GetTimestamp", BM_GetTimestamp(channels[i], &sampled))) goto cleanup;
                clocks[i].elapsed += (uint32_t)(sampled - clocks[i].sampled);
                clocks[i].sampled = sampled;
            }
            last_sample = now;
        }
        if (!drain_deadline) BM_WaitForNotifications(notifications, (int)count, 100);
        /* Bounded batches keep a busy channel from starving another selected queue. */
        for (i = 0; i < count; ++i) {
            unsigned batch;
            BM_CanMessageTypeDef message;
            uint32_t timestamp, source;
            for (batch = 0; batch < 64; ++batch) {
                BM_StatusTypeDef status;
                int64_t elapsed;
                if (!check_stop(start, duration, &cutoff, &drain_deadline)) goto cleanup;
                status = BM_ReadCanMessage(channels[i], &message, &source, &timestamp);
                if (!check_stop(start, duration, &cutoff, &drain_deadline)) goto cleanup;
                if (status == BM_ERROR_QRCVEMPTY) break;
                if (status != BM_ERROR_OK) {
                    asc_report_bus_error(channels[i], status);
                    goto cleanup;
                }
                all_empty = 0;
                elapsed = clocks[i].elapsed + (int32_t)(timestamp - clocks[i].sampled);
                /* Drain post-cutoff traffic too, but do not include it in this capture. */
                if (elapsed > 0 && (uint64_t)elapsed > cutoff) continue;
                /* Frames queued before the session baseline are retained at elapsed zero. */
                if (!write_frame(file, i + 1, elapsed > 0 ? (uint64_t)elapsed : 0, &message)) {
                    fprintf(stderr, "ASC write failed or frame unsupported; capture is incomplete.\n");
                    goto cleanup;
                }
                ++written;
            }
        }
        /* Require a complete empty sweep after the stop was latched. */
        if (stopping_at_start && all_empty) break;
    }
    result = 0;
cleanup:
    if (initialized) {
        if (!asc_close_channels(channels, count)) result = 1;
        BM_UnInit();
    }
    if (file && fclose(file) != 0) { perror("Close ASC file"); result = 1; }
    printf("Recorded %llu frames; %s.\n", (unsigned long long)written,
        result ? "incomplete capture (see error)" : "capture closed");
    return result;
}
