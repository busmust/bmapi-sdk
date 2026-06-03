/**
 * @file        bmapi_lin_master.c
 * @brief       BMAPI example: Transmit test LIN message and then wait until a new message is received.
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
#include <stdbool.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <time.h>
#include "bmapi.h"

#define TEST_MSG_TX_ID  0x30
#define TEST_MSG_LENGTH 0x8
#define TEST_MSG_COUNT  10
#define TEST_MSG_ROUND  100
#define TEST_MSG_CYCLE  10
#define TEST_MSG_TX_TIMEOUT 500
#define TEST_MSG_RX_TIMEOUT 5000
#define TEST_CHECKSUM_TYPE  1 // classic = 0 enhance = 1

enum LIN_MSG_TYPE
{
	LIN_RECEIVE = 0,
	LIN_MASTER_WRITE,
	LIN_MASTER_READ,
	LIN_SLAVE_WRITE,
};

static const char* msgtypes[] =
{
	"lin_receive",
	"lin_master_write",
	"lin_master_read",
	"lin_slave_write",
};

enum LIN_SEND_TYPE
{
	LIN_BM_WRITE = 0,
	LIN_BM_TXTASK,
};

static const char* sendtypes[] =
{
	"lin_bm_write",
	"lin_bm_txtask",
};

static int lin_transmit(BM_ChannelHandle channel, BM_NotificationHandle notification, uint32_t nroundmessages, uint32_t nrounds, uint32_t cycle, int mode, int type);
static int lin_receive(BM_ChannelHandle channel, BM_NotificationHandle notification, uint32_t nmessages);

uint8_t calculatePID(uint8_t ucId)
{
	char chChecksum = ucId;
	char chMask[8], byte[8];
	memset(chMask, 0, sizeof(char) * 8);
	memset(byte, 0, sizeof(char) * 8);
	for (int nIndex = 0; nIndex < 8; nIndex++)
	{
		chMask[nIndex] = nIndex + 1;
		byte[nIndex] = ((chChecksum) >> nIndex) & 1;
	}

	char chP0, chP1;
	// chP0 = ID0+ID1+ID2+ID4
	chP0 = byte[0] ^ byte[1] ^ byte[2] ^ byte[4];


	// chP1 = ID1+ID3+ID4+ID5
	chP1 = byte[1] + byte[3] + byte[4] + byte[5];

	chP1 = ~chP1;

	//00010110 -> p00010110
	chP0 = chP0 << 6;
	chP1 = chP1 << 7;
	chChecksum = chChecksum + chP0 + chP1;
	ucId = chChecksum;
	return ucId;
}

uint8_t calculateChecksum(const uint8_t* xData, uint8_t xDataLen, uint8_t pId)
{
	uint16_t sum = pId;
	for (uint8_t i = 0; i < xDataLen; ++i)
	{
		sum += xData[i];
		while (sum > 0xFF)
		{
			sum -= 0xFF;
		}
	}
	return ((~sum) & 0xff);
}

/**
 * @brief BMAPI test program: Transmit or receive messages according to command line arguments:
 * @param[in]  argc    Number of command line arguments
 * @param[in]  argv[0] Path of this program
 * @param[in]  argv[1] Channel: Selected port index
 * @param[in]  argv[2] Mode(master/slave)-MsgType(receive/write/read)-SendType(txtask/bmwrite)
 * @param[in]  argv[3] nmessages: Number of messages to receive/transmit within a round
 * @param[in]  argv[4] nrounds: Number of rounds to transmit
 * @param[in]  argv[5] ncycles: Round cycle, a delay value in milliseconds between consecutive rounds
 * @return     Program exit code
 */
int main(int argc, char* argv[])
{
	BM_ChannelHandle channel = NULL;
	BM_NotificationHandle notification = NULL;
	BM_ChannelInfoTypeDef channelinfos[32];
	int nchannels = sizeof(channelinfos) / sizeof(channelinfos[0]);
	int channelid = argc > 1 ? atoi(argv[1]) : 0;
	const char* mode = argc > 2 ? argv[2] : "master-write-txtask";
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

	/* Step2: Enumerate connected device channels */
	printf("Enumerating LIN channels ...\n");
	error = BM_EnumerateByCap(channelinfos, &nchannels, BM_LIN_CAP);
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
		uint32_t modeId = strstr(mode, "master") != NULL ? BM_LIN_MASTER_MODE : BM_LIN_SLAVE_MODE;
		printf("Opening channel %s in %s mode ...\n", channelinfos[channelid].name, (modeId == BM_LIN_MASTER_MODE) ? "master" : "slave");
		memset(&bitrate, 0, sizeof(bitrate));
		/* default LIN baudrate is 19200bps, feel free to change. */
		bitrate.nbitrate = 19200;
		error = BM_OpenEx(&channel, &channelinfos[channelid], modeId, BM_TRESISTOR_PULLUP_1K, &bitrate, NULL, 0);
		if (error != BM_ERROR_OK)
		{
			exitcode = 3;
			goto __exit;
		}
		BM_SetLinVoltage(channel, BM_LIN_VOLTAGE_12V_OUT);
		printf("Getting channel notification handle ...\n");
		error = BM_GetNotification(channel, &notification);
		if (error != BM_ERROR_OK)
		{
			exitcode = 4;
			goto __exit;
		}

		BM_ClearBuffer(channel);
		Sleep(100);

		/* Step4: TX (according to command line arguments) */
		uint32_t nmessages = argc > 3 ? atoi(argv[3]) : TEST_MSG_COUNT;
		uint32_t nrounds = argc > 4 ? atoi(argv[4]) : TEST_MSG_ROUND;
		uint32_t ncyclems = argc > 5 ? atoi(argv[5]) : TEST_MSG_CYCLE;
		int sendtype = strstr(mode, "-txtask") != NULL ? LIN_BM_TXTASK : LIN_BM_WRITE;
		int linmode = LIN_RECEIVE;
		if (modeId == BM_LIN_MASTER_MODE)
		{
			if (strstr(mode, "write") != NULL)
			{
				linmode = LIN_MASTER_WRITE;
			}
			else if (strstr(mode, "read") != NULL)
			{
				linmode = LIN_MASTER_READ;
			}
		}
		else
		{
			if (strstr(mode, "write") != NULL)
			{
				linmode = LIN_SLAVE_WRITE;
			}
		}

		if (linmode == LIN_RECEIVE)
		{
			lin_receive(channel, notification, nmessages * nrounds);
		}
		else
		{
			lin_transmit(channel, notification, nmessages, nrounds, ncyclems, linmode, sendtype);
		}

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

void printf_lin_msg(BM_LinMessageTypeDef msg, uint32_t nmessages, uint32_t n, uint32_t timestamp, bool istx)
{
	printf(
		"LIN %s [%d/%d] @%u: id=%02X, payload=[%02X, %02x, %02X, %02x, %02X, %02x, %02X, %02x] checksumType = %s checksum = %x\n", istx ? "Tx" : "Rx", n + 1, nmessages, timestamp,
		msg.id,
		msg.payload[0], msg.payload[1], msg.payload[2], msg.payload[3],
		msg.payload[4], msg.payload[5], msg.payload[6], msg.payload[7], msg.ctrl.lin.ENHANCED_CHECKSUM ? "Enhanced" : "Classic", msg.ctrl.lin.CHECKSUM
	);
}

static int lin_transmit(BM_ChannelHandle channel, BM_NotificationHandle notification, uint32_t nroundmessages, uint32_t nrounds, uint32_t cycle, int mode, int type)
{
	uint32_t n = 0;
	uint32_t nbadmsgs = 0;
	uint32_t nmessages = ((uint32_t)nroundmessages) * ((uint32_t)nrounds);
	BM_LinMessageTypeDef msg = { 0 };
	BM_StatusTypeDef error = BM_ERROR_OK;
	int checksumType = TEST_CHECKSUM_TYPE;
	BM_DataTypeDef bmData;
	uint8_t pid = 0;
	/* Compose a LIN message object for TX */
	printf("Transmitting test message as %s sendtype as %s...\n", msgtypes[mode], sendtypes[type]);
	time_t t1 = time(NULL);
	if (type == LIN_BM_TXTASK)
	{
		BM_TxTaskTypeDef txtask = { 0 };
		txtask.type = BM_TXTASK_FIXED;
		txtask.cycle = mode == LIN_SLAVE_WRITE ? 0 : cycle;
		txtask.nrounds = nrounds;
		txtask.nmessages = nroundmessages;
		txtask.lin.ID = TEST_MSG_TX_ID;
		if (checksumType == 1 && mode != LIN_MASTER_READ)
		{
			pid = calculatePID(txtask.lin.ID);
			txtask.flags |= BM_LIN_MESSAGE_FLAGS_ENHANCED_CHECKSUM;
		}
		if (mode == LIN_MASTER_WRITE || mode == LIN_SLAVE_WRITE)
		{
			txtask.flags = BM_LIN_MESSAGE_FLAGS_TRANSMIT;
			txtask.length = 8;
			memset(txtask.payload, TEST_MSG_TX_ID, sizeof(txtask.payload));
			txtask.lin.CHECKSUM = calculateChecksum(txtask.payload, 8, pid);
		}
		BM_SetTxTasks(channel, (BM_TxTaskTypeDef*)&txtask, 1);
		while (n < nmessages)
		{
			int incomingChannel = BM_WaitForNotifications(&notification, 1, TEST_MSG_RX_TIMEOUT);
			if (incomingChannel < 0) continue;

			while (BM_Read(channel, &bmData) == BM_ERROR_OK)
			{
				BM_LinMessageTypeDef* linMessage = (BM_LinMessageTypeDef*)bmData.payload;
				printf_lin_msg(*linMessage, nmessages, n, bmData.timestamp, true);
				n++;
			}
		}

	}
	else if (mode != LIN_SLAVE_WRITE && type == LIN_BM_WRITE)
	{
		for (n = 0; n < nmessages; n++)
		{
			uint32_t timestamp = 0;
			msg.id = TEST_MSG_TX_ID;

			if (checksumType == 1)
			{
				pid = calculatePID(msg.id);
			}

			if (mode == LIN_MASTER_WRITE)
			{
				msg.ctrl.lin.DLC = TEST_MSG_LENGTH;
				*(uint32_t*)&msg.payload[0] = n;
				msg.ctrl.lin.ENHANCED_CHECKSUM = TEST_CHECKSUM_TYPE;
				msg.ctrl.lin.CHECKSUM = calculateChecksum(msg.payload, 8, pid);
				msg.ctrl.lin.TRANSMIT = 1;
			}
			if(BM_ERROR_OK == BM_WriteLinMessage(channel, &msg, 0, TEST_MSG_TX_TIMEOUT, &timestamp))
			{
				printf_lin_msg(msg, nmessages, n, timestamp, true);
			}
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

static int lin_receive(BM_ChannelHandle channel, BM_NotificationHandle notification, uint32_t nmessages)
{
	BM_StatusTypeDef error = BM_ERROR_OK;

	uint32_t n = 0;
	uint32_t napifailures = 0;
	uint32_t nlostmsgs = 0;
	uint32_t nbadmsgs = 0;

	/* SWait for RX CAN message using notification handle assigned to the channel */
	printf("Waiting for RX notification ...\n");

	time_t t1 = time(NULL);
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
		BM_DataTypeDef bmData;
		while (error = BM_Read(channel, &bmData) == BM_ERROR_OK)
		{
			BM_LinMessageTypeDef* linMessage = (BM_LinMessageTypeDef*)bmData.payload;
			printf_lin_msg(*linMessage, nmessages, n, bmData.timestamp, false);
			n++;
		}
		/* BM_ERROR_QRCVEMPTY should not be considered as an error, otherwise report the error */
		if (error != BM_ERROR_QRCVEMPTY && error != BM_ERROR_OK)
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

