/**
 * @file        bmapi_transmit_only.c
 * @brief       BMAPI example: Transmit test CAN message and then wait until a new message is received.
 * @author      busmust
 * @version     1.0.0.1
 * @copyright   Copyright 2020 by Busmust Tech Co.,Ltd <br>
 *              All rights reserved. Property of Busmust Tech Co.,Ltd.<br>
 *              Restricted rights to use, duplicate or disclose of this code are granted through contract.
 */
#ifdef __GNUC__
#include <unistd.h>
#elif defined(_MSC_VER)
#include <Windows.h>
#define usleep(us) Sleep((us)/1000)
#endif
#include <stdint.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <time.h>
#include "bmapi.h"

#ifndef _MSC_VER
#include <time.h>
static uint32_t GetTickCount()
{
	struct timespec ts;
	clock_gettime(CLOCK_MONOTONIC, &ts);
	uint64_t tick = ts.tv_sec;
	return (uint32_t)(tick * 1000ull + ts.tv_nsec / 1000000u);
}
#endif
#define BMAPI_CAN_MODE BM_CAN_NORMAL_MODE
#define TEST_MSG_ID     0x123
#define TEST_MSG_DLC    0x8
#define TEST_MSG_LENGTH 0x8
#define TEST_MSG_COUNT  10000
#define TEST_MSG_ROUND  10000
#define TEST_MSG_CYCLE  10
#define TEST_MSG_TX_TIMEOUT 50
#define TEST_MSG_RX_TIMEOUT 30000

#define SEND_MSG_AND_CHECK_MAX_COUNT 20



struct
{
	uint64_t previousRecoveryTs;
	BM_ChannelInfoTypeDef info;
	uint32_t mode;
	BM_BitrateTypeDef bitrate;
	BM_TerminalResistorTypeDef tres;
	BM_RxFilterTypeDef rxfilters[2];
	BM_TxTaskTypeDef txtasks[64];
} channelRecoveryConfigs;

static int transmit(BM_ChannelHandle channel, BM_NotificationHandle notification, uint32_t nroundmessages, uint32_t nrounds, uint32_t cycle);

static int dataBytesToDlc(int n)
{
    int dlc = 0;
    if (n <= 8) {
        dlc = n;
    }
    else if (n <= 12) {
        dlc = 9;
    }
    else if (n <= 16) {
        dlc = 10;
    }
    else if (n <= 20) {
        dlc = 11;
    }
    else if (n <= 24) {
        dlc = 12;
    }
    else if (n <= 32) {
        dlc = 13;
    }
    else if (n <= 48) {
        dlc = 14;
    }
    else if (n <= 64) {
        dlc = 15;
    }
    return dlc;
}
static int openedChannelCount = 0;
BM_ChannelInfoTypeDef channelinfos[32];
BM_ChannelHandle channel = NULL;

void recoverFromError(BM_ChannelHandle* channel)
{
#ifdef QT_VERSION_MAJOR
#define DEMO_PRINT qWarning
#elif defined(_WIN32)
#define DEMO_PRINT OutputDebugStringA
#else
#define DEMO_PRINT printf
#endif

	// Note: It takes some time to run BM_Close and BM_OpenEx, 
	// and channel handles will be invalidated when recovering from error,
	// make sure no other threads are using channel handles (e.g. calling BM_Write) during the recovery,
	// or, use single-threaded design, just like this demo.
	uint64_t currentTs = GetTickCount();
	uint64_t elapsed = currentTs - channelRecoveryConfigs.previousRecoveryTs;
	if (elapsed >= 1000ULL)
	{
		BM_CanStatusInfoTypeDef canStatus;
		BM_StatusTypeDef status = BM_GetStatus(*channel, &canStatus);
		BM_StatusTypeDef error = BM_ERROR_OK;
		if (status & BM_ERROR_INITIALIZE)
		{
			// BM_Init() is not called yet.
			// Read our SDK documentation for details. 
			DEMO_PRINT("BM_ERROR_INITIALIZE\n");
		}
		else if (status & BM_ERROR_ILLPARAMVAL)
		{
			// Channel handle is invalid. 
			// Maybe it's not opened yet (using BM_OpenEx) or already closed?
			DEMO_PRINT("BM_ERROR_ILLPARAMVAL\n");
			error = BM_OpenEx(channel, &channelRecoveryConfigs.info, channelRecoveryConfigs.mode, channelRecoveryConfigs.tres, &channelRecoveryConfigs.bitrate, &channelRecoveryConfigs.rxfilters[0], 0);
			channelRecoveryConfigs.previousRecoveryTs = currentTs;
		}
		else if (status & BM_ERROR_ILLOPERATION)
		{
			// USB Device operation failed. 
			// Maybe the device is unplugged from host PC?

			DEMO_PRINT("BM_ERROR_ILLOPERATION\n");
			// Close all channels in the same device.
			uint64_t recoveredChannelMask = 0;
			BM_ChannelInfoTypeDef siblingInfo;
			BM_GetChannelInfo(*channel, &siblingInfo);
			if (memcmp(channelRecoveryConfigs.info.sn, siblingInfo.sn, sizeof(siblingInfo.sn)) == 0 &&
				memcmp(channelRecoveryConfigs.info.uid, siblingInfo.uid, sizeof(siblingInfo.uid)) == 0)
			{
				BM_Close(*channel);
				*channel = NULL;
			}

			// Try reset device and remove device from opened device list kept by bmapi.
			//BM_ResetDevice(channels[i]);
			error = BM_OpenEx(channel, &channelRecoveryConfigs.info, channelRecoveryConfigs.mode, channelRecoveryConfigs.tres, &channelRecoveryConfigs.bitrate, &channelRecoveryConfigs.rxfilters[0], 0);
			channelRecoveryConfigs.previousRecoveryTs = currentTs;
		}
		else if (status & BM_ERROR_ANYBUSERR)
		{
			// BUSOFF
			// Maybe the remote CAN device is disconnected,
			// or you might want to check your bitrate, sample-position, tres configuration.
			DEMO_PRINT("BUSOFF RECOVERY\n");
			BM_RecoverBusOff(*channel);
			channelRecoveryConfigs.previousRecoveryTs = currentTs;
		}
	}
}

/**
 * @brief BMAPI test program: Transmit or receive messages according to command line arguments:
 * @param[in]  argc    Number of command line arguments
 * @param[in]  argv[0] Path of this program
 * @param[in]  argv[1] Channel: Selected CAN port index
 * @param[in]  argv[2] Mode: only "tx" or "rx" is supported
 * @param[in]  argv[3] nmessages: Number of messages to receive/transmit within a round
 * @param[in]  argv[4] nrounds: Number of rounds to transmit
 * @param[in]  argv[4] ncycles: Round cycle, a delay value in milliseconds between consecutive rounds
 * @return     Program exit code
 */
int main(int argc, char* argv[])
{
	BM_NotificationHandle notification = NULL;
	int nchannels = sizeof(channelinfos) / sizeof(channelinfos[0]);
	int channelid = argc > 1 ? atoi(argv[1]) : 0;
	const char* mode = argc > 2 ? argv[2] : "Tx";
	uint32_t canmode = BMAPI_CAN_MODE;
	int exitcode = 0;
	BM_BitrateTypeDef bitrate;
	BM_StatusTypeDef error = BM_ERROR_OK;
	int i;

	printf(
		"\n"
		"========== BMAPI TEST PROGRAM ==========\n"
		"========= BUSMUST TECH Co.,Ltd. ========\n" 
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

	if (strstr(mode, "Loopback") != NULL)
	{
		canmode = BM_CAN_EXTERNAL_LOOPBACK_MODE;
	}
	if (strstr(mode, "NoAck") != NULL)
	{
		canmode |= BM_CAN_NOACK_MODE;
	}
	if (strstr(mode, "Debug") != NULL)
	{
		BM_SetLogLevel(BM_LOG_DBG);
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
		printf("Opening channel %s (%d.%d.%d.%d) in %s(%d) mode ...\n", 
			channelinfos[channelid].name, 
			channelinfos[channelid].version[0],
			channelinfos[channelid].version[1],
			channelinfos[channelid].version[2],
			channelinfos[channelid].version[3], 
			mode, canmode
		);
		memset(&bitrate, 0, sizeof(bitrate));
		bitrate.nbitrate = 500;
		bitrate.dbitrate = 2000;
		bitrate.nsamplepos = 80;
		bitrate.dsamplepos = 80;
		/* BTR is not mandatory, hardware will calculate BTR using bitrate&samplepos automatically */
		error = BM_OpenEx(&channel, &channelinfos[channelid], canmode, BM_TRESISTOR_120, &bitrate, NULL, 0);
		if (error != BM_ERROR_OK)
		{
			exitcode = 3;
			goto __exit;
		}
		memcpy(&channelRecoveryConfigs.info, &channelinfos[channelid], sizeof(channelinfos[channelid]));
		channelRecoveryConfigs.mode = canmode;
		channelRecoveryConfigs.tres = BM_TRESISTOR_120;
		memcpy(&channelRecoveryConfigs.bitrate, &bitrate, sizeof(bitrate));
		printf("Getting channel notification handle ...\n");
		error = BM_GetNotification(channel, &notification);
		if (error != BM_ERROR_OK)
		{
			exitcode = 4;
			goto __exit;
		}

		/* Step4: TX (according to command line arguments) */
		uint32_t nmessages = argc > 3 ? atoi(argv[3]) : TEST_MSG_COUNT;
		uint32_t nrounds = argc > 4 ? atoi(argv[4]) : TEST_MSG_ROUND;
		uint32_t ncyclems = argc > 5 ? atoi(argv[5]) : TEST_MSG_CYCLE;
		transmit(channel, notification, nmessages, nrounds, ncyclems);

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
	printf("Exit %s app.\n", mode);
	return exitcode;
}

static int transmit(BM_ChannelHandle channel, BM_NotificationHandle notification, uint32_t nroundmessages, uint32_t nrounds, uint32_t cycle)
{
	uint32_t n = 0;
	uint32_t nbadmsgs = 0;
	uint32_t nmessages = ((uint32_t)nroundmessages) * ((uint32_t)nrounds);
	BM_CanMessageTypeDef msg;
	BM_StatusTypeDef error = BM_ERROR_OK;

	/* Compose a CAN message object for TX */
	printf("Transmitting test message ...\n");
	memset(&msg, 0, sizeof(msg));
	msg.id.SID = TEST_MSG_ID;
	//According to ISO standard, CANFD message payload length could only be 0-8,12,16,20,24,32,48 or 64.
	int payloadLength = TEST_MSG_LENGTH;
	msg.ctrl.tx.DLC = dataBytesToDlc(payloadLength);

	msg.ctrl.tx.FDF = 0;
	msg.ctrl.tx.BRS = 0;
	time_t t1 = time(NULL);


	/**
	 * @brief Simple synchronous transmission:
	 *        Each message is transmitted to the USB device and the Write function get blocked until the message's ACK is received
	 *        You might want to set a timeout value (in milliseconds) to handle bus errors (i.e. BUSOFF)
	 */
	uint32_t previousTimestamp = 0;
	uint32_t checkCount = 0;
	for (n = 0; n < nmessages; n++)
	{
		/* Transmit the CAN message to bus using opened device */
		if (channel)
		{
			uint32_t timestamp = 0;
			BM_GetTimestamp(channel, &timestamp);
			*(uint32_t*)&msg.payload[0] = n;
			error = BM_WriteCanMessage(channel, &msg, 0, TEST_MSG_TX_TIMEOUT, &timestamp);
			if (error == BM_ERROR_BUSTIMEOUT)
			{
				printf("Transmit timeout, offset=%d.\n", n);
				recoverFromError(&channel);
			}
			else if (BM_ERROR_OK != error)
			{
				printf("Failed to transmit message, error=0X%08X.\n", error);
				recoverFromError(&channel);
				break;
			}
			else
			{
				checkCount++;
				if (checkCount < SEND_MSG_AND_CHECK_MAX_COUNT)
				{
					recoverFromError(&channel);
					checkCount = 0;
				}
				if (timestamp <= previousTimestamp)
				{
					printf("TX TS: %d (0x%08x) <= %d (0x%08x).\n", timestamp, timestamp, previousTimestamp, previousTimestamp);
				}
				printf("\033[1A\033[KTX in progress [%d/%d] ...\n", n, nmessages);
			}
		}
		else
		{
			n--;
			recoverFromError(&channel);
		}
	}

	time_t t2 = time(NULL);
	uint32_t td = (uint32_t)(t2 - t1);
	float fps = ((float)n) / ((float)td);

	printf(
		"\n"
		"================ TX STAT ================\n"
		"Transmitted messages: %u\n"
		"Bad messages: %u\n"
		"Total time: %u seconds\n"
		"Frame rate: %.3f fps\n"
		"=========================================\n",
		n, nbadmsgs, td, fps
	);

	return n;
}
/**
 * End of file
 */

