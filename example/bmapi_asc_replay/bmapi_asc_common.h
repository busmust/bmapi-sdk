/* Private helpers for this ASC example; this is not a public BMAPI header. */
#ifndef BMAPI_ASC_COMMON_H
#define BMAPI_ASC_COMMON_H
#if !defined(_WIN32) && !defined(_POSIX_C_SOURCE)
#define _POSIX_C_SOURCE 200809L /* clock_gettime/nanosleep under strict C99. */
#endif
#define _CRT_SECURE_NO_WARNINGS
#ifdef __MINGW32__
#define __USE_MINGW_ANSI_STDIO 1 /* C99 formats such as %llu with the MinGW CRT. */
#endif
#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>
#include <errno.h>
#include <limits.h>
#include <signal.h>
#include <time.h>
#ifdef _WIN32
#include <windows.h>
#else
#include <unistd.h>
#endif
#include "bmapi.h"

#define ASC_MAX_CHANNELS 32 /* demonstration limit, not a hardware limit */
#define ASC_ENUM_CAPACITY 256
/* Copy the bitrate/sample point/resistor configuration verified in BUSMASTER/BMCLI. */
#define ASC_NOMINAL_KBIT 500
#define ASC_DATA_KBIT 2000
#define ASC_SAMPLE_PERCENT 75
#define ASC_RESISTOR BM_TRESISTOR_DISABLED

static const unsigned asc_dlc_lengths[16] = {0,1,2,3,4,5,6,7,8,12,16,20,24,32,48,64};
static volatile sig_atomic_t asc_stopping;
static void asc_stop(int sig) { (void)sig; asc_stopping = 1; }

static uint64_t asc_now_us(void)
{
#ifdef _WIN32
    LARGE_INTEGER counter, frequency;
    QueryPerformanceCounter(&counter);
    QueryPerformanceFrequency(&frequency);
    return (uint64_t)(counter.QuadPart / frequency.QuadPart) * 1000000ULL +
        (uint64_t)((counter.QuadPart % frequency.QuadPart) * 1000000LL / frequency.QuadPart);
#else
    struct timespec value;
    clock_gettime(CLOCK_MONOTONIC, &value);
    return (uint64_t)value.tv_sec * 1000000ULL + (uint64_t)value.tv_nsec / 1000ULL;
#endif
}

static int asc_unsigned(const char* text, unsigned long maximum, unsigned* value, int base)
{
    char* end;
    unsigned long number;
    if (!text[0] || text[0] == '-' || text[0] == '+') return 0;
    errno = 0;
    number = strtoul(text, &end, base);
    if (errno || *end || number > maximum) return 0;
    *value = (unsigned)number;
    return 1;
}

static int asc_api_ok(const char* operation, BM_StatusTypeDef status)
{
    char text[256] = {0};
    if (status == BM_ERROR_OK) return 1;
    BM_GetErrorText(status, text, sizeof(text), 0);
    fprintf(stderr, "%s: 0x%08X %s\n", operation, (unsigned)status, text);
    return 0;
}

/* Recovery is not a retry of an uncertain send and never turns a failed run into PASS. */
static void asc_report_bus_error(BM_ChannelHandle channel, BM_StatusTypeDef status)
{
    asc_api_ok("CAN operation", status);
    if (status & BM_ERROR_BUSOFF) {
        BM_StatusTypeDef recovery = BM_RecoverBusOff(channel);
        asc_api_ok("BM_RecoverBusOff", recovery);
        fprintf(stderr, "Run stopped; check the bus/configuration before starting again.\n");
    }
}

static int asc_open_channels(BM_ChannelHandle channels[], unsigned first, unsigned count)
{
    BM_ChannelInfoTypeDef* info = (BM_ChannelInfoTypeDef*)calloc(ASC_ENUM_CAPACITY, sizeof(*info));
    BM_BitrateTypeDef bitrate;
    int available = ASC_ENUM_CAPACITY;
    unsigned i;
    int ok = 0;
    if (!info) { fprintf(stderr, "Allocate channel enumeration failed.\n"); return 0; }
    if (!asc_api_ok("BM_Enumerate", BM_Enumerate(info, &available))) goto done;
    if (available < 0 || available > ASC_ENUM_CAPACITY || first >= (unsigned)available ||
        count == 0 || count > ASC_MAX_CHANNELS || count > (unsigned)available - first) {
        fprintf(stderr, "Invalid range: first=%u count=%u, available=%d\n", first, count, available);
        goto done;
    }
    /* Validate the whole selection before opening any channel. */
    for (i = 0; i < count; ++i) {
        if (!(info[first + i].cap & (BM_CAN_CAP | BM_CAN_FD_CAP))) {
            fprintf(stderr, "Enumeration index %u is not a CAN channel.\n", first + i);
            goto done;
        }
        printf("ASC channel %u -> enumeration index %u: %s, port %u\n",
            i + 1, first + i, info[first + i].name, info[first + i].port);
    }
    memset(&bitrate, 0, sizeof(bitrate));
    bitrate.nbitrate = ASC_NOMINAL_KBIT;
    bitrate.dbitrate = ASC_DATA_KBIT;
    bitrate.nsamplepos = bitrate.dsamplepos = ASC_SAMPLE_PERCENT;
    for (i = 0; i < count; ++i) {
        if (!asc_api_ok("BM_OpenEx", BM_OpenEx(&channels[i], &info[first + i],
            BM_CAN_NORMAL_MODE, ASC_RESISTOR, &bitrate, NULL, 0))) goto done;
    }
    ok = 1;
done:
    free(info);
    return ok;
}

static int asc_close_channels(BM_ChannelHandle channels[], unsigned count)
{
    int ok = 1;
    unsigned i;
    for (i = 0; i < count; ++i) {
        if (channels[i] && !asc_api_ok("BM_Close", BM_Close(channels[i]))) ok = 0;
        channels[i] = NULL;
    }
    return ok;
}

static int asc_list_channels(void)
{
    BM_ChannelInfoTypeDef* info = (BM_ChannelInfoTypeDef*)calloc(ASC_ENUM_CAPACITY, sizeof(*info));
    int available = ASC_ENUM_CAPACITY, i;
    int ok;
    if (!info) { fprintf(stderr, "Allocate channel enumeration failed.\n"); return 1; }
    ok = asc_api_ok("BM_Init", BM_Init());
    if (!ok) { free(info); return 1; }
    ok = asc_api_ok("BM_Enumerate", BM_Enumerate(info, &available));
    if (ok && available >= 0 && available <= ASC_ENUM_CAPACITY) {
        for (i = 0; i < available; ++i)
            printf("[%d] %s, port %u, capabilities 0x%04X\n", i, info[i].name, info[i].port, info[i].cap);
    } else ok = 0;
    BM_UnInit();
    free(info);
    return ok ? 0 : 1;
}
#endif
