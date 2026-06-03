/**
 * @file bmapi_dualthread_txrx.cpp
 * @brief BMAPI Example: CAN FD Communication with Dual Threads (Tx/Rx) Using Only the First Enumerated Channel
 * Mode: External Loopback
 * Baud Rate: Arbitration Phase 1000 kbps, Data Phase 8000 kbps
 * Threads: Reception Thread (with PTP Timestamp), Transmission Thread (20 Incremental Payload Messages per Second)
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


struct tm* get_local_time(time_t* t)
{
#ifdef _WIN32
    static struct tm tm;
    localtime_s(&tm, t);
    return &tm;
#else
    static struct tm tm;
    localtime_r(t, &tm);
    return &tm;
#endif
}

#define BMAPI_LOOPBACK_ENABLED
#define UDS_USE_CANFD 1
#define UDS_USE_29BIT_ADDRESS 0
#define BMAPI_MAX_CHANNEL_COUNT 32

#define TX_THREAD_NAME "CAN_TX_Thread"  
#define RX_THREAD_NAME "CAN_RX_Thread"  
#define THREAD_STACK_SIZE 1024 * 8     
#define THREAD_PRIO THREAD_PRIORITY_TIME_CRITICAL

#define TX_MSG_ID 0x123                 
#define TX_MSG_DLC 8                    
#define TX_MSG_COUNT_PER_CYCLE 20       
#define TX_CYCLE_MS 1                
#define TX_TIMEOUT 0                    

#define RX_TIMEOUT 1000000                

#ifdef xBMAPI_LOOPBACK_ENABLED
#define BMAPI_CAN_MODE BM_CAN_EXTERNAL_LOOPBACK_MODE  
#else
#define BMAPI_CAN_MODE BM_CAN_NORMAL_MODE            
#endif

#define BITRATE_NOMINAL 500    
#define BITRATE_DATA 2000       
#define SAMPLE_POS_NOMINAL 75   
#define SAMPLE_POS_DATA 80      

typedef struct
{
    BM_ChannelHandle channel;           
    BM_NotificationHandle notification; 
    THREAD_HANDLE thread_handle;        
    volatile BOOL bRequestExit;         
} ThreadParams;


static ThreadExitCode can_tx_thread(void* param);
static ThreadExitCode can_rx_thread(void* param);

int main(int argc, char* argv[])
{
    ThreadParams tx_thread_params = { 0 };  
    ThreadParams rx_thread_params = { 0 }; 

    BM_NotificationHandle notification = NULL;
    BM_ChannelInfoTypeDef channelinfos[32];
    BM_ChannelHandle channel = NULL;
    int nchannels = sizeof(channelinfos) / sizeof(channelinfos[0]);
    int channelid = 0;
    int exitcode = 0;
    BM_BitrateTypeDef bitrate;
    BM_StatusTypeDef error = BM_ERROR_OK;
    int i;

    printf(
        "\n"
        "=========== BMAPI DUALTHREAD TXRX PROGRAM ==========\n"
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
    error = BM_Enumerate(channelinfos, &nchannels);
    if (error != BM_ERROR_OK)
    {
        exitcode = 2;
        goto __exit;
    }

    for (i = 0; i < nchannels; i++)
    {
        printf("[%d] %s\n", i, channelinfos[i].name);
    }

    if (channelid < nchannels)
    {
        /* Step3: Open the selected channel and configure baudrate */
        printf("Opening channel %s (%d.%d.%d.%d) in loopback mode ...\n",
            channelinfos[channelid].name,
            channelinfos[channelid].version[0],
            channelinfos[channelid].version[1],
            channelinfos[channelid].version[2],
            channelinfos[channelid].version[3]
        );
        memset(&bitrate, 0, sizeof(bitrate));
        bitrate.nbitrate = BITRATE_NOMINAL;
        bitrate.dbitrate = BITRATE_DATA;
        bitrate.nsamplepos = SAMPLE_POS_NOMINAL;
        bitrate.dsamplepos = SAMPLE_POS_DATA;
        /* BTR is not mandatory, hardware will calculate BTR using bitrate&samplepos automatically */
        error = BM_OpenEx(&channel, &channelinfos[channelid], BMAPI_CAN_MODE, BM_TRESISTOR_120, &bitrate, NULL, 0);
        if (error != BM_ERROR_OK)
        {
            exitcode = 3;
            goto __exit;
        }
        tx_thread_params.channel = channel;
        rx_thread_params.channel = channel;
        BM_SetPtpMode(tx_thread_params.channel, BM_PTP_INPUT_USB_SOF);
        if (BM_SyncPtpTimes(&tx_thread_params.channel, 1) != BM_ERROR_OK)
        {
            printf("Failed to sync PTP timestamps.\n");
        }
        printf("Getting channel notification handle ...\n");
        error = BM_GetNotification(channel, &notification);
        if (error != BM_ERROR_OK)
        {
            exitcode = 4;
            goto __exit;
        }

        BM_SetBuffer(channel, BM_READ_BUFFER, BM_NO_BUFFER); /* ����Ӳ���˱��Ļ����� */
        BM_SetBuffer(channel, BM_WRITE_BUFFER, BM_NO_BUFFER); /* ���������˱��Ļ����� */

        //BM_SetBuffer(channel, BM_READ_BUFFER, BM_DEFAULT_BUFFER); /* ����Ӳ���˱��Ļ����� */
        //BM_SetBuffer(channel, BM_WRITE_BUFFER, BM_DEFAULT_BUFFER); /* ���������˱��Ļ����� */
        BM_DataTypeDef ffmsg = { 0 };
        BM_DataTypeDef fcmsg = { 0 };

        uint64_t ffts1 = 0;
        uint64_t fcts1 = 0;

        BM_WaitForNotifications(&notification, 1, RX_TIMEOUT); /* Wait for ISOTP FF */
        ffts1 = BM_GetHostPtpTime();
        BM_Read(channel, &ffmsg); /* Read ISOTP FF */
        BM_CanMessageTypeDef msg = { 0 };
        uint32_t n = 0;
        BM_StatusTypeDef error = BM_ERROR_OK;

        msg.id.SID = TX_MSG_ID;
        msg.ctrl.tx.DLC = TX_MSG_DLC;
        msg.ctrl.tx.FDF = 1;
        msg.ctrl.tx.BRS = 1;
        msg.ctrl.tx.ESI = 0;
        *(uint32_t*)&msg.payload[0] = n;
        n++;
        error = BM_WriteCanMessage(channel, &msg, 0, 111, NULL);
        //BM_Write(channel, &fcmsg, 0/* Timeout value ignored*/, NULL); /* Write ISOTP FC */
        BM_WaitForNotifications(&notification, 1, 100);
        error =  BM_Read(channel, &fcmsg); /* Read ISOTP FC Transmit Complete Event (with timestamp) */


        uint64_t ffts = 0;
        uint64_t fcts = 0;
        BM_GetDataPtpTimestamp(channel, &ffmsg, &ffts);
        BM_GetDataPtpTimestamp(channel, &fcmsg, &fcts);
        fcts1 = BM_GetHostPtpTime();
        printf("%dms %u ns elapsed from FF to FC.\n", (uint32_t)(fcts - ffts)/1000000,(uint32_t)(fcts - ffts));
        printf("%dms %u ns elapsed from FF to FC.\n", (uint32_t)(fcmsg.timestamp - ffmsg.timestamp) / 1000000, (uint32_t)(fcmsg.timestamp - ffmsg.timestamp));
        //printf("%u ns elapsed from FF to FC.\n", (uint32_t)(fcts1 - ffts1));
        //rx_thread_params.notification = tx_thread_params.notification;
        //tx_thread_params.bRequestExit = FALSE;
        //rx_thread_params.bRequestExit = FALSE;
        //tx_thread_params.thread_handle = INVALID_THREAD_HANDLE;
        //rx_thread_params.thread_handle = INVALID_THREAD_HANDLE;
        //printf("\nTest will start in 1 second, press ENTER to exit program.\n");
        //Sleep(1000);

        //StartThread(
        //    tx_thread_params.thread_handle,
        //    can_tx_thread,
        //    &tx_thread_params
        //);
        //if (tx_thread_params.thread_handle == INVALID_THREAD_HANDLE)
        //{
        //    printf("Create TX Thread Failed! Err: %d\n", GetLastError());
        //    exitcode = 5;
        //    goto __exit;
        //}
        //printf("TX Thread Created: %s\n", TX_THREAD_NAME);

        //StartThread(
        //    rx_thread_params.thread_handle,
        //    can_rx_thread,
        //    &rx_thread_params
        //);
        //if (rx_thread_params.thread_handle == INVALID_THREAD_HANDLE)
        //{
        //    printf("Create RX Thread Failed! Err: %d\n", GetLastError());
        //    exitcode = 6;
        //    goto __exit;
        //}
        //printf("RX Thread Created: %s\n", RX_THREAD_NAME);

        //bool bExitMainLoop = FALSE;
        //while (!bExitMainLoop)
        //{
        //    char c = getchar();
        //    if (c == '\r' || c == '\n')
        //    {
        //        printf("\nDetected 'Enter' key, requesting threads to exit...\n");
        //        tx_thread_params.bRequestExit = TRUE;
        //        rx_thread_params.bRequestExit = TRUE;
        //        bExitMainLoop = TRUE;
        //    }
        //    Sleep(1);
        //}

        printf("Waiting for TX/RX threads to exit...\n");
        //CloseThread(tx_thread_params.thread_handle);
        //CloseThread(rx_thread_params.thread_handle);
        /* Step5: Close the opened channel and release connected hardware */
        printf("Closing channel ...\n");
        error = BM_Close(channel);
        if (error != BM_ERROR_OK)
        {
            exitcode = 5;
            goto __exit;
        }
    }
    else
    {
        printf("Input Error: There are %d available Busmust device, port index %d is invalid.\n", nchannels, channelid);
        exitcode = 6;
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
static const uint8_t dlc2len[16] = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 12, 16, 20, 24, 32, 48, 64 };

static void timeval_to_formatted_string(uint64_t utcTimestamp, char* str, size_t size)
{
    time_t time = (utcTimestamp / 1000000000);
    struct tm* tm_info;
    tm_info = get_local_time(&time);

    if (tm_info == NULL)
    {
        snprintf(str, size, "Invalid");
        return;
    }
    else
    {
        strftime(str, size, "%Y-%m-%d %H:%M:%S:", tm_info);
        snprintf(str + strlen(str), size - strlen(str), "%06ld", (long)(utcTimestamp % 1000000000 / 1000));
    }
}

static void print_msg(int channel, uint32_t timestamp, uint64_t utcts, const BM_CanMessageTypeDef* canMsg, int isTxMsg)
{
    int len = dlc2len[canMsg->ctrl.rx.DLC];
    char charUtcts[32];
    timeval_to_formatted_string(utcts, charUtcts, sizeof(charUtcts));
    printf(
        "CH%d %s utcts %s: ts=%u, id=%x, len=%d, payload=[",
        channel + 1, isTxMsg ? "TX" : "RX", charUtcts, timestamp, canMsg->id.SID, len
    );

    for (int i = 0; i < len; i++)
    {
        printf("%02x", canMsg->payload[i]);
    }
    printf("]\n");
}


static ThreadExitCode can_tx_thread(void* param)
{
    ThreadExitCode exitcode = ExitCode_OK;
    ThreadParams* tx_params = (ThreadParams*)param;
    BM_CanMessageTypeDef msg = { 0 };
    uint32_t n = 0;
    BM_StatusTypeDef error = BM_ERROR_OK;

    msg.id.SID = TX_MSG_ID;
    msg.ctrl.tx.DLC = TX_MSG_DLC;
    msg.ctrl.tx.FDF = 1;
    msg.ctrl.tx.BRS = 1;
    msg.ctrl.tx.ESI = 0;

    printf("\nTX Thread Started: Send %d messages every %d ms\n", TX_MSG_COUNT_PER_CYCLE, TX_CYCLE_MS);

    while (!tx_params->bRequestExit)
    {
        for (int32_t i = 0; i < TX_MSG_COUNT_PER_CYCLE; i++)
        {
            *(uint32_t*)&msg.payload[0] = n;
            n++;
            error = BM_WriteCanMessage(tx_params->channel, &msg, 0, TX_TIMEOUT, NULL);

            if (error != BM_ERROR_OK)
            {
                printf("TX Failed! Seq: %d | Err: 0x%08X\n", n - 1, error);
                Sleep(1);
            }
        }
        Sleep(TX_CYCLE_MS);
    }

    printf("TX Thread Exited\n");
    return exitcode;
}

static ThreadExitCode can_rx_thread(void* param)
{
    ThreadExitCode exitcode = ExitCode_OK;
    ThreadParams* rx_params = (ThreadParams*)param;
    BM_DataTypeDef data = { 0 };
    BM_StatusTypeDef error = BM_ERROR_OK;
    uint64_t utcts = 0;
    printf("\nRX Thread Started: Wait for Messages (Timeout: %d s)\n", RX_TIMEOUT / 1000);

    while (!rx_params->bRequestExit)
    {
        if (rx_params->bRequestExit) break;
        int index = BM_WaitForNotifications(&rx_params->notification, 1, RX_TIMEOUT);
        if (index < 0)
        {
            if (rx_params->bRequestExit) break;
            printf("RX Wait Timeout! Err: 0x%08X\n", error);
            continue;
        }

        while (1)
        {
            if (rx_params->bRequestExit) break;
            error = BM_Read(rx_params->channel, &data);
            if (error != BM_ERROR_OK)
            {
                if (error == BM_ERROR_QRCVEMPTY) break;
                printf("RX Read Failed! Err: 0x%08X\n", error);
                break;
            }

            BM_GetDataPtpTimestamp(rx_params->channel, &data, &utcts);
            BM_CanMessageTypeDef* msg = (BM_CanMessageTypeDef*)data.payload;
            bool isTxMsg = data.header.type & BM_ACK_DATA;
            print_msg(0, data.timestamp, utcts, msg, isTxMsg);
        }
    }

    printf("RX Thread Exited\n");
    return exitcode;
}
