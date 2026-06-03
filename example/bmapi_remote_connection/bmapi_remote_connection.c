/**
 * @file        bmapi_remote_connection.c
 * @brief       BMAPI example: Receive and verify CAN messages.
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

#define BMAPI_CAN_MODE BM_CAN_EXTERNAL_LOOPBACK_MODE

#define TEST_MSG_ID     0x123
#define TEST_MSG_DLC    0x8
#define TEST_MSG_LENGTH 0x8
#define TEST_MSG_COUNT  10000
#define TEST_MSG_ROUND  10000
#define TEST_MSG_CYCLE  10
#define TEST_MSG_TX_TIMEOUT 30000
#define TEST_MSG_RX_TIMEOUT 30000
#define TEST_MSG_PRINT_ALL

static int receive(BM_ChannelHandle channel, BM_NotificationHandle notification, uint32_t nmessages, uint32_t ninc);

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
	BM_ChannelHandle channel = NULL;
	BM_NotificationHandle notification = NULL;
	BM_ChannelInfoTypeDef channelinfos[32];
	int nchannels = sizeof(channelinfos) / sizeof(channelinfos[0]);
	int channelid = argc > 1 ? atoi(argv[1]) : 0;
	const char* mode = argc > 2 ? argv[2] : "rx";
	int exitcode = 0;
	BM_BitrateTypeDef bitrate;
	BM_StatusTypeDef error = BM_ERROR_OK;
	int i;

	printf(
		"\n"
		"========== BMAPI TEST PROGRAM ==========\n"
		"========= BUSMUST TECH Co.,Ltd. ========\n"
	);

	/* Step1: Initialize BMAPI library before any other operation */
	printf("Initializing BMAPI library ...\n");
	error = BM_Init();
	if (error != BM_ERROR_OK)
	{
		exitcode = 1;
		goto __exit;
	}

	/* Step2: Enumerate remote device channels */
	printf("Enumerating channels ...\n");
	uint8_t ipv4[4] = { 192, 168, 41, 255 };
	error = BM_EnumerateRemote(channelinfos, &nchannels, ipv4, 100);
	if (error != BM_ERROR_OK)
	{
		exitcode = 2;
		goto __exit;
	}

	for (i = 0; i < nchannels; i++)
	{
		printf("[%d] %s port=%d\n", i, channelinfos[i].name, channelinfos[i].port);
	}
	if (channelid < nchannels)
	{
		/* Step3: Open the selected channel and configure baudrate */
		printf("Opening channel %s in %s mode ...\n", channelinfos[channelid].name, mode);
		memset(&bitrate, 0, sizeof(bitrate));
		bitrate.nbitrate = 500;
		bitrate.dbitrate = 2000;
		bitrate.nsamplepos = 75;
		bitrate.dsamplepos = 75;
		/* BTR is not mandatory, hardware will calculate BTR using bitrate&samplepos automatically */
		error = BM_OpenEx(&channel, &channelinfos[channelid], BMAPI_CAN_MODE, BM_TRESISTOR_120, &bitrate, NULL, 0);
		if (error != BM_ERROR_OK)
		{
			exitcode = 3;
			goto __exit;
		}

		printf("Getting channel notification handle ...\n");
		error = BM_GetNotification(channel, &notification);
		if (error != BM_ERROR_OK)
		{
			exitcode = 4;
			goto __exit;
		}

#if 0
		for (int i = 0; i < 1400; i++)
		{
			BM_DataTypeDef data;
			uint32_t timestamp = 0;
			BM_INIT_CAN_FD_DATA(data, 0x123, 8, 0, 0, 0, 0, 0, NULL);
			BM_StatusTypeDef status = BM_Write(channel, &data, 1000, &timestamp);
			if (status == BM_ERROR_OK)
			{
                printf("Transmit test message %d to remote channel, ts=%d\n", i, timestamp);
                Sleep(100);
			}
			else
			{
				char errmsg[1024];
				BM_GetErrorText(status, errmsg, sizeof(errmsg), 0);
				printf("Failed to transmit remote message %d, status=%08x(%s)\n", i, status, errmsg);
			}
		}
#else
        BM_TxTaskTypeDef txtask = { 0 };
        static const uint16_t dlc2len[16] = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 12, 16, 20, 24, 32, 48, 64 };
        memset(&txtask, 0, sizeof(txtask));
        txtask.version = 1;
        txtask.type = BM_TXTASK_INCDATA;
        txtask.cycle = 1000;
        txtask.nrounds = 0xFFFFU;
        txtask.nmessages = 1;
        txtask.pattern.incdata.startbit = 0;
        txtask.pattern.incdata.nbits = 32;
        txtask.pattern.incdata.step = 1;
        txtask.pattern.incdata.min = 0;
        txtask.pattern.incdata.max = 0xFFFFFFFFU;
        txtask.pattern.incdata.format = 0x80; /* Intel little edian format */
        txtask.can.SID = 0x123;
        txtask.length = (uint8_t)dlc2len[8];
		txtask.flags = 0;// BM_CAN_MESSAGE_FLAGS_FDF | BM_CAN_MESSAGE_FLAGS_BRS;
        memset(txtask.payload, 0, sizeof(txtask.payload));
		if (BM_SetTxTasks(channel, &txtask, 1) != BM_ERROR_OK)
		{
            printf("ERROR: Failed to set TXTASK configuration.\n");
		}
#endif

		BM_LoggingConfigTypeDef logging = { 0 };
		logging.version = 1;
		logging.mode = BM_STORAGE_ALWAYS_ON;
        logging.format = BM_STORAGE_ASC_FORMAT;
        logging.channels = 0xFFFFU;
        logging.direction = BM_STORAGE_DIRECTION_ALL;
        logging.path.mode = BM_STORAGE_INDEX_PATH;
        logging.segmentation.createNewFileOnStart = 1;
        logging.segmentation.overwriteOldFileOnFull = 1;
		if (BM_SetLogging(channel, &logging) != BM_ERROR_OK)
		{
			printf("ERROR: Failed to set LOGGING configuration.\n");
		}
		
		if (BM_SaveConfig(channel, 0xFFFFFFFFUL) != BM_ERROR_OK)
		{
			printf("Failed to save OFFLINE configuration.\n");
		}

		/* Step4: RX(according to command line arguments) */
		uint32_t nmessages = argc > 3 ? atoi(argv[3]) : TEST_MSG_COUNT;
		uint32_t ninc = argc > 4 ? atoi(argv[4]) : 1;
		receive(channel, notification, nmessages, ninc);

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

uint32_t verify(BM_CanMessageTypeDef* msg, uint32_t timestamp, uint32_t* expectedPayloadInt, uint32_t* nbadmsgs, uint32_t* nlostmsgs)
{
	//According to ISO standard, CANFD message payload length could only be 0-8,12,16,20,24,32,48 or 64.
	static const uint16_t dlc2len[16] = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 12, 16, 20, 24, 32, 48, 64 };
	int payloadlength = (uint8_t)dlc2len[msg->ctrl.rx.DLC];

	uint32_t succeed = 0;
	uint32_t actualPayloadInt = ((uint32_t)msg->payload[0])
		| ((uint32_t)msg->payload[1]) << 8
		| ((uint32_t)msg->payload[2]) << 16
		| ((uint32_t)msg->payload[3]) << 24;
	if (msg->id.SID != TEST_MSG_ID || payloadlength != TEST_MSG_LENGTH/* || *(uint32_t*)&msg->payload[4] != 0*/)
	{
		printf("\nBad message received: id=%x, ts=%u, length=%d, payload=[%02x %02x %02x %02x %02x %02x %02x %02x].\n",
			msg->id.SID, timestamp, payloadlength,
			msg->payload[0], msg->payload[1], msg->payload[2], msg->payload[3],
			msg->payload[4], msg->payload[5], msg->payload[6], msg->payload[7]
		);
		*nbadmsgs += 1;
	}
	else if (actualPayloadInt != *expectedPayloadInt)
	{
		uint32_t nlost = actualPayloadInt - *expectedPayloadInt;
		printf("\nLost message detected: ts=%u, expected=%08x, received=%08x, lost=%d.\n",
			timestamp, *expectedPayloadInt, actualPayloadInt, nlost
		);
		*nlostmsgs += nlost;
		*expectedPayloadInt = actualPayloadInt;
	}
	else
	{
#ifdef TEST_MSG_PRINT_ALL
		printf("Received: id=%x, ts=%u, length=%d, payload=[%02x %02x %02x %02x %02x %02x %02x %02x].\n",
			msg->id.SID, timestamp, payloadlength,
			msg->payload[0], msg->payload[1], msg->payload[2], msg->payload[3],
			msg->payload[4], msg->payload[5], msg->payload[6], msg->payload[7]
		);
#else
		if (*expectedPayloadInt % 100 == 99)
		{
			printf(".");
			fflush(stdout);
		}
		if (*expectedPayloadInt % 10000 == 9999)
		{
			printf("\n");
		}
#endif
		succeed = 1;
	}

	return succeed;
}

int receive(BM_ChannelHandle channel, BM_NotificationHandle notification, uint32_t nmessages, uint32_t ninc)
{
	BM_StatusTypeDef error = BM_ERROR_OK;

	uint32_t n = 0;
	uint32_t napifailures = 0;
	uint32_t nlostmsgs = 0;
	uint32_t nbadmsgs = 0;

	/* SWait for RX CAN message using notification handle assigned to the channel */
	printf("Waiting for RX notification ...\n");

	time_t t1 = time(NULL);
	uint32_t expectedPayloadInt = 0;
	while (n < nmessages)
	{
		/* Once a new notification is received, read the CAN message out of BMAPI internal buffer */
		/* Note if you read before getting a new notification event, you might get BM_ERROR_QRCVEMPTY. */

		/* Wait until a new notification event arrived */
		if (BM_WaitForNotifications(&notification, 1, TEST_MSG_RX_TIMEOUT) < 0)
		{
			printf("Receive timeout, offset=%d.\n", n);
		}

		/* Note that a notification event might indicate multiple new messages, so keep reading until you cannot get more. */
		BM_DataTypeDef data;
		while ((error = BM_Read(channel, &data)) == BM_ERROR_OK)
		{
			BM_CanMessageTypeDef* msg = (BM_CanMessageTypeDef*)data.payload;
			//uint16_t port = data.header.schn;
			uint32_t timestamp = data.timestamp;
			if (data.header.type == BM_CAN_FD_DATA)
			{
                n += verify(msg, timestamp, &expectedPayloadInt, &nbadmsgs, &nlostmsgs);
                expectedPayloadInt += ninc;
			}
		}
		/* BM_ERROR_QRCVEMPTY should not be considered as an error, otherwise report the error */
		if (error != BM_ERROR_QRCVEMPTY)
		{
			napifailures++;
			printf("\nFailed to receive message, error=%d.\n", error);
		}
	}
	time_t t2 = time(NULL);
	uint32_t td = (uint32_t)(t2 - t1);
	float fps = ((float)n) / ((float)td);

	printf(
		"\n"
		"================ RX STAT ================\n"

		"Received messages: %d\n"
		"Lost messages: %u\n"
		"Bad messages: %u\n"
		"API Failures: %u\n"
		"Total time: %u seconds\n"
		"Frame rate: %.3f fps\n"
		"=========================================\n",
		n, nlostmsgs, nbadmsgs, napifailures, td, fps
	);

	return n;
}

/**
 * End of file
 */
