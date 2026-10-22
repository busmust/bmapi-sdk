/* Minimal online CAN/CAN FD ASC replay. --check validates a file without opening hardware. */
#include "bmapi_asc_common.h"

static void asc_sleep_ms(unsigned milliseconds)
{
#ifdef _WIN32
    Sleep(milliseconds);
#else
    struct timespec value;
    value.tv_sec = milliseconds / 1000;
    value.tv_nsec = (milliseconds % 1000) * 1000000L;
    while (nanosleep(&value, &value) != 0 && errno == EINTR && !asc_stopping) {}
#endif
}

typedef struct {
    uint64_t due_us;
    unsigned channel;
    BM_CanMessageTypeDef message;
} AscFrame;

/* Return 1 for a data frame, 0 for known metadata, -1 for malformed/unsupported input. */
static int parse_line(char* line, AscFrame* frame, int* header)
{
    char* words[96];
    unsigned n = 0, i, at, id, extended = 0, fd, brs = 0, esi = 0, dlc, length;
    char* word;
    char* end;
    double seconds;
    while (*line == ' ' || *line == '\t') ++line;
    if (!*line || *line == '\r' || *line == '\n' || !strncmp(line, "//", 2)) return 0;
    if (!strncmp(line, "date ", 5) || !strncmp(line, "internal events logged", 22) ||
        !strncmp(line, "Begin Triggerblock", 18) || !strncmp(line, "End TriggerBlock", 16)) return 0;
    for (word = strtok(line, " \t\r\n"); word; word = strtok(NULL, " \t\r\n")) {
        if (n == sizeof(words) / sizeof(words[0])) return -1;
        words[n++] = word;
    }
    if (n && !strcmp(words[0], "base")) {
        if (n != 4 || strcmp(words[1], "hex") || strcmp(words[2], "timestamps") || strcmp(words[3], "absolute")) return -1;
        *header = 1;
        return 0;
    }
    if (!*header || n < 4) return -1;
    errno = 0;
    seconds = strtod(words[0], &end);
    if (errno || end == words[0] || *end || seconds != seconds || seconds < 0 ||
        seconds >= (double)UINT64_MAX / 1000000.0) return -1;
    /* Common ASCWriter measurement marker carries no bus frame. */
    if (n == 4 && !strcmp(words[1], "Start") && !strcmp(words[2], "of") &&
        !strcmp(words[3], "measurement")) return 0;
    if (n < 6) return -1;
    memset(frame, 0, sizeof(*frame));
    frame->due_us = (uint64_t)(seconds * 1000000.0 + 0.5);
    fd = !strcmp(words[1], "CANFD");
    if (!asc_unsigned(words[fd ? 2 : 1], ASC_MAX_CHANNELS, &frame->channel, 10) || !frame->channel) return -1;
    if (strcmp(words[3], "Rx") && strcmp(words[3], "Tx")) return -1;
    at = fd ? 4 : 2;
    i = (unsigned)strlen(words[at]);
    if (i && (words[at][i - 1] == 'x' || words[at][i - 1] == 'X')) {
        extended = 1;
        words[at][i - 1] = '\0';
    }
    if (!asc_unsigned(words[at], extended ? 0x1FFFFFFFUL : 0x7FFUL, &id, 16)) return -1;
    if (extended) { BM_SET_EXT_MSG_ID(frame->message.id, id); }
    else { BM_SET_STD_MSG_ID(frame->message.id, id); }
    if (fd) {
        at = 5;
        /* A single optional symbolic frame name may precede the BRS field. */
        if (at < n && strcmp(words[at], "0") && strcmp(words[at], "1")) ++at;
        if (n < at + 4 || !asc_unsigned(words[at++], 1, &brs, 10) ||
            !asc_unsigned(words[at++], 1, &esi, 10) || !asc_unsigned(words[at++], 15, &dlc, 16) ||
            !asc_unsigned(words[at++], 64, &length, 10) || asc_dlc_lengths[dlc] != length) return -1;
    } else {
        if (strcmp(words[4], "d") || !asc_unsigned(words[5], 8, &dlc, 10)) return -1;
        length = dlc;
        at = 6;
    }
    frame->message.ctrl.tx.IDE = extended;
    frame->message.ctrl.tx.FDF = fd;
    frame->message.ctrl.tx.BRS = brs;
    frame->message.ctrl.tx.ESI = esi;
    frame->message.ctrl.tx.DLC = dlc;
    if (n < at + length) return -1;
    for (i = 0; i < length; ++i) {
        unsigned byte;
        if (!asc_unsigned(words[at++], 255, &byte, 16)) return -1;
        frame->message.payload[i] = (uint8_t)byte;
    }
    /* Accept the eight numeric Vector CAN FD metadata columns, or omit them all. */
    if (at != n) {
        if (!fd || n - at != 8) return -1;
        for (; at < n; ++at) {
            unsigned unused;
            if (!asc_unsigned(words[at], UINT_MAX, &unused, 16)) return -1;
        }
    }
    return 1;
}

static int read_frame(FILE* file, unsigned* line_number, int* header, AscFrame* frame)
{
    char line[2048];
    for (;;) {
        int parsed;
        char* terminator;
        /* Distinguish embedded NUL input from fgets' own string terminator. */
        memset(line, 1, sizeof(line));
        if (!fgets(line, sizeof(line), file)) break;
        ++*line_number;
        terminator = (char*)memchr(line, 0, sizeof(line));
        if (memchr(terminator + 1, 0, sizeof(line) - (size_t)(terminator + 1 - line))) {
            fprintf(stderr, "Embedded NUL at ASC line %u.\n", *line_number);
            return -1;
        }
        if (!strchr(line, '\n') && !feof(file)) {
            fprintf(stderr, "ASC line %u is too long.\n", *line_number);
            return -1;
        }
        parsed = parse_line(line, frame, header);
        if (parsed < 0) {
            fprintf(stderr, "Unsupported or malformed ASC record/header at line %u.\n", *line_number);
            return -1;
        }
        if (parsed) return 1;
    }
    if (ferror(file)) { perror("Read ASC file"); return -1; }
    return 0;
}

int main(int argc, char* argv[])
{
    BM_ChannelHandle channels[ASC_MAX_CHANNELS] = {0};
    AscFrame frame;
    FILE* file;
    unsigned first = 0, count = 0, line_number = 0;
    uint64_t total = 0, sent = 0, start;
    int check, header = 0, read_result, initialized = 0, result = 1;
    if (argc == 2 && !strcmp(argv[1], "--list")) return asc_list_channels();
    if (argc == 2 && !strcmp(argv[1], "--help")) {
        puts("Usage: bmapi_asc_replay input.asc first-enumeration-index channel-count\n"
             "       bmapi_asc_replay --check input.asc\n"
             "       bmapi_asc_replay --list\n"
             "Only base hex / timestamps absolute CAN/CAN FD data frames. Ctrl+C stops playback.");
        return 0;
    }
    check = argc == 3 && !strcmp(argv[1], "--check");
    if (!check && (argc != 4 || !asc_unsigned(argv[2], ASC_ENUM_CAPACITY - 1, &first, 10) ||
        !asc_unsigned(argv[3], ASC_MAX_CHANNELS, &count, 10) || !count)) {
        fprintf(stderr, "Invalid arguments; use --help. No hardware was opened.\n");
        return 2;
    }
    /* Binary mode prevents Windows Ctrl+Z from silently terminating validation. */
    file = fopen(argv[check ? 2 : 1], "rb");
    if (!file) { perror("Open ASC file"); return 2; }
    /* Validate every record first, so a malformed later record cannot cause a partial send. */
    while ((read_result = read_frame(file, &line_number, &header, &frame)) > 0) {
        if (!check && frame.channel > count) {
            fprintf(stderr, "ASC channel %u at line %u has no selected hardware mapping.\n", frame.channel, line_number);
            read_result = -1;
            break;
        }
        ++total;
    }
    if (read_result < 0) goto cleanup;
    if (check || !total) { printf("Validated %llu frames; no hardware opened.\n", (unsigned long long)total); result = 0; goto cleanup; }
    if (fseek(file, 0, SEEK_SET) != 0) { perror("Rewind ASC file"); goto cleanup; }
    if (!asc_api_ok("BM_Init", BM_Init())) goto cleanup;
    initialized = 1;
    if (!asc_open_channels(channels, first, count)) goto cleanup;
    signal(SIGINT, asc_stop);
    header = 0;
    line_number = 0;
    start = asc_now_us();
    while (!asc_stopping && (read_result = read_frame(file, &line_number, &header, &frame)) > 0) {
        BM_StatusTypeDef status;
        uint32_t timestamp;
        if (frame.channel > count) { fprintf(stderr, "File changed during playback at line %u.\n", line_number); goto cleanup; }
        while (!asc_stopping && asc_now_us() - start < frame.due_us) asc_sleep_ms(1);
        if (asc_stopping) break;
        status = BM_WriteCanMessage(channels[frame.channel - 1], &frame.message, 0, 1000, &timestamp);
        if (status != BM_ERROR_OK) {
            fprintf(stderr, "Playback stopped at line %u after %llu successful sends.\n", line_number, (unsigned long long)sent);
            asc_report_bus_error(channels[frame.channel - 1], status);
            goto cleanup;
        }
        ++sent;
    }
    if (read_result < 0) goto cleanup;
    result = asc_stopping ? 130 : 0;
cleanup:
    if (initialized) {
        if (!asc_close_channels(channels, count)) result = 1;
        BM_UnInit();
    }
    if (fclose(file) != 0) { perror("Close ASC file"); result = 1; }
    if (!check && initialized) printf("Sent %llu/%llu frames; %s.\n",
        (unsigned long long)sent, (unsigned long long)total, result ? "stopped/incomplete" : "complete");
    return result;
}
