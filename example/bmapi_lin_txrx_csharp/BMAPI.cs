using System;
using System.Text;
using System.Runtime.InteropServices;

/**
 * @enum  BM_CapabilityTypeDef
 * @brief Busmust Device capability flags, retrieved when enumerating devices using BM_Enumerate().
 */
public enum BM_CapabilityTypeDef
{
    BM_NONE_CAP = 0x0000,      /**< No capability */
    BM_LIN_CAP = 0x0001,       /**< The device is capable of handling LIN messages */
    BM_CAN_CAP = 0x0002,       /**< The device is capable of handling CAN messages */
    BM_CAN_FD_CAP = 0x0004,    /**< The device is capable of handling CANFD (and CAN) messages */
    BM_FLEXRAY_CAP = 0x0008,   /**< The device is capable of handling FLEXRAY messages */
    BM_MODBUS_CAP = 0x0010,    /**< The device is capable of handling MODBUS messages */
    BM_ETHERNET_CAP = 0x0020,  /**< The device is capable of handling ETHERNET messages */
    BM_AIO_CAP = 0x0100,       /**< The device is capable of analog IO */
    BM_DIO_CAP = 0x0200,       /**< The device is capable of digital IO */
    BM_VIRTUAL_CAP = 0x4000,   /**< The device is virtual, for simulation purpose */
    BM_REMOTE_CAP = 0x8000,    /**< The device is connected remotely and is capable of remote management */
    BM_ALL_CAP = 0xFFFF,       /**< Typically used for masking the CAP fields when programming */
};

/**
 * @enum  BM_DataTypeTypeDef
 * @brief Busmust data type flags, must be given in BM_DataTypeDef.
 */
public enum BM_DataTypeTypeDef
{
    BM_UNKNOWN_DATA = 0,        /**< Unknown data type */
    BM_LIN_DATA,                /**< LIN message data type */
    BM_CAN_FD_DATA,             /**< CAN or CAN-FD message data type (check FDF flag further) */
    BM_FLEXRAY_DATA,            /**< Flexray message data type */
    BM_MODBUS_DATA,             /**< MODBUS message data type */
    BM_ETHERNET_DATA,           /**< Ethernet message data type */
    BM_ACK_DATA = 0x8,          /**< ACK from bus, which indicates TXCMPLT event if this is BM_CAN_FD_DATA */
};

/**
 * @enum  BM_StatusTypeDef
 * @brief Busmust device & operation status, most APIs would return a status code to indicate the result of an operation.
 */
public enum BM_StatusTypeDef
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
    BM_ERROR_CONFIG = 0x1000000,               /**< Invalid configuration */
    BM_ERROR_CAUTION = 0x2000000,               /**< Reserved */
    BM_ERROR_INITIALIZE = 0x4000000,            /**< The device/library is not initialized */
    BM_ERROR_ILLOPERATION = 0x8000000           /**< Invalid operation */
};

/**
 * @enum  BM_CanModeTypeDef
 * @brief CAN mode IDs, used by BM_SetCanMode() to change the operation mode of CAN device.
 */
public enum BM_CanModeTypeDef
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
    BM_CAN_NON_ISO_MODE = 0x08,                /**< OR-Bitmask: The device is running with the capability to handle CAN and NON-ISO(Bosch) CANFD messages */
    BM_CAN_NON_AUTORETX_MODE = 0x10,           /**< OR-Bitmask: The device will not try to re-transmit failed messages if this bitmask is set */
    BM_CAN_NOACK_MODE = 0x20,                  /**< OR-Bitmask: ACK from remote ECU is not checked, always try to send message */
    BM_CAN_DISABLE_AUTO_BUSOFF_RECOVERY = 0x40, /**< OR-Bitmask: Disable automatic bus-off recovery, only manual BM_RecoverBusOff() triggers recovery */
};

/**
 * @enum  BM_TerminalResistorTypeDef
 * @brief Terminal resistor values, used by BM_SetTerminalResistor() to change the terminal resistor of CAN device.
 */
public enum BM_TerminalResistorTypeDef
{
    BM_TRESISTOR_AUTO = 0,              /**< Reserved, currently unsupported */
    BM_TRESISTOR_60 = 60,               /**< Currently unsupported */
    BM_TRESISTOR_120 = 120,             /**< 120Ohm */
    BM_TRESISTOR_PULLUP_1K = 1000,      /**< 1000Ohm pull-up resistor for LIN bus */
    BM_TRESISTOR_DISABLED = 0xFFFF,     /**< Disable terminal resistor */
};

/**
* @enum  BM_LinVoltageTypeDef
* @brief LIN voltage configuration values, used by BM_SetLinVoltage().
*/

public enum BM_LinVoltageTypeDef
{
    BM_LIN_VOLTAGE_12V_IN = 0,          /**< Accept 12V as input */
    BM_LIN_VOLTAGE_12V_OUT = 1          /**< Use internal 12V DC-DC for pulling up */
};


/**
* @enum  BM_LinMessageFlagsTypeDef
* @brief LIN Message type flags, used in BM_TxTaskTypeDef.
*/

public enum BM_LinMessageFlagsTypeDef
{
    BM_LIN_MESSAGE_FLAGS_TRANSMIT = 0x01,                    /**< Transfer direction: 0=receive, 1=transmit */
    BM_LIN_MESSAGE_FLAGS_WAKEUP = 0x02,                      /**< This is a wakeup event */
    BM_LIN_MESSAGE_FLAGS_SLEEP = 0x04,                       /**< This is a sleep event */
    BM_LIN_MESSAGE_FLAGS_ENHANCED_CHECKSUM = 0x08,           /**< Flag for enhanced checksum */
    BM_LIN_MESSAGE_FLAGS_USER_CHECKSUM = 0x10,               /**< Flag for user defined checksum, use value from 'CHECKSUM' field if USER_CHECKSUM==1  */
};


/**
* @enum  BM_LinProtocolChecksumTypeDef
* @brief LIN protocol checksum options.
*/

public enum BM_LinProtocolChecksumTypeDef
{
    BM_LIN_MANUAL_CHECKSUM = 0,                              /**< Reserved */
    BM_LIN_NORMAL_CHECKSUM = 1,                              /**< Normal checksum (usually used by LIN1.3) */
    BM_LIN_ENHANCED_CHECKSUM = 2                             /**< Enhanced checksum (can only be used by LIN2.x) */
};

/**
* @enum  BM_LinProtocolVersionTypeDef
* @brief LIN protocol version.
*/

public enum BM_LinProtocolVersionTypeDef
{
    BM_LIN_VER_1_3 = 0x13,                                   /**< Version1.3 */
    BM_LIN_VER_2_0 = 0x20,                                   /**< Version2.0 */
    BM_LIN_VER_2_1 = 0x21,                                   /**< Version2.1 */
    BM_LIN_VER_2_2 = 0x22,                                   /**< Version2.2 */
};

/**
* @enum  BM_LinErrorTypeDef
* @brief LIN error code, used in BM_LinStatusInfoTypedef.
*/

public enum BM_LinErrorTypeDef
{
    BM_LIN_NO_ERROR = 0x00,                                  /**< No error detected */
    BM_LIN_BIT_ERROR = 0x01,                                 /**< Bit error */
    BM_LIN_CHECKSUM_ERROR = 0x02,                            /**< Checksum error */
    BM_LIN_PARITY_ERROR = 0x04,                              /**< Parity error */
    BM_LIN_BREAK_ERROR = 0x08,                               /**< Break error */
    BM_LIN_BUS_IDLE_TIMEOUT = 0x10,                          /**< Bus idle timeout */
    BM_LIN_TRANSMIT_TIMEOUT = 0x20,                          /**< Transmit timeout */
};

/**
 * @enum  BM_LedTypeDef
 * @brief LED indicator status codes, used by BM_SetLed() to change the CAN LED indicator of CAN device.
 */
public enum BM_LedTypeDef
{
    BM_LED_OFF = 0,                     /**< CAN LED is OFF */
    BM_LED_ON = 1,                      /**< CAN LED is ON */
};

/**
 * @enum  BM_MessageChannelTypeDef
 * @brief Message channel IDs in BM_DataTypeDef header, used for routing indication.
 * @note  You could also use integers directly, please note that valid channel IDs start from zero.
 */
public enum BM_MessageChannelTypeDef
{
    BM_MESSAGE_CHANNEL_0 = 0x0,        /**< Channel 0 */
    BM_MESSAGE_CHANNEL_1 = 0x1,        /**< Channel 1 */
    BM_MESSAGE_CHANNEL_2 = 0x2,        /**< Channel 2 */
    BM_MESSAGE_CHANNEL_3 = 0x3,        /**< Channel 3 */
    BM_MESSAGE_CHANNEL_4 = 0x4,        /**< Channel 4 */
    BM_MESSAGE_CHANNEL_5 = 0x5,        /**< Channel 5 */
    BM_MESSAGE_CHANNEL_6 = 0x6,        /**< Channel 6 */
    BM_MESSAGE_CHANNEL_7 = 0x7,        /**< Channel 7 */
    BM_MESSAGE_ANY_CHANNEL = 0xF,      /**< Any channel, set this value in BM_DataTypeDef header if not used (e.g. TX.header.schn or RX.header.dchn) */
};

/**
 * @enum  BM_MessageFlagsTypeDef
 * @brief CAN Message type flags, used in BM_CanMessageTypeDef.
 */
public enum BM_MessageFlagsTypeDef
{
    BM_MESSAGE_FLAGS_NORMAL = 0,        /**< Normal CAN message */
    BM_CAN_MESSAGE_FLAGS_IDE = 0x01,        /**< Extended CAN message */
    BM_CAN_MESSAGE_FLAGS_RTR = 0x02,        /**< Remote CAN message */
    BM_CAN_MESSAGE_FLAGS_BRS = 0x04,        /**< CAN-FD bitrate switching is enabled */
    BM_CAN_MESSAGE_FLAGS_FDF = 0x08,        /**< CAN-FD message */
    BM_CAN_MESSAGE_FLAGS_ESI = 0x10,        /**< Reserved for gateways */
    BM_MESSAGE_FLAGS_IDE = BM_CAN_MESSAGE_FLAGS_IDE,         /**< For backward compatibility */
    BM_MESSAGE_FLAGS_RTR = BM_CAN_MESSAGE_FLAGS_RTR,         /**< For backward compatibility */
    BM_MESSAGE_FLAGS_BRS = BM_CAN_MESSAGE_FLAGS_BRS,         /**< For backward compatibility */
    BM_MESSAGE_FLAGS_FDF = BM_CAN_MESSAGE_FLAGS_FDF,         /**< For backward compatibility */
    BM_MESSAGE_FLAGS_ESI = BM_CAN_MESSAGE_FLAGS_ESI,         /**< For backward compatibility */
};

/**
* @enum  BM_LinModeTypeDef
* @brief LIN mode IDs, used by BM_SetLinMode() to change the operation mode of LIN device.
*/

public enum BM_LinModeTypeDef
{
    BM_LIN_OFF_MODE = 0x00,                     /**< The device is logically disconnected from LIN bus */
    BM_LIN_SLAVE_MODE = 0x01,                   /**< The device is running as a slave node (with the capability to transmit and receive LIN messages */
    BM_LIN_INTERNAL_LOOPBACK_MODE = 0x02,       /**< The device is looping back messages internally without impacting the physical LIN bus */
    BM_LIN_LISTEN_ONLY_MODE = 0x03,             /**< The device is receiving messages without impacting the physical LIN bus */
    BM_LIN_MASTER_MODE = 0x08                   /**< The device is running as a master node */
};

/**
 * @enum  BM_RxFilterTypeTypeDef
 * @brief CAN RX filter type IDs, used in BM_RxFilterTypeDef.
 */
public enum BM_RxFilterTypeTypeDef
{
    BM_RXFILTER_INVALID = 0,            /**< Invalid (unused) RX filter entry */
    BM_RXFILTER_BASIC,                  /**< Basic RX filter, traditional acceptance filter based on message ID mask */
    BM_RXFILTER_ADVANCED,               /**< Busmust advanced RX filter, check both message ID and message payload */
    BM_RXFILTER_E2EPASS,                /**< Busmust E2E RX filter, accept only messages that passed E2E checking */
    BM_RXFILTER_E2EFAIL,                /**< Busmust E2E RX filter, accept only messages that failed E2E checking (for debugging purpose) */
};

/**
 * @enum  BM_TxTaskTypeTypeDef
 * @brief CAN TX task type IDs, used in BM_TxTaskTypeDef.
 */
public enum BM_TxTaskTypeTypeDef
{
    BM_TXTASK_INVALID = 0,              /**< Invalid (unused) TX task entry */
    BM_TXTASK_FIXED,                    /**< Basic TX task, send fixed ID and fixed payload */
    BM_TXTASK_INCDATA,                  /**< Self-increment Data TX task */
    BM_TXTASK_INCID,                    /**< Self-increment ID TX task */
    BM_TXTASK_RANDOMDATA,               /**< Random Data TX task */
    BM_TXTASK_RANDOMID,                 /**< Random ID TX task */
};

/**
 * @enum  BM_StatTypeDef
 * @brief CAN runtime statistics item IDs, used in BM_GetStat().
 */
public enum BM_StatTypeDef
{
    BM_STAT_NONE = 0,                   /**< Invalid statistics item */
    BM_STAT_TX_MESSAGE,                 /**< Number of TX messages */
    BM_STAT_RX_MESSAGE,                 /**< Number of RX messages */
    BM_STAT_TX_BYTE,                    /**< Number of TX bytes */
    BM_STAT_RX_BYTE,                    /**< Number of RX bytes */
    BM_STAT_TX_ERROR,                   /**< Number of TX errors */
    BM_STAT_RX_ERROR,                   /**< Number of RX errors */
};

/**
 * @enum  BM_IsotpModeTypeDef
 * @brief ISOTP operation mode, used in BM_IsotpConfigTypeDef.
 */
public enum BM_IsotpModeTypeDef
{
    BM_ISOTP_NORMAL_TESTER = 0,         /**< Default mode: normal (non-extended-addressing) UDS client(tester) */
    BM_ISOTP_NORMAL_ECU,                /**< normal (non-extended-addressing) UDS server(ECU)                  */
    BM_ISOTP_EXTENDED_TESTER,           /**< Currently unsupported: extended-addressing UDS client(tester)     */
    BM_ISOTP_EXTENDED_ECU,              /**< Currently unsupported: extended-addressing UDS server(ECU)        */
};


/**
 * @enum  BM_StorageModeTypeDef
 * @brief Logging or replay mode, used in BM_LoggingConfigTypeDef and BM_ReplayConfigTypeDef.
 */
public enum BM_StorageModeTypeDef
{
    BM_STORAGE_DISABLED = 0,
    BM_STORAGE_ALWAYS_ON,
    BM_STORAGE_TRIGGERED,
};

/**
 * @enum  BM_StorageDirectionTypeDef
 * @brief Logging or replay mode, used in BM_LoggingConfigTypeDef and BM_ReplayConfigTypeDef.
 */
public enum BM_StorageDirectionTypeDef
{
    BM_STORAGE_DIRECTION_NONE = 0,
    BM_STORAGE_DIRECTION_RX,
    BM_STORAGE_DIRECTION_TX,
    BM_STORAGE_DIRECTION_ALL = BM_STORAGE_DIRECTION_RX | BM_STORAGE_DIRECTION_TX,
};

/**
 * @enum  BM_StorageFormatTypeDef
 * @brief Logging or replay mode, used in BM_LoggingConfigTypeDef and BM_ReplayConfigTypeDef.
 */
public enum BM_StorageFormatTypeDef
{
    BM_STORAGE_DEFAULT_FORMAT = 0,
    BM_STORAGE_BBD_FORMAT = BM_STORAGE_DEFAULT_FORMAT,
    BM_STORAGE_PCAP_FORMAT = 1,
    BM_STORAGE_LOG_FORMAT = 2,
    BM_STORAGE_ASC_FORMAT = 3,
    BM_STORAGE_BLF_FORMAT = 4,
};

/**
 * @enum  BM_StoragePathModeTypeDef
 * @brief Logging or replay mode, used in BM_LoggingConfigTypeDef and BM_ReplayConfigTypeDef.
 */
public enum BM_StoragePathModeTypeDef
{
    BM_STORAGE_FIXED_PATH = 0,
    BM_STORAGE_INDEX_PATH,
    BM_STORAGE_TIME_PATH
};

/**
 * @enum  BM_LogLevelTypeDef
 * @brief Busmust library log level, see BM_SetLogLevel() for details.
 */
public enum BM_LogLevelTypeDef
{
    BM_LOG_NONE = 0,             		/**< Show nothing on debug console                                                                                 */
    BM_LOG_ERR,                  		/**< Show only ERR level messages on debug console, note this is the default level for release versions            */
    BM_LOG_WRN,                  		/**< Show ERR and WRN level messages on debug console                                                              */
    BM_LOG_INF,                  		/**< Show ERR|WRN|INF level messages on debug console                                                              */
    BM_LOG_DBG                   		/**< Show all messages on debug console, including debug messages, note this is NOT available for release versions */
};

/**
 * @typedef BM_CanMessageTypeDef
 * @brief   Busmust CAN Message concrete type, usually used as payload of BM_DataTypeDef.
 * @note    The total length of this structure is 72B, it support both classic and FD CAN messages.
 */
[StructLayout(LayoutKind.Sequential)]
public struct BM_CanMessageTypeDef
{
    uint id;                             /**< CAN message ID, see BM_MessageIdTypeDef for details. */
    uint ctrl;                           /**< CAN message control fields, whether TX or RX is taken depends on the message direction. */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public byte[] payload;                      /**< buffer holding concrete message payload (i.e. a CAN message in BM_CanMessageTypeDef format). */
    public BM_CanMessageTypeDef(uint id = 0, uint DLC = 0, bool IDE = false, bool RTR = false, bool BRS = false, bool FDF = false, bool ESI = false)
    {
        if (IDE)
        {
            this.id = ((id >> 18) & 0x7FF) | ((id & 0x3FFFF) << 11);
        }
        else
        {
            this.id = id & 0x7FF;
        }
        this.ctrl = (DLC & 0x0F);
        if (IDE) { this.ctrl |= 1 << 4; }
        if (RTR) { this.ctrl |= 1 << 5; }
        if (BRS) { this.ctrl |= 1 << 6; }
        if (FDF) { this.ctrl |= 1 << 7; }
        if (ESI) { this.ctrl |= 1 << 8; };
        this.payload = new byte[64];
    }

    public uint GetID()
    {
        if (this.IsIDE())
        {
            return ((this.id & 0x7FF) << 18) | ((this.id >> 11) & 0x3FFFF);
        }
        else
        {
            return this.id & 0x7FF;
        }
    }

    public uint GetDLC()
    {
        return (ctrl & 0xF);
    }

    public bool IsIDE()
    {
        return (ctrl & (1 << 4)) != 0;
    }
    public bool IsRTR()
    {
        return (ctrl & (1 << 5)) != 0;
    }
    public bool IsBRS()
    {
        return (ctrl & (1 << 6)) != 0;
    }
    public bool IsFDF()
    {
        return (ctrl & (1 << 7)) != 0;
    }
    public bool IsESI()
    {
        return (ctrl & (1 << 8)) != 0;
    }

    public uint GetLength()
    {
        uint[] DlcToDataBytes = new uint[]
        {
            0,  1,  2,  3,  4,  5,  6,  7,
            8, 12, 16, 20, 24, 32, 48, 64,
        };
        uint length = DlcToDataBytes[GetDLC()];
        return length;
    }

    public uint lengthToDlc(uint n)
    {
        uint dlc = 0;
        if (n <= 8)
        {
            dlc = n;
        }
        else if (n <= 12)
        {
            dlc = 9;
        }
        else if (n <= 16)
        {
            dlc = 10;
        }
        else if (n <= 20)
        {
            dlc = 11;
        }
        else if (n <= 24)
        {
            dlc = 12;
        }
        else if (n <= 32)
        {
            dlc = 13;
        }
        else if (n <= 48)
        {
            dlc = 14;
        }
        else
        {
            dlc = 15;
        }
        return dlc;
    }

    public String GetTypeString()
    {
        String type = "";
        if (IsIDE()) { type += "IDE "; }
        if (IsRTR()) { type += "RTR "; }
        if (IsBRS()) { type += "BRS "; }
        if (IsFDF()) { type += "FDF "; }
        if (IsESI()) { type += "ESI "; }
        if (type.Length == 0)
        {
            type = "STD";
        }
        return type;
    }

    public String GetPyloadString()
    {
        uint length = GetLength();
        String payload = "";
        int i;
        for (i = 0; i < length; i++)
        {
            payload += Convert.ToString(this.payload[i], 16).ToUpper().PadLeft(2, '0') + " ";
        }
        return payload;
    }
};

/**
 * @typedef BM_LinMessageCtrlTypeDef
 * @brief   Busmust LIN Message control fields.
 */
[StructLayout(LayoutKind.Explicit)]
public struct BM_LinMessageCtrlTypeDef
{
    [FieldOffset(0)]
    public uint AllBits;

    // DLC: 4 bits
    // LIN message DLC(0-8), if DLC=0b1111, data length is given by ID[5:4]: 0:2, 1:2, 2:4, 3:8
    public byte DLC
    {
        get => (byte)(AllBits & 0x0F);
        set => AllBits = (AllBits & 0xFFFFFFF0) | (uint)(value & 0x0F);
    }

    // TRANSMIT: 1 bit
    // Transfer direction: 0=receive, 1=transmit
    public bool TRANSMIT
    {
        get => ((AllBits >> 4) & 0x01) == 1;
        set => AllBits = (AllBits & 0xFFFFFFEF) | (uint)(value ? 1 << 4 : 0);
    }

    // WAKEUP: 1 bit
    // This is a wakeup event
    public bool WAKEUP
    {
        get => ((AllBits >> 5) & 0x01) == 1;
        set => AllBits = (AllBits & 0xFFFFFFDF) | (uint)(value ? 1 << 5 : 0);
    }

    // SLEEP: 1 bit
    // This is a sleep event
    public bool SLEEP
    {
        get => ((AllBits >> 6) & 0x01) == 1;
        set => AllBits = (AllBits & 0xFFFFFFBF) | (uint)(value ? 1 << 6 : 0);
    }

    // ENHANCED_CHECKSUM: 1 bit
    // Flag for enhanced checksum
    public bool ENHANCED_CHECKSUM
    {
        get => ((AllBits >> 7) & 0x01) == 1;
        set => AllBits = (AllBits & 0xFFFFFF7F) | (uint)(value ? 1 << 7 : 0);
    }

    // USER_CHECKSUM: 1 bit
    // Flag for user defined checksum, use value from 'CHECKSUM' field if USER_CHECKSUM==1
    public bool USER_CHECKSUM
    {
        get => ((AllBits >> 8) & 0x01) == 1;
        set => AllBits = (AllBits & 0xFFFFFEFF) | (uint)(value ? 1 << 8 : 0);
    }

    // SEQ: 7 bits
    // hardware-sync message ID, the ACK message's SEQ is always equal to the TX message's SEQ or TXTASK index
    public byte SEQ
    {
        get => (byte)((AllBits >> 9) & 0x7F);
        set => AllBits = (AllBits & 0xFFFFF8FF) | (uint)((value & 0x7F) << 9);
    }

    // ISTXTASK: 1 bit
    // A indicator for TXTASK TEF event, SEQ=TXTASK index if ISTXTASK=1
    public bool ISTXTASK
    {
        get => ((AllBits >> 16) & 0x01) == 1;
        set => AllBits = (AllBits & 0xFFFF7FFF) | (uint)(value ? 1 << 16 : 0);
    }

    // ERRORS: 6 bits
    // Error code, which is a bitmask, see BM_LinErrorTypeDef for details.
    public byte ERRORS
    {
        get => (byte)((AllBits >> 17) & 0x3F);
        set => AllBits = (AllBits & 0xFFFFC1FF) | (uint)((value & 0x3F) << 17);
    }

    // ID_PARITY: 1 bit
    // Parity bit
    public bool ID_PARITY
    {
        get => ((AllBits >> 23) & 0x01) == 1;
        set => AllBits = (AllBits & 0xFFEFFFFF) | (uint)(value ? 1 << 23 : 0);
    }

    // CHECKSUM: 8 bits
    // Checksum value
    public byte CHECKSUM
    {
        get => (byte)(AllBits >> 24);
        set => AllBits = (AllBits & 0x00FFFFFF) | (uint)(value << 24);
    }
}

/**
* @typedef BM_LinMessageTypeDef
* @brief   Busmust CAN Message concrete type, usually used as payload of BM_DataTypeDef.
* @note    The total length of this structure is 16B.
*/
[StructLayout(LayoutKind.Sequential)]
public struct BM_LinMessageTypeDef
{

    public byte id;                             /**< LIN message ID */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public byte[] padding;
    public BM_LinMessageCtrlTypeDef ctrl;                           /**< LIN message control fields, invalid if this is NOT a LIN message. */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] payload;               /**< LIN message payload */
    public BM_LinMessageTypeDef(BM_LinMessageCtrlTypeDef ctrl, uint id = 0, uint dlc = 0)
    {
        this.id = (byte)(id & 0x3F);
        this.payload = new byte[8];
        this.padding = new byte[3];
        this.ctrl = ctrl;
    }

    public uint GetID()
    {
        return (byte)(this.id & 0x3F);
    }

    public uint GetDlc()
    {
        return (this.ctrl.DLC);
    }

    public uint GetChecksum()
    {
        return (this.ctrl.CHECKSUM);
    }

    public String GetPyloadString()
    {
        uint length = GetDlc();
        String payload = "";
        int i;
        for (i = 0; i < length; i++)
        {
            payload += Convert.ToString(this.payload[i], 16).ToUpper().PadLeft(2, '0') + " ";
        }
        return payload;
    }
};

/**
 * @typedef BM_DataHeaderTypeDef
 * @brief   Busmust data header, each BM_DataTypeDef contains a header which indicates payload information.
 */
[StructLayout(LayoutKind.Sequential)]
public struct BM_DataHeaderTypeDef
{
    //uint16_t type : 4;                  /**< Data type, see BM_DataTypeTypeDef for details. */
    //uint16_t flags : 4;                 /**< Reserved flags, keep 0 */
    //uint16_t dchn : 4;                  /**< Destination channel ID, starting from zero, used by TX data to indicate the hardware about the target port. */
    //uint16_t schn : 4;                  /**< Source channel ID, starting from zero, used by RX data to indicate the application about the source port. */
    ushort value;

    public BM_DataHeaderTypeDef(BM_DataTypeTypeDef type, ushort flags, ushort dchn, ushort schn)
    {
        this.value = (ushort)(((((ushort)type) & 0xF) << 0) | ((flags & 0xF) << 4) | ((dchn & 0xF) << 8) | ((schn & 0xF) << 12));
    }

    public BM_DataTypeTypeDef GetDataType()
    {
        return (BM_DataTypeTypeDef)(this.value & 0xF);
    }

    public ushort GetFlags()
    {
        return (ushort)((this.value >> 4) & 0xF);
    }

    public ushort GetDstChannel()
    {
        return (ushort)((this.value >> 8) & 0xF);
    }

    public ushort GetSrcChannel()
    {
        return (ushort)((this.value >> 12) & 0xF);
    }

    public bool IsAckData()
    {
        return ((ushort)(((BM_DataTypeTypeDef)(this.value & 0xF)) & BM_DataTypeTypeDef.BM_ACK_DATA)) != 0;
    }

    public void SetAckData()
    {
        this.value |= (ushort)BM_DataTypeTypeDef.BM_ACK_DATA;
    }
};

/**
 * @typedef BM_DataTypeDef
 * @brief   Busmust data, abstract structure which holds concrete payload messages of various types (i.e. CAN messages).
 */
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BM_DataTypeDef
{
    public BM_DataHeaderTypeDef header;             /**< data header, see BM_DataHeaderTypeDef for details. */
    public ushort length;                           /**< length in bytes of the payload only (header excluded). */
    public uint timestamp;                          /**< 32-bit device local high precision timestamp in microseconds. */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)BMAPI.BM_DATA_PAYLOAD_MAX_SIZE)]
    public byte[] payload;                          /**< raw payload holding a concrete message followed by an optional data tail. */

    public BM_CanMessageTypeDef can
    {
        get { return GetCanMessage(); }
        set { SetCanMessage(value); }
    }

    public BM_DataTypeDef(BM_DataHeaderTypeDef header, ushort length, uint timestamp, BM_CanMessageTypeDef canMsg)
    {
        this.header = header;
        this.length = length;
        this.timestamp = timestamp;
        this.payload = new byte[BMAPI.BM_DATA_PAYLOAD_MAX_SIZE];
        SetCanMessage(canMsg);
    }

    public BM_CanMessageTypeDef GetCanMessage()
    {
        BM_CanMessageTypeDef message = new BM_CanMessageTypeDef();
        if (payload == null || payload.Length < Marshal.SizeOf(typeof(BM_CanMessageTypeDef)))
        {
            return message;
        }

        IntPtr buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BM_CanMessageTypeDef)));
        try
        {
            Marshal.Copy(payload, 0, buffer, Marshal.SizeOf(typeof(BM_CanMessageTypeDef)));
            return (BM_CanMessageTypeDef)Marshal.PtrToStructure(buffer, typeof(BM_CanMessageTypeDef));
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public void SetCanMessage(BM_CanMessageTypeDef message)
    {
        if (payload == null || payload.Length != BMAPI.BM_DATA_PAYLOAD_MAX_SIZE)
        {
            payload = new byte[BMAPI.BM_DATA_PAYLOAD_MAX_SIZE];
        }

        IntPtr buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BM_CanMessageTypeDef)));
        try
        {
            Marshal.StructureToPtr(message, buffer, false);
            int messageSize = Marshal.SizeOf(typeof(BM_CanMessageTypeDef));
            int tailSize = (int)BMAPI.BM_DATA_TAIL_SIZE;
            int bytesToCopy = messageSize;
            int tailOffset = -1;
            byte[] tail = null;
            if ((header.GetFlags() & 1) != 0 && length >= tailSize && length <= payload.Length)
            {
                tailOffset = length - tailSize;
                bytesToCopy = Math.Min(messageSize, tailOffset);
                tail = new byte[tailSize];
                Array.Copy(payload, tailOffset, tail, 0, tail.Length);
            }
            Marshal.Copy(buffer, payload, 0, bytesToCopy);
            if (tail != null)
            {
                Array.Copy(tail, 0, payload, tailOffset, tail.Length);
            }
            if (length == 0) { length = (ushort)messageSize; }
        }
        finally
        {
            Marshal.DestroyStructure(buffer, typeof(BM_CanMessageTypeDef));
            Marshal.FreeHGlobal(buffer);
        }
    }
};



[StructLayout(LayoutKind.Sequential)]
public struct BM_IpAddrTypeDef
{
    public byte b1;                           /**< Device address byte1, this is usually a IPV4 address and can be converted to in_addr struct pointer (address in network byte order) */
    public byte b2;                           /**< Device address byte2, this is usually a IPV4 address and can be converted to in_addr struct pointer (address in network byte order) */
    public byte b3;                           /**< Device address byte3, this is usually a IPV4 address and can be converted to in_addr struct pointer (address in network byte order) */
    public byte b4;                           /**< Device address byte4, this is usually a IPV4 address and can be converted to in_addr struct pointer (address in network byte order) */
};

/**
 * @typedef BM_ChannelInfoTypeDef
 * @brief   Channel information, created when enumerating devices by BM_Enumerate() and used when opening device by BM_OpenEx().
 */
[StructLayout(LayoutKind.Sequential)]
public struct BM_ChannelInfoTypeDef
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public byte[] name;                              /**< Device full name, for display purpose */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] sn;                                /**< Device SN */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
    public byte[] uid;                               /**< Device UID */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] version;                           /**< Device Firmware Version */
    public ushort vid;                               /**< Device VID */
    public ushort pid;                               /**< Device PID */
    public ushort port;                              /**< Port ID (0-7) of the device, note a multi-port device is enumerated as multiple dedicated BM_ChannelInfoTypeDef entries */
    public ushort cap;                               /**< Device Capability flags, see BM_CapabilityTypeDef for details. */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] addr;                              /**< Device address, this is usually a IPV4 address and can be converted to in_addr struct pointer (address in network byte order) */

    private String ByteArrayToString(byte[] d)
    {
        String s = System.Text.Encoding.Default.GetString(d);
        int index = s.IndexOf('\0');
        if (index >= 0)
        {
            s = s.Remove(index);
        }
        return s;
    }

    public String GetName()
    {
        return ByteArrayToString(name);
    }

    public String GetSn()
    {
        return ByteArrayToString(sn);
    }

    public String GetUid()
    {
        return ByteArrayToString(uid);
    }
};

/**
 * @typedef BM_CanStatusInfoTypedef
 * @brief   CAN channel status detailed information, retrieved by calling BM_GetCanStatus(), see ISO11898 for details.
 */
[StructLayout(LayoutKind.Sequential)]
struct BM_CanStatusInfoTypedef
{
    public byte TXBO;                               /**< The CAN channel is in BUSOFF state */
    public byte reserved;                           /**< Reserved */
    public byte TXBP;                               /**< The CAN channel is in TX bus passive state */
    public byte RXBP;                               /**< The CAN channel is in RX bus passive state */
    public byte TXWARN;                             /**< The CAN channel is in TX warn state */
    public byte RXWARN;                             /**< The CAN channel is in RX warn state */
    public byte TEC;                                /**< TX Bus Error counter */
    public byte REC;                                /**< RX Bus Error counter */
};

/**
 * @typedef BM_BitrateTypeDef
 * @brief   CAN channel bitrate configuration, used by BM_SetBitrate().
 */
[StructLayout(LayoutKind.Sequential)]
public struct BM_BitrateTypeDef
{
    public ushort nbitrate;                          /**< Nominal bitrate in kbps, default as 500, note this is the only valid birate in CAN CLASSIC mode. */
    public ushort dbitrate;                          /**< Data bitrate in kbps, default as 500, note this is ignored in CAN CLASSIC mode. */
    public byte nsamplepos;                          /**< Nominal sample position (percentage), 0-100, default as 75 */
    public byte dsamplepos;                          /**< Data sample position (percentage), 0-100, default as 75 */
    public byte clockfreq;                           /**< CAN controller clock in Mhz, default as 0 */
    public byte reserved;                            /**< Reserved */
    public byte nbtr0;                               /**< Nominal BTR0 register value, note this value is calculated using clockfreq, which might not be 16MHz */
    public byte nbtr1;                               /**< Nominal BTR1 register value, note this value is calculated using clockfreq, which might not be 16MHz */
    public byte dbtr0;                               /**< Data BTR0 register value, note this value is calculated using clockfreq, which might not be 16MHz */
    public byte dbtr1;                               /**< Data BTR1 register value, note this value is calculated using clockfreq, which might not be 16MHz */
};

/**
 * @typedef BM_RxFilterTypeDef
 * @brief   CAN channel RX filter item structure, used by BM_SetRxFilter().
 * @note    The filter support masking ID, flags and payload according to its type, 
 *          in order for a message to be accepted, all the fields are masked using AND logic:
 *          (flags & filter.flags_mask == filter.flags_value) AND (ID & filter.id_mask == filter.id_value) AND (payload & filter.payload_mask == filter.payload_value)
 */
[StructLayout(LayoutKind.Sequential)]
public struct BM_RxFilterTypeDef
{
    public byte type;                              /**< Type ID of the RX filter, see BM_RxFilterTypeTypeDef for details. */
    public byte unused;                            /**< Reserved */
    public byte flags_mask;                        /**< CAN message control Flags masks, see BM_MessageFlagsTypeDef for details. */
    public byte flags_value;                       /**< CAN message control Flags values, see BM_MessageFlagsTypeDef for details. */
    public uint reserved;                          /**< Reserved */
    uint id_mask;                           /**< CAN message ID masks, see BM_MessageIdTypeDef for details. */
    uint id_value;                          /**< CAN message ID values, see BM_MessageIdTypeDef for details. */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] payload_mask;                    /**< CAN message payload masks, for CAN-FD messages, only the first 8 bytes are checked. */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] payload_value;                   /**< CAN message payload values, for CAN-FD messages, only the first 8 bytes are checked. */

    public void SetIdFilter(uint mask, uint value, bool ide)
    {
        if ((mask > 0x7FF || value > 0x7FF) || ide)
        {
            this.id_mask = ((mask >> 18) & 0x7FF) | ((mask & 0x3FFFF) << 11);
            this.id_value = ((value >> 18) & 0x7FF) | ((value & 0x3FFFF) << 11);
        }
        else
        {
            this.id_mask = mask & 0x7FF;
            this.id_value = value & 0x7FF;
        }
    }
};
[StructLayout(LayoutKind.Explicit)]
public struct BM_TxTaskIdTypeDef
{
    [FieldOffset(0)]
    public uint id;

    [FieldOffset(0)]
    public CanId can;

    [FieldOffset(0)]
    public LinId lin;
}

[StructLayout(LayoutKind.Explicit)]
public struct CanId
{
    [FieldOffset(0)]
    [MarshalAs(UnmanagedType.U4)]
    public uint Value;

    [FieldOffset(0)]
    [MarshalAs(UnmanagedType.U2)]
    public ushort SID; // 11 bits

    [FieldOffset(2)]
    [MarshalAs(UnmanagedType.U2)]
    public ushort EID; // 18 bits, but stored in 16 bits here for simplicity, adjust as needed

    [FieldOffset(3)]
    [MarshalAs(UnmanagedType.U1)]
    public byte SID11AndUnimplemented; // 1 bit + 2 bits

    // Helper properties to access individual bits
    public bool SID11 => (SID11AndUnimplemented & 0x01) != 0;
    public byte Unimplemented1 => (byte)((SID11AndUnimplemented >> 1) & 0x03);
}

[StructLayout(LayoutKind.Sequential)]
public struct LinId
{
    public byte ID;
    public byte CHECKSUM;
    public ushort reserved;
}

/**
 * @typedef BM_TxTaskTypeDef
 * @brief   CAN channel TX task item structure, used by BM_SetTxTask().
 * @note    Once the CAN device is armed with TX tasks, it will try to parse the TX task and send CAN messages automatically.
 *          The difference with a software triggered CAN message in BusMaster is that 
 *          hardware triggered CAN messages are more precise in time and could reach a higher throughput.
 */
[StructLayout(LayoutKind.Sequential)]
public struct BM_TxTaskTypeDef
{
    public byte type;                                /**< Type ID of the TX task, see BM_TxTaskTypeTypeDef for details. */
    public byte version;                             /**< Version of BM_TxTaskTypeDef, set to 1 for BMAPI1.x. */
    public byte flags;                               /**< CAN message control Flags, see BM_MessageFlagsTypeDef for details. */
    public byte length;                              /**< LSB of Length of payload in bytes (not DLC) */
    public byte length_msb;                          /**< MSB of Length of payload in bytes (not DLC), set to 0 if not used (e.g. for CANFD messages) */
    public byte reserved;                            /**< Reserved */

    public ushort cycle;                             /**< ms delay between rounds */
    public ushort nrounds;                           /**< num of cycles */
    public ushort nmessages;                         /**< messages per round */


    public BM_TxTaskIdTypeDef id;                                  /**< CAN message arbitration id, see BM_MessageIdTypeDef for details. */

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public byte[] pattern;                                  /**< TX task pattern data, see C++ header files for details */

    public byte e2e;                                 /**< Index of E2E (in E2E table), currently unsupported */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
    public byte[] e2e_reserved;                                  /**< Reserved for future (e.g. E2E information) */

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public byte[] payload;                           /**< Default payload data, note this is also the template payload of the unchanged part in a volatile TX task */
};

/**
 * @typedef BM_IsotpStatusTypeDef
 * @brief   ISOTP status report, used by ISOTP operation callback function.
 */
public struct BM_IsotpStatusTypeDef
{
    public byte version;                           /**< Currently always 0x01 */
    public byte flowcontrol;                       /**< Current flow control status, 0=continue, 1=wait, 2=overflow, ff=timeout, */
    public byte stmin;                             /**< Current flow control status, i.e. 30 00 00 */
    public byte blocksize;                         /**< Current flow control status, i.e. 30 00 00 */
    public uint ntransferredbytes;                 /**< Number of transferred bytes by now. */
    public uint ntotalbytes;                       /**< Number of total bytes indicated by ISOTP FF or SF. */
    public uint timestamp;                         /**< Current timestamp reported by device. */
    public uint reserved1;                         /**< Reserved for future */
    public uint reserved2;                         /**< Reserved for future */
    public uint reserved3;                         /**< Reserved for future */
    public uint reserved4;                         /**< Reserved for future */
};

/**
 * @typedef BM_IsotpTimeoutConfigTypeDef
 * @brief   ISOTP Protocol (See ISO15765-2 for details) timeout configuration, used by BM_ConfigIsotp().
 */
[StructLayout(LayoutKind.Sequential)]
public struct BM_IsotpTimeoutConfigTypeDef
{
    public ushort a;                             /**< A timeout in milliseconds: =N_As if writing as tester or reading as ECU, otherwise =N_Ar */
    public ushort b;                             /**< B timeout in milliseconds: =N_Bs if writing as tester or reading as ECU, otherwise =N_Br */
    public ushort c;                             /**< C timeout in milliseconds: =N_Cs if writing as tester or reading as ECU, otherwise =N_Cr */
}

/**
 * @typedef BM_IsotpFlowcontrolConfigTypeDef
 * @brief   ISOTP Protocol (See ISO15765-2 for details) flowcontrol configuration, used by BM_ConfigIsotp().
 */
[StructLayout(LayoutKind.Sequential)]
public struct BM_IsotpFlowcontrolConfigTypeDef
{
    public byte stmin;                           /**< STmin raw value (0x00-0x7F or 0xF1-0xF9) if Busmust device is acting as UDS server       */
    public byte blocksize;                       /**< Blocksize if can card is acting as UDS server, 0 means no further FC is needed           */
    public byte fcFrameLength;                   /**< Flow control frame length in bytes                                                       */
    public byte reserved;
}

/**
 * @typedef BM_IsotpConfigTypeDef
 * @brief   ISOTP Protocol (See ISO15765-2 for details) configuration, used by BM_ConfigIsotp().
 */
[StructLayout(LayoutKind.Sequential)]
public struct BM_IsotpConfigTypeDef
{
    public byte version;                                /**< Currently must be set to 0x01                                                            */
    public byte mode;                                   /**< Currently only 0 is supported: normal (non-extended-addressing) UDS client(tester)       */
    public BM_IsotpTimeoutConfigTypeDef testerTimeout;  /**< Tester ISOTP timeout configuration                                                       */
    public BM_IsotpTimeoutConfigTypeDef ecuTimeout;     /**< ECU ISOTP timeout configuration                                                          */
    public BM_IsotpFlowcontrolConfigTypeDef flowcontrol;/**< ISOTP Flowcontrol configuration                                                          */
    public byte extendedAddress;                        /**< UDS Address in Extended Addressing mode                                                  */
    public byte paddingEnabled;                         /**< Enable padding for unused payload bytes/                                                 */
    public byte paddingValue;                           /**< Padding byte value (i.e. 0xCC) for unused payload bytes                                  */
    public byte longPduEnabled;                         /**< Enable long PDU (>4095), note if CAN message DLC>8, long PDU is enabled by default       */
    public ushort padding;
    public IntPtr callbackFunc;                         /**< TODO: Callback function when any progress is made, used typically by GUI to show progress bar  */
    public IntPtr callbackUserarg;                      /**< TODO: Callback userarg when any progress is made, used typically by GUI to show progress bar   */
    public BM_DataTypeDef testerDataTemplate;           /**< All tester messages will be formatted/checked using this template, configure CAN message ID and IDE/FDF flags here  */
    public BM_DataTypeDef ecuDataTemplate;              /**< All ECU messages will be formatted/checked using this template, configure CAN message ID and IDE/FDF flags here */
};

/**
 * @typedef BM_PathSpecTypeDef
 * @brief   File Path spec (on data files), used by BM_LoggingConfigTypeDef and BM_ReplayConfigTypeDef.
 */
[StructLayout(LayoutKind.Sequential)]
public struct BM_PathSpecTypeDef
{
    public byte mode;                           /**< Naming mode, see BM_StoragePathModeTypeDef for details                                   */
    public byte arg;                            /**< Mode argument, reserved for future                                                       */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
    public char[] format;                       /**< A printf-like string, used by path mode parser                                           */
};

/**
 * @typedef BM_EventTriggerTypeDef
 * @brief   Event trigger (on rx messages), used by BM_LoggingConfigTypeDef and BM_ReplayConfigTypeDef.
 */
[StructLayout(LayoutKind.Sequential)]
public struct BM_EventTriggerTypeDef
{
    public ushort channels;                       /**< A Bitmask of RX channels, only channels with '1' are allowed, channels=0 means invalid   */
    public ushort reserved;
    public ushort flags_mask;                     /**< Source message flag mask, message will be routed if msg.flags & flagsmask == flagsvalue  */
    public ushort flags_value;                    /**< Source message flag value, See BM_MessageFlagsTypeDef for details                        */
    public uint id_mask;                          /**< Source message ID mask, message will be routed if msg.id & idmask == idvalue             */
    public uint id_value;                         /**< Source message ID value, See BM_MessageIdTypeDef for details                             */
};

[StructLayout(LayoutKind.Sequential)]
public struct BM_LoggingSegmentationTypeDef
{
    /**< For FIXED PATH mode: Remove all data in existing logging file on logging startup/trigger, otherwise append to existing file      */
    /**< For INDEX PATH mode: Increase file index and create new file on logging startup/trigger, otherwise continue using existing file  */
    public byte createNewFileOnStart;
    public byte overwriteOldFileOnFull;        /**< Enable overwriting, e.g. Overwrite 000.bbd if failed to create 003.bbd since disk is full*/
    public ushort nfiles;                      /**< Max number of files, file name will wrap around on upper limit                           */
    public uint nmessagesPerFile;              /**< Max number of messages per log file (currently unsupported)                              */
    public uint nbytesPerFile;                 /**< Max number of bytes per log file                                                         */
    public uint nsecondsPerFile;               /**< Max number of seconds per log file                                                       */
};

[StructLayout(LayoutKind.Sequential)]
public struct BM_ReplayTimingTypeDef
{
    public ushort msgdelay;                      /**< Delay in ms between replay messages, 0 means keeping original delay                    */
    public ushort sessiondelay;                  /**< Delay in ms between replay sessions, 0 means keeping original delay                    */
    public ushort cycledelay;                    /**< Delay in ms between replay cycles, 0 means keeping original delay                      */
    public byte forceZeroTimestampOnFirstMsg;    /**< Align 1st replayable message with zero timestamp, and send it immediately on startup   */
    public byte reserved;
};

/**
 * @typedef BM_LoggingConfigTypeDef
 * @brief   Logging configuration, used by BM_ConfigLogging().
 */
[StructLayout(LayoutKind.Sequential)]
public struct BM_LoggingConfigTypeDef
{
    public byte version;                                /**< Currently must be set to 0x01                                                            */
    public byte mode;                                   /**< Logging mode, see BM_StorageModeTypeDef for details                                      */
    public byte format;                                 /**< Log file format: See BM_StorageFormatTypeDef for details.                                */
    public byte reserved;
    public ushort channels;                             /**< A bitmask of TX channels, only channels with '1' are allowed as logging source channel   */
    public byte direction;                              /**< Logging direction, see BM_StorageDirectionTypeDef for details                            */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
    byte[] padding;
    public BM_PathSpecTypeDef path;                     /**< Pathspec to find available files, see BM_StoragePathModeTypeDef for details.             */
    public BM_EventTriggerTypeDef starttrigger;         /**< Trigger condition, recording starts automatically when it is met, only valid if mode=2   */
    public BM_EventTriggerTypeDef stoptrigger;          /**< Trigger condition, recording starts automatically when it is met, only valid if mode=2   */
    public BM_LoggingSegmentationTypeDef segmentation;  /**< Log File segmentation criteria, OR logic                                                 */
};

/**
 * @typedef BM_ReplayConfigTypeDef
 * @brief   Replay configuration, used by BM_ConfigReplay().
 */
[StructLayout(LayoutKind.Sequential)]
public struct BM_ReplayConfigTypeDef
{
    public byte version;                                /**< Currently must be set to 0x01                                                          */
    public byte mode;                                   /**< Replay mode, see BM_StorageModeTypeDef for details                                     */
    public byte format;                                 /**< Replay file format: See BM_StorageFormatTypeDef for details.                           */
    public byte reserved;
    public ushort channels;                             /**< A bitmask of TX channels, only channels with '1' are allowed as logging source channel */
    public byte direction;                              /**< Replay direction, see BM_StorageDirectionTypeDef for details                           */
    public byte cyclic;                                 /**< Replay is cyclic, otherwise the device only replay input file once on startup          */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    byte[] padding;
    public BM_PathSpecTypeDef path;                     /**< Pathspec to find available files, see BM_StoragePathModeTypeDef for details.           */
    public BM_EventTriggerTypeDef starttrigger;         /**< Trigger condition, replay starts automatically when it is met, only valid if mode=2    */
    public BM_EventTriggerTypeDef stoptrigger;          /**< Trigger condition, replay stops automatically when it is met, only valid if mode=2     */
    public BM_ReplayTimingTypeDef timing;               /* Replay timing */
};


/**
 * @enum  BM_PtpModeTypeDef
 * @brief PTP timestamp synchronization mode values, used by BM_SetPtpMode() to change the synchronization mode for a given hardware channel.
 */
public enum BM_PtpModeTypeDef
{
    BM_PTP_DISABLED = 0,       /**< Disable PTP feature, work asynchronously using hardware local timestamp                 */
    BM_PTP_INPUT_USB_SOF = 1,  /**< Use host computer's USB SOF packet to sync with host computer                           */
    BM_PTP_INPUT_PPS = 2,      /**< PTP slave: Use dedicated PPS input pin (if supported by hardware) as sync pulse input   */
    BM_PTP_OUTPUT_PPS = 4,     /**< PTP master: Use dedicated PPS input pin (if supported by hardware) as sync pulse output */
};

public enum BM_AutosetModeTypeDef
{
    BM_AUTOSET_PASSIVE_LISTEN = 0,     /**< Listen-only detection: do not transmit or ACK CAN frames. */
    BM_AUTOSET_ACTIVE_DETECT = 1,      /**< Active detection: send probe frames in normal mode and require TX ACK. */
};

[StructLayout(LayoutKind.Sequential)]
public struct BM_AutosetProbeMessageTypeDef
{
    public uint id;
    public uint ctrl;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public byte[] payload;
};

[StructLayout(LayoutKind.Sequential)]
public struct BM_AutosetConfigTypeDef
{
    public byte version;                              /**< Currently must be set to 0x01. */
    public byte mode;                                 /**< See BM_AutosetModeTypeDef for details. */
    public byte minLockMessages;                      /**< Required RX or TX-ACK confirmations, values below 3 use the default minimum of 3. */
    public byte reserved0;                            /**< Reserved, set to 0. */
    public ushort passiveObservationMs;               /**< Passive listen time per candidate, default is selected internally when set to 0. */
    public ushort activeWriteTimeoutMs;               /**< Active probe write timeout in milliseconds, default is 50 when set to 0. */
    public BM_AutosetProbeMessageTypeDef probeMessage;/**< Active probe CAN template; ID and payload are used, FD/BRS/DLC are adjusted per candidate. */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public uint[] reserved;                           /**< Reserved, set to 0. */
};

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate uint BM_AutosetCallbackHandle(
    ref BM_BitrateTypeDef bitrate,
    BM_TerminalResistorTypeDef tres,
    int nrxmessages,
    IntPtr userarg
);

/**
 * @enum  BM_SleepStatusTypeDef
 * @brief Sleep status codes, used by BM_SetSleepStatus() to change the sleep status of a physical channel.
 */
public enum BM_SleepStatusTypeDef
{
    BM_WAKEUP = 0,                    /**< Channel is awake */
    BM_SLEEP = 1,                     /**< Channel is sleeping */
};

/**
 * @enum  BM_BufferTypeDef
 * @brief Buffer direction flag, used by BM_SetBuffer() to select the read or write side.
 */
public enum BM_BufferTypeDef
{
    BM_WRITE_BUFFER = 0,              /**< Write buffer */
    BM_READ_BUFFER = 0x8000           /**< Read buffer */
};

/**
 * @enum  BM_BufferId
 * @brief Buffer identifiers, used by BM_SetBuffer() to switch the internal buffer target.
 */
public enum BM_BufferId
{
    BM_DEFAULT_BUFFER = 0x0000,       /**< Default buffer: txq for write, rxq for read */
    BM_LOGGINGFILE_BUFFER = 0x0001,   /**< Buffer for logging file operations */
    BM_REPLAYFILE_BUFFER = 0x0002,    /**< Buffer for replay file operations */
    BM_LOGGINGQ_BUFFER = 0x0111,      /**< Buffer for logging queue */
    BM_REPLAYQ_BUFFER = 0x0222,       /**< Buffer for replay queue */
    BM_RXQ_BUFFER = 0x1111,           /**< RX queue buffer (compatible with BM_ClearBuffer) */
    BM_TXQ_BUFFER = 0x2222,           /**< TX queue buffer (compatible with BM_ClearBuffer) */
    BM_NO_BUFFER = 0xFFFF             /**< No buffer: direct physical channel access for low latency */
};

/** Owner category reported for a remote control session. */
public enum BM_RemoteOwnerTypeDef
{
    BM_REMOTE_OWNER_NONE = 0,
    BM_REMOTE_OWNER_LOCAL = 1,
    BM_REMOTE_OWNER_REMOTE = 2
};

/** Current state of a remote control session. */
public enum BM_RemoteSessionStateTypeDef
{
    BM_REMOTE_SESSION_UNKNOWN = 0,
    BM_REMOTE_SESSION_AVAILABLE = 1,
    BM_REMOTE_SESSION_CLAIMED = 2,
    BM_REMOTE_SESSION_EXPIRED = 3,
    BM_REMOTE_SESSION_FAULTED = 4
};

/** Capabilities advertised by a remote control-session endpoint. */
public enum BM_RemoteSessionCapabilityTypeDef
{
    BM_REMOTE_SESSION_CAP_NONE = 0,
    BM_REMOTE_SESSION_CAP_CLAIM = 0x01,
    BM_REMOTE_SESSION_CAP_HEARTBEAT = 0x02,
    BM_REMOTE_SESSION_CAP_CHANNEL_RELEASE = 0x04,
    BM_REMOTE_SESSION_CAP_CONTROL_STATUS = 0x08,
    BM_REMOTE_SESSION_CAP_TX_SEQ_TRANSLATION = 0x10
};

/** Protocol-level result reported for a remote control-session operation. */
public enum BM_RemoteSessionResultTypeDef
{
    BM_REMOTE_SESSION_RESULT_OK = 0,
    BM_REMOTE_SESSION_RESULT_UNSUPPORTED = 1,
    BM_REMOTE_SESSION_RESULT_BUSY = 2,
    BM_REMOTE_SESSION_RESULT_STALE = 3,
    BM_REMOTE_SESSION_RESULT_DENIED = 4,
    BM_REMOTE_SESSION_RESULT_INVALID = 5,
    BM_REMOTE_SESSION_RESULT_INTERNAL = 6
};

/**
 * @typedef BM_LinStatusInfoTypedef
 * @brief   LIN channel status detailed information, retrieved by calling BM_GetLinStatus().
 */
[StructLayout(LayoutKind.Sequential)]
public struct BM_LinStatusInfoTypeDef
{
    public byte ACTIVE;                             /**< The LIN channel is in ACTIVE state */
    public byte SLEEP;                              /**< The LIN channel is in SLEEP state */
    public byte ERRORS;                             /**< Error status bitmask for the LIN channel, see BM_LinErrorTypeDef for details */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    public byte[] reserved;                         /**< Reserved */
};

/**
 * @typedef BM_LinProtocolTimingTypeDef
 * @brief   LIN protocol timing configuration, used in BM_LinProtocolConfigTypeDef.
 */
[StructLayout(LayoutKind.Sequential)]
public struct BM_LinProtocolTimingTypeDef
{
    public byte wakeLength;                         /**< 0:250us, 1:500us, 2:750us, ..., 7:2000us */
    public byte breakLength;                        /**< 0:13bit, 1:15bit, 2:17bit, ..., 7:27bit */
    public byte busInactiveTime;                    /**< 0:4s, 1:6s, 2:8s, 3:10s */
    public byte wakeupRepeatTime;                   /**< 0:180ms, 1:200ms, 2:220ms, 3:240ms */
};

/**
 * @typedef BM_LinProtocolConfigTypeDef
 * @brief   LIN Protocol configuration, used by BM_SetLinProtocol() and BM_GetLinProtocol().
 */
[StructLayout(LayoutKind.Sequential)]
public struct BM_LinProtocolConfigTypeDef
{
    public byte version;                            /**< 0x13=LIN1.3, 0x20=LIN2.0, 0x21=LIN2.1, 0x22=LIN2.2, see BM_LinProtocolVersionTypeDef for details */
    public byte checksum;                           /**< 0x00=manual, 0x01=normal, 0x02=enhanced, see BM_LinProtocolChecksumTypeDef for details */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public byte[] reserved;                         /**< Reserved */
    public BM_LinProtocolTimingTypeDef timing;      /**< LIN timing configuration */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] reserved2;                        /**< Reserved */
};

/**
 * @typedef BM_MessageRouteTypeDef
 * @brief   Message route item structure, used by BM_SetMsgRoutes() and BM_GetMsgRoutes().
 */
[StructLayout(LayoutKind.Sequential)]
public struct BM_MessageRouteTypeDef
{
    public byte type;                               /**< 0 = invalid, 1 = unicast, 2 = broadcast */
    public byte source;                             /**< Source channel index (0-15) */
    public ushort target;                           /**< Target channel: index if type==1, bitmask of target channels if type==2 */
    public ushort reserved;                         /**< Reserved for future */
    public byte flagsmask;                          /**< Source message flag mask, message will be routed if msg.flags & flagsmask == flagsvalue */
    public byte flagsvalue;                         /**< Source message flag value, see BM_MessageFlagsTypeDef for details */
    public uint idmask;                             /**< Source message ID mask, message will be routed if msg.id & idmask == idvalue */
    public uint idvalue;                            /**< Source message ID value, see BM_MessageIdTypeDef for details */
};

/**
 * @typedef BM_RemoteSessionConfigTypeDef
 * @brief   Configuration used when claiming a remote control session via BM_ClaimRemoteSession().
 */
[StructLayout(LayoutKind.Sequential)]
public struct BM_RemoteSessionConfigTypeDef
{
    public uint size;                               /**< Set to (uint)Marshal.SizeOf(typeof(BM_RemoteSessionConfigTypeDef)). */
    public uint version;                            /**< Set to BM_REMOTE_SESSION_ABI_VERSION. */
    public uint timeout_ms;                         /**< Control-lock lease, 0 for the 10000 ms default. */
    public uint heartbeat_interval_ms;              /**< 0 for the 2000 ms default; at most half the lease. */
    public uint flags;                              /**< Must be zero in ABI version 1. */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public byte[] client_name;                      /**< Optional UTF-8 display name; NUL terminated. */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 44)]
    public byte[] reserved;
};

/**
 * @typedef BM_RemoteSessionInfoTypeDef
 * @brief   Status and ownership information for a remote control session.
 */
[StructLayout(LayoutKind.Sequential)]
public struct BM_RemoteSessionInfoTypeDef
{
    public uint size;
    public uint version;
    public uint capabilities;                       /**< Bitmask of BM_RemoteSessionCapabilityTypeDef values. */
    public uint state;                              /**< See BM_RemoteSessionStateTypeDef for details. */
    public uint result;                             /**< See BM_RemoteSessionResultTypeDef for details. */
    public uint timeout_ms;
    public uint expires_in_ms;
    public uint config_generation;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] session_id;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] owner_ipv4;
    public byte owner_kind;                         /**< See BM_RemoteOwnerTypeDef for details. */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public byte[] reserved0;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public byte[] owner_name;                       /**< UTF-8 display name; NUL terminated. */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public byte[] reserved;
};


class BMAPI
{
    public const uint BM_DATA_TAIL_SIZE = 16;   /**< Optional 16-byte data tail appended after the CAN message payload, see bm_usb_def.h. */
    public const int BM_DATA_PAYLOAD_MAX_SIZE = 72 + (int)BM_DATA_TAIL_SIZE;
    /**
     * @brief  Initialize BMAPI library, this function shall be called before any other API calls and shall only be called once.
     * @return Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_Init();

    /**
     * @brief  Un-initialize BMAPI library, this function shall be called after any other API calls and shall only be called once.
     * @return Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_UnInit();

    /**
     * @brief        Enumerate all connected Busmust device.
     * @param[out]   channelinfos  An array of BM_ChannelInfoTypeDef structure which holds info of all the enumerated Busmust devices.
     * @param[inout] nchannels     Number of device channels available, which is also the number of valid entries in channelinfos, 
     *                             this param must be initialized with the maximum length of the channelinfos array when calling this function.
     * @return       Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_Enumerate(IntPtr channelinfos, ref int nchannels);

    /**
    * @brief        Enumerate all connected Busmust device.
    * @param[out]   channelinfos  An array of BM_ChannelInfoTypeDef structure which holds info of all the enumerated Busmust devices.
    * @param[inout] nchannels     Number of device channels available, which is also the number of valid entries in channelinfos,
    *                             this param must be initialized with the maximum length of the channelinfos array when calling this function.
    * @param[in]    cap           A bitmask of channel capability (see BM_CapabilityTypeDef for details).
    *                             e.g. Call BM_EnumerateByCap(channelinfos, nchannels, BM_CAN_FD_CAP) to find all available CANFD channels.
    * @return       Operation status, see BM_StatusTypeDef for details.
    */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_EnumerateByCap(IntPtr channelinfos, ref int nchannels, ushort cap);

    /**
     * @brief        Enumerate all remote Busmust devices.
     * @param[out]   channelinfos  An array of BM_ChannelInfoTypeDef structure which holds info of all the enumerated Busmust devices.
     * @param[inout] nchannels     Number of device channels available, which is also the number of valid entries in channelinfos,
     *                             this param must be initialized with the maximum length of the channelinfos array when calling this function.
     * @param[in]    ipv4          The IPV4 unicast/broadcast address in which to find remote devices, in network byte order.
     *                             e.g. ipv4 = {192, 168, 0, 255} indicates broadcasting within 192.168.0.x 
     *                             e.g. ipv4 = {192, 168, 0, 100} indicates enumerating 192.168.0.100 only
     * @param[in]    timeout       Timeout (in milliseconds) before waiting response from remote devices.
     * @return       Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_EnumerateRemote(IntPtr channelinfos, ref int nchannels, ref BM_IpAddrTypeDef addr, int timeout);

    /**
     * @brief Open the specified CAN device port.
     * @param[in] port  Index of the port, starting from zero, note this is the index of all enumerated ports.
     * @return Handle to the opened CAN device channel, return NULL if failed to open the specified port.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr BM_OpenCan(int index);

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
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_OpenEx(
        ref IntPtr handle,
        ref BM_ChannelInfoTypeDef channelinfo,
        BM_CanModeTypeDef mode,
        BM_TerminalResistorTypeDef tres,
        ref BM_BitrateTypeDef bitrate,
        IntPtr rxfilter,
        int nrxfilters
    );

    /**
     * @brief     Close an opened channel.
     * @param[in] handle  Handle to the channel to be closed.
     * @return    Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_Close(IntPtr handle);

    /**
     * @brief     Reset an opened channel.
     * @param[in] handle  Handle to the channel to be reset.
     * @return    Operation status, see BM_StatusTypeDef for details.
     * @note      The configuration options will not lost when the channel is reset, so BM_Reset() is basically identical to BM_Close() and then BM_OpenEx().
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_Reset(IntPtr handle);

    /**
	 * @brief     Reset the device hardware which an opened channel belongs to, in case the device hardware is in unknown error state and is unrecoverable using BM_OpenEx.
	 * @param[in] handle  Handle to a opened channel (which belongs to the physical device to reset).
	 * @return    Operation status, see BM_StatusTypeDef for details.
	 * @note      !!!CAUTION!!! Note this API will break all active USB/Ethernet connection, just like you have manually unplugged it and then plugged it back.
	 *            All opened channel handles that belongs to the physical device under reset will be invalid after reset.
	 *            You MUST re-open and re-configure all channels using BM_OpenEx or other configuration APIs after reset.
	 */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_ResetDevice(IntPtr handle);

    /**
     * @brief     Activate an opened channel, and thus goes on bus for the selected port and channels. 
                  At this point, the user can transmit and receive messages on the bus.
     * @param[in] handle  Handle to the channel to be activated.
     * @return    Operation status, see BM_StatusTypeDef for details.
     * @note      Channel is default to be activated after BM_OpenEx() is called.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_Activate(IntPtr handle);

    /**
     * @brief     Deactivate an opened channel, and thus the selected channels goes off the bus and stay in BUSOFF state until re-activation.
     * @param[in] handle  Handle to the channel to be deactivated.
     * @return    Operation status, see BM_StatusTypeDef for details.
     * @note      Any call to BM_Write() or BM_Read() will return BM_ERROR_BUSOFF immediately if the channel is deactivated.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_Deactivate(IntPtr handle);

    /**
     * @brief     Clear TX&RX message buffer of an opened channel.
     * @param[in] handle  Handle to the channel to be cleared.
     * @return    Operation status, see BM_StatusTypeDef for details.
     * @note      This function is available since BMAPI1.3, hardware status will not be changed when clearing buffer.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_ClearBuffer(IntPtr handle);

    /**
     * @brief      Read any message/event out of the given channel.
     * @param[in]  handle  Handle to the channel to read from.
     * @param[out] data    A caller-allocated buffer to hold the message/event output, see BM_DataTypeDef for details.
     * @return     Operation status, see BM_StatusTypeDef for details.
     * @note       This function is non-blocked, and thus will return BM_ERROR_QRCVEMPTY if no message is received.
     *             Please use notifications to wait for Rx events and then read message/event out of BMAPI internal RX buffer, otherwise you could also poll the device periodically.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_Read(IntPtr handle, ref BM_DataTypeDef data);

    /**
     * @brief        Read multiple messages/events out of the given channel.
     * @param[in]    handle  Handle to the channel to read from.
     * @param[out]   data       A caller-allocated buffer to hold the messages/events array output, see BM_DataTypeDef for details.
     * @param[inout] nmessages  Number of read messages, user shall initialize this param with the size (in messages) of the data buffer.
     * @param[in]    timeout    Timeout (in milliseconds) before the message is received successfully from the bus.
     *                          Set any negative number (i.e. -1) to wait infinitely.
     *                          Set 0 if you would like to receive asynchronously: read from BMAPI internal buffer and return immediately, use BM_WaitForNotifications() before reading.
     * @return       Operation status, see BM_StatusTypeDef for details.
     * @note         This function is non-blocked, and thus will return BM_ERROR_QRCVEMPTY if not all messages are received.
     *               Please use notifications to wait for Rx events and then read message/event out of BMAPI internal RX buffer, otherwise you could also poll the device periodically.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_ReadMultiple(IntPtr handle, IntPtr data, ref uint nmessages, int timeout);

    /**
     * @brief        Read data block using ISOTP protocol.
     *               This API enables rapid transmission using ISOTP without app intervention, a simple example would be reading VIN using UDS:
     *               uint8_t request[] = { 0x22, 0xF1, 0x80 };
     *               uint8_t response[4096];
     *               uint32_t nbytes = sizeof(response);
     *               BM_WriteIsotp(channel, request, sizeof(request), config);
     *               BM_ReadIsotp(channel, response, nbytes, config);
     *               assert(response[0] == 0x62 && response[1] == 0xF1 && response[2] == 0x80);
     * @param[in]    handle    Handle to the channel to read from.
     * @param[in]    data      A caller-allocated buffer to hold the data block output.
     * @param[inout] nbytes    Length of the received data block, in bytes. Caller must initialize this arg with the size of the caller-allocated buffer.
     * @param[in]    timeout   Timeout (in milliseconds) before the message is received successfully from the bus.
     *                         Set any negative number (i.e. -1) to wait infinitely.
     *                         Set 0 if you would like to receive asynchronously: read from BMAPI internal buffer and return immediately, use BM_WaitForNotifications() before reading.
     * @param[in]    config    ISOTP configuration used by current transfer.
     * @return     Operation status, see BM_StatusTypeDef for details.
     * @note       This function is allowed to be called from multiple threads since BMAPI1.5.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl, EntryPoint = "BM_ReadIsotp")]
    public static extern BM_StatusTypeDef BM_ReadIsotpImpl(IntPtr handle, IntPtr data, ref uint nbytes, int timeout, ref BM_IsotpConfigTypeDef config);

    public static BM_StatusTypeDef BM_ReadIsotp(IntPtr handle, ref byte[] payload, ref uint nbytes, int timeout, ref BM_IsotpConfigTypeDef config)
    {
        IntPtr infomem = Marshal.UnsafeAddrOfPinnedArrayElement(payload, 0);
        BM_StatusTypeDef status = BM_ReadIsotpImpl(handle, infomem, ref nbytes, timeout, ref config);
        Marshal.Copy(infomem, payload, 0, (int)nbytes);
        return status;
    }

    /**
     * @brief      Read CAN message out of the given channel.
     * @param[in]  handle     Handle to the channel to read from.
     * @param[out] msg        A caller-allocated buffer to hold the CAN message output, see BM_CanMessageTypeDef for details.
     * @param[out] channel    The source channel ID from which the message is received, starting from zero, could be NULL if not required.
     * @param[out] timestamp  The device local high precision timestamp in microseconds, when the message is physically received on the CAN bus, could be NULL if not required.
     * @return     Operation status, see BM_StatusTypeDef for details. 
     * @note       Note this function is a simple wrapper of BM_Read(), see BM_Read() for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_ReadCanMessage(IntPtr handle, ref BM_CanMessageTypeDef msg, ref uint channel, ref uint timestamp);

    /**
     * @brief        Read multiple CAN messages out of the given channel.
     * @param[in]    handle  Handle to the channel to read from.
     * @param[out]   data       A caller-allocated buffer to hold the CAN message array output, see BM_CanMessageTypeDef for details.
     * @param[inout] nmessages  Number of read messages, user shall initialize this param with the size (in messages) of the data buffer.
     * @param[in]    timeout    Timeout (in milliseconds) before the message is received successfully from the bus.
     *                          Set any negative number (i.e. -1) to wait infinitely.
     *                          Set 0 if you would like to receive asynchronously: read from BMAPI internal buffer and return immediately, use BM_WaitForNotifications() before reading.
     * @param[out]   channel    The source channel ID from which the message is received, starting from zero, could be NULL if not required.
     * @param[out]   timestamp  The device local high precision timestamp array in microseconds, when the message is physically transmitted on the CAN bus, could be NULL if not required.
     * @return       Operation status, see BM_StatusTypeDef for details.
     * @note         This function is non-blocked, and thus will return BM_ERROR_QRCVEMPTY if not all messages are received.
     *               Please use notifications to wait for Rx events and then read message/event out of BMAPI internal RX buffer, otherwise you could also poll the device periodically.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_ReadMultipleCanMessage(IntPtr handle, IntPtr msg, ref uint nmessages, int timeout, IntPtr channel, IntPtr timestamp);

    /**
     * @brief      Write any message/event to the given channel.
     * @param[in]  handle  Handle to the channel to write to.
     * @param[in]  data      A caller-allocated buffer to hold the message/event input, see BM_DataTypeDef for details.
     * @param[in]  timeout   Timeout (in milliseconds) before the message is transmitted successfully to the bus.
     *                       Set any negative number (i.e. -1) to wait infinitely.
     *                       Set 0 if you would like to transmit asynchronously: put to BMAPI internal buffer and return immediately, then receive TXCMPLT event over BM_Read() later.
     * @param[in]  timestamp The device local high precision timestamp in microseconds, when the message is physically transmitted on the CAN bus, could be NULL if not required.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_Write(IntPtr handle, ref BM_DataTypeDef data, int timeout, ref uint timestamp);

    /**
     * @brief        Write multiple messages/events to the given channel.
     * @param[in]    handle    Handle to the channel to write to.
     * @param[in]    data      A caller-allocated buffer to hold the messages/events array input, see BM_DataTypeDef for details.
     * @param[inout] nmessages Number of written messages, user shall initialize this param with the size (in messages) of the data buffer.
     * @param[in]    timeout   Timeout (in milliseconds) before the message is transmitted successfully to the bus.
     *                         Set any negative number (i.e. -1) to wait infinitely.
     *                         Set 0 if you would like to transmit asynchronously: put to BMAPI internal buffer and return immediately, then receive TXCMPLT event over BM_Read() later.
     * @param[out]   timestamp The device local high precision timestamp array in microseconds, when the message is physically transmitted on the CAN bus, could be NULL if not required.
     * @return     Operation status, see BM_StatusTypeDef for details.
     * @note       This function is allowed to be called from multiple threads since BMAPI1.3.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_WriteMultiple(IntPtr handle, IntPtr data, ref uint nmessages, int timeout, IntPtr timestamp);

    /**
     * @brief        Write data block using ISOTP protocol.
     *               This API enables rapid transmission using ISOTP without app intervention, a simple example would be writing VIN using UDS:
     *               uint8_t request[] = { 0x2E, 0xF1, 0x80, ... ... };
     *               BM_WriteIsotp(channel, request, sizeof(request), config);
     * @param[in]    handle    Handle to the channel to write to.
     * @param[in]    data      A caller-allocated buffer to hold the data block input.
     * @param[in]    nbytes    Length of the data block, in bytes.
     * @param[in]    timeout   Timeout (in milliseconds) before any message segment is transmitted successfully to the bus.
     *                         Note this is only for bus level timeout waiting for CAN ACK, for setting ISOTP protocol timeouts, see BM_IsotpConfigTypeDef for details.
     * @param[in]    config    ISOTP configuration used by current transfer.
     * @return     Operation status, see BM_StatusTypeDef for details.
     * @note       This function is allowed to be called from multiple threads since BMAPI1.5.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl, EntryPoint = "BM_WriteIsotp")]
    public static extern BM_StatusTypeDef BM_WriteIsotpImpl(IntPtr handle, IntPtr data, uint nbytes, int timeout, ref BM_IsotpConfigTypeDef config);

    public static BM_StatusTypeDef BM_WriteIsotp(IntPtr handle, byte[] payload, int timeout, ref BM_IsotpConfigTypeDef config)
    {
        IntPtr infomem = Marshal.UnsafeAddrOfPinnedArrayElement(payload, 0);
        return BM_WriteIsotpImpl(handle, infomem, (uint)payload.Length, timeout, ref config);
    }

    /**
     * @brief      Write CAN message to the given channel.
     * @param[in]  handle     Handle to the channel to write to.
     * @param[in]  msg        A caller-allocated buffer to hold the CAN message output, see BM_CanMessageTypeDef for details.
     * @param[in]  channel    The target channel ID to which the message is transmitted, starting from zero.
     * @param[in]  timeout   Timeout (in milliseconds) before the message is transmitted successfully to the bus.
     *                       Set any negative number (i.e. -1) to wait infinitely.
     *                       Set 0 if you would like to transmit asynchronously: put to BMAPI internal buffer and return immediately, then receive TXCMPLT event over BM_Read() later.
     * @param[in]  timestamp The device local high precision timestamp in microseconds, when the message is physically transmitted on the CAN bus, could be NULL if not required.
     * @note       Note this function is a simple wrapper of BM_Write(), see BM_Write() for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_WriteCanMessage(IntPtr handle, ref BM_CanMessageTypeDef msg, uint channel, int timeout, ref uint timestamp);

    /**
    * @brief      Write LIN message to the given channel.
    * @param[in]  handle     Handle to the channel to write to.
    * @param[in]  msg        A caller-allocated buffer to hold the LIN message output, see BM_LinMessageTypeDef for details.
    * @param[in]  _channel   The target channel ID to which the message is transmitted, starting from zero. This parameter is reserved for future, always 0 now.
    * @param[in]  timeout   Timeout (in milliseconds) before the message is transmitted successfully to the bus.
    *                       Set any negative number (i.e. -1) to wait infinitely.
    *                       Set 0 if you would like to transmit asynchronously: put to BMAPI internal buffer and return immediately, then receive TXCMPLT event over BM_Read() later.
    * @param[in]  timestamp The device local high precision timestamp in microseconds, when the message is physically transmitted on the LIN bus, could be NULL if not required.
    * @note       Note this function is a simple wrapper of BM_Write(), see BM_Write() for details.
    */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_WriteLinMessage(IntPtr handle, ref BM_LinMessageTypeDef msg, uint channel, int timeout, ref uint timestamp);


    /**
    * @brief      Set LIN voltage option of the given channel.
    * @param[in]  handle  Handle to the channel to operate on.
    * @param[in]  tres    Expected LIN voltage, see BM_LinVoltageTypeDef for details.
    * @return     Operation status, see BM_StatusTypeDef for details.
    */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_SetLinVoltage(IntPtr handle, BM_LinVoltageTypeDef voltage);
    /**
     * @brief        Write multiple CAN messages to the given channel.
     * @param[in]    handle    Handle to the channel to write to.
     * @param[in]    msg       A caller-allocated buffer to hold the CAN message array input, see BM_CanMessageTypeDef for details.
     * @param[inout] nmessages Number of written messages, user shall initialize this param with the size (in messages) of the data buffer.
     * @param[in]    _channel  The target channel ID to which the message is transmitted, starting from zero. This parameter is reserved for future, always 0 now, or simply pass NULL into the API.
     * @param[in]    timeout   Timeout (in milliseconds) before the message is transmitted successfully to the bus.
     *                         Set any negative number (i.e. -1) to wait infinitely.
     *                         Set 0 if you would like to transmit asynchronously: put to BMAPI internal buffer and return immediately, then receive TXCMPLT event over BM_Read() later.
     * @param[out]   timestamp The device local high precision timestamp array in microseconds, when the message is physically transmitted on the CAN bus, could be NULL if not required.
     * @return     Operation status, see BM_StatusTypeDef for details.
     * @note       This function is allowed to be called from multiple threads since BMAPI1.3.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_WriteMultipleCanMessage(IntPtr handle, IntPtr msg, ref uint nmessages, IntPtr _channel, int timeout, IntPtr timestamp);

    /**
     * @brief Control the given channel, this is an advanced interface and is typically used internally by BMAPI.
     * @param[in]    handle   Handle to the channel to control.
     * @param[in]    command  Control command.
     * @param[in]    value    Control value.
     * @param[in]    index    Control index.
     * @param[inout] data     Control data, could be NULL.
     * @param[inout] nbytes   Length in bytes of the control data, could be zero.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_Control(IntPtr handle, byte command, ushort value, ushort index, IntPtr data, int nbytes);

    /**
     * @brief      Get current CAN status of the given channel.
     * @param[in]  handle      Handle to the channel to operate on.
     * @param[out] statusinfo  Detailed information of current CAN status, see BM_CanStatusInfoTypedef for details.
     * @return     Current status code, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_GetStatus(IntPtr handle, ref BM_CanStatusInfoTypedef statusinfo);

    /**
     * @brief      Get current local high precision device timestamp, in microseconds.
     * @param[in]  handle     Handle to the channel to operate on.
     * @param[out] timestamp  Timestamp value.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_GetTimestamp(IntPtr handle, ref uint timestamp);

    /**
     * @brief      Set CAN mode option of the given channel.
     * @param[in]  handle  Handle to the channel to operate on.
     * @param[in]  mode    Expected CAN mode, see BM_CanModeTypeDef for details.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_SetCanMode(IntPtr handle, BM_CanModeTypeDef mode);

    /**
     * @brief      Set terminal resistor option of the given channel.
     * @param[in]  handle  Handle to the channel to operate on.
     * @param[in]  tres    Expected terminal resistor, see BM_TerminalResistorTypeDef for details.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_SetTerminalRegister(IntPtr handle, BM_TerminalResistorTypeDef tres);

    /**
     * @brief      Set bitrate option of the given channel.
     * @param[in]  handle  Handle to the channel to operate on.
     * @param[in]  bitrate Expected bitrate, see BM_BitrateTypeDef for details.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_SetBitrate(IntPtr handle, ref BM_BitrateTypeDef bitrate);

    /**
     * @brief      Set TX tasks option of the given channel.
     * @param[in]  handle    Handle to the channel to operate on.
     * @param[in]  txtasks   An array of TX task information, see BM_TxTaskTypeDef for details.
     * @param[in]  ntxtasks  Number of valid TX tasks in the txtasks array.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_SetTxTasks(IntPtr handle, IntPtr txtasks, int ntxtasks);

    /**
     * @brief      Set RX filters option of the given channel.
     * @param[in]  handle      Handle to the channel to operate on.
     * @param[in]  rxfilters   An array of RX filter information, see BM_RxFilterTypeDef for details.
     * @param[in]  nrxfilters  Number of valid RX filters in the rxfilters array.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_SetRxFilters(IntPtr handle, IntPtr rxfilters, int nrxfilters);

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
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_GetNotification(IntPtr handle, ref IntPtr notification);

    /**
     * @brief     A platform/OS independent implementation to wait for single/multiple notification handles.
     * @param[in] handles     An array of channel notification handles.
     * @param[in] nhandles    Number of valid notification handles.
     * @param[in] ntimeoutms  This function will block current thread for ntimeoutms milliseconds if no notification is received.
     *                        Note this function will return immediately once a new notification is received, the ntimeoutms param is ignored in this normal case.
     * @return    This function returns the index in handles array of the channel from which a new notification is posted.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern int BM_WaitForNotifications(IntPtr[] handles, int nhandles, int ntimeoutms);

    /**
     * @brief      Set offline logging configuration for current channel.
     * @param[in]  handle    Handle to the channel to operate on.
     * @param[in]  logging   Logging configuration, see BM_LoggingConfigTypeDef for details.
     * @return     Operation status, see BM_StatusTypeDef for details.
     * @note       All channels of a physical device share the same logging configuration, that is, you simply need to select any channel (e.g. CH0) and configure logging once.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_SetLogging(IntPtr handle, ref BM_LoggingConfigTypeDef logging);

    /**
     * @brief      Get offline logging configuration for current channel.
     * @param[in]  handle    Handle to the channel to operate on.
     * @param[in]  logging   Logging configuration, see BM_LoggingConfigTypeDef for details.
     * @return     Operation status, see BM_StatusTypeDef for details.
     * @note       All channels of a physical device share the same logging configuration, that is, you simply need to select any channel (e.g. CH0) and configure logging once.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_GetLogging(IntPtr handle, ref BM_LoggingConfigTypeDef logging);

    /**
     * @brief      Set offline replay configuration for current channel.
     * @param[in]  handle    Handle to the channel to operate on.
     * @param[in]  replay    Replay configuration, see BM_ReplayConfigTypeDef for details.
     * @return     Operation status, see BM_StatusTypeDef for details.
     * @note       All channels of a physical device share the same replay configuration, that is, you simply need to select any channel (e.g. CH0) and configure replay once.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_SetReplay(IntPtr handle, ref BM_ReplayConfigTypeDef replay);

    /**
     * @brief      Set offline replay configuration for current channel.
     * @param[in]  handle    Handle to the channel to operate on.
     * @param[in]  replay    Replay configuration, see BM_ReplayConfigTypeDef for details.
     * @return     Operation status, see BM_StatusTypeDef for details.
     * @note       All channels of a physical device share the same replay configuration, that is, you simply need to select any channel (e.g. CH0) and configure replay once.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_GetReplay(IntPtr handle, ref BM_ReplayConfigTypeDef replay);

    /**
     * @brief      Load configuration from off-line storage media for the given channels.
     * @param[in]  handle    Handle to the channel to operate on.
     * @param[in]  configmask   Device channel bit-mask, each bit indicates a channel, e.g. bit0=channel0, bit15=channel15.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_LoadConfig(IntPtr handle, uint configmask);

    /**
     * @brief      Save configuration to off-line storage media for the given channels.
     * @param[in]  handle    Handle to the channel to operate on.
     * @param[in]  configmask   Device channel bit-mask, each bit indicates a channel, e.g. bit0=channel0, bit15=channel15.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_SaveConfig(IntPtr handle, uint configmask);

    /**
     * @brief      Clear configuration in off-line storage media for the given channels.
     * @param[in]  handle    Handle to the channel to operate on.
     * @param[in]  configmask   Device channel bit-mask, each bit indicates a channel, e.g. bit0=channel0, bit15=channel15.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_ClearConfig(IntPtr handle, uint configmask);

    /**
     * @brief      Translate error code to string, this is a helper function to ease application programming.
     * @param[in]  errorcode  The errorcode to be translated.
     * @param[out] buffer     A caller-allocated string buffer to hold the translated string.
     * @param[in]  nbytes     Number in bytes of the string buffer.
     * @param[in]  language   Reserved, only English is supported currently.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern void BM_GetErrorText(BM_StatusTypeDef errorcode, [Out] char[] buffer, int nbytes, ushort language);

    /**
     * @brief      Translate data (i.e. CAN message) to string, this is a helper function to ease application programming.
     * @param[in]  data       The message data to be translated.
     * @param[out] buffer     A caller-allocated string buffer to hold the translated string.
     * @param[in]  nbytes     Number in bytes of the string buffer.
     * @param[in]  language   Reserved, only English is supported currently.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern void BM_GetDataText(ref BM_DataTypeDef data, [Out] char[] buffer, int nbytes, ushort language);
	
	/**
	 * @brief      Get library log level.
	 * @return     Current log level, all messages equal to or less than this level are currently printed on debug console.
	 */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
	public static extern BM_LogLevelTypeDef BM_GetLogLevel();

	/**
	 * @brief      Set library log level.
	 * @param[in]  level       Target log level, all messages equal to or less than this level will be printed on debug console.
	 */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
	public static extern void BM_SetLogLevel(BM_LogLevelTypeDef level);

	/**
	 * @brief      Get library (*.dll|*.so) BMAPI version.
	 * @return     32-bit version code:
	 *             bit31-28 = major
	 *             bit27-24 = minor
	 *             bit23-16 = revision
	 *             bit15-00 = build
	 * @note       This API is used to get the library version, 
	 *             in case that *.h is mismatch with *.dll|*.so, use the macro BM_API_VERSION defined in bmapi.h to check consistency.
	 */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
	public static extern uint BM_GetVersion();
    /* ==== APIs synchronized with BMAPI 1.14.2.45 ==== */

    public const uint BM_REMOTE_SESSION_ABI_VERSION = 1;          /**< Remote control-session public ABI version, see bm_usb_def.h. */
    public const uint BM_REMOTE_SESSION_DEFAULT_TIMEOUT_MS = 10000; /**< Default remote control-lock lease in milliseconds. */
    public const uint BM_REMOTE_SESSION_DEFAULT_HEARTBEAT_MS = 2000; /**< Default remote control-session heartbeat interval in milliseconds. */

    /**
     * @brief Start AUTOSET sequence, for a BM-USB-CAN(FD) device, the AUTOSET sequence will detect the correct bitrate and terminal resistor.
     * @param[in]  channelinfo  Info of the device channel to run AUTOSET on, usually the info is filled by BM_Enumerate().
     * @param[out] bitrate      The detected bitrate.
     * @param[out] tres         The detected terminal resistor.
     * @param[in]  callback     A callback function which will be called on each step of AUTOSET.
     * @param[in]  userarg      Arbitrary user argument of the callback function, this argument will be passed to the callback as is.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_Autoset(
        ref BM_ChannelInfoTypeDef channelinfo,
        ref BM_BitrateTypeDef bitrate,
        ref BM_TerminalResistorTypeDef tres,
        BM_AutosetCallbackHandle callback,
        IntPtr userarg
    );

    /**
     * @brief Start AUTOSET sequence with explicit passive-listen or active-detect strategy.
     * @param[in]  channelinfo  Info of the device channel to run AUTOSET on, usually the info is filled by BM_Enumerate().
     * @param[out] bitrate      The detected bitrate.
     * @param[out] tres         The detected terminal resistor.
     * @param[in]  config       AUTOSET configuration. When NULL, passive listen defaults are used.
     * @param[in]  callback     A callback function which will be called on each step of AUTOSET.
     * @param[in]  userarg      Arbitrary user argument of the callback function, this argument will be passed to the callback as is.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_AutosetEx(
        ref BM_ChannelInfoTypeDef channelinfo,
        ref BM_BitrateTypeDef bitrate,
        ref BM_TerminalResistorTypeDef tres,
        ref BM_AutosetConfigTypeDef config,
        BM_AutosetCallbackHandle callback,
        IntPtr userarg
    );

    [DllImport("BMAPI.DLL", EntryPoint = "BM_AutosetEx", CallingConvention = CallingConvention.Cdecl)]
    private static extern BM_StatusTypeDef BM_AutosetExDefaultImpl(
        ref BM_ChannelInfoTypeDef channelinfo,
        ref BM_BitrateTypeDef bitrate,
        ref BM_TerminalResistorTypeDef tres,
        IntPtr config,
        BM_AutosetCallbackHandle callback,
        IntPtr userarg
    );

    /**
     * @brief Start AUTOSET with the native passive-listen default configuration.
     */
    public static BM_StatusTypeDef BM_AutosetEx(
        ref BM_ChannelInfoTypeDef channelinfo,
        ref BM_BitrateTypeDef bitrate,
        ref BM_TerminalResistorTypeDef tres,
        BM_AutosetCallbackHandle callback,
        IntPtr userarg
    )
    {
        return BM_AutosetExDefaultImpl(ref channelinfo, ref bitrate, ref tres, IntPtr.Zero, callback, userarg);
    }

    /**
     * @brief      Recover from CAN BUSOFF state.
     *             Gen2/Gen2.5 firmware 2.6.0.0+ and Gen3 firmware 3.1.0.0+ use
     *             firmware-side recovery. Gen2/Gen2.5 firmware below 2.6.0.0 and
     *             Gen3 firmware 3.0.x use loopback + dummy message recovery.
     * @param[in]  handle  Handle to the channel to recover.
     * @return    Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_RecoverBusOff(IntPtr handle);

    /**
     * @brief     Cancel all pending write requests for a given channel, this operation will release internal listener resources in a multi-threading environment.
     * @param[in] handle  Handle to the channel to cancel writing.
     * @return    Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_CancelWrite(IntPtr handle);

    /**
     * @brief      Get channel information, please only open a channel and get its handle before calling BM_GetChannelInfo().
     * @param[in]  handle  Handle to the channel to query.
     * @param[out] info    A caller-allocated buffer for the returned channel information.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_GetChannelInfo(IntPtr handle, ref BM_ChannelInfoTypeDef info);

    /**
     * @brief      Open the specified remote device port.
     * @param[in]  ipv4  Remote IPV4, note this is the forwarding-target ip when creating a virtual remote-forwarding channel.
     * @param[in]  localinfo  Optional local server information for networking service, pass IntPtr.Zero if not required.
     * @return Handle to the opened CAN device channel, return NULL if failed to open the specified port.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr BM_OpenRemote(byte[] ipv4, IntPtr localinfo);

    /**
     * @brief      Query remote control-lock support and the current owner without claiming.
     * @param[in]  ipv4  The IPV4 unicast/broadcast address in which to find remote devices, in network byte order.
     * @param[in]  timeout_ms  Timeout for this request only, no lease effect.
     * @param[out] info  Remote session status and ownership information.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_QueryRemoteSession(byte[] ipv4, int timeout_ms, ref BM_RemoteSessionInfoTypeDef info);

    /**
     * @brief      Claim the remote control session and start automatic heartbeats.
     *             This never falls back to unlocked mode. The handle must be released via BM_ReleaseRemoteSession().
     * @param[out] session  The claimed remote session handle.
     * @param[in]  ipv4  The IPV4 address of the remote device, in network byte order.
     * @param[in]  config  Remote session configuration, see BM_RemoteSessionConfigTypeDef for details.
     * @param[out] info  Remote session status and ownership information.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_ClaimRemoteSession(ref IntPtr session, byte[] ipv4, ref BM_RemoteSessionConfigTypeDef config, ref BM_RemoteSessionInfoTypeDef info);

    /**
     * @brief      Query live status for a claimed remote session.
     * @param[in]  session  The claimed remote session handle.
     * @param[out] info  Remote session status and ownership information.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_GetRemoteSessionInfo(IntPtr session, ref BM_RemoteSessionInfoTypeDef info);

    /**
     * @brief      Release the remote control session and destroy the handle.
     *             BM_UnInit also performs best-effort release of outstanding handles.
     * @param[in]  session  The remote session handle to release.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_ReleaseRemoteSession(IntPtr session);

    /**
     * @brief      Get the device handle which an opened channel belongs to.
     * @param[in]  channel  Handle to the opened channel.
     * @param[out] device   Handle to the device that owns the channel.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_GetDevice(IntPtr channel, ref IntPtr device);

    /**
     * @brief      Set device internal buffer target when calling BM_Read and BM_Write to transfer messages.
     *             For example, you might want to use this API to switch to the device's internal REPLAYQ buffer for hardware replay purpose.
     * @param[in]  device  Handle to the device to operate on, retrieved by BM_GetDevice().
     * @param[in]  type    Buffer direction, see BM_BufferTypeDef for details.
     * @param[in]  id      Buffer identifier, see BM_BufferId for details.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_SetBuffer(IntPtr device, BM_BufferTypeDef type, BM_BufferId id);

    /**
     * @brief      Set PTP timestamp synchronization mode.
     * @param[in]  handle  Handle to the channel to operate on.
     * @param[in]  mode    Expected PTP timestamp synchronization mode, see BM_PtpModeTypeDef for details.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_SetPtpMode(IntPtr handle, BM_PtpModeTypeDef mode);

    /**
     * @brief      Set PTP timestamp in nano-seconds of the given channel.
     * @param[in]  handle  Handle to the channel to operate on.
     * @param[in]  ns      Expected PTP timestamp in nano-seconds, in local timezone, since 1970-1-1.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_SetPtpTime(IntPtr handle, ulong ptpTimestamp);

    /**
     * @brief      Get PTP timestamp in nano-seconds of the given channel.
     * @param[in]  handle  Handle to the channel to operate on.
     * @param[out] ns      Current PTP timestamp in nano-seconds, in local timezone, since 1970-1-1.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_GetPtpTime(IntPtr handle, ref ulong ptpTimestamp);

    /**
     * @brief      Get PTP timestamp in nano-seconds of the host machine, which is calling BMAPI.
     * @return     Current PTP timestamp in nano-seconds, in local timezone, since 1970-1-1.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong BM_GetHostPtpTime();

    /**
     * @brief     A platform/OS independent implementation to synchronize PTP timestamp with the host machine for single/multiple channels.
     * @param[in] handles     An array of channel handles.
     * @param[in] nhandles    Number of valid channel handles.
     * @return    Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_SyncPtpTimes(IntPtr[] channelhandles, int nHandles);

    /**
    * @brief      Extract PTP timestamp from data (i.e. CAN message), this is a helper function to ease application programming.
    * @param[in]  handle     Handle to the channel to operate on. Could be NULL if the device supports hardware PTP.
    *                        Otherwise you would need this handle to align device local timestamp with host machine's UTC time.
    * @param[in]  data       The message data from which to extract an PTP timestamp.
    * @param[out] timestamp  The extracted PTP timestamp.
    */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_GetDataPtpTimestamp(IntPtr handle, ref BM_DataTypeDef data, ref ulong ptpTimestamp);

    /**
     * @brief      Get current LIN status of the given channel.
     * @param[in]  handle      Handle to the channel to operate on.
     * @param[out] statusinfo  Detailed information of current LIN status, see BM_LinStatusInfoTypeDef for details.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_GetLinStatus(IntPtr handle, ref BM_LinStatusInfoTypeDef statusinfo);

    /**
     * @brief      Get TX tasks option of the given channel.
     * @param[in]  handle    Handle to the channel to operate on.
     * @param[out] txtasks   An array of TX task information, see BM_TxTaskTypeDef for details.
     * @param[in]  ntxtasks  Number of valid TX tasks in the txtasks array.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_GetTxTasks(IntPtr handle, IntPtr txtasks, int ntxtasks);

    /**
     * @brief      Get Message Routes option of the given channel.
     * @param[in]  handle    Handle to the channel to operate on.
     * @param[out] msgroutes An array of Message Routes information, see BM_MessageRouteTypeDef for details.
     * @param[in]  nmsgroute Number of valid Message Routes in the routes array.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_GetMsgRoutes(IntPtr handle, IntPtr msgroutes, int nmsgroute);

    /**
     * @brief      Set channel mode option of the given channel.
     * @param[in]  handle  Handle to the channel to operate on.
     * @param[in]  mode    Channel mode, see BM_CanModeTypeDef, BM_LinModeTypeDef for details.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_SetMode(IntPtr handle, uint mode);

    /**
     * @brief      Get CAN mode option of the given channel.
     * @param[in]  handle  Handle to the channel to operate on.
     * @param[out] mode    Current CAN mode, see BM_CanModeTypeDef for details.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_GetCanMode(IntPtr handle, ref BM_CanModeTypeDef mode);

    /**
     * @brief      Set sleep option of the given channel.
     * @param[in]  handle  Handle to the channel to operate on.
     * @param[in]  status  Expected sleep status, see BM_SleepStatusTypeDef for details.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_SetSleepStatus(IntPtr handle, BM_SleepStatusTypeDef status);

    /**
     * @brief      Get sleep option of the given channel.
     * @param[in]  handle  Handle to the channel to operate on.
     * @param[in]  status  Current sleep status, see BM_SleepStatusTypeDef for details.
     *                   Note: bmapi.h declares this parameter by value, the binding mirrors that ABI.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_GetSleepStatus(IntPtr handle, BM_SleepStatusTypeDef status);

    /**
     * @brief      Get terminal resistor option of the given channel.
     * @param[in]  handle  Handle to the channel to operate on.
     * @param[out] tres    Current terminal resistor, see BM_TerminalResistorTypeDef for details.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_GetTerminalRegister(IntPtr handle, ref BM_TerminalResistorTypeDef tres);

    /**
     * @brief      Get LIN voltage option of the given channel.
     * @param[in]  handle  Handle to the channel to operate on.
     * @param[out] voltage Current LIN voltage, see BM_LinVoltageTypeDef for details.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_GetLinVoltage(IntPtr handle, ref BM_LinVoltageTypeDef voltage);

    /**
     * @brief      Set LIN protocol option of the given channel.
     * @param[in]  handle  Handle to the channel to operate on.
     * @param[in]  protocol Expected LIN protocol, see BM_LinProtocolConfigTypeDef for details.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_SetLinProtocol(IntPtr handle, ref BM_LinProtocolConfigTypeDef protocol);

    /**
     * @brief      Get LIN protocol option of the given channel.
     * @param[in]  handle   Handle to the channel to operate on.
     * @param[out] protocol Current LIN protocol, see BM_LinProtocolConfigTypeDef for details.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_GetLinProtocol(IntPtr handle, ref BM_LinProtocolConfigTypeDef protocol);

    /**
     * @brief      Set bitrate option of the given LIN channel.
     * @param[in]  handle  Handle to the channel to operate on.
     * @param[in]  bitrate Expected bitrate, e.g. Set bitrate=19200 if 19200bps is expected.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_SetLinBitrate(IntPtr handle, ushort bitrate);

    /**
     * @brief      Set bitrate option of the given Ethernet channel.
     * @param[in]  handle  Handle to the channel to operate on.
     * @param[in]  bitrate Expected speed, e.g. Set bitrate=1000 if 1000Mbps(1Gbps) is expected.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_SetEthSpeed(IntPtr handle, ushort bitrate);

    /**
     * @brief      Get bitrate option of the given channel.
     * @param[in]  handle  Handle to the channel to operate on.
     * @param[out] bitrate Current bitrate, see BM_BitrateTypeDef for details.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_GetBitrate(IntPtr handle, ref BM_BitrateTypeDef bitrate);

    /**
     * @brief      Set Message Routes option of the given channel.
     * @param[in]  handle    Handle to the channel to operate on.
     * @param[in]  msgroutes An array of Message Routes information, see BM_MessageRouteTypeDef for details.
     * @param[in]  nmsgroute Number of valid Message Routes in the routes array.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_SetMsgRoutes(IntPtr handle, IntPtr msgroutes, int nmsgroute);

    /**
     * @brief      Get RX filters option of the given channel.
     * @param[in]  handle      Handle to the channel to operate on.
     * @param[out] rxfilters   An array of RX filter information, see BM_RxFilterTypeDef for details.
     * @param[in]  nrxfilters  Number of valid RX filters in the rxfilters array.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_GetRxFilters(IntPtr handle, IntPtr rxfilters, int nrxfilters);

    /**
     * @brief      Convert from 32-bit hardware timestamp to 64-bit UTC timestamp.
     * @param[in]  handle  Handle to the channel to operate on.
     * @param[in]  timestamp32  Hardware 32-bit timestamp.
     * @param[out] timestamp64  Converted 64-bit timestamp in micro-second, since 1970-1-1.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_MapTimestamp(IntPtr handle, uint timestamp32, ref ulong timestamp64);

    /**
     * @brief      Read LIN message out of the given channel.
     * @param[in]  handle     Handle to the channel to read from.
     * @param[out] msg        A caller-allocated buffer to hold the LIN message output, see BM_LinMessageTypeDef for details.
     * @param[out] channel    The source channel ID from which the message is received, starting from zero.
     * @param[out] timestamp  The device local high precision timestamp in microseconds, when the message is physically received on the LIN bus.
     * @return     Operation status, see BM_StatusTypeDef for details.
     * @note       Note this function is a simple wrapper of BM_Read(), see BM_Read() for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_ReadLinMessage(IntPtr handle, ref BM_LinMessageTypeDef msg, ref uint channel, ref uint timestamp);

    /**
     * @brief      Set background thread priority for performance-tuning purpose.
     * @param[in]  priority  A integer, which is, 0-15 on Windows, and 0-100 on Unix.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern void BM_SetThreadPriority(uint priority);

    /**
     * @brief      Get current CAN status of the given channel.
     * @param[in]  handle      Handle to the channel to operate on.
     * @param[out] statusinfo  Detailed information of current CAN status, see BM_CanStatusInfoTypedef for details.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_GetCanStatus(IntPtr handle, ref BM_CanStatusInfoTypedef statusinfo);

    /**
     * @brief      Set LIN mode option of the given channel.
     * @param[in]  handle  Handle to the channel to operate on.
     * @param[in]  mode    Expected LIN mode, see BM_LinModeTypeDef for details.
     * @return     Operation status, see BM_StatusTypeDef for details.
     */
    [DllImport("BMAPI.DLL", CallingConvention = CallingConvention.Cdecl)]
    public static extern BM_StatusTypeDef BM_SetLinMode(IntPtr handle, BM_LinModeTypeDef mode);

}
