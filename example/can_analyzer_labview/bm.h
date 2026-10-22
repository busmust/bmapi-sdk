/**
 * @file        bm.h
 * @brief       Busmust device communication API.
 * @author      busmust
 * @version     1.4.0.5
 * @copyright   Copyright 2020 by Busmust Tech Co.,Ltd <br>
 *              All rights reserved. Property of Busmust Tech Co.,Ltd.<br>
 *              Restricted rights to use, duplicate or disclose of this code are granted through contract.
 */
#ifndef __BMAPI_H__
#define __BMAPI_H__

#ifdef __cplusplus
extern "C" {
#endif

////////////////////////////////////////////////////////////////////////// stdint.h //////////////////////////////////////////////////////////////////////////
#ifndef _STDINT

typedef signed char        int8_t;
typedef short              int16_t;
typedef int                int32_t;
typedef long long          int64_t;
typedef unsigned char      uint8_t;
typedef unsigned short     uint16_t;
typedef unsigned int       uint32_t;
typedef unsigned long long uint64_t;

typedef signed char        int_least8_t;
typedef short              int_least16_t;
typedef int                int_least32_t;
typedef long long          int_least64_t;
typedef unsigned char      uint_least8_t;
typedef unsigned short     uint_least16_t;
typedef unsigned int       uint_least32_t;
typedef unsigned long long uint_least64_t;

typedef signed char        int_fast8_t;
typedef int                int_fast16_t;
typedef int                int_fast32_t;
typedef long long          int_fast64_t;
typedef unsigned char      uint_fast8_t;
typedef unsigned int       uint_fast16_t;
typedef unsigned int       uint_fast32_t;
typedef unsigned long long uint_fast64_t;

typedef long long          intmax_t;
typedef unsigned long long uintmax_t;

#ifndef _UINTPTR_T_DEFINED
    #define _UINTPTR_T_DEFINED
    #ifdef _WIN64
        typedef unsigned __int64  uintptr_t;
    #else
        typedef unsigned int uintptr_t;
    #endif
#endif

// These macros must exactly match those in the Windows SDK's intsafe.h.
#define INT8_MIN         (-127i8 - 1)
#define INT16_MIN        (-32767i16 - 1)
#define INT32_MIN        (-2147483647i32 - 1)
#define INT64_MIN        (-9223372036854775807i64 - 1)
#define INT8_MAX         127i8
#define INT16_MAX        32767i16
#define INT32_MAX        2147483647i32
#define INT64_MAX        9223372036854775807i64
#define UINT8_MAX        0xffui8
#define UINT16_MAX       0xffffui16
#define UINT32_MAX       0xffffffffui32
#define UINT64_MAX       0xffffffffffffffffui64

#define INT_LEAST8_MIN   INT8_MIN
#define INT_LEAST16_MIN  INT16_MIN
#define INT_LEAST32_MIN  INT32_MIN
#define INT_LEAST64_MIN  INT64_MIN
#define INT_LEAST8_MAX   INT8_MAX
#define INT_LEAST16_MAX  INT16_MAX
#define INT_LEAST32_MAX  INT32_MAX
#define INT_LEAST64_MAX  INT64_MAX
#define UINT_LEAST8_MAX  UINT8_MAX
#define UINT_LEAST16_MAX UINT16_MAX
#define UINT_LEAST32_MAX UINT32_MAX
#define UINT_LEAST64_MAX UINT64_MAX

#define INT_FAST8_MIN    INT8_MIN
#define INT_FAST16_MIN   INT32_MIN
#define INT_FAST32_MIN   INT32_MIN
#define INT_FAST64_MIN   INT64_MIN
#define INT_FAST8_MAX    INT8_MAX
#define INT_FAST16_MAX   INT32_MAX
#define INT_FAST32_MAX   INT32_MAX
#define INT_FAST64_MAX   INT64_MAX
#define UINT_FAST8_MAX   UINT8_MAX
#define UINT_FAST16_MAX  UINT32_MAX
#define UINT_FAST32_MAX  UINT32_MAX
#define UINT_FAST64_MAX  UINT64_MAX

#ifdef _WIN64
    #define INTPTR_MIN   INT64_MIN
    #define INTPTR_MAX   INT64_MAX
    #define UINTPTR_MAX  UINT64_MAX
#else
    #define INTPTR_MIN   INT32_MIN
    #define INTPTR_MAX   INT32_MAX
    #define UINTPTR_MAX  UINT32_MAX
#endif

#define INTMAX_MIN       INT64_MIN
#define INTMAX_MAX       INT64_MAX
#define UINTMAX_MAX      UINT64_MAX

#define PTRDIFF_MIN      INTPTR_MIN
#define PTRDIFF_MAX      INTPTR_MAX

#ifndef SIZE_MAX
    #define SIZE_MAX     UINTPTR_MAX
#endif

#define SIG_ATOMIC_MIN   INT32_MIN
#define SIG_ATOMIC_MAX   INT32_MAX

#define WCHAR_MIN        0x0000
#define WCHAR_MAX        0xffff

#define WINT_MIN         0x0000
#define WINT_MAX         0xffff

#define INT8_C(x)    (x)
#define INT16_C(x)   (x)
#define INT32_C(x)   (x)
#define INT64_C(x)   (x ## LL)

#define UINT8_C(x)   (x)
#define UINT16_C(x)  (x)
#define UINT32_C(x)  (x ## U)
#define UINT64_C(x)  (x ## ULL)

#define INTMAX_C(x)  INT64_C(x)
#define UINTMAX_C(x) UINT64_C(x)

#endif /* #ifndef _STDINT */

//////////////////////////////////////////////////////////////////////// bm_usb_def.h ////////////////////////////////////////////////////////////////////////

/**
 * @def   BM_DATA_HEADER_SIZE
 * @brief Size (in bytes) of BM Data header, which contains type, routing, length and timestamp.
 */
#define BM_DATA_HEADER_SIZE (8U)

/**
 * @def   BM_DATA_PAYLOAD_MAX_SIZE
 * @brief Size (in bytes) of BM Data payload, which contains a concrete message in CANFD|LIN|FLEXRAY|... type.
 */
#define BM_DATA_TAIL_SIZE (16U)
#define BM_DATA_PAYLOAD_MAX_SIZE (72U + BM_DATA_TAIL_SIZE)

/**
 * @def   BM_DATA_MAX_SIZE
 * @brief Size (in bytes) of BM Data, which contains a header and payload.
 */
#define BM_DATA_MAX_SIZE (BM_DATA_HEADER_SIZE + BM_DATA_PAYLOAD_MAX_SIZE)

/**
 * @enum  BM_CapabilityTypeDef
 * @brief Busmust Device capability flags, retrieved when enumerating devices using BM_Enumerate().
 */
typedef enum
{
    BM_NONE_CAP = 0x0000U,      /**< No capability */
    BM_LIN_CAP = 0x0001U,       /**< The device is capable of handling LIN messages */
    BM_CAN_CAP = 0x0002U,       /**< The device is capable of handling CAN messages */
    BM_CAN_FD_CAP = 0x0004U,    /**< The device is capable of handling CANFD (and CAN) messages */
    BM_FLEXRAY_CAP = 0x0008U,   /**< The device is capable of handling FLEXRAY messages */
    BM_MODBUS_CAP = 0x0010U,    /**< The device is capable of handling MODBUS messages */
    BM_ETHERNET_CAP = 0x0020U,  /**< The device is capable of handling ETHERNET messages */
    BM_ALL_CAP = 0xFFFFU        /**< Typically used for masking the CAP fields when programming */
} BM_CapabilityTypeDef;

/**
 * @enum  BM_DataTypeTypeDef
 * @brief Busmust data type flags, must be given in BM_DataTypeDef.
 */
typedef enum
{
    BM_UNKNOWN_DATA = 0,        /**< Unknown data type */
    BM_LIN_DATA,                /**< LIN message data type */
    BM_CAN_FD_DATA,             /**< CAN or CAN-FD message data type (check FDF flag further) */
    BM_FLEXRAY_DATA,            /**< Flexray message data type */
    BM_MODBUS_DATA,             /**< MODBUS message data type */
    BM_ETHERNET_DATA,           /**< Ethernet message data type */
    BM_ACK_DATA = 0x8U,         /**< ACK from bus, which indicates TXCMPLT event if this is BM_CAN_FD_DATA */
} BM_DataTypeTypeDef;

/**
 * @enum  BM_StatusTypeDef
 * @brief Busmust device & operation status, most APIs would return a status code to indicate the result of an operation.
 */
typedef enum
{
    BM_ERROR_OK = 0x00000,                      /**< SUCCESS: No error occurred */
    BM_ERROR_XMTFULL = 0x00001,                 /**< Low level Transmit buffer is full */
    BM_ERROR_OVERRUN = 0x00002,                 /**< Bus overrun (the device cannot keep up with the high bus throughput) */
    BM_ERROR_BUSLIGHT = 0x00004,                /**< CAN Bus communication is light, see ISO11898 for details */
    BM_ERROR_BUSHEAVY = 0x00008,                /**< CAN Bus communication is heavy, see ISO11898 for details */
    BM_ERROR_BUSWARNING = BM_ERROR_BUSHEAVY,    /**< CAN Bus communication is in warning state, see ISO11898 for details */
    BM_ERROR_BUSPASSIVE = 0x40000,              /**< CAN node is in passive state, see ISO11898 for details */
    BM_ERROR_BUSTIMEOUT = 0x80000,              /**< CAN node failed to transmit message within specified time, the node might be in PASSIVE or BUSOFF state */
    BM_ERROR_BUSOFF = 0x00010,                  /**< CAN bus is in BUSOFF state, see ISO11898 for details */
    BM_ERROR_ANYBUSERR = (BM_ERROR_BUSWARNING | BM_ERROR_BUSLIGHT | BM_ERROR_BUSHEAVY | BM_ERROR_BUSOFF | BM_ERROR_BUSPASSIVE), /**< CAN bus error occurred */
    BM_ERROR_QRCVEMPTY = 0x00020,               /**< Receive buffer is empty, this might NOT be an error if you use BMAPI in polling mode */
    BM_ERROR_QOVERRUN = 0x00040,                /**< BMAPI internal Q overrun */
    BM_ERROR_QXMTFULL = 0x00080,                /**< High level Transmit queue is full */
    BM_ERROR_REGTEST = 0x00100,                 /**< Reserved */
    BM_ERROR_NODRIVER = 0x00200,                /**< Reserved */
    BM_ERROR_HWINUSE = 0x00400,                 /**< Hardware is in use (opened by another application) */
    BM_ERROR_NETINUSE = 0x00800,                /**< Reserved */
    BM_ERROR_ILLHW = 0x01400,                   /**< Hardware error or invalid hardware handle */
    BM_ERROR_ILLNET = 0x01800,                  /**< Invalid bus */
    BM_ERROR_ILLCLIENT = 0x01C00,               /**< Invalid client */
    BM_ERROR_ILLHANDLE = (BM_ERROR_ILLHW | BM_ERROR_ILLNET | BM_ERROR_ILLCLIENT), /* Invalid handle*/
    BM_ERROR_RESOURCE = 0x02000,                /**< Out of resource */
    BM_ERROR_ILLPARAMTYPE = 0x04000,            /**< Invalid parameter type in API call */
    BM_ERROR_ILLPARAMVAL = 0x08000,             /**< Invalid parameter value in API call */
    BM_ERROR_UNKNOWN = 0x10000,                 /**< Unknown error */
    BM_ERROR_ILLDATA = 0x20000,                 /**< Invalid data received/transmitted */
    BM_ERROR_CAUTION = 0x2000000,               /**< Reserved */
    BM_ERROR_INITIALIZE = 0x4000000,            /**< The device/library is not initialized */
    BM_ERROR_ILLOPERATION = 0x8000000           /**< Invalid operation */
} BM_StatusTypeDef;

/**
 * @enum  BM_CanModeTypeDef
 * @brief CAN mode IDs, used by BM_SetCanMode() to change the operation mode of CAN device.
 */
typedef enum
{
    BM_CAN_OFF_MODE = 0x01,                     /**< The device is logically disconnected from CAN bus */
    BM_CAN_NORMAL_MODE = 0x00,                  /**< The device is running normally (with the capability to handle CAN and CANFD messages */
    BM_CAN_SLEEP_MODE = 0x01,                   /**< The device is logically disconnected from CAN bus */
    BM_CAN_INTERNAL_LOOPBACK_MODE = 0x02,       /**< The device is looping back messages internally without impacting the physical CAN bus */
    BM_CAN_LISTEN_ONLY_MODE = 0x03,             /**< The device is receiving messages without impacting the physical CAN bus (do not send ACKs to the bus) */
    BM_CAN_CONFIGURATION_MODE = 0x04,           /**< The device is under configuration and temporarily disconnected from CAN bus, For Internal usage only */
    BM_CAN_EXTERNAL_LOOPBACK_MODE = 0x05,       /**< The device is looping back messages externally, all transmitted messages are echoed by the device itself */
    BM_CAN_CLASSIC_MODE = 0x06,                 /**< The device is running normally (with the capability to handle only classical CAN2.0 messages */
    BM_CAN_RESTRICTED_MODE = 0x07,              /**< Reserved */
    BM_CAN_NORMAL_NON_ISO_MODE = 0x08,          /**< The device is running with the capability to handle CAN and NON-ISO(Bosch) CANFD messages */
} BM_CanModeTypeDef;

/**
 * @enum  BM_TerminalResistorTypeDef
 * @brief Terminal resistor values, used by BM_SetTerminalResistor() to change the terminal resistor of CAN device.
 */
typedef enum
{
    BM_TRESISTOR_AUTO = 0,              /**< Reserved, currently unsupported */
    BM_TRESISTOR_60 = 60,               /**< Currently unsupported */
    BM_TRESISTOR_120 = 120,             /**< 120Ohm */
    BM_TRESISTOR_DISABLED = 0xFFFFU,    /**< Disable terminal resistor */
} BM_TerminalResistorTypeDef;

/**
 * @enum  BM_LedTypeDef
 * @brief LED indicator status codes, used by BM_SetLed() to change the CAN LED indicator of CAN device.
 */
typedef enum
{
    BM_LED_OFF = 0,                     /**< CAN LED is OFF */
    BM_LED_ON = 1,                      /**< CAN LED is ON */
} BM_LedTypeDef;

/**
 * @enum  BM_MessageChannelTypeDef
 * @brief Message channel IDs in BM_DataTypeDef header, used for routing indication.
 * @note  You could also use integers directly, please note that valid channel IDs start from zero.
 */
typedef enum
{
    BM_MESSAGE_CHANNEL_0 = 0x0U,        /**< Channel 0 */
    BM_MESSAGE_CHANNEL_1 = 0x1U,        /**< Channel 1 */
    BM_MESSAGE_CHANNEL_2 = 0x2U,        /**< Channel 2 */
    BM_MESSAGE_CHANNEL_3 = 0x3U,        /**< Channel 3 */
    BM_MESSAGE_CHANNEL_4 = 0x4U,        /**< Channel 4 */
    BM_MESSAGE_CHANNEL_5 = 0x5U,        /**< Channel 5 */
    BM_MESSAGE_CHANNEL_6 = 0x6U,        /**< Channel 6 */
    BM_MESSAGE_CHANNEL_7 = 0x7U,        /**< Channel 7 */
    BM_MESSAGE_ANY_CHANNEL = 0xFU,      /**< Any channel, set this value in BM_DataTypeDef header if not used (e.g. TX.header.schn or RX.header.dchn) */
} BM_MessageChannelTypeDef;

/**
 * @enum  BM_MessageFlagsTypeDef
 * @brief CAN Message type flags, used in BM_CanMessageTypeDef.
 */
typedef enum
{
    BM_MESSAGE_FLAGS_NORMAL = 0,        /**< Normal CAN message */
    BM_MESSAGE_FLAGS_IDE = 0x01,        /**< Extended CAN message */
    BM_MESSAGE_FLAGS_RTR = 0x02,        /**< Remote CAN message */
    BM_MESSAGE_FLAGS_BRS = 0x04,        /**< CAN-FD bitrate switching is enabled */
    BM_MESSAGE_FLAGS_FDF = 0x08,        /**< CAN-FD message */
    BM_MESSAGE_FLAGS_ESI = 0x10,        /**< Reserved for gateways */
} BM_MessageFlagsTypeDef;

/**
 * @enum  BM_RxFilterTypeTypeDef
 * @brief CAN RX filter type IDs, used in BM_RxFilterTypeDef.
 */
typedef enum
{
    BM_RXFILTER_INVALID = 0,            /**< Invalid (unused) RX filter entry */
    BM_RXFILTER_BASIC,                  /**< Basic RX filter, traditional acceptance filter based on message ID mask */
    BM_RXFILTER_ADVANCED,               /**< Busmust advanced RX filter, check both message ID and message payload */
    BM_RXFILTER_E2EPASS,                /**< Busmust E2E RX filter, accept only messages that passed E2E checking */
    BM_RXFILTER_E2EFAIL,                /**< Busmust E2E RX filter, accept only messages that failed E2E checking (for debugging purpose) */
} BM_RxFilterTypeTypeDef;

/**
 * @enum  BM_TxTaskTypeTypeDef
 * @brief CAN TX task type IDs, used in BM_TxTaskTypeDef.
 */
typedef enum
{
    BM_TXTASK_INVALID = 0,              /**< Invalid (unused) TX task entry */
    BM_TXTASK_FIXED,                    /**< Basic TX task, send fixed ID and fixed payload */
    BM_TXTASK_INCDATA,                  /**< Self-increment Data TX task */
    BM_TXTASK_INCID,                    /**< Self-increment ID TX task */
    BM_TXTASK_RANDOMDATA,               /**< Random Data TX task */
    BM_TXTASK_RANDOMID,                 /**< Random ID TX task */
} BM_TxTaskTypeTypeDef;

/**
 * @enum  BM_StatTypeDef
 * @brief CAN runtime statistics item IDs, used in BM_GetStat().
 */
typedef enum
{
    BM_STAT_NONE = 0U,                  /**< Invalid statistics item */
    BM_STAT_TX_MESSAGE,                 /**< Number of TX messages */
    BM_STAT_RX_MESSAGE,                 /**< Number of RX messages */
    BM_STAT_TX_BYTE,                    /**< Number of TX bytes */
    BM_STAT_RX_BYTE,                    /**< Number of RX bytes */
    BM_STAT_TX_ERROR,                   /**< Number of TX errors */
    BM_STAT_RX_ERROR,                   /**< Number of RX errors */
} BM_StatTypeDef;

/**
 * @typedef BM_DataHeaderTypeDef
 * @brief   Busmust data header, each BM_DataTypeDef contains a header which indicates payload information.
 */
typedef struct
{
    uint16_t type : 4;                  /**< Data type, see BM_DataTypeTypeDef for details. */
    uint16_t flags : 4;                 /**< Reserved flags, keep 0 */
    uint16_t dchn : 4;                  /**< Destination channel ID, starting from zero, used by TX data to indicate the hardware about the target port. */
    uint16_t schn : 4;                  /**< Source channel ID, starting from zero, used by RX data to indicate the application about the source port. */
} BM_DataHeaderTypeDef;

/**
 * @def   BM_DATA_HEADER
 * @brief Helper macro to define a BM data header value
 */
#define BM_DATA_HEADER(type, flags, dchn, schn) (((type) & 0x0FU) | (((flags) << 4U) & 0xF0U) | (((dchn) << 8U) & 0x0F00U) | (((schn) << 12U) & 0xF000U))

/**
 * @typedef BM_DataTypeDef
 * @brief   Busmust data, abstract structure which holds concrete payload messages of various types (i.e. CAN messages).
 */
typedef struct
{
    uint16_t header;                            /**< data header, see BM_DataHeaderTypeDef for details. */
    uint16_t length;                            /**< length in bytes of the payload only (header excluded). */
    uint32_t timestamp;                         /**< 32-bit device local high precision timestamp in microseconds. */
    uint8_t payload[BM_DATA_PAYLOAD_MAX_SIZE];  /**< buffer holding concrete message payload (i.e. a CAN message in BM_CanMessageTypeDef format). */
} BM_DataTypeDef;

/**
 * @typedef BM_MessageIdTypeDef
 * @brief   Busmust CAN Message ID.
 * @note    You could also use a uint32_t, but please take care of memory alignments.
 */
typedef struct {
    uint32_t SID : 11;                          /**< Standard ID */
    uint32_t EID : 18;                          /**< Extended ID */
    uint32_t SID11 : 1;                         /**< Reserved */
    uint32_t unimplemented1 : 2;                /**< Reserved */
} BM_MessageIdTypeDef;

/**
 * @typedef BM_TxMessageCtrlTypeDef
 * @brief   Busmust TX CAN Message control fields.
 * @note    The first a few fields (until FDF) are bit compatible with BM_RxMessageCtrlTypeDef.
 */
typedef struct {
    uint32_t DLC : 4;                           /**< CAN message DLC(0-F), note this is not the message length */
    uint32_t IDE : 1;                           /**< This message is an extended CAN message */
    uint32_t RTR : 1;                           /**< This message is a remote CAN message */
    uint32_t BRS : 1;                           /**< This message requires CAN-FD bitrate switching */
    uint32_t FDF : 1;                           /**< This message is a CAN-FD CAN message */
    uint32_t ESI : 1;                           /**< Reserved for gateways */
    uint32_t SEQ : 23;                          /**< Reserved for hardware sync */
} BM_TxMessageCtrlTypeDef;

/**
 * @typedef BM_RxMessageCtrlTypeDef
 * @brief   Busmust RX CAN Message control fields.
 * @note    The first a few fields (until FDF) are bit compatible with BM_TxMessageCtrlTypeDef.
 */
typedef struct {
    uint32_t DLC : 4;                           /**< CAN message DLC(0-F), note this is not the message length */
    uint32_t IDE : 1;                           /**< This message is an extended CAN message */
    uint32_t RTR : 1;                           /**< This message is a remote CAN message */
    uint32_t BRS : 1;                           /**< This message requires CAN-FD bitrate switching */
    uint32_t FDF : 1;                           /**< This message is a CAN-FD CAN message */
    uint32_t ESI : 1;                           /**< Reserved for gateways */
    uint32_t unimplemented1 : 2;                /**< Reserved */
    uint32_t FilterHit : 5;                     /**< By wich RX filter the message is accepted */
    uint32_t unimplemented2 : 16;               /**< Reserved */
} BM_RxMessageCtrlTypeDef;

/**
 * @typedef BM_CanMessageTypeDef
 * @brief   Busmust CAN Message concrete type, usually used as payload of BM_DataTypeDef.
 * @note    The total length of this structure is 72B, it support both classic and FD CAN messages.
 */
typedef struct 
{
    uint32_t id;                                /**< CAN message ID, see BM_MessageIdTypeDef for details. */
    union
    {
        uint32_t tx;                            /**< TX CAN message control fields, invalid if this is NOT a TX can message. */
        uint32_t rx;                            /**< RX CAN message control fields, invalid if this is NOT a RX can message. */
    } ctrl;                                     /**< CAN message control fields, whether TX or RX is taken depends on the message direction. */
    uint8_t payload[64];                        /**< CAN message payload */
} BM_CanMessageTypeDef;

/**
 * @typedef BM_ChannelInfoTypeDef
 * @brief   Channel information, created when enumerating devices by BM_Enumerate() and used when opening device by BM_OpenEx().
 */
typedef struct
{
    char name[64];                              /**< Device full name, for display purpose */
    uint8_t  sn[16];                            /**< Device SN */
    uint8_t  uid[12];                           /**< Device UID */
    uint8_t  version[4];                        /**< Device Firmware Version */
    uint16_t vid;                               /**< Device VID */
    uint16_t pid;                               /**< Device PID */
    uint16_t port;                              /**< Port ID (0-7) of the device, note a multi-port device is enumerated as multiple dedicated BM_ChannelInfoTypeDef entries */
    uint16_t cap;                               /**< Device Capability flags, see BM_CapabilityTypeDef for details. */
    uint8_t  reserved[4];                       /**< Reserved */
} BM_ChannelInfoTypeDef;

/**
 * @typedef BM_CanStatusInfoTypedef
 * @brief   CAN channel status detailed information, retrieved by calling BM_GetCanStatus(), see ISO11898 for details.
 */
typedef struct
{
    uint8_t TXBO;                               /**< The CAN channel is in BUSOFF state */
    uint8_t reserved[1];                        /**< Reserved */
    uint8_t TXBP;                               /**< The CAN channel is in TX bus passive state */
    uint8_t RXBP;                               /**< The CAN channel is in RX bus passive state */
    uint8_t TXWARN;                             /**< The CAN channel is in TX warn state */
    uint8_t RXWARN;                             /**< The CAN channel is in RX warn state */
    uint8_t TEC;                                /**< TX Bus Error counter */
    uint8_t REC;                                /**< RX Bus Error counter */
} BM_CanStatusInfoTypedef;

/**
 * @typedef BM_BitrateTypeDef
 * @brief   CAN channel bitrate configuration, used by BM_SetBitrate().
 */
typedef struct
{
    uint16_t nbitrate;                          /**< Nominal bitrate in kbps, default as 500, note this is the only valid birate in CAN CLASSIC mode. */
    uint16_t dbitrate;                          /**< Data bitrate in kbps, default as 500, note this is ignored in CAN CLASSIC mode. */
    uint8_t nsamplepos;                         /**< Nominal sample position (percentage), 0-100, default as 75 */
    uint8_t dsamplepos;                         /**< Data sample position (percentage), 0-100, default as 75 */
    /* Setting any of the fields below would override the nbitrate configuration */
    uint8_t  clockfreq;                         /**< CAN controller clock in Mhz, default as 0 */
    uint8_t  reserved;                          /**< Reserved */
    uint8_t  nbtr0;                             /**< Nominal BTR0 register value, note this value is calculated using clockfreq, which might not be 16MHz */
    uint8_t  nbtr1;                             /**< Nominal BTR1 register value, note this value is calculated using clockfreq, which might not be 16MHz */
    uint8_t  dbtr0;                             /**< Data BTR0 register value, note this value is calculated using clockfreq, which might not be 16MHz */
    uint8_t  dbtr1;                             /**< Data BTR1 register value, note this value is calculated using clockfreq, which might not be 16MHz */
} BM_BitrateTypeDef;

typedef enum
{
    BM_AUTOSET_PASSIVE_LISTEN = 0,
    BM_AUTOSET_ACTIVE_DETECT = 1,
} BM_AutosetModeTypeDef;

typedef struct
{
    uint8_t version;
    uint8_t mode;
    uint8_t minLockMessages;
    uint8_t reserved0;
    uint16_t passiveObservationMs;
    uint16_t activeWriteTimeoutMs;
    BM_CanMessageTypeDef probeMessage;
    uint32_t reserved[4];
} BM_AutosetConfigTypeDef;

/**
 * @typedef BM_RxFilterTypeDef
 * @brief   CAN channel RX filter item structure, used by BM_SetRxFilter().
 * @note    The filter support masking ID, flags and payload according to its type, 
 *          in order for a message to be accepted, all the fields are masked using AND logic:
 *          (flags & filter.flags_mask == filter.flags_value) AND (ID & filter.id_mask == filter.id_value) AND (payload & filter.payload_mask == filter.payload_value)
 */
typedef struct
{
    uint8_t  type;                              /**< Type ID of the RX filter, see BM_RxFilterTypeTypeDef for details. */
    uint8_t  unused;                            /**< Reserved */
    uint8_t  flags_mask;                        /**< CAN message control Flags masks, see BM_MessageFlagsTypeDef for details. */
    uint8_t  flags_value;                       /**< CAN message control Flags values, see BM_MessageFlagsTypeDef for details. */
    uint8_t  reserved[4];                       /**< Reserved */
    uint32_t id_mask;                           /**< CAN message ID masks, see BM_MessageIdTypeDef for details. */
    uint32_t id_value;                          /**< CAN message ID values, see BM_MessageIdTypeDef for details. */
    uint8_t payload_mask[8];                    /**< CAN message payload masks, for CAN-FD messages, only the first 8 bytes are checked. */
    uint8_t payload_value[8];                   /**< CAN message payload values, for CAN-FD messages, only the first 8 bytes are checked. */
} BM_RxFilterTypeDef;

/**
 * @typedef BM_TxTaskTypeDef
 * @brief   CAN channel TX task item structure, used by BM_SetTxTask().
 * @note    Once the CAN device is armed with TX tasks, it will try to parse the TX task and send CAN messages automatically.
 *          The difference with a software triggered CAN message in BusMaster is that 
 *          hardware triggered CAN messages are more precise in time and could reach a higher throughput.
 */
typedef struct
{
    uint8_t  type;                              /**< Type ID of the TX task, see BM_TxTaskTypeTypeDef for details. */
    uint8_t  unused;                            /**< Reserved */
    uint8_t  flags;                             /**< CAN message control Flags, see BM_MessageFlagsTypeDef for details. */
    uint8_t  length;                            /**< Length of payload in bytes (not DLC) */
    uint8_t  e2e;                               /**< Index of E2E (in E2E table), currently unsupported */
    uint8_t  reserved[1];                       /**< Reserved */

    uint16_t cycle;                             /**< ms delay between rounds */
    uint16_t nrounds;                           /**< num of cycles */
    uint16_t nmessages;                         /**< messages per round */

    uint32_t id;                                /**< CAN message arbitration id, see BM_MessageIdTypeDef for details. */

    union
    {
        /* The child fields are invalid if the TX task type is NOT BM_TXTASK_INCDATA */
        struct {
            uint16_t startbit;                  /**< Start bit of data increment, currently only 8-bit aligned value is accepted */
            uint8_t nbits;                      /**< Number of bits of data increment, currently only 32 is accepted */
            uint8_t format;                     /**< 0x80=Intel, 0x00=Motorola */
            uint32_t min;                       /**< Minimum value of the Increment range */
            uint32_t max;                       /**< Maximum value of the Increment range */
            uint32_t step;                      /**< Step of the Increment range */
        } incdata;

        /* The child fields are invalid if the TX task type is NOT BM_TXTASK_INCID */
        struct {
            uint32_t min;                       /**< Minimum value of the Increment range */
            uint32_t max;                       /**< Maximum value of the Increment range */
            uint32_t step;                      /**< Step of the Increment range */
        } incid;

        /* The child fields are invalid if the TX task type is NOT BM_TXTASK_RANDOMDATA */
        struct {
            uint16_t startbit;                  /**< Start bit of random data, currently only 8-bit aligned value is accepted */
            uint8_t nbits;                      /**< Number of bits of random data, currently only 32 is accepted */
            uint8_t format;                     /**< 0x80=Intel, 0x00=Motorola */
            uint32_t min;                       /**< Minimum value of the Increment range */
            uint32_t max;                       /**< Maximum value of the Increment range */
            uint32_t seed;                      /**< Random seed */
        } randomdata;

        /* The child fields are invalid if the TX task type is NOT BM_TXTASK_RANDOMID */
        struct {
            uint32_t min;                       /**< Minimum value of the random range */
            uint32_t max;                       /**< Maximum value of the random range */
            uint32_t seed;                      /**< Random seed */
        } randomid;

        uint8_t unused[48];                     /**< Reserved */
    } pattern;                                  /**< Changing pattern of a Volatile TX task */

    uint8_t  payload[64];                       /**< Default payload data, note this is also the template payload of the unchanged part in a volatile TX task */
} BM_TxTaskTypeDef;


/////////////////////////////////////////////////////////////////////////// BMAPI.h ///////////////////////////////////////////////////////////////////////////
/**
 * @def   BM_API_VERSION
 * @brief API version, format: major.minor.revision.build
 * @note  This macro might be checked by the application so that it could adapt to different versions of BMAPI.
 */
#define BM_API_VERSION         0x010E022D  /* 1.14.2.45 */

/**
 * @def   BMAPI
 * @brief All function declared with BMAPI modifier are exported by BMAP dynamic library (*.dll, *.so).
 */
#ifdef BMAPI_EXPORT
#ifdef _MSC_VER
#define BMAPI   __declspec(dllexport)
#else
#define BMAPI   __attribute__((visibility("default")))
#endif
#else
#ifdef _MSC_VER
#define BMAPI   __declspec(dllimport)
#else
#define BMAPI
#endif
#endif

/**
 * @typedef BM_ChannelHandle
 * @brief   Abstract handle to opened Busmust device channel, 
 *          most APIs would require a handle to operate on the target device channel.
 */
typedef void* BM_ChannelHandle;

/**
 * @typedef BM_NotificationHandle
 * @brief   Abstract handle to notification event of opened Busmust device channel, 
 *          call BM_WaitForNotifications to wait for new events (i.e. CAN RX message event).
 */
typedef void* BM_NotificationHandle;

/**
 * @typedef BM_AutosetCallbackHandle
 * @brief   Pointer to a Callback function when AUTOSET status is updates, indicating a bitrate option has passed or failed.
 * @param[in] bitrate      The bitrate option value which has passed or failed.
 * @param[in] tres         The terminal resistor option value which has passed or failed.
 * @param[in] nrxmessages  Number of received messages while listening to the bus using bitrate and tres.
 * @param[in] userarg      Arbitrary user argument passed by BM_Autoset().
 * @return  Return 0 to continue AUTOSET sequence, return non-zero value to cancel AUTOSET sequence.
 */
typedef uint32_t (*BM_AutosetCallbackHandle)(const BM_BitrateTypeDef* bitrate, BM_TerminalResistorTypeDef tres, int nrxmessages, uintptr_t userarg);

/**
 * @brief  Initialize BMAPI library, this function shall be called before any other API calls and shall only be called once.
 * @return Operation status, see BM_StatusTypeDef for details.
 */
BMAPI BM_StatusTypeDef BM_Init(void);

/**
 * @brief  Un-initialize BMAPI library, this function shall be called after any other API calls and shall only be called once.
 * @return Operation status, see BM_StatusTypeDef for details.
 */
BMAPI BM_StatusTypeDef BM_UnInit(void);

/**
 * @brief        Enumerate all connected Busmust device.
 * @param[out]   channelinfos  An array of BM_ChannelInfoTypeDef structure which holds info of all the enumerated Busmust devices.
 * @param[inout] nchannels     Number of device channels available, which is also the number of valid entries in channelinfos, 
 *                             this param must be initialized with the maximum length of the channelinfos array when calling this function.
 * @return       Operation status, see BM_StatusTypeDef for details.
 */
BMAPI BM_StatusTypeDef BM_Enumerate(BM_ChannelInfoTypeDef channelinfos[], int* nchannels);

/**
 * @brief Start AUTOSET sequence, for a BM-USB-CAN(FD) device, the AUTOSET sequence will detect the correct bitrate and terminal resistor.
 * @param[in]  channelinfo  Info of the device channel to run AUTOSET on, usually the info is filled by BM_Enumerate().
 * @param[out] bitrate      The detected bitrate.
 * @param[out] tres         The detected terminal resistor.
 * @param[in]  callback     A callback function which will be called on each step of AUTOSET.
 * @param[in]  userarg      Arbitrary user argument of the callback function, this argument will be passed to the callback as is.
 */
BMAPI BM_StatusTypeDef BM_Autoset(
    BM_ChannelInfoTypeDef* channelinfo,
    BM_BitrateTypeDef* bitrate,
    BM_TerminalResistorTypeDef* tres,
    BM_AutosetCallbackHandle callback,
    uintptr_t userarg
);

BMAPI BM_StatusTypeDef BM_AutosetEx(
    BM_ChannelInfoTypeDef* channelinfo,
    BM_BitrateTypeDef* bitrate,
    BM_TerminalResistorTypeDef* tres,
    const BM_AutosetConfigTypeDef* config,
    BM_AutosetCallbackHandle callback,
    uintptr_t userarg
);

/**
 * @brief Open the specified CAN device port.
 * @param[in] port  Index of the port, starting from zero, note this is the index of all enumerated ports.
 * @return Handle to the opened CAN device channel, return NULL if failed to open the specified port.
 */
BMAPI BM_ChannelHandle BM_OpenCan(uint16_t port);

/**
 * @brief Open the specified device port using given configuration.
 * @param[out] handle      Handle to the opened device channel.
 * @param[in]  channelinfo Info of the device channel to open, usually the info is filled by BM_Enumerate().
 * @param[in]  mode        CAN operation mode option of the opened channel, see BM_CanModeTypeDef for details.
 * @param[in]  tres        Terminal resistor option of the opened channel, see BM_TerminalResistorTypeDef for details.
 * @param[in]  bitrate     Bitrate option of the opened channel, see BM_BitrateTypeDef for details.
 * @param[in]  rxfilter    CAN acceptance filters option of the opened channel, see BM_RxFilterTypeDef for details.
 * @param[in]  nrxfilters  Number of acceptance filters, usually there could be up to 2 filters.
 * @return Operation status, see BM_StatusTypeDef for details.
 */
BMAPI BM_StatusTypeDef BM_OpenEx(
        BM_ChannelHandle* handle,
        BM_ChannelInfoTypeDef* channelinfo,
        BM_CanModeTypeDef mode,
        BM_TerminalResistorTypeDef tres,
        const BM_BitrateTypeDef* bitrate,
        const BM_RxFilterTypeDef* rxfilter,
        int nrxfilters
        );

/**
 * @brief     Close an opened channel.
 * @param[in] handle  Handle to the channel to be closed.
 * @return    Operation status, see BM_StatusTypeDef for details.
 */
BMAPI BM_StatusTypeDef BM_Close(BM_ChannelHandle handle);

/**
 * @brief     Reset an opened channel.
 * @param[in] handle  Handle to the channel to be reset.
 * @return    Operation status, see BM_StatusTypeDef for details.
 * @note      The configuration options will not lost when the channel is reset, so BM_Reset() is basically identical to BM_Close() and then BM_OpenEx().
 */
BMAPI BM_StatusTypeDef BM_Reset(BM_ChannelHandle handle);

/**
 * @brief     Clear TX&RX message buffer of an opened channel.
 * @param[in] handle  Handle to the channel to be cleared.
 * @return    Operation status, see BM_StatusTypeDef for details.
 * @note      This function is available since BMAPI1.3, hardware status will not be changed when clearing buffer.
 */
BMAPI BM_StatusTypeDef BM_ClearBuffer(BM_ChannelHandle handle);

/**
 * @brief      Read any message/event out of the given channel.
 * @param[in]  handle  Handle to the channel to read from.
 * @param[out] data    A caller-allocated buffer to hold the message/event output, see BM_DataTypeDef for details.
 * @return     Operation status, see BM_StatusTypeDef for details.
 * @note       This function is non-blocked, and thus will return BM_ERROR_QRCVEMPTY if no message is received.
 *             Please use notifications to wait for Rx events and then read message/event out of BMAPI internal RX buffer, otherwise you could also poll the device periodically.
 */
BMAPI BM_StatusTypeDef BM_Read(BM_ChannelHandle handle, BM_DataTypeDef* data);

/**
 * @brief      Read CAN message out of the given channel.
 * @param[in]  handle     Handle to the channel to read from.
 * @param[out] msg        A caller-allocated buffer to hold the CAN message output, see BM_CanMessageTypeDef for details.
 * @param[out] channel    The source channel ID from which the message is received, starting from zero, could be NULL if not required.
 * @param[out] timestamp  The device local high precision timestamp in microseconds, when the message is physically received on the CAN bus, could be NULL if not required.
 * @return     Operation status, see BM_StatusTypeDef for details. 
 * @note       Note this function is a simple wrapper of BM_Read(), see BM_Read() for details.
 */
BMAPI BM_StatusTypeDef BM_ReadCanMessage(BM_ChannelHandle handle, BM_CanMessageTypeDef* msg, uint32_t* channel, uint32_t* timestamp);

/**
 * @brief      Write any message/event to the given channel.
 * @param[in]  handle  Handle to the channel to write to.
 * @param[in]  data      A caller-allocated buffer to hold the message/event input, see BM_DataTypeDef for details.
 * @param[in]  timeout   Timeout (in milliseconds) before the message is transmitted successfully to the bus.
 *                       Set any negative number (i.e. -1) to wait infinitely.
 *                       Set 0 if you would like to transmit asynchronously: put to BMAPI internal buffer and return immediately, then receive TXCMPLT event over BM_Read() later.
 * @param[in]  timestamp The device local high precision timestamp in microseconds, when the message is physically transmitted on the CAN bus, could be NULL if not required.
 * @return     Operation status, see BM_StatusTypeDef for details.
 * @note       This function is allowed to be called from multiple threads since BMAPI1.3.
 */
BMAPI BM_StatusTypeDef BM_Write(BM_ChannelHandle handle, const BM_DataTypeDef* data, int timeout, uint32_t* timestamp);

/**
 * @brief      Write CAN message to the given channel.
 * @param[in]  handle     Handle to the channel to write to.
 * @param[in]  msg        A caller-allocated buffer to hold the CAN message output, see BM_CanMessageTypeDef for details.
 * @param[in]  _channel   The target channel ID to which the message is transmitted, starting from zero. This parameter is reserved for future, always 0 now.
 * @param[in]  timeout   Timeout (in milliseconds) before the message is transmitted successfully to the bus.
 *                       Set any negative number (i.e. -1) to wait infinitely.
 *                       Set 0 if you would like to transmit asynchronously: put to BMAPI internal buffer and return immediately, then receive TXCMPLT event over BM_Read() later.
 * @param[in]  timestamp The device local high precision timestamp in microseconds, when the message is physically transmitted on the CAN bus, could be NULL if not required.
 * @note       Note this function is a simple wrapper of BM_Write(), see BM_Write() for details.
 */
BMAPI BM_StatusTypeDef BM_WriteCanMessage(BM_ChannelHandle handle, BM_CanMessageTypeDef* msg, uint32_t _channel, int timeout, uint32_t* timestamp);

/**
 * @brief Control the given channel, this is an advanced interface and is typically used internally by BMAPI.
 * @param[in]    handle   Handle to the channel to control.
 * @param[in]    command  Control command.
 * @param[in]    value    Control value.
 * @param[in]    index    Control index.
 * @param[inout] data     Control data, could be NULL.
 * @param[inout] nbytes   Length in bytes of the control data, could be zero.
 */
BMAPI BM_StatusTypeDef BM_Control(BM_ChannelHandle handle, uint8_t command, uint16_t value, uint16_t index, void* data, int nbytes);

/**
 * @brief      Get current CAN status of the given channel.
 * @param[in]  handle      Handle to the channel to operate on.
 * @param[out] statusinfo  Detailed information of current CAN status, see BM_CanStatusInfoTypedef for details.
 * @return     Current status code, see BM_StatusTypeDef for details.
 */
BMAPI BM_StatusTypeDef BM_GetStatus(BM_ChannelHandle handle, BM_CanStatusInfoTypedef* statusinfo);

/**
 * @brief      Get current local high precision device timestamp, in microseconds.
 * @param[in]  handle     Handle to the channel to operate on.
 * @param[out] timestamp  Timestamp value.
 * @return     Operation status, see BM_StatusTypeDef for details.
 */
BMAPI BM_StatusTypeDef BM_GetTimestamp(BM_ChannelHandle handle, uint32_t* timestamp);

/**
 * @brief      Set CAN mode option of the given channel.
 * @param[in]  handle  Handle to the channel to operate on.
 * @param[in]  mode    Expected CAN mode, see BM_CanModeTypeDef for details.
 * @return     Operation status, see BM_StatusTypeDef for details.
 */
BMAPI BM_StatusTypeDef BM_SetCanMode(BM_ChannelHandle handle, BM_CanModeTypeDef mode);

/**
 * @brief      Set terminal resistor option of the given channel.
 * @param[in]  handle  Handle to the channel to operate on.
 * @param[in]  tres    Expected terminal resistor, see BM_TerminalResistorTypeDef for details.
 * @return     Operation status, see BM_StatusTypeDef for details.
 */
BMAPI BM_StatusTypeDef BM_SetTerminalRegister(BM_ChannelHandle handle, BM_TerminalResistorTypeDef  tres);

/**
 * @brief      Set bitrate option of the given channel.
 * @param[in]  handle  Handle to the channel to operate on.
 * @param[in]  bitrate Expected bitrate, see BM_BitrateTypeDef for details.
 * @return     Operation status, see BM_StatusTypeDef for details.
 */
BMAPI BM_StatusTypeDef BM_SetBitrate(BM_ChannelHandle handle, const BM_BitrateTypeDef* bitrate);

/**
 * @brief      Set TX tasks option of the given channel.
 * @param[in]  handle    Handle to the channel to operate on.
 * @param[in]  txtasks   An array of TX task information, see BM_TxTaskTypeDef for details.
 * @param[in]  ntxtasks  Number of valid TX tasks in the txtasks array.
 * @return     Operation status, see BM_StatusTypeDef for details.
 */
BMAPI BM_StatusTypeDef BM_SetTxTasks(BM_ChannelHandle handle, BM_TxTaskTypeDef* txtasks, int ntxtasks);

/**
 * @brief      Set RX filters option of the given channel.
 * @param[in]  handle      Handle to the channel to operate on.
 * @param[in]  rxfilters   An array of RX filter information, see BM_RxFilterTypeDef for details.
 * @param[in]  nrxfilters  Number of valid RX filters in the txtasks rxfilters.
 * @return     Operation status, see BM_StatusTypeDef for details.
 */
BMAPI BM_StatusTypeDef BM_SetRxFilters(BM_ChannelHandle handle, BM_RxFilterTypeDef* rxfilters, int nrxfilters);

/**
 * @brief Get the platform/OS independent notification handle for the given channel, so that the application could wait for notifications later.
 * @param[in]  handle        Handle to the channel that owns the notification handle.
 * @param[out] notification  The platform/OS independent notification handle.
 * @return     Operation status, see BM_StatusTypeDef for details.
 * @note       By using notification handles in a background thread, it is easy to implement an asynchronous message receiver as below:
 * @code
 *             channel = BM_OpenCan(...);
 *             BM_GetNotification(channel, notification);
 *             while (!exit) {
 *               BM_WaitForNotifications(&notification, 1, -1); // Wait infinitely for new message notification.
 *               BM_ReadCanMessage(...);
 *             }
 * @endcode
 */
BMAPI BM_StatusTypeDef BM_GetNotification(BM_ChannelHandle handle, BM_NotificationHandle* notification);

/**
 * @brief     A platform/OS independent implementation to wait for single/multiple notification handles.
 * @param[in] handles     An array of channel notification handles.
 * @param[in] nhandles    Number of valid notification handles.
 * @param[in] ntimeoutms  This function will block current thread for ntimeoutms milliseconds if no notification is received.
 *                        Note this function will return immediately once a new notification is received, the ntimeoutms param is ignored in this normal case.
 * @return    This function returns the index in handles array of the channel from which a new notification is posted.
 */
BMAPI int BM_WaitForNotifications(BM_NotificationHandle handles[], int nhandles, int ntimeoutms);

/**
 * @brief      Translate error code to string, this is a helper function to ease application programming.
 * @param[in]  errorcode  The errorcode to be translated.
 * @param[out] buffer     A caller-allocated string buffer to hold the translated string.
 * @param[in]  nbytes     Number in bytes of the string buffer.
 * @param[in]  language   Reserved, only English is supported currently.
 */
BMAPI void BM_GetErrorText(BM_StatusTypeDef errorcode, char* buffer, int nbytes, uint16_t language);

/**
 * @brief      Translate data (i.e. CAN message) to string, this is a helper function to ease application programming.
 * @param[in]  data       The message data to be translated.
 * @param[out] buffer     A caller-allocated string buffer to hold the translated string.
 * @param[in]  nbytes     Number in bytes of the string buffer.
 * @param[in]  language   Reserved, only English is supported currently.
 */
BMAPI void BM_GetDataText(BM_DataTypeDef* data, char* buffer, int nbytes, uint16_t language);


/* ==== Types and APIs synchronized with bmapi.h/bm_usb_def.h 1.14.2.45 (CAN-332) ==== */

typedef enum
{
    BM_LOG_NONE = 0,             /**< Show nothing on debug console */
    BM_LOG_ERR,                  /**< Show only ERR level messages on debug console */
    BM_LOG_WRN,                  /**< Show ERR and WRN level messages on debug console */
    BM_LOG_INF,                  /**< Show ERR|WRN|INF level messages on debug console */
    BM_LOG_DBG                   /**< Show all messages on debug console, including debug messages */
} BM_LogLevelTypeDef;

typedef enum
{
    BM_DEFAULT_BUFFER = 0x0000,                 /**< Default buffer: txq for write, rxq for read */
    BM_LOGGINGFILE_BUFFER = 0x0001,             /**< Buffer for logging file operations */
    BM_REPLAYFILE_BUFFER = 0x0002,              /**< Buffer for replay file operations */
    BM_LOGGINGQ_BUFFER = 0x0111,                /**< Buffer for logging queue */
    BM_REPLAYQ_BUFFER = 0x0222,                 /**< Buffer for replay queue */
    BM_RXQ_BUFFER = 0x1111,                     /**< RX queue buffer (compatible with BM_ClearBuffer) */
    BM_TXQ_BUFFER = 0x2222,                     /**< TX queue buffer (compatible with BM_ClearBuffer) */
    BM_NO_BUFFER = 0xFFFF                       /**< No buffer: direct physical channel access for low latency */
} BM_BufferId;

typedef enum
{
    BM_WRITE_BUFFER = 0,                        /**< Write buffer */
    BM_READ_BUFFER = 0x8000U                    /**< Read buffer */
} BM_BufferTypeDef;

typedef enum
{
    BM_WAKEUP = 0,                    /**< Channel is awake */
    BM_SLEEP = 1,                     /**< Channel is sleeping */
} BM_SleepStatusTypeDef;

typedef enum
{
    BM_PTP_DISABLED = 0,                    /**< Disable PTP feature, work asynchronously using hardware local timestamp */
    BM_PTP_INPUT_USB_SOF = 1,               /**< Use host computer's USB SOF packet to sync with host computer */
    BM_PTP_INPUT_PPS = 2,                   /**< PTP slave: Use dedicated PPS input pin (if supported by hardware) as sync pulse input */
    BM_PTP_OUTPUT_PPS = 4,                  /**< PTP master: Use dedicated PPS input pin (if supported by hardware) as sync pulse output */
} BM_PtpModeTypeDef;

typedef enum
{
    BM_LIN_OFF_MODE = 0x00,                     /**< The device is logically disconnected from LIN bus */
    BM_LIN_SLAVE_MODE = 0x01,                   /**< The device is running as a slave node */
    BM_LIN_INTERNAL_LOOPBACK_MODE = 0x02,       /**< The device is looping back messages internally */
    BM_LIN_LISTEN_ONLY_MODE = 0x03,             /**< The device is receiving messages without impacting the LIN bus */
    BM_LIN_MASTER_MODE = 0x08,                  /**< The device is running as a master node */
} BM_LinModeTypeDef;

typedef enum
{
    BM_LIN_VOLTAGE_12V_IN = 0,          /**< Accept 12V as input */
    BM_LIN_VOLTAGE_12V_OUT,             /**< Use internal 12V DC-DC for pulling up */
} BM_LinVoltageTypeDef;

typedef enum
{
    BM_REMOTE_OWNER_NONE = 0,
    BM_REMOTE_OWNER_LOCAL = 1,
    BM_REMOTE_OWNER_REMOTE = 2
} BM_RemoteOwnerTypeDef;

typedef enum
{
    BM_REMOTE_SESSION_UNKNOWN = 0,
    BM_REMOTE_SESSION_AVAILABLE = 1,
    BM_REMOTE_SESSION_CLAIMED = 2,
    BM_REMOTE_SESSION_EXPIRED = 3,
    BM_REMOTE_SESSION_FAULTED = 4
} BM_RemoteSessionStateTypeDef;

typedef enum
{
    BM_REMOTE_SESSION_CAP_NONE = 0,
    BM_REMOTE_SESSION_CAP_CLAIM = 1U << 0,
    BM_REMOTE_SESSION_CAP_HEARTBEAT = 1U << 1,
    BM_REMOTE_SESSION_CAP_CHANNEL_RELEASE = 1U << 2,
    BM_REMOTE_SESSION_CAP_CONTROL_STATUS = 1U << 3,
    BM_REMOTE_SESSION_CAP_TX_SEQ_TRANSLATION = 1U << 4
} BM_RemoteSessionCapabilityTypeDef;

typedef enum
{
    BM_REMOTE_SESSION_RESULT_OK = 0,
    BM_REMOTE_SESSION_RESULT_UNSUPPORTED = 1,
    BM_REMOTE_SESSION_RESULT_BUSY = 2,
    BM_REMOTE_SESSION_RESULT_STALE = 3,
    BM_REMOTE_SESSION_RESULT_DENIED = 4,
    BM_REMOTE_SESSION_RESULT_INVALID = 5,
    BM_REMOTE_SESSION_RESULT_INTERNAL = 6
} BM_RemoteSessionResultTypeDef;

#define BM_REMOTE_SESSION_ABI_VERSION 1U
#define BM_REMOTE_SESSION_DEFAULT_TIMEOUT_MS 10000U
#define BM_REMOTE_SESSION_DEFAULT_HEARTBEAT_MS 2000U
#define BM_REMOTE_SESSION_MIN_TIMEOUT_MS 3000U
#define BM_REMOTE_SESSION_MAX_TIMEOUT_MS 60000U

/** Configuration used when claiming a remote control session. */
typedef struct
{
    uint32_t size;                  /**< Set to sizeof(BM_RemoteSessionConfigTypeDef). */
    uint32_t version;               /**< Set to BM_REMOTE_SESSION_ABI_VERSION. */
    uint32_t timeout_ms;            /**< Control-lock lease, 0 for the 10000 ms default. */
    uint32_t heartbeat_interval_ms; /**< 0 for the 2000 ms default; at most half the lease. */
    uint32_t flags;                 /**< Must be zero in ABI version 1. */
    char client_name[32];           /**< Optional UTF-8 display name; NUL terminated. */
    uint8_t reserved[44];
} BM_RemoteSessionConfigTypeDef;

/** Status and ownership information for a remote control session. */
typedef struct
{
    uint32_t size;
    uint32_t version;
    uint32_t capabilities;
    uint32_t state;
    uint32_t result;
    uint32_t timeout_ms;
    uint32_t expires_in_ms;
    uint32_t config_generation;
    uint8_t session_id[16];
    uint8_t owner_ipv4[4];
    uint8_t owner_kind;
    uint8_t reserved0[3];
    char owner_name[32];
    uint8_t reserved[32];
} BM_RemoteSessionInfoTypeDef;

typedef void* BM_DeviceHandle;

/** Explicit control lock for one remote BMAPI endpoint. */
typedef void* BM_RemoteSessionHandle;

/**
 * @typedef BM_LinStatusInfoTypedef
 * @brief   LIN channel status detailed information, retrieved by calling BM_GetLinStatus() or BM_GetStatus().
 */
typedef struct
{
    uint8_t ACTIVE;                               /**< The LIN channel is in ACTIVE state */
    uint8_t SLEEP;                                /**< The LIN channel is in SLEEP state */
    uint8_t ERRORS;                               /**< Error status bitmask for the LIN channel, see BM_LinErrorTypeDef for details */
    uint8_t reserved[5];                          /**< Reserved */
} BM_LinStatusInfoTypeDef;

/**
 * @typedef BM_LinMessageCtrlTypeDef
 * @brief   Busmust LIN Message control fields.
 */
typedef struct {
    uint32_t DLC : 4;                           /**< LIN message DLC(0-8) */
    uint32_t TRANSMIT : 1;                      /**< Transfer direction: 0=receive, 1=transmit */
    uint32_t WAKEUP : 1;                        /**< This is a wakeup event */
    uint32_t SLEEP : 1;                         /**< This is a sleep event */
    uint32_t ENHANCED_CHECKSUM : 1;             /**< Flag for enhanced checksum */
    uint32_t USER_CHECKSUM : 1;                 /**< Flag for user defined checksum */
    uint32_t SEQ : 7;                           /**< hardware-sync message ID */
    uint32_t ISTXTASK : 1;                      /**< A indicator for TXTASK TEF event */
    uint32_t ERRORS : 6;                        /**< Error code, see BM_LinErrorTypeDef for details */
    uint32_t SYNC_BREAK : 1;                    /**< Header-only LIN frame indicator */
    uint32_t CHECKSUM : 8;                      /**< Checksum value */
} BM_LinMessageCtrlTypeDef;

/**
 * @typedef BM_LinMessageTypeDef
 * @brief   Busmust LIN Message concrete type, usually used as payload of BM_DataTypeDef.
 * @note    The total length of this structure is 16B.
 */
typedef struct
{
    uint8_t id;                                /**< LIN message ID */
    uint8_t padding[3];
    union
    {
        BM_LinMessageCtrlTypeDef lin;          /**< LIN message control fields. */
    } ctrl;                                    /**< Message control fields. */
    uint8_t payload[8];                        /**< LIN message payload */
} BM_LinMessageTypeDef;

/**
 * @typedef BM_LinProtocolTimingTypeDef
 * @brief   LIN protocol timing configuration, used in BM_LinProtocolConfigTypeDef.
 */
typedef struct
{
    uint8_t wakeLength;                     /**< 0:250us, 1:500us, 2:750us, ..., 7:2000us */
    uint8_t breakLength;                    /**< 0:13bit, 1:15bit, 2:17bit, ..., 7:27bit */
    uint8_t busInactiveTime;                /**< 0:4s, 1:6s, 2:8s, 3:10s */
    uint8_t wakeupRepeatTime;               /**< 0:180ms, 1:200ms, 2:220ms, 3:240ms */
} BM_LinProtocolTimingTypeDef;

/**
 * @typedef BM_LinProtocolConfigTypeDef
 * @brief   LIN Protocol configuration, used by BM_SetLinProtocol() and BM_GetLinProtocol().
 */
typedef struct
{
    uint8_t version;                            /**< 0x13=LIN1.3, 0x20=LIN2.0, 0x21=LIN2.1, 0x22=LIN2.2 */
    uint8_t checksum;                           /**< 0x00=manual, 0x01=normal, 0x02=enhanced */
    uint8_t reserved[2];
    BM_LinProtocolTimingTypeDef timing;
    uint8_t reserved2[8];
} BM_LinProtocolConfigTypeDef;

typedef struct
{
    uint8_t type;           /**< 0 = invalid, 1 = unicast, 2 = broadcast */
    uint8_t source;         /**< Source channel index (0-15) */
    uint16_t target;        /**< Target channel: index if type==1, bitmask of target channels if type==2 */
    uint16_t reserved;      /**< Reserved for future */
    uint8_t flagsmask;      /**< Source message flag mask */
    uint8_t flagsvalue;     /**< Source message flag value */
    uint32_t idmask;        /**< Source message ID mask */
    uint32_t idvalue;       /**< Source message ID value */
} BM_MessageRouteTypeDef;

/**
 * @typedef BM_IsotpStatusTypeDef
 * @brief   ISOTP status report, used by ISOTP operation callback function.
 */
typedef struct
{
    uint8_t  version;                           /**< Currently always 0x01 */
    uint8_t  flowcontrol;                       /**< Current flow control status */
    uint8_t  stmin;                             /**< Current flow control status */
    uint8_t  blocksize;                         /**< Current flow control status */
    uint32_t ntransferredbytes;                 /**< Number of transferred bytes by now. */
    uint32_t ntotalbytes;                       /**< Number of total bytes indicated by ISOTP FF or SF. */
    uint32_t timestamp;                         /**< Current timestamp reported by device. */
    uint32_t reserved[4];                       /**< Reserved for future */
} BM_IsotpStatusTypeDef;

/**
 * @typedef BM_IsotpCallbackHandle
 * @brief   Pointer to a Callback function when ISOTP transaction progress updates.
 */
typedef uint8_t (*BM_IsotpCallbackHandle)(const BM_IsotpStatusTypeDef* status, uintptr_t userarg);

/**
 * @typedef BM_IsotpConfigTypeDef
 * @brief   ISOTP Protocol (See ISO15765-2 for details) configuration, used by BM_ReadIsotp()/BM_WriteIsotp().
 */
typedef struct
{
    uint8_t version;                            /**< Currently must be set to 0x01 */
    uint8_t mode;                               /**< See BM_IsotpModeTypeDef for details */
    struct
    {
        uint16_t a;
        uint16_t b;
        uint16_t c;
    } testerTimeout;
    struct
    {
        uint16_t a;
        uint16_t b;
        uint16_t c;
    } ecuTimeout;
    struct
    {
        uint8_t stmin;
        uint8_t blockSize;
        uint8_t fcFrameLength;
        uint8_t hardwareIsotpDisabled;
    } flowcontrol;
    uint8_t extendedAddress;
    uint8_t paddingEnabled;
    uint8_t paddingValue;
    uint8_t longPduEnabled;
    uint8_t functionalAddressingEnabled;
    uint8_t padding[1];
    BM_IsotpCallbackHandle callbackFunc;
    uintptr_t callbackUserarg;
    BM_DataTypeDef testerDataTemplate;
    BM_DataTypeDef ecuDataTemplate;
} BM_IsotpConfigTypeDef;


/**
 * @enum  BM_StorageModeTypeDef / BM_StorageDirectionTypeDef / BM_StorageFormatTypeDef / BM_StoragePathModeTypeDef
 * @brief Logging or replay enums, used in BM_LoggingConfigTypeDef and BM_ReplayConfigTypeDef.
 */
typedef enum { BM_STORAGE_DISABLED = 0, BM_STORAGE_ALWAYS_ON, BM_STORAGE_TRIGGERED } BM_StorageModeTypeDef;
typedef enum { BM_STORAGE_DIRECTION_NONE = 0, BM_STORAGE_DIRECTION_RX, BM_STORAGE_DIRECTION_TX,
               BM_STORAGE_DIRECTION_ALL = BM_STORAGE_DIRECTION_RX | BM_STORAGE_DIRECTION_TX } BM_StorageDirectionTypeDef;
typedef enum { BM_STORAGE_DEFAULT_FORMAT = 0, BM_STORAGE_BBD_FORMAT = BM_STORAGE_DEFAULT_FORMAT,
               BM_STORAGE_PCAP_FORMAT = 1, BM_STORAGE_LOG_FORMAT = 2, BM_STORAGE_ASC_FORMAT = 3,
               BM_STORAGE_BLF_FORMAT = 4 } BM_StorageFormatTypeDef;
typedef enum { BM_STORAGE_FIXED_PATH = 0, BM_STORAGE_INDEX_PATH, BM_STORAGE_TIME_PATH } BM_StoragePathModeTypeDef;

/**
 * @typedef BM_PathSpecTypeDef
 * @brief   File Path spec (on data files), used by BM_LoggingConfigTypeDef and BM_ReplayConfigTypeDef.
 */
typedef struct
{
    uint8_t mode;                           /**< Naming mode, see BM_StoragePathModeTypeDef for details */
    uint8_t arg;                            /**< Mode argument, reserved for future */
    char format[30];                        /**< A printf-like string, used by path mode parser */
} BM_PathSpecTypeDef;

/**
 * @typedef BM_EventTriggerTypeDef
 * @brief   Event trigger (on rx messages), used by BM_LoggingConfigTypeDef and BM_ReplayConfigTypeDef.
 */
typedef struct
{
    uint16_t channels;                      /**< A Bitmask of RX channels */
    uint16_t reserved;
    uint16_t flags_mask;
    uint16_t flags_value;
    uint32_t id_mask;
    uint32_t id_value;
} BM_EventTriggerTypeDef;

typedef struct
{
    uint8_t  createNewFileOnStart;
    uint8_t  overwriteOldFileOnFull;
    uint16_t nfiles;
    uint32_t nmessagesPerFile;
    uint32_t nbytesPerFile;
    uint32_t nsecondsPerFile;
} BM_LoggingSegmentationTypeDef;

typedef struct
{
    uint16_t msgdelay;
    uint16_t sessiondelay;
    uint16_t cycledelay;
    uint8_t forceZeroTimestampOnFirstMsg;
    uint8_t reserved[1];
} BM_ReplayTimingTypeDef;

/**
 * @typedef BM_LoggingConfigTypeDef
 * @brief   Logging configuration, used by BM_SetLogging().
 */
typedef struct
{
    uint8_t version;                            /**< Currently must be set to 0x01 */
    uint8_t mode;                               /**< Logging mode, see BM_StorageModeTypeDef for details */
    uint8_t format;                             /**< Log file format, see BM_StorageFormatTypeDef for details */
    uint8_t reserved;
    uint16_t channels;
    uint8_t direction;                          /**< Logging direction, see BM_StorageDirectionTypeDef for details */
    uint8_t padding[9];
    BM_PathSpecTypeDef path;
    BM_EventTriggerTypeDef starttrigger;
    BM_EventTriggerTypeDef stoptrigger;
    BM_LoggingSegmentationTypeDef segmentation;
} BM_LoggingConfigTypeDef;

/**
 * @typedef BM_ReplayConfigTypeDef
 * @brief   Replay configuration, used by BM_SetReplay().
 */
typedef struct
{
    uint8_t version;                            /**< Currently must be set to 0x01 */
    uint8_t mode;                               /**< Replay mode, see BM_StorageModeTypeDef for details */
    uint8_t format;                             /**< Replay file format, see BM_StorageFormatTypeDef for details */
    uint8_t reserved;
    uint16_t channels;
    uint8_t direction;                          /**< Replay direction, see BM_StorageDirectionTypeDef for details */
    uint8_t cyclic;
    uint8_t padding[8];
    BM_PathSpecTypeDef path;
    BM_EventTriggerTypeDef starttrigger;
    BM_EventTriggerTypeDef stoptrigger;
    BM_ReplayTimingTypeDef timing;
} BM_ReplayConfigTypeDef;

/* OR-bitmask CAN mode additions (bm_usb_def.h 1.14.2.45). */
#ifndef BM_CAN_NON_ISO_MODE
#define BM_CAN_NON_ISO_MODE 0x08
#endif
#ifndef BM_CAN_NON_AUTORETX_MODE
#define BM_CAN_NON_AUTORETX_MODE 0x10
#endif
#ifndef BM_CAN_NOACK_MODE
#define BM_CAN_NOACK_MODE 0x20
#endif
#ifndef BM_CAN_DISABLE_AUTO_BUSOFF_RECOVERY
#define BM_CAN_DISABLE_AUTO_BUSOFF_RECOVERY 0x40
#endif

BMAPI BM_StatusTypeDef BM_Activate(BM_ChannelHandle handle);
BMAPI BM_StatusTypeDef BM_Deactivate(BM_ChannelHandle handle);
BMAPI BM_StatusTypeDef BM_RecoverBusOff(BM_ChannelHandle handle);
BMAPI BM_StatusTypeDef BM_CancelWrite(BM_ChannelHandle handle);
BMAPI BM_StatusTypeDef BM_EnumerateByCap(BM_ChannelInfoTypeDef channelinfos[], int* nchannels, uint16_t cap);
BMAPI BM_StatusTypeDef BM_EnumerateRemote(BM_ChannelInfoTypeDef channelinfos[], int* nchannels, uint8_t ipv4[4], int timeout);
BMAPI BM_StatusTypeDef BM_QueryRemoteSession(const uint8_t ipv4[4], int timeout_ms, BM_RemoteSessionInfoTypeDef* info);
BMAPI BM_StatusTypeDef BM_ClaimRemoteSession(BM_RemoteSessionHandle* session, const uint8_t ipv4[4], const BM_RemoteSessionConfigTypeDef* config, BM_RemoteSessionInfoTypeDef* info);
BMAPI BM_StatusTypeDef BM_GetRemoteSessionInfo(BM_RemoteSessionHandle session, BM_RemoteSessionInfoTypeDef* info);
BMAPI BM_StatusTypeDef BM_ReleaseRemoteSession(BM_RemoteSessionHandle session);
BMAPI BM_ChannelHandle BM_OpenRemote(uint8_t ipv4[4], const BM_ChannelInfoTypeDef* localinfo);
BMAPI BM_StatusTypeDef BM_GetChannelInfo(BM_ChannelHandle handle, BM_ChannelInfoTypeDef* info);
BMAPI BM_StatusTypeDef BM_ResetDevice(BM_DeviceHandle handle);
BMAPI BM_StatusTypeDef BM_GetDevice(BM_ChannelHandle channel, BM_DeviceHandle* device);
BMAPI BM_StatusTypeDef BM_SetBuffer(BM_DeviceHandle device, BM_BufferTypeDef type, BM_BufferId id);
BMAPI BM_StatusTypeDef BM_SetPtpMode(BM_ChannelHandle handle, BM_PtpModeTypeDef mode);
BMAPI BM_StatusTypeDef BM_SetPtpTime(BM_ChannelHandle handle, uint64_t ns);
BMAPI BM_StatusTypeDef BM_GetPtpTime(BM_ChannelHandle handle, uint64_t* ns);
BMAPI uint64_t BM_GetHostPtpTime(void);
BMAPI BM_StatusTypeDef BM_SyncPtpTimes(BM_ChannelHandle handles[], int nhandles);
BMAPI BM_StatusTypeDef BM_MapTimestamp(BM_ChannelHandle handle, uint32_t timestamp32, uint64_t* timestamp64);
BMAPI BM_StatusTypeDef BM_ReadMultiple(BM_ChannelHandle handle, BM_DataTypeDef data[], uint32_t* nmessages, int timeout);
BMAPI BM_StatusTypeDef BM_ReadMultipleCanMessage(BM_ChannelHandle handle, BM_CanMessageTypeDef msg[], uint32_t* nmessages, int timeout, uint32_t channel[], uint32_t timestamp[]);
BMAPI BM_StatusTypeDef BM_ReadIsotp(BM_ChannelHandle handle, const void* data, uint32_t* nbytes, int timeout, BM_IsotpConfigTypeDef* config);
BMAPI BM_StatusTypeDef BM_ReadLinMessage(BM_ChannelHandle handle, BM_LinMessageTypeDef* msg, uint32_t* channel, uint32_t* timestamp);
BMAPI BM_StatusTypeDef BM_WriteIsotp(BM_ChannelHandle handle, const void* data, uint32_t nbytes, int timeout, BM_IsotpConfigTypeDef* config);
BMAPI BM_StatusTypeDef BM_WriteLinMessage(BM_ChannelHandle handle, BM_LinMessageTypeDef* msg, uint32_t _channel, int timeout, uint32_t* timestamp);
BMAPI BM_StatusTypeDef BM_WriteMultiple(BM_ChannelHandle handle, const BM_DataTypeDef data[], uint32_t* nmessages, int timeout, uint32_t timestamp[]);
BMAPI BM_StatusTypeDef BM_WriteMultipleCanMessage(BM_ChannelHandle handle, const BM_CanMessageTypeDef msg[], uint32_t* nmessages, uint32_t _channel[], int timeout, uint32_t timestamp[]);
BMAPI BM_StatusTypeDef BM_GetCanStatus(BM_ChannelHandle handle, BM_CanStatusInfoTypedef* statusinfo);
BMAPI BM_StatusTypeDef BM_GetLinStatus(BM_ChannelHandle handle, BM_LinStatusInfoTypeDef* statusinfo);
BMAPI BM_StatusTypeDef BM_GetTxTasks(BM_ChannelHandle handle, BM_TxTaskTypeDef* txtasks, int ntxtasks);
BMAPI BM_StatusTypeDef BM_GetMsgRoutes(BM_ChannelHandle handle, BM_MessageRouteTypeDef* msgroutes, int nmsgroute);
BMAPI BM_StatusTypeDef BM_SetMode(BM_ChannelHandle handle, uint32_t mode);
BMAPI BM_StatusTypeDef BM_SetLinMode(BM_ChannelHandle handle, BM_LinModeTypeDef mode);
BMAPI BM_StatusTypeDef BM_GetCanMode(BM_ChannelHandle handle, BM_CanModeTypeDef* mode);
BMAPI BM_StatusTypeDef BM_SetSleepStatus(BM_ChannelHandle handle, BM_SleepStatusTypeDef status);
BMAPI BM_StatusTypeDef BM_GetSleepStatus(BM_ChannelHandle handle, BM_SleepStatusTypeDef status);
BMAPI BM_StatusTypeDef BM_GetTerminalRegister(BM_ChannelHandle handle, BM_TerminalResistorTypeDef* tres);
BMAPI BM_StatusTypeDef BM_SetLinVoltage(BM_ChannelHandle handle, BM_LinVoltageTypeDef voltage);
BMAPI BM_StatusTypeDef BM_GetLinVoltage(BM_ChannelHandle handle, BM_LinVoltageTypeDef* voltage);
BMAPI BM_StatusTypeDef BM_SetLinProtocol(BM_ChannelHandle handle, const BM_LinProtocolConfigTypeDef* protocol);
BMAPI BM_StatusTypeDef BM_GetLinProtocol(BM_ChannelHandle handle, BM_LinProtocolConfigTypeDef* protocol);
BMAPI BM_StatusTypeDef BM_SetLinBitrate(BM_ChannelHandle handle, uint16_t bitrate);
BMAPI BM_StatusTypeDef BM_SetEthSpeed(BM_ChannelHandle handle, uint16_t bitrate);
BMAPI BM_StatusTypeDef BM_GetBitrate(BM_ChannelHandle handle, BM_BitrateTypeDef* bitrate);
BMAPI BM_StatusTypeDef BM_SetMsgRoutes(BM_ChannelHandle handle, BM_MessageRouteTypeDef* msgroutes, int nmsgroute);
BMAPI BM_StatusTypeDef BM_GetRxFilters(BM_ChannelHandle handle, BM_RxFilterTypeDef* rxfilters, int nrxfilters);
BMAPI BM_StatusTypeDef BM_GetDataPtpTimestamp(BM_ChannelHandle channel, BM_DataTypeDef* data, uint64_t* timestamp);
BMAPI BM_LogLevelTypeDef BM_GetLogLevel(void);
BMAPI void BM_SetLogLevel(BM_LogLevelTypeDef level);
BMAPI void BM_SetThreadPriority(uint32_t priority);
BMAPI uint32_t BM_GetVersion(void);
BMAPI BM_StatusTypeDef BM_GetLogging(BM_ChannelHandle handle, BM_LoggingConfigTypeDef* logging);
BMAPI BM_StatusTypeDef BM_SetLogging(BM_ChannelHandle handle, BM_LoggingConfigTypeDef* logging);
BMAPI BM_StatusTypeDef BM_GetReplay(BM_ChannelHandle handle, BM_ReplayConfigTypeDef* replay);
BMAPI BM_StatusTypeDef BM_SetReplay(BM_ChannelHandle handle, BM_ReplayConfigTypeDef* replay);
BMAPI BM_StatusTypeDef BM_LoadConfig(BM_ChannelHandle handle, uint32_t configmask);
BMAPI BM_StatusTypeDef BM_SaveConfig(BM_ChannelHandle handle, uint32_t configmask);
BMAPI BM_StatusTypeDef BM_ClearConfig(BM_ChannelHandle handle, uint32_t configmask);

#ifdef __cplusplus
};
#endif

#endif /* #ifndef __BMAPI_H__ */
/**
 * End of file
 */
