/**
 * @file nobuffer_txrx.cpp
 * @brief BMAPI Example: This program demonstrates how to use BMAPI (Busmaster API) to send and receive CAN/CAN FD messages in no buffer mode.
 * @brief The characteristic of non buffering mode is that messages are directly sent/received through hardware without passing through intermediate buffers, which is suitable for scenarios with high real-time requirements.
 * Mode: NORMAL
 * Baud Rate: Arbitration Phase 1000 kbps, Data Phase 8000 kbps
 * @author busmust
 * @version 1.0.0.1
 * @copyright Copyright 2020 by Busmust Tech Co., Ltd.
 */

#include <stdint.h>
#include <stdbool.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <time.h>
#include "bmapi.h"
#include "osal.h"

#define BMAPI_MAX_CHANNEL_COUNT 32

#define ECU_MSG_ID              0x799
#define TESTER_MSG_ID           0x791
#define TX_MSG_DLC              8
#define RX_TIMEOUT              1000000
#define TX_TIMEOUT              5000
#define BMAPI_CAN_MODE          BM_CAN_NORMAL_MODE
#define BITRATE_NOMINAL         1000
#define BITRATE_DATA            8000
#define SAMPLE_POS_NOMINAL      70
#define SAMPLE_POS_DATA         70

 /**
  * @brief This program demonstrates how to use BMAPI (Busmaster API) to send and receive CAN/CAN FD messages in no buffer mode.
  * @param[in]  argc    Number of command line arguments
  * @param[in]  argv[0] Path of this program
  * @param[in]  argv[1] Tester channel id
  * @param[in]  argv[2] Ecuchannelid channel id
  * @return     Program exit code
  */

int main(int argc, char* argv[])
{
    BM_NotificationHandle notification[BMAPI_MAX_CHANNEL_COUNT] = { NULL };
    BM_ChannelInfoTypeDef channelinfos[BMAPI_MAX_CHANNEL_COUNT];
    BM_ChannelHandle channel[BMAPI_MAX_CHANNEL_COUNT] = { NULL };
    BM_ChannelHandle syncPtpTimesChannel[BMAPI_MAX_CHANNEL_COUNT] = { NULL };
    int nchannels = sizeof(channelinfos) / sizeof(channelinfos[0]);
    int testerchannelid = argc > 1 ? atoi(argv[1]) : 0;
    int ecuchannelid = argc > 2 ? atoi(argv[2]) : 1;
    int exitcode = 0;
    BM_BitrateTypeDef bitrate;
    BM_StatusTypeDef error = BM_ERROR_OK;
    uint64_t ffts = 0;
    uint64_t fcts = 0;
    BM_DataTypeDef ffmsg = { 0 };
    BM_DataTypeDef fcmsg = { 0 };
    uint8_t payload[64] = { 0,1,2,3,4,5,6,7 };
    int i;
    printf(
        "\n"
        "===========  BMAPI NOBUFFER TXRX PROGRAM  ==========\n"
        "===========     BUSMUST TECH Co.,Ltd.     ==========\n"
        __DATE__ " " __TIME__ "\n"
    );
    /* Step1: Initialize BMAPI library before any other operation */
    printf("Initializing BMAPI library ...\n");
    error = BM_Init();
    if (error != BM_ERROR_OK)
    {
        exitcode = 1;
        goto __exit;
    }
    /* Step2: Enumerate connected device channels */
    printf("Enumerating channels ...\n");
    error = BM_EnumerateByCap(channelinfos, &nchannels, BM_CAN_CAP);
    if (error != BM_ERROR_OK)
    {
        printf("Operation error occurred\n");
        exitcode = 2;
        goto __exit;
    }

    if (testerchannelid >= nchannels)
    {
        printf("There are %d available bus devices, port index %d is invalid.\n", testerchannelid, nchannels);
        exitcode = 2;
        goto __exit;
    }

    if (ecuchannelid >= nchannels)
    {
        printf("There are %d available bus devices, port index %d is invalid.\n", ecuchannelid, nchannels);
        exitcode = 2;
        goto __exit;
    }

    for (i = 0; i < nchannels; i++)
    {
        printf("[%d] %s\n", i, channelinfos[i].name);
    }
    for (i = 0; i < nchannels; i++)
    {
        if (i == testerchannelid || i == ecuchannelid)
        {
            /* Step3: Open the selected channel and configure baudrate */
            printf("Opening channel %s (%d.%d.%d.%d) in normal mode ...\n",
                channelinfos[i].name,
                channelinfos[i].version[0],
                channelinfos[i].version[1],
                channelinfos[i].version[2],
                channelinfos[i].version[3]
            );
            memset(&bitrate, 0, sizeof(bitrate));
            bitrate.nbitrate = BITRATE_NOMINAL;
            bitrate.dbitrate = BITRATE_DATA;
            bitrate.nsamplepos = SAMPLE_POS_NOMINAL;
            bitrate.dsamplepos = SAMPLE_POS_DATA;
            /* BTR is not mandatory, hardware will calculate BTR using bitrate&samplepos automatically */
            error = BM_OpenEx(&channel[i], &channelinfos[i], BMAPI_CAN_MODE, BM_TRESISTOR_120, &bitrate, NULL, 0);
            if (error != BM_ERROR_OK)
            {
                exitcode = 3;
                goto __exit;
            }
            BM_SetPtpMode(channel[i], BM_PTP_INPUT_USB_SOF);
            printf("Getting channel notification handle ...\n");
            error = BM_GetNotification(channel[i], &notification[i]);
            if (error != BM_ERROR_OK)
            {
                exitcode = 4;
                goto __exit;
            }
            BM_SetBuffer(channel[i], BM_READ_BUFFER, BM_NO_BUFFER);
            BM_SetBuffer(channel[i], BM_WRITE_BUFFER, BM_NO_BUFFER);
        }
    }
    syncPtpTimesChannel[0] = channel[testerchannelid];
    syncPtpTimesChannel[1] = channel[ecuchannelid];
    if (BM_SyncPtpTimes(&syncPtpTimesChannel[0], 2) != BM_ERROR_OK)
    {
        printf("Failed to sync PTP timestamps.\n");
    }
    BM_INIT_CAN_FD_DATA(
        fcmsg,
        ECU_MSG_ID,
        TX_MSG_DLC,
        0,
        1, 1, 0, 0,
        payload
    );

    BM_INIT_CAN_FD_DATA(
        ffmsg,
        TESTER_MSG_ID,
        TX_MSG_DLC,
        0,
        1, 1, 0, 0,
        payload
    );

    error = BM_Write(channel[testerchannelid], &ffmsg, TX_TIMEOUT, NULL);
    BM_WaitForNotifications(&notification[ecuchannelid], 1, RX_TIMEOUT);
    memset(&ffmsg, 0, sizeof(BM_DataTypeDef));
    error = BM_Read(channel[ecuchannelid], &ffmsg);

    error = BM_Write(channel[ecuchannelid], &fcmsg, 0, NULL);
    BM_WaitForNotifications(&notification[ecuchannelid], 1, RX_TIMEOUT);
    memset(&fcmsg, 0, sizeof(BM_DataTypeDef));
    error = BM_Read(channel[ecuchannelid], &fcmsg);

    BM_GetDataPtpTimestamp(channel[ecuchannelid], &ffmsg, &ffts);
    BM_GetDataPtpTimestamp(channel[ecuchannelid], &fcmsg, &fcts);
    if (error == BM_ERROR_OK)
    {
        printf("%d.%dms elapsed from FF to FC.\n", (uint32_t)(fcts - ffts) / 1000000, (uint32_t)(fcts - ffts)% 1000000);
    }
    else
    {
        exitcode = 6;
        goto __exit;
    }

    printf("Closing Tester channel ...\n");
    error = BM_Close(channel[testerchannelid]);
    if (error != BM_ERROR_OK)
    {
        exitcode = 5;
        goto __exit;
    }
    printf("Closing ECU channel ...\n");
    error = BM_Close(channel[ecuchannelid]);
    if (error != BM_ERROR_OK)
    {
        exitcode = 5;
        goto __exit;
    }

__exit:
    if (error != BM_ERROR_OK)
    {
        char buffer[256] = { 0 };
        BM_GetErrorText(error, buffer, sizeof(buffer), 0);
        printf("Error 0X%08X: %s.\n", error, buffer);
    }
    /* Step6: Cleanup library resource */
    BM_UnInit();
    printf("Exit app.\n");
    return exitcode;
}


