/**
 * @file hw_replay_demo.cpp
 * @brief BMAPI Example: Hardware-based message replay functionality
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
#define BMAPI_MAX_CHANNEL_COUNT         32
/* Configuration save mask bits (from bm_usb_private_def.h) */
#define BM_CAN_MODE_FLAG                0x0001U    /* CAN mode configuration */
#define BM_CAN_BITRATE_FLAG             0x0002U    /* Bitrate configuration */
#define BM_CAN_TERMINAL_RESISTOR_FLAG   0x0004U    /* Terminal resistor configuration */
#define BM_CAN_REPLAY_CONFIG_FLAG       0x000CU    /* Replay configuration */
#define MAX_TX_MSG_COUNT                400
#define TX_MSG_DLC                      8
#define RX_TIMEOUT                      1000000
#define TX_TIMEOUT                      5000
#define BMAPI_CAN_MODE                  BM_CAN_EXTERNAL_LOOPBACK_MODE
#define BITRATE_NOMINAL                 500
#define BITRATE_DATA                    2000
#define SAMPLE_POS_NOMINAL              70
#define SAMPLE_POS_DATA                 70
#define ENABLE_OFFLINE_REPLAY           0
#define ENABLE_TRIGGER_REPLAY_MODE      1
#define ENABLE_TRIGGER_TEST             1
#define TRIGGER_START_ID                0x100
#define TRIGGER_STOP_ID                 0x7FF


/**
 * @brief Prepares an array of CAN FD messages for transmission
 *
 * @param msgs Pointer to array of BM_DataTypeDef structures to populate
 * @param nmsg Number of messages to prepare
 */
void prepare_tx_msg(BM_DataTypeDef* msgs, int nmsg)
{
    // Validate input parameters to avoid null pointer dereference or invalid count
    if (msgs == NULL || nmsg <= 0)
    {
        return;
    }

    // Base parameters for message generation (configurable as needed)
    const uint32_t base_id = 0x101;       // Base CAN ID (messages increment from this value)
    const uint8_t data_len = TX_MSG_DLC;  // Data length (8 bytes, standard for CAN FD data frames)
    uint8_t base_data[64] = { 0 };

    // Loop to populate each message in the array
    for (int i = 0; i < nmsg; i++)
    {
        // Calculate unique ID for current message (increment by 1 per message)
        uint32_t current_id = base_id + i;
        memcpy(&base_data[0], &i, sizeof(current_id));
        msgs[i].timestamp = i * 10000;
        BM_INIT_CAN_FD_DATA(
            msgs[i],               // Target message in the array
            current_id,            // Unique CAN ID for this message
            data_len,              // Data length (fixed to 8 bytes here)
            0,                     // IDE: 0 = Standard Frame (11-bit ID), 1 = Extended Frame (29-bit)
            1,                     // FDF: 1 = Enable CAN FD mode, 0 = Classic CAN mode
            1,                     // BRS: 1 = Use Bit Rate Switch (data phase uses FD bitrate), 0 = No switch
            0,                     // RTR: 0 = Data Frame (carries data), 1 = Remote Request Frame (no data)
            0,                     // ESI: 0 = Error Active state, 1 = Error Passive state
            base_data              // Data payload for the message
        );
    }
}


/**
 * @brief Main function demonstrating BMAPI hardware replay functionality
 * This program demonstrates: Hardware-based message replay functionality
 * @param[in]  argc    Number of command line arguments
 * @param[in]  argv    Array of command line arguments
 * @return     Program exit code (0 for success, non-zero for errors)
 */

int main(int argc, char* argv[])
{
    BM_ReplayConfigTypeDef replay = { 0 };
    BM_NotificationHandle notification = NULL;

    BM_DataTypeDef triggerMsg;
    uint8_t triggerPayload[8] = { 0 };
    BM_ChannelInfoTypeDef channelinfos[BMAPI_MAX_CHANNEL_COUNT];
    BM_ChannelHandle channel = NULL;
    /* Prepare all message contents for the transmit buffer in advance */
    BM_DataTypeDef msgs[MAX_TX_MSG_COUNT] = { 0 };
    uint32_t timestamps[MAX_TX_MSG_COUNT] = { 0 };
    uint32_t nmsg = 0;
    int nchannels = sizeof(channelinfos) / sizeof(channelinfos[0]);
    int channelid = 0;
    int exitcode = 0;
    BM_BitrateTypeDef bitrate;
    BM_StatusTypeDef error = BM_ERROR_OK;
    int i;
    int nreadmsg = 0;
    int count = 0;
    printf(
        "\n"
        "===========    BMAPI HW REPLAY PROGRAM    ==========\n"
        "===========     BUSMUST TECH Co.,Ltd.     ==========\n"
        __DATE__ " " __TIME__ "\n"
    );
    /* Step 1: Initialize BMAPI library before any other operation */
    printf("Initializing BMAPI library ...\n");
    error = BM_Init();
    if (error != BM_ERROR_OK)
    {
        exitcode = 1;
        goto __exit;
    }
    /* Step 2: Enumerate connected device channels */
    printf("Enumerating channels ...\n");
    error = BM_EnumerateByCap(channelinfos, &nchannels, BM_CAN_CAP);
    if (error != BM_ERROR_OK)
    {
        printf("Operation error occurred\n");
        exitcode = 2;
        goto __exit;
    }

    if (channelid >= nchannels)
    {
        printf("There are %d available bus devices, port index %d is invalid.\n", channelid, nchannels);
        exitcode = 2;
        goto __exit;
    }

    for (i = 0; i < nchannels; i++)
    {
        printf("[%d] %s\n", i, channelinfos[i].name);
    }

    /* Step 3: Open the selected channel and configure baudrate */
    printf("Opening channel %s (%d.%d.%d.%d) in normal mode ...\n",
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
    // Get notification handle for the channel
    printf("Getting channel notification handle ...\n");
    error = BM_GetNotification(channel, &notification);
    if (error != BM_ERROR_OK)
    {
        exitcode = 4;
        goto __exit;
    }

    BM_DeviceHandle device;
    BM_GetDevice(channel, &device);
    // Set buffer type for replay functionality
    /* Configure and start fully automatic sequence playback */
    replay.version = 1;
    if (ENABLE_OFFLINE_REPLAY)
    {
        strcpy_s(replay.path.format, sizeof(replay.path.format), "0000");
    }
    else
    {
        strcpy_s(replay.path.format, sizeof(replay.path.format), "<RAMBUF>");
    }
    /* Step 4: Initialize replay mode to disabled */
    replay.mode = BM_STORAGE_DISABLED;
    BM_SetReplay(device, &replay);

    /* Step 5: Prepare transmit messages */
    nmsg = MAX_TX_MSG_COUNT;
    printf("Preparing %d transmit messages ...\n", nmsg);
    prepare_tx_msg(msgs, nmsg);

    /* Step 6: Configure buffer for replay functionality */ 
    if (ENABLE_OFFLINE_REPLAY)
    {
        BM_SetBuffer(device, BM_WRITE_BUFFER, BM_REPLAYFILE_BUFFER);
    }
    else
    {
        BM_SetBuffer(device, BM_WRITE_BUFFER, BM_REPLAYQ_BUFFER);
    }


    /* Step 7: Download messages to hardware buffer */
    BM_WriteMultiple(channel, msgs, &nmsg, TX_TIMEOUT, timestamps);


    /* Step 7: Restore default buffer configuration */
    BM_SetBuffer(device, BM_WRITE_BUFFER, BM_DEFAULT_BUFFER);

    /* Step 8: Configure and start automatic sequence playback */
    replay.version = 1;
#if ENABLE_TRIGGER_REPLAY_MODE
    /* Trigger mode: replay starts/stops based on RX message matching trigger conditions */
    replay.mode = BM_STORAGE_TRIGGERED;      /* Trigger mode: actual start depends on trigger match */
    replay.channels = 0xFFFFU;                   /* All channels enabled */
    replay.direction = BM_STORAGE_DIRECTION_ALL; /* Both TX and RX directions */
    replay.cyclic = 0;                       /* Single playback (non-cyclic) */

    /* Start trigger: begin replay when RX message matches ID=0x100 on CH1 */
    replay.starttrigger.channels = 0xFFFFU;      /* RX trigger source: CH1 (bit0) */
    replay.starttrigger.flags_mask = 0x0000U;    /* Match all flags (IDE, RTR, FDF, BRS) */
    replay.starttrigger.flags_value = 0x0000U;   /* Expected flag values */
    replay.starttrigger.id_mask = 0x1FFFFFFFU;   /* Match full 29-bit extended ID */
    replay.starttrigger.id_value = TRIGGER_START_ID;       /* Start trigger: RX ID == 0x100 */

    /* Stop trigger: stop replay when RX message matches ID=0x7FF on CH1 */
    replay.stoptrigger.channels = 0xFFFFU;       /* RX trigger source: CH1 (bit0) */
    replay.stoptrigger.flags_mask = 0x0000U;     /* Match all flags */
    replay.stoptrigger.flags_value = 0x0000U;    /* Expected flag values */
    replay.stoptrigger.id_mask = 0x1FFFFFFFU;    /* Match full 29-bit extended ID */
    replay.stoptrigger.id_value = TRIGGER_STOP_ID;        /* Stop trigger: RX ID == 0x7FF */

    printf("Replay mode: Event-triggered mode enabled.\n");
    printf("  - Start trigger: RX ID=0x100 on CH1\n");
    printf("  - Stop trigger:  RX ID=0x7FF on CH1\n");
#else
    /* Normal mode: start playing sequence immediately after BM_SetReplay() */
    replay.mode = BM_STORAGE_ALWAYS_ON;          /* Normal mode: immediate start */
    replay.channels = 0xFFFFU;                   /* All channels enabled */
    replay.direction = BM_STORAGE_DIRECTION_ALL; /* Both TX and RX directions */
    replay.cyclic = 0;                           /* Single playback (non-cyclic) */
    printf("Replay mode: Normal always-on mode enabled.\n");
#endif

    BM_SetReplay(device, &replay);

    /* Save configuration to offline storage if offline replay is enabled */
    if (ENABLE_OFFLINE_REPLAY)
    {
        /* Save: CAN mode, bitrate, terminal resistor, and replay configuration */
        BM_SaveConfig(channel,
            BM_CAN_MODE_FLAG |
            BM_CAN_BITRATE_FLAG |
            BM_CAN_TERMINAL_RESISTOR_FLAG |
            BM_CAN_REPLAY_CONFIG_FLAG
        );
    }
    /* Automatic periodic transmission of messages has started at this point */
    Sleep(1000);

#if ENABLE_TRIGGER_REPLAY_MODE
#if ENABLE_TRIGGER_TEST
    /* Trigger test: send a CAN message to trigger replay start/stop */
    printf("\n--- Trigger Test Demo ---\n");
    BM_INIT_CAN_FD_DATA(triggerMsg, TRIGGER_START_ID, 8, 0, 0, 0, 0, 0, triggerPayload);

    printf("Sending start trigger message (ID=0x100)...\n");
    error = BM_Write(channel, &triggerMsg, TX_TIMEOUT, NULL);
    if (error == BM_ERROR_OK)
    {
        printf("Start trigger sent successfully. Replay should begin.\n");
    }
    else
    {
        printf("Failed to send start trigger: error=0x%08X\n", error);
    }

    /* Wait for replay to process some messages */
    Sleep(2000);
    BM_DataTypeDef ackData;
    while ((error = BM_Read(channel, &ackData)) == BM_ERROR_OK)
    {
        /* You might need more post-processing on the ACK data, i.e. To display the transmitted messages on GUI. */
        BM_CanMessageTypeDef* msg = (BM_CanMessageTypeDef*)ackData.payload;
        if (msg->id.SID != TRIGGER_START_ID)
        {
            printf("\nTRIGGER message received: id=%x, ts=%u, DLC=%d, payload=[%02x %02x %02x %02x %02x %02x %02x %02x].\n",
                msg->id.SID, ackData.timestamp, msg->ctrl.rx.DLC,
                msg->payload[0], msg->payload[1], msg->payload[2], msg->payload[3],
                msg->payload[4], msg->payload[5], msg->payload[6], msg->payload[7]
            );
        }
    }
    /* Prepare and send stop trigger message (ID=0x7FF) */
    BM_INIT_CAN_FD_DATA(triggerMsg, 0x7FF, 8, 0, 0, 0, 0, 0, triggerPayload);
    printf("Sending stop trigger message (ID=0x7FF)...\n");
    error = BM_Write(channel, &triggerMsg, TX_TIMEOUT, NULL);
    if (error == BM_ERROR_OK)
    {
        printf("Stop trigger sent successfully. Replay should stop.\n");
    }
    else
    {
        printf("Failed to send stop trigger: error=0x%08X\n", error);
    }
    printf("--- End of Trigger Test ---\n\n");
#else
    printf("Trigger test disabled. Waiting for external trigger message...\n");
    printf("Send CAN ID=0x100 to start replay, ID=0x7FF to stop.\n");
#endif
#else
    BM_DataTypeDef ackData;
    while ((error = BM_Read(channel, &ackData)) == BM_ERROR_OK)
    {
        /* You might need more post-processing on the ACK data, i.e. To display the transmitted messages on GUI. */
        BM_CanMessageTypeDef* msg = (BM_CanMessageTypeDef*)ackData.payload;
        printf("\nALWAYS_ON message received: id=%x, ts=%u, DLC=%d, payload=[%02x %02x %02x %02x %02x %02x %02x %02x].\n",
            msg->id.SID, ackData.timestamp, msg->ctrl.rx.DLC,
            msg->payload[0], msg->payload[1], msg->payload[2], msg->payload[3],
            msg->payload[4], msg->payload[5], msg->payload[6], msg->payload[7]
        );
    }
#endif

    /* Step 9: Set mode to disabled to stop automatic sequence playback */
    replay.mode = BM_STORAGE_DISABLED;
    BM_SetReplay(device, &replay);

    printf("Closing channel ...\n");
    error = BM_Close(channel);
    if (error != BM_ERROR_OK)
    {
        exitcode = 5;
        goto __exit;
    }

__exit:
    // Print error information if any occurred
    if (error != BM_ERROR_OK)
    {
        char buffer[256] = { 0 };
        BM_GetErrorText(error, buffer, sizeof(buffer), 0);
        printf("Error 0X%08X: %s.\n", error, buffer);
    }

    /* Step 10: Cleanup library resources */
    BM_UnInit();
    printf("Exit app.\n");
    return exitcode;
}
