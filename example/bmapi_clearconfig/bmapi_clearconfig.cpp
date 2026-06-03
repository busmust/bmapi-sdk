#include <stdint.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <stdbool.h>
#include <unordered_set>
#include <string>
#include "bmapi.h"
#include <conio.h>
#ifdef __GNUC__
#include <unistd.h>
#elif defined(_MSC_VER)
#include <Windows.h>
#define usleep(us) Sleep((us)/1000)
#endif

#define MAX_CHANNELS 32
#define DELAY_MS 1
#define BMAPI_MAX_CHANNEL_COUNT 32
#ifdef BMAPI_LOOPBACK_WRITE
#define BMAPI_CAN_MODE BM_CAN_EXTERNAL_LOOPBACK_MODE
#else
#define BMAPI_CAN_MODE 1
#endif

void delay_ms(int ms)
{
    usleep(ms * 1000);
}

int isOffLine(const char* name, int version, int supportOffLine)
{
    if (version >= 0x03000000UL)
    {
        return supportOffLine;
    }
    else
    {
        int isRemote = (strstr(name, "[") != NULL) && (strstr(name, "]") != NULL);
        return (strstr(name, "-F1") != NULL) ||
            (strstr(name, "-F2") != NULL) ||
            (strstr(name, "X8R") != NULL) ||
            (strstr(name, "X4R") != NULL) ||
            (strstr(name, "X2R") != NULL) ||
            (strstr(name, "X1R") != NULL) ||
            isRemote;
    }
}

void cleanup_history(std::unordered_set<std::string>& clearedHistory,
    BM_ChannelInfoTypeDef channelinfos[], int nchannels)
{
    std::unordered_set<std::string> currentChannels;
    for (int i = 0; i < nchannels; ++i)
    {
        currentChannels.insert(channelinfos[i].name);
    }

    for (auto it = clearedHistory.begin(); it != clearedHistory.end();)
    {
        if (currentChannels.find(*it) == currentChannels.end())
        {
            printf("Removing disconnected channel from history: %s\n", it->c_str());
            it = clearedHistory.erase(it);
        }
        else
        {
            ++it;
        }
    }
}

int main()
{
    int exitcode = 0;
    BM_ChannelInfoTypeDef channelinfos[BMAPI_MAX_CHANNEL_COUNT];
    BM_ChannelHandle channels[BMAPI_MAX_CHANNEL_COUNT];
    int nchannels;
    int temp = 0;;
    std::unordered_set<std::string> clearedHistory;

    printf(
        "\n"
        "====== BUSMUST OFFLINE DATA ERASER =====\n"
        "========= BUSMUST TECH Co.,Ltd. ========\n"
    );

    printf("Initializing BMAPI library ...\n");
    BM_StatusTypeDef status = BM_ERROR_OK;
    const uint32_t configmask = 0xFFFFFFFF;
    int opencount = 0;
    bool prompt = true;
    status = BM_Init();
    if (status != BM_ERROR_OK)
    {
        exitcode = 1;
        goto __exit;
    }
    while (1)
    {
        nchannels = BMAPI_MAX_CHANNEL_COUNT;
        memset(channelinfos, 0, sizeof(BM_ChannelInfoTypeDef) * BMAPI_MAX_CHANNEL_COUNT);
        status = BM_Enumerate(channelinfos, &nchannels);
        bool isDevice = false;
        if (status == BM_ERROR_OK)
        {
            for (int i = 0; i < nchannels; i++)
            {
                std::string string = std::string(channelinfos[i].name);
                if (string.find("Virtual Channel") == std::string::npos)
                {
                    isDevice = true;
                }
                else
                {
                    clearedHistory.insert(channelinfos[i].name);
                }
            }
        }
        cleanup_history(clearedHistory, channelinfos, nchannels);
        if (isDevice)
        {
            opencount = 0;
            for (int i = 0; i < nchannels; ++i)
            {

                std::string channelName = channelinfos[i].name;
                if (clearedHistory.find(channelName) != clearedHistory.end())
                {
                    continue;
                }

                BM_BitrateTypeDef bitrate;
                memset(&bitrate, 0, sizeof(bitrate));
                bitrate.nbitrate = 500;
                bitrate.dbitrate = 2000;
                bitrate.nsamplepos = 75;
                bitrate.dsamplepos = 75;

                status = BM_OpenEx(
                    &channels[opencount],
                    &channelinfos[i],
                    BMAPI_CAN_MODE,
                    BM_TRESISTOR_DISABLED,
                    &bitrate,
                    NULL,
                    0
                );

                if (status == BM_ERROR_OK)
                {
                    prompt = false;
                    printf("Channel %s opened successfully, checking configuration...\n", channelName.c_str());
                    int supportOffLine = 0;
                    uint32_t version = BM_VERSION_CODE(channelinfos[i].version);
                    BM_Control(channels[opencount], 0xF4U, BM_STAT_SUPPORT_OFFLINE,
                        (uint16_t)channelinfos[i].port, &supportOffLine, sizeof(supportOffLine));

                    if (isOffLine(channelinfos[i].name, version, supportOffLine))
                    {
                        status = BM_ClearConfig(channels[opencount], configmask);
                        if (status == BM_ERROR_OK)
                        {
                            clearedHistory.insert(channelName);
                            printf("Channel %s configuration cleared successfully!\n", channelName.c_str());
                        }
                        else
                        {
                            printf("Faild to clear offline configuration for channel %s, error code: 0x%x\n\n",
                                channelName.c_str(), status);
                        }
                    }
                    else
                    {
                        clearedHistory.insert(channelName);
                    }

                    if (clearedHistory.size() == nchannels)
                    {
                        printf("Successfully erased offline data for all connected channels.\n");
                        printf("You can close this window or insert a new device to erase its offline data.\n");
                    }
                    opencount++;
                }
                else
                {
                    if (temp != nchannels)
                    {
                        printf("Failed to open channel %s, error code: %d\n\n", channelName.c_str(), status);
                    }
                }
            }
            for (int i = 0; i < opencount; ++i)
            {
                BM_Close(channels[i]);
            }
        }
        if (prompt)
        {
            prompt = false;
            printf("Please plug in your Busmust device to erase all offline configuration.\n");
        }
        delay_ms(DELAY_MS);
        temp = nchannels;
    }

__exit:
    if (status != BM_ERROR_OK)
    {
        char buffer[256] = { 0 };
        BM_GetErrorText(status, buffer, sizeof(buffer), 0);
        printf("Error 0X%08X: %s.\n", status, buffer);
    }

    printf("Un-initialize library.\n");
    BM_UnInit();
    return exitcode;
}