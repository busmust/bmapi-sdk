Imports System.Runtime.InteropServices

''**
' * @enum  BM_CapabilityTypeDef
' * @brief Busmust Device capability flags  retrieved when enumerating devices using BM_Enumerate().
' *'
Public Enum BM_CapabilityTypeDef

    BM_NONE_CAP = &H0      ''**< No capability *'
    BM_LIN_CAP = &H1       ''**< The device Is capable Of handling LIN messages *'
    BM_CAN_CAP = &H2       ''** <The device Is capable Of handling CAN messages *'
    BM_CAN_FD_CAP = &H4    ''** <The device Is capable Of handling CANFD (And CAN) messages *'
    BM_FLEXRAY_CAP = &H8   ''**<The device Is capable Of handling FLEXRAY messages *'
    BM_MODBUS_CAP = &H10   ''**<The device Is capable Of handling MODBUS messages *'
    BM_ETHERNET_CAP = &H20 ''**<The device Is capable Of handling ETHERNET messages *'
    BM_AIO_CAP = &H100     ''**< The device is capable of analog IO *'
    BM_DIO_CAP = &H200     ''**< The device is capable of digital IO *'
    BM_VIRTUAL_CAP = &H4000  ''**< The device is virtual, for simulation purpose *'
    BM_REMOTE_CAP = &H8000   ''**< The device is connected remotely and is capable of remote management *'
    BM_ALL_CAP = &HFFFF    ''**<Typically used For masking the CAP fields When programming *'
End Enum

'/**
' * @enum  BM_PtpModeTypeDef
' * @brief PTP timestamp synchronization mode values, used by BM_SetPtpMode() to change the synchronization mode for a given hardware channel.
' */ 
Public Enum BM_PtpModeTypeDef
    BM_PTP_DISABLED = 0       '/**< Disable PTP feature, work asynchronously Using hardware local timestamp                 */
    BM_PTP_INPUT_USB_SOF = 1  '/**< Use host computer's USB SOF packet to sync with host computer                           */
    BM_PTP_INPUT_PPS = 2      '/**< PTP slave: Use dedicated PPS input pin (If supported by hardware) As sync pulse input   */
    BM_PTP_OUTPUT_PPS = 4     '/**< PTP master: Use dedicated PPS input pin (If supported by hardware) As sync pulse output */
End Enum


''**
' * @enum  BM_DataTypeTypeDef
' * @brief Busmust data type flags  must be given in BM_DataTypeDef.
' *'
Public Enum BM_DataTypeTypeDef
    BM_UNKNOWN_DATA = 0        ''** <Unknown data type *'
    BM_LIN_DATA                ''** <LIN message data type *'
    BM_CAN_FD_DATA             ''**<CAN Or CAN-FD message data type (check FDF flag further) *'
    BM_FLEXRAY_DATA            ''**<Flexray message data type *'
    BM_MODBUS_DATA             ''**<MODBUS message data type *'
    BM_ETHERNET_DATA           ''**<Ethernet message data type *'
    BM_ACK_DATA = &H8          ''**<ACK from bus  which indicates TXCMPLT Event If this Is BM_CAN_FD_DATA *'
End Enum

''**
' * @enum  BM_StatusTypeDef
' * @brief Busmust device & operation status  most APIs would return a status code to indicate the result of an operation.
' *'
Public Enum BM_StatusTypeDef
    BM_ERROR_OK = &H0                         '**< SUCCESS: No Error occurred *
    BM_ERROR_XMTFULL = &H1                    '**< Low level Transmit buffer Is full *
    BM_ERROR_OVERRUN = &H2                    '**< Bus overrun (the device cannot keep up With the high bus throughput) *'
    BM_ERROR_BUSLIGHT = &H4                   '**< CAN Bus communication Is light  see ISO11898 For details *'
    BM_ERROR_BUSHEAVY = &H8                   '**< CAN Bus communication Is heavy  see ISO11898 For details *'
    BM_ERROR_BUSWARNING = BM_ERROR_BUSHEAVY   '**< CAN Bus communication Is In warning state  see ISO11898 For details *'
    BM_ERROR_BUSPASSIVE = &H40000             '**< CAN node Is In passive state  see ISO11898 For details *'
    BM_ERROR_BUSTIMEOUT = &H80000             '**< CAN node failed To transmit message within specified time  the node might be In PASSIVE Or BUSOFF state *'
    BM_ERROR_BUSOFF = &H10                    '**< CAN bus Is In BUSOFF state  see ISO11898 For details *'
    BM_ERROR_ANYBUSERR = (BM_ERROR_BUSWARNING Or BM_ERROR_BUSLIGHT Or BM_ERROR_BUSHEAVY Or BM_ERROR_BUSOFF Or BM_ERROR_BUSPASSIVE) '**< CAN bus error occurred *'
    BM_ERROR_QRCVEMPTY = &H20                 '**< Receive buffer Is empty  this might Not be an Error If you use BMAPI In polling mode *'
    BM_ERROR_QOVERRUN = &H40                  '**< BMAPI internal Q overrun *'
    BM_ERROR_QXMTFULL = &H80                  '**< High level Transmit queue Is full *'
    BM_ERROR_REGTEST = &H100                  '**< Reserved *'
    BM_ERROR_NODRIVER = &H200                 '**< Reserved *'
    BM_ERROR_HWINUSE = &H400                  '**< Hardware Is In use (opened by another application) *'
    BM_ERROR_NETINUSE = &H800                 '**< Reserved *'
    BM_ERROR_ILLHW = &H1400                   '**< Hardware Error Or invalid hardware handle *'
    BM_ERROR_ILLNET = &H1800                  '**< Invalid bus *'
    BM_ERROR_ILLCLIENT = &H1C00               '**< Invalid client *'
    BM_ERROR_ILLHANDLE = (BM_ERROR_ILLHW Or BM_ERROR_ILLNET Or BM_ERROR_ILLCLIENT) '* Invalid handle*'
    BM_ERROR_RESOURCE = &H2000                '**< Out Of resource *'
    BM_ERROR_ILLPARAMTYPE = &H4000            '**< Invalid parameter type In API Call *'
    BM_ERROR_ILLPARAMVAL = &H8000             '**< Invalid parameter value In API Call *'
    BM_ERROR_UNKNOWN = &H10000                '**< Unknown Error *'
    BM_ERROR_ILLDATA = &H20000                '**< Invalid data received'transmitted *'
    BM_ERROR_CAUTION = &H2000000              '**< Reserved *'
    BM_ERROR_INITIALIZE = &H4000000           '**< The device'library Is Not initialized *'
    BM_ERROR_ILLOPERATION = &H8000000         '**< Invalid operation *'
End Enum

'/**
'* @Enum  BM_LinModeTypeDef
'* @brief LIN mode IDs, used by BM_SetLinMode() To change the operation mode Of LIN device.
'*/
Public Enum BM_LinModeTypeDef
    BM_LIN_OFF_MODE = &H0                     '/**< The device Is logically disconnected from LIN bus */
    BM_LIN_SLAVE_MODE = &H1                   '/**< The device Is running As a slave node (With the capability To transmit And receive LIN messages */
    BM_LIN_INTERNAL_LOOPBACK_MODE = &H2       '/**< The device Is looping back messages internally without impacting the physical LIN bus */
    BM_LIN_LISTEN_ONLY_MODE = &H3             '/**< The device Is receiving messages without impacting the physical LIN bus */
    BM_LIN_MASTER_MODE = &H8                  '/**< The device Is running As a master node */
End Enum

'/**
'* @Enum  BM_LinVoltageTypeDef
'* @brief LIN voltage configuration values, used by BM_SetLinVoltage().
'*/
Public Enum BM_LinVoltageTypeDef
    BM_LIN_VOLTAGE_12V_IN = &H0             '/**< Accept 12V As input */
    BM_LIN_VOLTAGE_12V_OUT = &H1            '/**< Use internal 12V DC-DC For pulling up */
End Enum

'/**
'* @Enum  BM_LinMessageFlagsTypeDef
'* @brief LIN Message type flags, used In BM_TxTaskTypeDef.
'*/
Public Enum BM_LinMessageFlagsTypeDef
    BM_LIN_MESSAGE_FLAGS_TRANSMIT = &H1                    '/**< Transfer direction: 0=receive, 1=transmit */
    BM_LIN_MESSAGE_FLAGS_WAKEUP = &H2                      '/**< This Is a wakeup Event */
    BM_LIN_MESSAGE_FLAGS_SLEEP = &H4                       '/**< This Is a sleep Event */
    BM_LIN_MESSAGE_FLAGS_ENHANCED_CHECKSUM = &H8           '/**< Flag For enhanced checksum */
    BM_LIN_MESSAGE_FLAGS_USER_CHECKSUM = &H10              '/**< Flag For user defined checksum, use value from 'CHECKSUM' field if USER_CHECKSUM==1  */
End Enum

'/**
'* @Enum  BM_LinProtocolVersionTypeDef
'* @brief LIN protocol version.
'*/
Public Enum BM_LinProtocolVersionTypeDef
    BM_LIN_VER_1_3 = &H13                                   '/**< Version1.3 */
    BM_LIN_VER_2_0 = &H20                                   '/**< Version2.0 */
    BM_LIN_VER_2_1 = &H21                                   '/**< Version2.1 */
    BM_LIN_VER_2_2 = &H22                                   '/**< Version2.2 */
End Enum


'/**
'* @Enum  BM_LinProtocolChecksumTypeDef
'* @brief LIN protocol checksum options.
'*/
Public Enum BM_LinProtocolChecksumTypeDef
    BM_LIN_MANUAL_CHECKSUM = &H0                              '/**< Reserved */
    BM_LIN_NORMAL_CHECKSUM = &H1                              '/**< Normal checksum (usually used by LIN1.3) */
    BM_LIN_ENHANCED_CHECKSUM = &H2                            '/**< Enhanced checksum (can only be used by LIN2.x) */
End Enum


'/**
'* @Enum  BM_LinErrorTypeDef
'* @brief LIN Error code, used In BM_LinStatusInfoTypedef.
'*/
Public Enum BM_LinErrorTypeDef
    BM_LIN_NO_ERROR = &H0                                   '/**< No Error detected */
    BM_LIN_BIT_ERROR = &H1                                  '/**< Bit Error */
    BM_LIN_CHECKSUM_ERROR = &H2                             '/**< Checksum Error */
    BM_LIN_PARITY_ERROR = &H4                               '/**< Parity Error */
    BM_LIN_BREAK_ERROR = &H8                                '/**< Break Error */
    BM_LIN_BUS_IDLE_TIMEOUT = &H10                          '/**< Bus idle timeout */
    BM_LIN_TRANSMIT_TIMEOUT = &H20                          '/**< Transmit timeout */
End Enum


''**
' * @enum  BM_CanModeTypeDef
' * @brief CAN mode IDs  used by BM_SetCanMode() to change the operation mode of CAN device.
' *'
Public Enum BM_CanModeTypeDef
    BM_CAN_OFF_MODE = &H1                     '** <The device Is logically disconnected from CAN bus *'
    BM_CAN_NORMAL_MODE = &H0                  '**<The device Is running normally (With the capability To handle CAN And CANFD messages *'
    BM_CAN_SLEEP_MODE = &H1                   '**<The device Is logically disconnected from CAN bus *'
    BM_CAN_INTERNAL_LOOPBACK_MODE = &H2       '**<The device Is looping back messages internally without impacting the physical CAN bus *'
    BM_CAN_LISTEN_ONLY_MODE = &H3             '**<The device Is receiving messages without impacting the physical CAN bus (Do Not send ACKs To the bus) *'
    BM_CAN_CONFIGURATION_MODE = &H4           '**<The device Is under configuration And temporarily disconnected from CAN bus  For Internal usage only *'
    BM_CAN_EXTERNAL_LOOPBACK_MODE = &H5       '**<The device Is looping back messages externally  all transmitted messages are echoed by the device itself *'
    BM_CAN_CLASSIC_MODE = &H6                 '**<The device Is running normally (With the capability To handle only classical CAN2.0 messages *'
    BM_CAN_RESTRICTED_MODE = &H7              '**<Reserved *'
End Enum


''**
' * @enum  BM_TerminalResistorTypeDef
' * @brief Terminal resistor values  used by BM_SetTerminalResistor() to change the terminal resistor of CAN device.
' *'
Public Enum BM_TerminalResistorTypeDef
    BM_TRESISTOR_AUTO = 0               '**< Reserved  currently unsupported *'
    BM_TRESISTOR_60 = 60                '**< Currently unsupported *'
    BM_TRESISTOR_120 = 120              '**< 120Ohm *'
    BM_TRESISTOR_PULLUP_1K = 1000       '**< 1000Ohm pull-up resistor For LIN bus */
    BM_TRESISTOR_DISABLED = &HFFFF      '**< Disable terminal resistor *'
End Enum


''**
' * @enum  BM_LedTypeDef
' * @brief LED indicator status codes  used by BM_SetLed() to change the CAN LED indicator of CAN device.
' *'
Public Enum BM_LedTypeDef
    BM_LED_OFF = 0                      ''** <CAN LED Is OFF *'
    BM_LED_ON = 1                       ''**<CAN LED Is On *'
End Enum


''**
' * @enum  BM_MessageChannelTypeDef
' * @brief Message channel IDs in BM_DataTypeDef header  used for routing indication.
' * @note  You could also use integers directly  please note that valid channel IDs start from zero.
' *'
Public Enum BM_MessageChannelTypeDef
    BM_MESSAGE_CHANNEL_0 = &H0 '** <Channel 0 *'
    BM_MESSAGE_CHANNEL_1 = &H1         '**<Channel 1 *'
    BM_MESSAGE_CHANNEL_2 = &H2         '**<Channel 2 *'
    BM_MESSAGE_CHANNEL_3 = &H3         '**<Channel 3 *'
    BM_MESSAGE_CHANNEL_4 = &H4         '**<Channel 4 *'
    BM_MESSAGE_CHANNEL_5 = &H5         '**<Channel 5 *'
    BM_MESSAGE_CHANNEL_6 = &H6         '**<Channel 6 *'
    BM_MESSAGE_CHANNEL_7 = &H7         '**<Channel 7 *'
    BM_MESSAGE_ANY_CHANNEL = &HF       '**<Any channel  Set this value In BM_DataTypeDef header If Not used (e.g. TX.header.schn Or RX.header.dchn) *'
End Enum

'**
'* @enum  BM_MessageFlagsTypeDef
'* @brief CAN Message type flags  used in BM_CanMessageTypeDef.
'*
Public Enum BM_MessageFlagsTypeDef
    BM_MESSAGE_FLAGS_NORMAL = &H0 '** <Normal CAN message *'
    BM_MESSAGE_FLAGS_IDE = &H1         '**<Extended CAN message *'
    BM_MESSAGE_FLAGS_RTR = &H2         '**<Remote CAN message *'
    BM_MESSAGE_FLAGS_BRS = &H4         '**<CAN-FD bitrate switching Is enabled *'
    BM_MESSAGE_FLAGS_FDF = &H8         '**<CAN-FD message *'
    BM_MESSAGE_FLAGS_ESI = &H10         '**<Reserved For gateways *'
End Enum


'**
'* @enum  BM_RxFilterTypeTypeDef
'* @brief CAN RX filter type IDs  used in BM_RxFilterTypeDef.
'*
Public Enum BM_RxFilterTypeTypeDef
    BM_RXFILTER_INVALID = 0             '** <Invalid (unused) RX filter entry *'
    BM_RXFILTER_BASIC                   '** <Basic RX filter  traditional acceptance filter based on message ID mask *'
    BM_RXFILTER_ADVANCED                '**<Busmust advanced RX filter  check both message ID and message payload *'
    BM_RXFILTER_E2EPASS                 '**<Busmust E2E RX filter  accept only messages that passed E2E checking *'
    BM_RXFILTER_E2EFAIL                 '**<Busmust E2E RX filter  accept only messages that failed E2E checking (for debugging purpose) *'
End Enum


'**
'* @enum  BM_TxTaskTypeTypeDef
'* @brief CAN TX task type IDs  used in BM_TxTaskTypeDef.
'*
Public Enum BM_TxTaskTypeTypeDef
    BM_TXTASK_INVALID = 0               '** <Invalid (unused) TX task entry *'
    BM_TXTASK_FIXED                     '** <Basic TX task  send fixed ID and fixed payload *'
    BM_TXTASK_INCDATA                   '**<Self-increment Data TX task *'
    BM_TXTASK_INCID                     '**<Self-increment ID TX task *'
    BM_TXTASK_RANDOMDATA                '**<Random Data TX task *'
    BM_TXTASK_RANDOMID                  '**<Random ID TX task *'
End Enum


''**
'* @enum  BM_StatTypeDef
'* @brief CAN runtime statistics item IDs  used in BM_GetStat().
'*
Public Enum BM_StatTypeDef

    BM_STAT_NONE = 0 '** <Invalid statistics item *'
    BM_STAT_TX_MESSAGE                  '** <Number of TX messages *'
    BM_STAT_RX_MESSAGE                  '**<Number of RX messages *'
    BM_STAT_TX_BYTE                     '**<Number of TX bytes *'
    BM_STAT_RX_BYTE                     '**<Number of RX bytes *'
    BM_STAT_TX_ERROR                    '**<Number of TX errors *'
    BM_STAT_RX_ERROR                    '**<Number of RX errors *'
End Enum


'**
'* @enum  BM_StorageModeTypeDef
'* @brief Logging or replay mode, used in BM_LoggingConfigTypeDef and BM_ReplayConfigTypeDef.
'*/
Public Enum BM_StorageModeTypeDef
    BM_STORAGE_DISABLED = 0
    BM_STORAGE_ALWAYS_ON
    BM_STORAGE_TRIGGERED
End Enum

'**
'* @enum  BM_StorageDirectionTypeDef
'* @brief Logging Or replay mode, used in BM_LoggingConfigTypeDef And BM_ReplayConfigTypeDef.
'*/
Public Enum BM_StorageDirectionTypeDef
    BM_STORAGE_DIRECTION_NONE = 0
    BM_STORAGE_DIRECTION_RX = 1
    BM_STORAGE_DIRECTION_TX = 2
    BM_STORAGE_DIRECTION_ALL = 3
End Enum

'**
'* @enum  BM_StorageFormatTypeDef
'* @brief Logging Or replay mode, used in BM_LoggingConfigTypeDef And BM_ReplayConfigTypeDef.
'*/
Public Enum BM_StorageFormatTypeDef
    BM_STORAGE_DEFAULT_FORMAT = 0
    BM_STORAGE_BBD_FORMAT = BM_STORAGE_DEFAULT_FORMAT
    BM_STORAGE_PCAP_FORMAT = 1
    BM_STORAGE_LOG_FORMAT = 2
    BM_STORAGE_ASC_FORMAT = 3
    BM_STORAGE_BLF_FORMAT = 4
End Enum

'**
'* @enum  BM_StoragePathModeTypeDef
'* @brief Logging Or replay mode, used in BM_LoggingConfigTypeDef And BM_ReplayConfigTypeDef.
'*/
Public Enum BM_StoragePathModeTypeDef
    BM_STORAGE_FIXED_PATH = 0
    BM_STORAGE_INDEX_PATH
    BM_STORAGE_TIME_PATH
End Enum

'**
'* @enum  BM_LogLevelTypeDef
'* @brief Busmust library log level, see BM_SetLogLevel() for details.
'*
Public Enum BM_LogLevelTypeDef

    BM_LOG_NONE = 0                     '**< Show Nothing On debug console                                                                                '*/
    BM_LOG_ERR                          '** <Show only ERR level messages On debug console, note this Is the Default level For release versions           '*/
    BM_LOG_WRN                          '**<Show ERR And WRN level messages On debug console                                                             '*/
    BM_LOG_INF                          '**<Show ERR|WRN|INF level messages On debug console                                                             '*/
    BM_LOG_DBG                          '**<Show all messages On debug console, including debug messages, note this Is Not available For release versions'*/
End Enum

'**
'* @enum  BM_IsotpModeTypeDef
'* @brief ISOTP operation mode  used in BM_IsotpConfigTypeDef.
'*
Public Enum BM_IsotpModeTypeDef

    BM_ISOTP_NORMAL_TESTER = 0          '** <Default mode:normal(non - extended - addressing) UDS client(tester) *'
    BM_ISOTP_NORMAL_ECU                 '** <normal (non-extended-addressing) UDS server(ECU)                  *'
    BM_ISOTP_EXTENDED_TESTER            '**<Currently unsupported:extended-addressing UDS client(tester)     *'
    BM_ISOTP_EXTENDED_ECU               '**<Currently unsupported:extended-addressing UDS server(ECU)        *'
End Enum

'/**
' * @typedef BM_LinMessageCtrlTypeDef
' * @brief   Busmust LIN Message control fields.
' */
<StructLayout(LayoutKind.Explicit)>
Public Structure BM_LinMessageCtrlTypeDef
    <FieldOffset(0)>
    Public AllBits As UInteger

    ' DLC: 4 bits
    ' LIN message DLC(0-8)
    Public Property DLC() As UInteger
        Get
            Return CUInt(AllBits And &HF)
        End Get
        Set(ByVal value As UInteger)
            AllBits = (AllBits And &HFFFFFFF0) Or CUInt(value And &HF)
        End Set
    End Property

    ' TRANSMIT: 1 bit
    ' Transfer direction: 0=receive, 1=transmit
    Public Property TRANSMIT() As Boolean
        Get
            Return ((AllBits >> 4) And &H1) = 1
        End Get
        Set(ByVal value As Boolean)
            AllBits = (AllBits And &HFFFFFFEF) Or CUInt(If(value, 1 << 4, 0))
        End Set
    End Property

    ' WAKEUP: 1 bit
    ' This is a wakeup event
    Public Property WAKEUP() As Boolean
        Get
            Return ((AllBits >> 5) And &H1) = 1
        End Get
        Set(ByVal value As Boolean)
            AllBits = (AllBits And &HFFFFFFDF) Or CUInt(If(value, 1 << 5, 0))
        End Set
    End Property

    ' SLEEP: 1 bit
    ' This is a sleep event
    Public Property SLEEP() As Boolean
        Get
            Return ((AllBits >> 6) And &H1) = 1
        End Get
        Set(ByVal value As Boolean)
            AllBits = (AllBits And &HFFFFFFBF) Or CUInt(If(value, 1 << 6, 0))
        End Set
    End Property

    ' ENHANCED_CHECKSUM: 1 bit
    ' Flag for enhanced checksum
    Public Property ENHANCED_CHECKSUM() As Boolean
        Get
            Return ((AllBits >> 7) And &H1) = 1
        End Get
        Set(ByVal value As Boolean)
            AllBits = (AllBits And &HFFFFFF7F) Or CUInt(If(value, 1 << 7, 0))
        End Set
    End Property

    ' USER_CHECKSUM: 1 bit
    ' Flag for user defined checksum, use value from 'CHECKSUM' field if USER_CHECKSUM==1
    Public Property USER_CHECKSUM() As Boolean
        Get
            Return ((AllBits >> 8) And &H1) = 1
        End Get
        Set(ByVal value As Boolean)
            AllBits = (AllBits And &HFFFFFEFF) Or CUInt(If(value, 1 << 8, 0))
        End Set
    End Property

    ' SEQ: 7 bits
    ' hardware-sync message ID, the ACK message's SEQ is always equal to the TX message's SEQ or TXTASK index
    Public Property SEQ() As UInteger
        Get
            Return CUInt((AllBits >> 9) And &H7F)
        End Get
        Set(ByVal value As UInteger)
            AllBits = (AllBits And &HFFFFF8FF) Or CUInt((value And &H7F) << 9)
        End Set
    End Property

    ' ISTXTASK: 1 bit
    ' A indicator for TXTASK TEF event, SEQ=TXTASK index if ISTXTASK=1
    Public Property ISTXTASK() As Boolean
        Get
            Return ((AllBits >> 16) And &H1) = 1
        End Get
        Set(ByVal value As Boolean)
            AllBits = (AllBits And &HFFFF7FFF) Or CUInt(If(value, 1 << 16, 0))
        End Set
    End Property

    ' ERRORS: 6 bits
    ' Error code, which is a bitmask, see BM_LinErrorTypeDef for details.
    Public Property ERRORS() As UInteger
        Get
            Return CUInt((AllBits >> 17) And &H3F)
        End Get
        Set(ByVal value As UInteger)
            AllBits = (AllBits And &HFFFFC1FF) Or CUInt((value And &H3F) << 17)
        End Set
    End Property

    ' ID_PARITY: 1 bit
    ' Parity bit
    Public Property ID_PARITY() As Boolean
        Get
            Return ((AllBits >> 23) And &H1) = 1
        End Get
        Set(ByVal value As Boolean)
            AllBits = (AllBits And &HFFEFFFFF) Or CUInt(If(value, 1 << 23, 0))
        End Set
    End Property

    ' CHECKSUM: 8 bits
    ' Checksum value
    Public Property CHECKSUM() As UInteger
        Get
            Return CUInt(AllBits >> 24)
        End Get
        Set(ByVal value As UInteger)
            AllBits = (AllBits And &HFFFFFF) Or CUInt(value << 24)
        End Set
    End Property
End Structure

'/**
' * @typedef BM_DataTailTypeDef
' * @brief   Busmust data tail, each BM_DataTypeDef contains an optional (if header.hastail) tail which indicates packet side-band information, i.e. 64-bit UTC timestamp.
' */
<StructLayout(LayoutKind.Sequential, Pack:=1)>
Public Structure BM_DataTailTypeDef
    Public type As Byte                                ' Tail type, reserved for future, default as zero.

    <MarshalAs(UnmanagedType.ByValArray, SizeConst:=3)>
    Public reserved() As Byte                         ' Reserved bytes

    Public packetid As UShort                         ' Packet id, sequential counter for packet loss detection
    Public checksum As UShort                         ' Checksum of the whole packet (header+data+tail)
    Public utctsl As UInteger                         ' Low 32 bits of 64-bit UTC timestamp (microseconds)
    Public utctsh As UInteger                         ' High 32 bits of 64-bit UTC timestamp (microseconds)

    Public Function GetUtcTimestamp() As ULong
        Return (CLng(utctsh) << 32) Or utctsl
    End Function
End Structure

'/**
'* @typedef BM_LinMessageTypeDef
'* @brief   Busmust CAN Message concrete type, usually used As payload Of BM_DataTypeDef.
'* @note    The total length Of this Structure Is 16B.
'*/
<StructLayout(LayoutKind.Sequential)>
Public Structure BM_LinMessageTypeDef
    Public id As Byte                               '/**< LIN message ID */
    <MarshalAs(UnmanagedType.ByValArray, SizeConst:=3)>
    Public padding() As Byte                        '/**< LIN message control fields, invalid If this Is Not a LIN message. */
    Public ctrl As BM_LinMessageCtrlTypeDef                     '/**< LIN message control fields, invalid If this Is Not a LIN message. */   
    Public payload() As Byte                        '/**< LIN message payload */

    Public Sub New(ctrl As BM_LinMessageCtrlTypeDef,
                   Optional ByVal id As UInteger = 0)
        Me.ctrl = ctrl
        Me.id = CByte((id And &H3F))
        Me.payload = New Byte(7) {}
        Me.padding = New Byte(2) {}
    End Sub

    Public Function GetID() As UInteger
        Return (Me.id And &H3F)
    End Function

    Public Function GetDlc() As UInteger
        Return (Me.ctrl.DLC)
    End Function

    Public Function GetChecksum() As UInteger
        Return (Me.ctrl.CHECKSUM)
    End Function

    Public Function GetPayloadString() As String
        Dim length As UInteger = GetDlc()
        Dim payloadStr As String = ""
        For i As UInteger = 0 To length - 1
            payloadStr &= String.Format("{0:X2} ", Me.payload(i))
        Next
        Return payloadStr.TrimEnd()
    End Function
End Structure

'**
'* @typedef BM_CanMessageTypeDef
'* @brief   Busmust CAN Message concrete type  usually used as payload of BM_DataTypeDef.
'* @note    The total length of this structure Is 72B  it support both classic And FD CAN messages.
'*

<StructLayout(LayoutKind.Sequential)>
Public Structure BM_CanMessageTypeDef
    Dim id As Integer                             '**< CAN message ID  see BM_MessageIdTypeDef For details. *'
    Dim ctrl As UInteger                            '**< CAN message control fields  whether TX Or RX Is taken depends On the message direction. *'
    <MarshalAs(UnmanagedType.ByValArray, SizeConst:=64 + BMAPI.BM_DATA_TAIL_SIZE)>
    Public payload() As Byte                      '**< buffer holding concrete message payload (i.e. a CAN message In BM_CanMessageTypeDef format). *'
    Public Sub BM_CanMessageTypeDef(thisId As Integer, DLC As Integer, IDE As Boolean, RTR As Boolean, BRS As Boolean, FDF As Boolean, ESI As Boolean)
        id = thisId
        If IDE Then
            id = ((id >> 18) And &H7FF) Or ((id And &H3FFFF) << 11)
        Else
            id = id And &H7FF
        End If
        ctrl = (DLC And &HF)
        If IDE Then
            ctrl = ctrl Or 1 << 4
        End If
        If RTR Then
            ctrl = ctrl Or 1 << 5
        End If

        If BRS Then
            ctrl = ctrl Or 1 << 6
        End If

        If FDF Then
            ctrl = ctrl Or 1 << 7
        End If

        If ESI Then
            ctrl = ctrl Or 1 << 8
        End If
        Dim i As Integer
        Dim payloadlen As Integer = 64 + BMAPI.BM_DATA_TAIL_SIZE
        ReDim payload(0 To (payloadlen - 1))
        For i = 0 To (payloadlen - 1)
            payload(i) = 0
        Next
    End Sub
    Public Function GetID() As Integer
        If (IsIDE()) Then
            Return (id And &H7FF) << 18 Or (id >> 11) And &H3FFFF
        Else
            Return id And &H7FF
        End If
    End Function


    Public Function GetDLC() As Integer
        Return (ctrl And &HF)
    End Function

    Public Function IsIDE() As Boolean
        If (ctrl And (1 << 4)) Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function IsRTR() As Boolean
        If (ctrl And (1 << 5)) Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function IsBRS() As Boolean
        If (ctrl And (1 << 6)) Then
            Return True
        Else
            Return False
        End If
    End Function
    Public Function IsFDF() As Boolean
        If (ctrl And (1 << 7)) Then
            Return True
        Else
            Return False
        End If
    End Function
    Public Function IsESI() As Boolean
        If (ctrl And (1 << 8)) Then
            Return True
        Else
            Return False
        End If
    End Function
    Public Function GetLength() As Integer
        Dim DlcToDataBytes() As Integer = New Integer(15) {0, 1, 2, 3,
                                                           4, 5, 6, 7,
                                                           8, 12, 16, 20,
                                                           24, 32, 48, 64}
        Dim length As Integer
        length = DlcToDataBytes(GetDLC)
        Return length
    End Function

    Public Function lengthToDLc(n As Integer) As Integer
        Dim dlc As Integer
        If (n <= 8) Then
            dlc = n
        ElseIf (n <= 12) Then
            dlc = 9
        ElseIf (n <= 16) Then
            dlc = 10
        ElseIf (n <= 20) Then
            dlc = 11
        ElseIf (n <= 24) Then
            dlc = 12
        ElseIf (n <= 32) Then
            dlc = 13
        ElseIf (n <= 48) Then
            dlc = 14
        Else
            dlc = 15
        End If
        Return dlc
    End Function



    Public Function GetTypeString() As String
        Dim Type As String = ""
        If (IsIDE()) Then Type += "IDE "
        If (IsRTR()) Then Type += "RTR "
        If (IsBRS()) Then Type += "BRS "
        If (IsFDF()) Then Type += "FDF "
        If (IsESI()) Then Type += "ESI "
        If (Type.Length() = 0) Then
            Type = "STD"
        End If
        Return Type
    End Function

    Public Function GetPayloadString() As String
        Dim length As Integer
        length = GetLength()
        Dim i As Integer
        Dim thisPayload As String = ""
        For i = 0 To length - 1
            thisPayload = thisPayload + Convert.ToString(payload(i), 16).ToUpper().PadLeft(2, "0") + " "
        Next
        Return thisPayload
    End Function
End Structure


'**
'* @typedef BM_DataHeaderTypeDef
'* @brief   Busmust data header  each BM_DataTypeDef contains a header which indicates payload information.
'*
<StructLayout(LayoutKind.Sequential)>
Public Structure BM_DataHeaderTypeDef

    ''uint16_t type : 4                  '**< Data type  see BM_DataTypeTypeDef for details. *'
    ''uint16_t flags : 4                 '**< Reserved flags  keep 0 *'
    ''uint16_t dchn : 4                  '**< Destination channel ID  starting from zero  used by TX data to indicate the hardware about the target port. *'
    ''uint16_t schn : 4                  '**< Source channel ID  starting from zero  used by RX data to indicate the application about the source port. *'
    Dim value As UShort

    Public Sub BM_DataHeaderTypeDef(type As BM_DataTypeTypeDef, flags As UShort, dchn As UShort, schn As UShort)
        value = (type And (&HF << 0)) Or ((flags And &HF) << 4) Or ((dchn Or &HF) << 8) Or ((schn And &HF) << 12)
    End Sub

    Public Function GetDataType() As BM_DataTypeTypeDef
        Return (value And &HF)
    End Function

    Public Function GetFlags() As Short
        Return ((value >> 4) And &HF)
    End Function
    Public Function GetDstChannel() As Short
        Return ((value >> 8) And &HF)
    End Function

    Public Function GetSrcChannel() As Short
        Return ((value >> 12) And &HF)
    End Function

    Public Function IsAckData() As Boolean
        Return (value And &HF) And BM_DataTypeTypeDef.BM_ACK_DATA
    End Function

    Public Function SetAckData()
        value = value Or BM_DataTypeTypeDef.BM_ACK_DATA
        Return value
    End Function
End Structure


'**
'* @typedef BM_DataTypeDef
'* @brief   Busmust data  abstract structure which holds concrete payload messages of various types (i.e. CAN messages).
'*
<StructLayout(LayoutKind.Sequential)>
Public Structure BM_DataTypeDef

    Public header As BM_DataHeaderTypeDef             '**< data header  see BM_DataHeaderTypeDef For details. *'
    Public length As UShort                           '**< length In bytes Of the payload only (header excluded). *'
    Public timestamp As Integer                       '**< 32-bit device local high precision timestamp In microseconds. *'
    Public can As BM_CanMessageTypeDef                '**< buffer holding concrete message payload (i.e. a CAN message In BM_CanMessageTypeDef format). *'

    Public Sub BM_DataTypeDef(thisHeader As BM_DataHeaderTypeDef, thisLength As UShort, timestamp As Integer, canMsg As BM_CanMessageTypeDef)
        header = thisHeader
        length = thisLength
        timestamp = timestamp
        can = canMsg
    End Sub
End Structure

<StructLayout(LayoutKind.Sequential)>
Public Structure BM_IpAddrTypeDef
    Public b1 As Byte                           '**< Device address byte1, this Is usually a IPV4 address And can be converted To in_addr struct pointer (address In network Byte order) */
    Public b2 As Byte                           '**< Device address byte2, this Is usually a IPV4 address And can be converted To in_addr struct pointer (address In network Byte order) */
    Public b3 As Byte                           '**< Device address byte3, this Is usually a IPV4 address And can be converted To in_addr struct pointer (address In network Byte order) */
    Public b4 As Byte                           '**< Device address byte4, this Is usually a IPV4 address And can be converted To in_addr struct pointer (address In network Byte order) */
End Structure

'**
'* @typedef BM_ChannelInfoTypeDef
'* @brief   Channel information  created when enumerating devices by BM_Enumerate() And used when opening device by BM_OpenEx().
'*
<StructLayout(LayoutKind.Sequential)>
Public Structure BM_ChannelInfoTypeDef
    <MarshalAs(UnmanagedType.ByValArray, SizeConst:=64)>
    Public name() As Byte                              '**< Device full name  For display purpose *'
    <MarshalAs(UnmanagedType.ByValArray, SizeConst:=16)>
    Public sn() As Byte                                '**< Device SN *'
    <MarshalAs(UnmanagedType.ByValArray, SizeConst:=12)>
    Public uid() As Byte                                '**< Device UID *'
    <MarshalAs(UnmanagedType.ByValArray, SizeConst:=4)>
    Public version() As Byte                           '**< Device Firmware Version *'
    Public vid As UShort                               '**< Device VID *'
    Public pid As UShort                               '**< Device PID *'
    Public port As UShort                              '**< Port ID (0-7) Of the device  note a multi-port device Is enumerated As multiple dedicated BM_ChannelInfoTypeDef entries *'
    Public cap As UShort                               '**< Device Capability flags  see BM_CapabilityTypeDef For details. *'
    <MarshalAs(UnmanagedType.ByValArray, SizeConst:=4)>
    Public addr() As Byte                              '**< Device address, this is usually a IPV4 address and can be converted to in_addr struct pointer (address in network byte order) *'

    Private Function ByteArrayToString(d() As Byte) As String
        Dim s As String = System.Text.Encoding.Default.GetString(d)
        Dim index As Integer = s.IndexOf(Chr(0))
        If (index >= 0) Then
            s = s.Remove(index)
        End If
        Return s
    End Function

    Public Function GetName() As String
        Return ByteArrayToString(name)
    End Function
End Structure


'**
'* @typedef BM_CanStatusInfoTypedef
'* @brief   CAN channel status detailed information  retrieved by calling BM_GetCanStatus()  see ISO11898 for details.
'*
<StructLayout(LayoutKind.Sequential)>
Structure BM_CanStatusInfoTypedef

    Public TXBO As Byte                               '**< The CAN channel Is In BUSOFF state *'
    Public reserved As Byte                           '**< Reserved *'
    Public TXBP As Byte                               '**< The CAN channel Is In TX bus passive state *'
    Public RXBP As Byte                               '**< The CAN channel Is In RX bus passive state *'
    Public TXWARN As Byte                             '**< The CAN channel Is In TX warn state *'
    Public RXWARN As Byte                             '**< The CAN channel Is In RX warn state *'
    Public TEC As Byte                                '**< TX Bus Error counter *'
    Public REC As Byte                                '**< RX Bus Error counter *'
End Structure


'**
'* @typedef BM_BitrateTypeDef
'* @brief   CAN channel bitrate configuration  used by BM_SetBitrate().
'*
<StructLayout(LayoutKind.Sequential)>
Public Structure BM_BitrateTypeDef

    Public nbitrate As UShort                  '**< Nominal bitrate In kbps  Default As 500  note this Is the only valid birate In CAN CLASSIC mode. *'
    Public dbitrate As UShort                  '**< Data bitrate In kbps  Default As 500  note this Is ignored In CAN CLASSIC mode. *'
    Public nsamplepos As Byte                  '**< Nominal sample position (percentage)  0-100  Default As 75 *'
    Public dsamplepos As Byte                  '**< Data sample position (percentage)  0-100  Default As 75 *'
    Public clockfreq As Byte                   '**< CAN controller clock In Mhz  Default As 0 *'
    Public reserved As Byte                    '**< Reserved *'
    Public nbtr0 As Byte                       '**< Nominal BTR0 register value  note this value Is calculated Using clockfreq  which might Not be 16MHz *'
    Public nbtr1 As Byte                       '**< Nominal BTR1 register value  note this value Is calculated Using clockfreq  which might Not be 16MHz *'
    Public dbtr0 As Byte                       '**< Data BTR0 register value  note this value Is calculated Using clockfreq  which might Not be 16MHz *'
    Public dbtr1 As Byte                       '**< Data BTR1 register value  note this value Is calculated Using clockfreq  which might Not be 16MHz *'
End Structure


'**
'* @typedef BM_RxFilterTypeDef
'* @brief   CAN channel RX filter item structure  used by BM_SetRxFilter().
'* @note    The filter support masking ID  flags And payload according to its type  
'*          in order for a message to be accepted  all the fields are masked using And logic:
'*          (flags & filter.flags_mask == filter.flags_value) And (ID & filter.id_mask == filter.id_value) And (payload & filter.payload_mask == filter.payload_value)
'*
<StructLayout(LayoutKind.Sequential)>
Public Structure BM_RxFilterTypeDef

    Public type As Byte                               '**< Type ID Of the RX filter  see BM_RxFilterTypeTypeDef For details. *'
    Public unused As Byte                           '**< Reserved *'
    Public flags_mask As Byte                      '**< CAN message control Flags masks  see BM_MessageFlagsTypeDef For details. *'
    Public flags_value As Byte                     '**< CAN message control Flags values  see BM_MessageFlagsTypeDef For details. *'
    Public reserved As Integer                      '**< Reserved *'
    Public id_mask As Integer                     '**< CAN message ID masks  see BM_MessageIdTypeDef For details. *'
    Public id_value As Integer                 '**< CAN message ID values  see BM_MessageIdTypeDef For details. *'
    <MarshalAs(UnmanagedType.ByValArray, SizeConst:=8)>
    Public payload_mask() As Byte                  '**< CAN message payload masks  For CAN-FD messages  only the first 8 bytes are checked. *'
    <MarshalAs(UnmanagedType.ByValArray, SizeConst:=8)>
    Public payload_value() As Byte                 '**< CAN message payload values  For CAN-FD messages  only the first 8 bytes are checked. *'

    Public Function SetIdFilter(mask As Integer, value As Integer, ide As Boolean)
        If (mask > &H7FF Or value > &H7FF Or ide) Then
            id_mask = ((mask >> 18) And &H7FF) Or ((mask And &H3FFFF) << 11)
            id_value = ((value >> 18) And &H7FF) Or ((value And &H3FFFF) << 11)
        Else
            id_mask = mask And &H7FF
            id_value = value And &H7FF
        End If
        Return vbNull
    End Function

End Structure

<StructLayout(LayoutKind.Explicit)>
Public Structure BM_TxTaskIdTypeDef
    <FieldOffset(0)>
    Public id As UInteger

    <FieldOffset(0)>
    Public can As CanId

    <FieldOffset(0)>
    Public lin As LinId
End Structure

<StructLayout(LayoutKind.Explicit)>
Public Structure CanId
    <FieldOffset(0)>
    <MarshalAs(UnmanagedType.U4)>
    Public Value As UInteger

    <FieldOffset(0)>
    <MarshalAs(UnmanagedType.U2)>
    Public SID As UShort ' 11 bits

    <FieldOffset(2)>
    <MarshalAs(UnmanagedType.U2)>
    Public EID As UShort ' 18 bits, but stored in 16 bits here for simplicity, adjust as needed

    <FieldOffset(3)>
    <MarshalAs(UnmanagedType.U1)>
    Public SID11AndUnimplemented As Byte ' 1 bit + 2 bits

    ' Helper properties to access individual bits
    Public ReadOnly Property SID11 As Boolean
        Get
            Return (SID11AndUnimplemented And &H1) <> 0
        End Get
    End Property

    Public ReadOnly Property Unimplemented1 As Byte
        Get
            Return CByte((SID11AndUnimplemented >> 1) And &H3)
        End Get
    End Property
End Structure

<StructLayout(LayoutKind.Sequential)>
Public Structure LinId
    Public ID As Byte
    Public CHECKSUM As Byte
    Public reserved As UShort
End Structure

'**
'* @typedef BM_TxTaskTypeDef
'* @brief   CAN channel TX task item structure  used by BM_SetTxTask().
'* @note    Once the CAN device Is armed with TX tasks  it will try to parse the TX task And send CAN messages automatically.
'*          The difference with a software triggered CAN message in BusMaster Is that 
'*          hardware triggered CAN messages are more precise in time And could reach a higher throughput.
'*
<StructLayout(LayoutKind.Sequential)>
Public Structure BM_TxTaskTypeDef

    Public type As Byte                                '**< Type ID Of the TX task  see BM_TxTaskTypeTypeDef For details. *'
    Public version As Byte                         '**< Version of BM_TxTaskTypeDef, set to 1 for BMAPI1.x. *'
    Public flags As Byte                       '**< CAN message control Flags  see BM_MessageFlagsTypeDef For details. *'
    Public length As Byte                      '**< Length Of payload In bytes (Not DLC) *'
    Public length_msb As Byte                    '**< MSB of Length of payload in bytes (not DLC), set to 0 if not used (e.g. for CANFD messages) *'
    Public reserved As Byte                  '**< Reserved *'

    Public cycle As UShort                    '**< ms delay between rounds *'
    Public nrounds As UShort               '**< num Of cycles *'
    Public nmessages As UShort             '**< messages per round *'

    Public id As BM_TxTaskIdTypeDef                             '**< CAN message arbitration id  see BM_MessageIdTypeDef For details. *'

    <MarshalAs(UnmanagedType.ByValArray, SizeConst:=32)>
    Public pattern() As Byte                                '**< Reserved *'

    Public e2e As Byte                                 '**< Index Of E2E (In E2E table), currently unsupported'*/
    <MarshalAs(UnmanagedType.ByValArray, SizeConst:=15)>
    Public e2e_reserved() As Byte                                '**< Reserved *'

    <MarshalAs(UnmanagedType.ByValArray, SizeConst:=64)>
    Public payload() As Byte                     '**< Default payload data  note this Is also the template payload Of the unchanged part In a volatile TX task *'
End Structure


''**
'* @typedef BM_IsotpStatusTypeDef
'* @brief   ISOTP status report  used by ISOTP operation callback function.
'*
Public Structure BM_IsotpStatusTypeDef

    Public version As Byte                      '**< Currently always &H01 *'
    Public flowcontrol As Byte                   '**< Current flow control status  0=Continue  1=wait  2=overflow  ff=timeout  *'
    Public stmin As Byte                   '**< Current flow control status  i.e. 30 00 00 *'
    Public blocksize As Byte                 '**< Current flow control status  i.e. 30 00 00 *'
    Public ntransferredbytes As Integer             '**< Number Of transferred bytes by now. *'
    Public ntotalbytes As Integer                 '**< Number Of total bytes indicated by ISOTP FF Or SF. *'
    Public timestamp As Integer                 '**< Current timestamp reported by device. *'
    Public reserved1 As Integer                 '**< Reserved For future *'
    Public reserved2 As Integer               '**< Reserved For future *'
    Public reserved3 As Integer               '**< Reserved For future *'
    Public reserved4 As Integer              '**< Reserved For future *'
End Structure


'**
'* @typedef BM_IsotpTimeoutConfigTypeDef
'* @brief   ISOTP Protocol (See ISO15765-2 for details) timeout configuration  used by BM_ConfigIsotp().
'*'
<StructLayout(LayoutKind.Sequential)>
Public Structure BM_IsotpTimeoutConfigTypeDef
    Public a As UShort                    '**< A timeout In milliseconds:=N_As If writing As tester Or reading As ECU  otherwise =N_Ar *'
    Public b As UShort                    '**< B timeout In milliseconds:=N_Bs If writing As tester Or reading As ECU  otherwise =N_Br *'
    Public c As UShort                    '**< C timeout In milliseconds:=N_Cs If writing As tester Or reading As ECU  otherwise =N_Cr *'
End Structure


'**
'* @typedef BM_IsotpFlowcontrolConfigTypeDef
'* @brief   ISOTP Protocol (See ISO15765-2 for details) flowcontrol configuration  used by BM_ConfigIsotp().
'*
<StructLayout(LayoutKind.Sequential)>
Public Structure BM_IsotpFlowcontrolConfigTypeDef

    Public stmin As Byte                          '**< STmin raw value (&H00-&H7F Or &HF1-&HF9) If Busmust device Is acting As UDS server       *'
    Public blocksize As Byte                      '**< Blocksize If can card Is acting As UDS server  0 means no further FC Is needed           *'
    Public fcFrameLength As Byte                  '**< Flow control frame length In bytes                                                       *'
    Public reserved As Byte
End Structure

'**
'* @typedef BM_IsotpConfigTypeDef
'* @brief   ISOTP Protocol (See ISO15765-2 for details) configuration  used by BM_ConfigIsotp().
'*
<StructLayout(LayoutKind.Sequential)>
Public Structure BM_IsotpConfigTypeDef

    Public version As Byte                              '**< Currently must be Set To &H01                                                            *'
    Public mode As Byte                           '**< Currently only 0 Is supported: normal(non - extended - addressing) UDS client(tester)       *'
    Public testerTimeout As BM_IsotpTimeoutConfigTypeDef    '**< Tester ISOTP timeout configuration                                                       *'
    Public ecuTimeout As BM_IsotpTimeoutConfigTypeDef  '**< ECU ISOTP timeout configuration                                                          *'
    Public flowcontrol As BM_IsotpFlowcontrolConfigTypeDef '**< ISOTP Flowcontrol configuration                                                          *'
    Public extendedAddress As Byte                '**< UDS Address In Extended Addressing mode                                                  *'
    Public paddingEnabled As Byte               '**< Enable padding For unused payload bytes'                                                 *'
    Public paddingValue As Byte               '**< Padding Byte value (i.e. &HCC) For unused payload bytes                                  *'
    Public longPduEnabled As Byte             '**< Enable Long PDU (>4095)  note If CAN message DLC>8  Long PDU Is enabled by Default       *'
    Public padding As UShort
    Public callbackFunc As IntPtr                 '**< TODO: Callback Function when() any progress Is made  used typically by GUI To show progress bar  *'
    Public callbackUserarg As IntPtr                    '**< TODO: Callback userarg When any progress Is made  used typically by GUI to show progress bar   *'
    Public testerDataTemplate As BM_DataTypeDef        '**< All tester messages will be formatted'checked Using this template  configure CAN message ID And IDE'FDF flags here  *'
    Public ecuDataTemplate As BM_DataTypeDef        '**< All ECU messages will be formatted'checked Using this template  configure CAN message ID And IDE'FDF flags here *'
End Structure

'**
'* @typedef BM_PathSpecTypeDef
'* @brief   File Path spec (on data files), used by BM_LoggingConfigTypeDef And BM_ReplayConfigTypeDef.
'*/
<StructLayout(LayoutKind.Sequential)>
Public Structure BM_PathSpecTypeDef
    Public mode As Byte                            '**< Naming mode, see BM_StoragePathModeTypeDef For details                                  '*/
    Public arg As Byte                             '**< Mode argument, reserved For future                                                      '*/
    <MarshalAs(UnmanagedType.ByValArray, SizeConst:=30)>
    Public format() As Char                        '**< A printf-Like String, used by path mode parser                                          '*/
End Structure

'**
'* @typedef BM_EventTriggerTypeDef
'* @brief   Event trigger (on rx messages), used by BM_LoggingConfigTypeDef And BM_ReplayConfigTypeDef.
'*/
<StructLayout(LayoutKind.Sequential)>
Public Structure BM_EventTriggerTypeDef
    Public channels As UShort                       '**< A Bitmask Of RX channels, only channels With '1' are allowed, channels=0 means invalid  '*/
    Public reserved As UShort
    Public flags_mask As UShort                     '**< Source message flag mask, message will be routed If msg.flags & flagsmask == flagsvalue '*/
    Public flags_value As UShort                    '**< Source message flag value, See BM_MessageFlagsTypeDef For details                       '*/
    Public id_mask As UInt32                        '**< Source message ID mask, message will be routed If msg.id & idmask == idvalue            '*/
    Public id_value As UInt32                       '**< Source message ID value, See BM_MessageIdTypeDef For details                            '*/
End Structure

<StructLayout(LayoutKind.Sequential)>
Public Structure BM_LoggingSegmentationTypeDef
    '**< For FIXED PATH mode: Remove all data In existing logging file On logging startup/trigger, otherwise append To existing file     '*/
    '**< For INDEX PATH mode: Increase file index And create New file On logging startup/trigger, otherwise Continue Using existing file '*/
    Public createNewFileOnStart As Byte
    Public overwriteOldFileOnFull As Byte         '**< Enable overwriting, e.g. Overwrite 000.bbd If failed To create 003.bbd since disk Is full*/
    Public nfiles As UShort                       '**< Max number Of files, file name will wrap around On upper limit                          '*/
    Public nmessagesPerFile As UInt32             '**< Max number Of messages per log file (currently unsupported)                             '*/
    Public nbytesPerFile As UInt32                '**< Max number Of bytes per log file                                                        '*/
    Public nsecondsPerFile As UInt32              '**< Max number Of seconds per log file                                                      '*/
End Structure

<StructLayout(LayoutKind.Sequential)>
Public Structure BM_ReplayTimingTypeDef
    Public msgdelay As UShort                       '**< Delay In ms between replay messages, 0 means keeping original delay                   '*/
    Public sessiondelay As UShort                   '**< Delay In ms between replay sessions, 0 means keeping original delay                   '*/
    Public cycledelay As UShort                     '**< Delay In ms between replay cycles, 0 means keeping original delay                     '*/
    Public forceZeroTimestampOnFirstMsg As Byte     '**< Align 1St replayable message With zero timestamp, And send it immediately On startup  '*/
    Public reserved As Byte
End Structure

'**
'* @typedef BM_LoggingConfigTypeDef
'* @brief   Logging configuration, used by BM_ConfigLogging().
'*/
<StructLayout(LayoutKind.Sequential)>
Public Structure BM_LoggingConfigTypeDef
    Public version As Byte                                 '**< Currently must be Set To 0x01                                                         '*/
    Public mode As Byte                                    '**< Logging mode, see BM_StorageModeTypeDef For details                                    '*/
    Public format As Byte                                  '**< Logging file format: See BM_StorageFormatTypeDef For details.                          '*/
    Public reserved As Byte
    Public channels As UShort                              '**< A bitmask Of TX channels, only channels With '1' are allowed as logging source channel'*/
    Public direction As Byte                               '**< Logging direction, see BM_StorageDirectionTypeDef For details                          '*/
    <MarshalAs(UnmanagedType.ByValArray, SizeConst:=9)>
    Public padding() As Byte
    Public path As BM_PathSpecTypeDef                      '**< Pathspec To find available files, see BM_StoragePathModeTypeDef For details.          '*/
    Public starttrigger As BM_EventTriggerTypeDef          '**< Trigger condition, replay starts automatically When it Is met, only valid If mode=2   '*/
    Public stoptrigger As BM_EventTriggerTypeDef           '**< Trigger condition, replay stops automatically When it Is met, only valid If mode=2    '*/
    Public segmentation As BM_LoggingSegmentationTypeDef   '**< Log File segmentation criteria, Or logic                                                '*/
End Structure

'**
'* @typedef BM_ReplayConfigTypeDef
'* @brief   Replay configuration, used by BM_ConfigReplay().
'*/
<StructLayout(LayoutKind.Sequential)>
Public Structure BM_ReplayConfigTypeDef
    Public version As Byte                                 '**< Currently must be Set To 0x01                                                         '*/
    Public mode As Byte                                    '**< Replay mode, see BM_StorageModeTypeDef For details                                    '*/
    Public format As Byte                                  '**< Replay file format: See BM_StorageFormatTypeDef For details.                          '*/
    Public reserved As Byte
    Public channels As UShort                              '**< A bitmask Of TX channels, only channels With '1' are allowed as logging source channel'*/
    Public direction As Byte                               '**< Replay direction, see BM_StorageDirectionTypeDef For details                          '*/
    Public cyclic As Byte                                  '**< Replay Is cyclic, otherwise the device only replay input file once On startup         '*/
    <MarshalAs(UnmanagedType.ByValArray, SizeConst:=8)>
    Public padding() As Byte
    Public path As BM_PathSpecTypeDef                      '**< Pathspec To find available files, see BM_StoragePathModeTypeDef For details.          '*/
    Public starttrigger As BM_EventTriggerTypeDef          '**< Trigger condition, replay starts automatically When it Is met, only valid If mode=2   '*/
    Public stoptrigger As BM_EventTriggerTypeDef           '**< Trigger condition, replay stops automatically When it Is met, only valid If mode=2    '*/
    Public timing As BM_ReplayTimingTypeDef                '**< Replay timing'*/
End Structure

Class BMAPI
    Public Const BM_DATA_TAIL_SIZE As UInteger = 16
    Public Const BM_DATA_HEADER_SIZE As UInteger = 8

    '**
    '* @brief  Initialize BMAPI library  this function shall be called before any other API calls And shall only be called once.
    '* @return Operation status  see BM_StatusTypeDef for details.
    '*
    Public Declare Function BM_Init Lib "BMAPI" () As BM_StatusTypeDef

    '**
    '* @brief  Un-initialize BMAPI library  this function shall be called after any other API calls And shall only be called once.
    '* @return Operation status  see BM_StatusTypeDef for details.
    '*
    Public Declare Function BM_UnInit Lib "BMAPI" () As BM_StatusTypeDef

    '**
    '* @brief        Enumerate all connected Busmust device.
    '* @param[out]   channelinfos  An array of BM_ChannelInfoTypeDef structure which holds info of all the enumerated Busmust devices.
    '* @param[inout] nchannels     Number of device channels available  which Is also the number of valid entries in channelinfos  
    '*                             this param must be initialized with the maximum length of the channelinfos array when calling this function.
    '* @return       Operation status  see BM_StatusTypeDef for details.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_Enumerate(ByVal channelinfos As IntPtr, ByRef nchannels As Integer) As BM_StatusTypeDef
    End Function

    '/**
    '* @brief        Enumerate all connected Busmust device.
    '* @param[out]   channelinfos  An array of BM_ChannelInfoTypeDef structure which holds info of all the enumerated Busmust devices.
    '* @param[inout] nchannels     Number of device channels available, which Is also the number of valid entries in channelinfos,
    '*                             this param must be initialized with the maximum length of the channelinfos array when calling this function.
    '* @param[in]    cap           A bitmask of channel capability (see BM_CapabilityTypeDef for details).
    '*                             e.g. Call BM_EnumerateByCap(channelinfos, nchannels, BM_CAN_FD_CAP) to find all available CANFD channels.
    '* @return       Operation status, see BM_StatusTypeDef for details.
    '*/
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_EnumerateByCap(ByVal channelinfos As IntPtr, ByRef nchannels As Integer, cap As UShort) As BM_StatusTypeDef
    End Function

    '**
    '* @brief        Enumerate all remote Busmust devices.
    '* @param[out]   channelinfos  An array of BM_ChannelInfoTypeDef structure which holds info of all the enumerated Busmust devices.
    '* @param[inout] nchannels     Number of device channels available, which Is also the number of valid entries in channelinfos,
    '*                             this param must be initialized with the maximum length of the channelinfos array when calling this function.
    '* @param[in]    ipv4          The IPV4 unicast/broadcast address in which to find remote devices, in network byte order.
    '*                             e.g. ipv4 = {192, 168, 0, 255} indicates broadcasting within 192.168.0.x 
    '*                             e.g. ipv4 = {192, 168, 0, 100} indicates enumerating 192.168.0.100 only
    '* @param[in]    timeout       Timeout (in milliseconds) before waiting response from remote devices.
    '* @return       Operation status, see BM_StatusTypeDef for details.
    '*/
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_EnumerateRemote(ByVal channelinfos As IntPtr, ByRef nchannels As Integer, ByVal ipv4 As Byte(), timeout As Integer) As BM_StatusTypeDef
    End Function

    ''**
    '* @brief Open the specified CAN device port.
    '* @param[in] port  Index of the port  starting from zero  note this Is the index of all enumerated ports.
    '* @return Handle to the opened CAN device channel  return NULL if failed to open the specified port.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_OpenCan(index As Integer) As IntPtr
    End Function

    '**
    '* @brief Open the specified device port using given configuration.
    '* @param[out] handle      Handle to the opened device channel.
    '* @param[in]  channelinfo Info of the device channel to open  usually the info Is filled by BM_Enumerate().
    '* @param[in]  mode        CAN operation mode option of the opened channel  see BM_CanModeTypeDef for details.
    '* @param[in]  tres        Terminal resistor option of the opened channel  see BM_TerminalResistorTypeDef for details.
    '* @param[in]  bitrate     Bitrate option of the opened channel  see BM_BitrateTypeDef for details.
    '* @param[in]  rxfilter    CAN acceptance filters option of the opened channel  see BM_RxFilterTypeDef for details.
    '* @param[in]  nrxfilters  Number of acceptance filters  usually there could be up to 2 filters.
    '* @return Operation status  see BM_StatusTypeDef for details.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_OpenEx(
        ByRef handle As IntPtr,
        ByRef channelinfo As BM_ChannelInfoTypeDef,
        mode As BM_CanModeTypeDef,
        tres As BM_TerminalResistorTypeDef,
        ByRef bitrate As BM_BitrateTypeDef,
        rxfilter As IntPtr,
        nrxfilters As Integer
    ) As BM_StatusTypeDef
    End Function
    ''**
    '* @brief     Close an opened channel.
    '* @param[in] handle  Handle to the channel to be closed.
    '* @return    Operation status  see BM_StatusTypeDef for details.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_Close(handle As IntPtr) As BM_StatusTypeDef
    End Function


    ''**
    '* @brief     Reset an opened channel.
    '* @param[in] handle  Handle to the channel to be reset.
    '* @return    Operation status  see BM_StatusTypeDef for details.
    '* @note      The configuration options will Not lost when the channel Is reset  so BM_Reset() Is basically identical to BM_Close() And then BM_OpenEx().
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_Reset(handle As IntPtr) As BM_StatusTypeDef
    End Function

    ''**
    '* @brief     Reset the device hardware which an opened channel belongs to, in case the device hardware is in unknown error state and is unrecoverable using BM_OpenEx.
    '* @param[in] handle  Handle to a opened channel (which belongs to the physical device to reset).
    '* @return    Operation status, see BM_StatusTypeDef for details.
    '* @note      !!!CAUTION!!! Note this API will break all active USB/Ethernet connection, just Like you have manually unplugged it And then plugged it back.
    '*            All opened channel handles that belongs to the physical device under reset will be invalid after reset.
    '*            You MUST re-open And re-configure all channels using BM_OpenEx Or other configuration APIs after reset.
    '*/
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_ResetDevice(handle As IntPtr) As BM_StatusTypeDef
    End Function

    '**
    '* @brief     Activate an opened channel  And thus goes on bus for the selected port And channels. 
    '             At this point  the user can transmit And receive messages On the bus.
    '* @param[in] handle  Handle to the channel to be activated.
    '* @return    Operation status  see BM_StatusTypeDef for details.
    '* @note      Channel Is default to be activated after BM_OpenEx() Is called.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_Activate(handle As IntPtr) As BM_StatusTypeDef
    End Function

    '**
    '* @brief     Deactivate an opened channel  And thus the selected channels goes off the bus And stay in BUSOFF state until re-activation.
    '* @param[in] handle  Handle to the channel to be deactivated.
    '* @return    Operation status  see BM_StatusTypeDef for details.
    '* @note      Any call to BM_Write() Or BM_Read() will return BM_ERROR_BUSOFF immediately if the channel Is deactivated.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_Deactivate(handle As IntPtr) As BM_StatusTypeDef
    End Function

    '**
    '* @brief     Clear TX&RX message buffer of an opened channel.
    '* @param[in] handle  Handle to the channel to be cleared.
    '* @return    Operation status  see BM_StatusTypeDef for details.
    '* @note      This function Is available since BMAPI1.3  hardware status will Not be changed when clearing buffer.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_ClearBuffer(handle As IntPtr) As BM_StatusTypeDef
    End Function

    '**
    '* @brief      Read any message'event out of the given channel.
    '* @param[in]  handle  Handle to the channel to read from.
    '* @param[out] data    A caller-allocated buffer to hold the message'event output  see BM_DataTypeDef for details.
    '* @return     Operation status  see BM_StatusTypeDef for details.
    '* @note       This function Is non-blocked  And thus will return BM_ERROR_QRCVEMPTY if no message Is received.
    '*             Please use notifications to wait for Rx events And then read message'event out of BMAPI internal RX buffer  otherwise you could also poll the device periodically.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_Read(handle As IntPtr, ByRef data As BM_DataTypeDef) As BM_StatusTypeDef
    End Function

    '**
    '* @brief        Read multiple messages'events out of the given channel.
    '* @param[in]    handle  Handle to the channel to read from.
    '* @param[out]   data       A caller-allocated buffer to hold the messages'events array output  see BM_DataTypeDef for details.
    '* @param[inout] nmessages  Number of read messages  user shall initialize this param with the size (in messages) of the data buffer.
    '* @param[in]    timeout    Timeout (in milliseconds) before the message Is received successfully from the bus.
    '*                          Set any negative number (i.e. -1) to wait infinitely.
    '*                          Set 0 if you would Like to receive asynchronously: read from BMAPI internal buffer And Return immediately  use BM_WaitForNotifications() before reading.
    '* @return       Operation status  see BM_StatusTypeDef for details.
    '* @note         This function Is non-blocked  And thus will return BM_ERROR_QRCVEMPTY if Not all messages are received.
    '*               Please use notifications to wait for Rx events And then read message'event out of BMAPI internal RX buffer  otherwise you could also poll the device periodically.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_ReadMultiple(handle As IntPtr, ByRef data As IntPtr, nmessages As Integer, timeout As Integer) As BM_StatusTypeDef
    End Function

    '**
    '* @brief        Read data block using ISOTP protocol.
    '*               This API enables rapid transmission using ISOTP without app intervention  a simple example would be reading VIN using UDS:
    '*               uint8_t request[] =  &H22  &HF1  &H80 End Enum
    '*               uint8_t response[4096]
    '*               uint32_t nbytes = sizeof(response)
    '*               BM_WriteIsotp(channel  request  sizeof(request)  config)
    '*               BM_ReadIsotp(channel  response  nbytes  config)
    '*               assert(response[0] == &H62 && response[1] == &HF1 && response[2] == &H80)
    '* @param[in]    handle    Handle to the channel to read from.
    '* @param[in]    data      A caller-allocated buffer to hold the data block output.
    '* @param[inout] nbytes    Length of the received data block  in bytes. Caller must initialize this arg with the size of the caller-allocated buffer.
    '* @param[in]    timeout   Timeout (in milliseconds) before the message Is received successfully from the bus.
    '*                         Set any negative number (i.e. -1) to wait infinitely.
    '*                         Set 0 if you would Like to receive asynchronously: read from BMAPI internal buffer And Return immediately  use BM_WaitForNotifications() before reading.
    '* @param[in]    config    ISOTP configuration used by current transfer.
    '* @return     Operation status  see BM_StatusTypeDef for details.
    '* @note       This function Is allowed to be called from multiple threads since BMAPI1.5.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_ReadIsotp(handle As IntPtr, data As IntPtr, ByRef nbytes As Integer, timeout As Integer, ByRef config As BM_IsotpConfigTypeDef) As BM_StatusTypeDef
    End Function

    Public Shared Function BM_ReadIsotpBytes(handle As IntPtr, ByRef payload() As Byte, ByRef nbytes As Integer, timeout As Integer, ByRef config As BM_IsotpConfigTypeDef) As BM_StatusTypeDef
        Dim infomem As IntPtr = Marshal.UnsafeAddrOfPinnedArrayElement(payload, payload.Length)
        Dim status As BM_StatusTypeDef = BM_ReadIsotp(handle, infomem, nbytes, timeout, config)
        Marshal.Copy(infomem, payload, 0, nbytes)
        Return status
    End Function

    '**
    '* @brief      Read CAN message out of the given channel.
    '* @param[in]  handle     Handle to the channel to read from.
    '* @param[out] msg        A caller-allocated buffer to hold the CAN message output  see BM_CanMessageTypeDef for details.
    '* @param[out] channel    The source channel ID from which the message Is received  starting from zero  could be NULL if Not required.
    '* @param[out] timestamp  The device local high precision timestamp in microseconds  when the message Is physically received on the CAN bus  could be NULL if Not required.
    '* @return     Operation status  see BM_StatusTypeDef for details. 
    '* @note       Note this function Is a simple wrapper of BM_Read()  see BM_Read() for details.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_ReadCanMessage(handle As IntPtr, ByRef msg As BM_CanMessageTypeDef, ByRef channel As Integer, ByRef timestamp As Integer) As BM_StatusTypeDef
    End Function
    '**
    '* @brief        Read multiple CAN messages out of the given channel.
    '* @param[in]    handle  Handle to the channel to read from.
    '* @param[out]   data       A caller-allocated buffer to hold the CAN message array output  see BM_CanMessageTypeDef for details.
    '* @param[inout] nmessages  Number of read messages  user shall initialize this param with the size (in messages) of the data buffer.
    '* @param[in]    timeout    Timeout (in milliseconds) before the message Is received successfully from the bus.
    '*                          Set any negative number (i.e. -1) to wait infinitely.
    '*                          Set 0 if you would Like to receive asynchronously: read from BMAPI internal buffer And Return immediately  use BM_WaitForNotifications() before reading.
    '* @param[out]   channel    The source channel ID from which the message Is received  starting from zero  could be NULL if Not required.
    '* @param[out]   timestamp  The device local high precision timestamp array in microseconds  when the message Is physically transmitted on the CAN bus  could be NULL if Not required.
    '* @return       Operation status  see BM_StatusTypeDef for details.
    '* @note         This function Is non-blocked  And thus will return BM_ERROR_QRCVEMPTY if Not all messages are received.
    '*               Please use notifications to wait for Rx events And then read message'event out of BMAPI internal RX buffer  otherwise you could also poll the device periodically.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_ReadMultipleCanMessage(handle As IntPtr, msg As IntPtr, ByRef nmessages As Integer, timeout As Integer, channel As IntPtr, timestamp As IntPtr) As BM_StatusTypeDef
    End Function
    '**
    '* @brief      Write any message'event to the given channel.
    '* @param[in]  handle  Handle to the channel to write to.
    '* @param[in]  data      A caller-allocated buffer to hold the message'event input  see BM_DataTypeDef for details.
    '* @param[in]  timeout   Timeout (in milliseconds) before the message Is transmitted successfully to the bus.
    '*                       Set any negative number (i.e. -1) to wait infinitely.
    '*                       Set 0 if you would Like to transmit asynchronously: put to BMAPI internal buffer And return immediately  then receive TXCMPLT event over BM_Read() later.
    '* @param[in]  timestamp The device local high precision timestamp in microseconds  when the message Is physically transmitted on the CAN bus  could be NULL if Not required.
    '* @return     Operation status  see BM_StatusTypeDef for details.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_Write(handle As IntPtr, ByRef data As BM_DataTypeDef, timeout As Integer, ByRef timestamp As Integer) As BM_StatusTypeDef
    End Function
    '**
    '* @brief        Write multiple messages'events to the given channel.
    '* @param[in]    handle    Handle to the channel to write to.
    '* @param[in]    data      A caller-allocated buffer to hold the messages'events array input  see BM_DataTypeDef for details.
    '* @param[inout] nmessages Number of written messages  user shall initialize this param with the size (in messages) of the data buffer.
    '* @param[in]    timeout   Timeout (in milliseconds) before the message Is transmitted successfully to the bus.
    '*                         Set any negative number (i.e. -1) to wait infinitely.
    '*                         Set 0 if you would Like to transmit asynchronously: put to BMAPI internal buffer And return immediately  then receive TXCMPLT event over BM_Read() later.
    '* @param[out]   timestamp The device local high precision timestamp array in microseconds  when the message Is physically transmitted on the CAN bus  could be NULL if Not required.
    '* @return     Operation status  see BM_StatusTypeDef for details.
    '* @note       This function Is allowed to be called from multiple threads since BMAPI1.3.
    '*

    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_WriteMultiple(handle As IntPtr, data As IntPtr, ByRef nmessages As Integer, timeout As Integer, timestamp As IntPtr) As BM_StatusTypeDef
    End Function
    '**
    '* @brief        Write data block using ISOTP protocol.
    '*               This API enables rapid transmission using ISOTP without app intervention  a simple example would be writing VIN using UDS:
    '*               uint8_t request[] =  &H2E  &HF1  &H80  ... ... End Enum
    '*               BM_WriteIsotp(channel  request  sizeof(request)  config)
    '* @param[in]    handle    Handle to the channel to write to.
    '* @param[in]    data      A caller-allocated buffer to hold the data block input.
    '* @param[in]    nbytes    Length of the data block  in bytes.
    '* @param[in]    timeout   Timeout (in milliseconds) before any message segment Is transmitted successfully to the bus.
    '*                         Note this Is only for bus level timeout waiting for CAN ACK  for setting ISOTP protocol timeouts  see BM_IsotpConfigTypeDef for details.
    '* @param[in]    config    ISOTP configuration used by current transfer.
    '* @return     Operation status  see BM_StatusTypeDef for details.
    '* @note       This function Is allowed to be called from multiple threads since BMAPI1.5.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_WriteIsotp(handle As IntPtr, data As IntPtr, nbytes As Integer, timeout As Integer, ByRef config As BM_IsotpConfigTypeDef) As BM_StatusTypeDef
    End Function

    Public Shared Function BM_WriteIsotpBytes(handle As IntPtr, payload() As Byte, timeout As Integer, ByRef config As BM_IsotpConfigTypeDef) As BM_StatusTypeDef
        Dim infomem As IntPtr = Marshal.UnsafeAddrOfPinnedArrayElement(payload, payload.Length)
        Return BM_WriteIsotp(handle, infomem, payload.Length, timeout, config)
    End Function

    '**
    '* @brief      Write CAN message to the given channel.
    '* @param[in]  handle     Handle to the channel to write to.
    '* @param[in]  msg        A caller-allocated buffer to hold the CAN message output  see BM_CanMessageTypeDef for details.
    '* @param[in]  channel    The target channel ID to which the message Is transmitted  starting from zero.
    '* @param[in]  timeout   Timeout (in milliseconds) before the message Is transmitted successfully to the bus.
    '*                       Set any negative number (i.e. -1) to wait infinitely.
    '*                       Set 0 if you would Like to transmit asynchronously: put to BMAPI internal buffer And return immediately  then receive TXCMPLT event over BM_Read() later.
    '* @param[in]  timestamp The device local high precision timestamp in microseconds  when the message Is physically transmitted on the CAN bus  could be NULL if Not required.
    '* @note       Note this function Is a simple wrapper of BM_Write()  see BM_Write() for details.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_WriteCanMessage(handle As IntPtr, ByRef msg As BM_CanMessageTypeDef, channel As Integer, timeout As Integer, ByRef timestamp As Integer) As BM_StatusTypeDef
    End Function

    '/**
    '* @brief      Write LIN message to the given channel.
    '* @param[in]  handle     Handle to the channel to write to.
    '* @param[in]  msg        A caller-allocated buffer to hold the LIN message output, see BM_LinMessageTypeDef for details.
    '* @param[in]  _channel   The target channel ID to which the message Is transmitted, starting from zero. This parameter Is reserved for future, always 0 now.
    '* @param[in]  timeout   Timeout (in milliseconds) before the message Is transmitted successfully to the bus.
    '*                       Set any negative number (i.e. -1) to wait infinitely.
    '*                       Set 0 if you would Like to transmit asynchronously: put to BMAPI internal buffer And return immediately, then receive TXCMPLT event over BM_Read() later.
    '* @param[in]  timestamp The device local high precision timestamp in microseconds, when the message Is physically transmitted on the LIN bus, could be NULL if Not required.
    '* @note       Note this function Is a simple wrapper of BM_Write(), see BM_Write() for details.
    '*/
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_WriteLinMessage(handle As IntPtr, ByRef msg As BM_LinMessageTypeDef, channel As Integer, timeout As Integer, ByRef timestamp As Integer) As BM_StatusTypeDef
    End Function

    '**
    '* @brief        Write multiple CAN messages to the given channel.
    '* @param[in]    handle    Handle to the channel to write to.
    '* @param[in]    msg       A caller-allocated buffer to hold the CAN message array input  see BM_CanMessageTypeDef for details.
    '* @param[inout] nmessages Number of written messages  user shall initialize this param with the size (in messages) of the data buffer.
    '* @param[in]    _channel  The target channel ID to which the message Is transmitted  starting from zero. This parameter Is reserved for future  always 0 now  Or simply pass NULL into the API.
    '* @param[in]    timeout   Timeout (in milliseconds) before the message Is transmitted successfully to the bus.
    '*                         Set any negative number (i.e. -1) to wait infinitely.
    '*                         Set 0 if you would Like to transmit asynchronously: put to BMAPI internal buffer And return immediately  then receive TXCMPLT event over BM_Read() later.
    '* @param[out]   timestamp The device local high precision timestamp array in microseconds  when the message Is physically transmitted on the CAN bus  could be NULL if Not required.
    '* @return     Operation status  see BM_StatusTypeDef for details.
    '* @note       This function Is allowed to be called from multiple threads since BMAPI1.3.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_WriteMultipleCanMessage(handle As IntPtr, msg As IntPtr, ByRef nmessages As Integer, _channel As IntPtr, timeout As Integer, timestamp As IntPtr) As BM_StatusTypeDef
    End Function
    '**
    '* @brief Control the given channel  this Is an advanced interface And Is typically used internally by BMAPI.
    '* @param[in]    handle   Handle to the channel to control.
    '* @param[in]    command  Control command.
    '* @param[in]    value    Control value.
    '* @param[in]    index    Control index.
    '* @param[inout] data     Control data  could be NULL.
    '* @param[inout] nbytes   Length in bytes of the control data  could be zero.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_Control(handle As IntPtr, command As Byte, value As UShort, index As UShort, data As IntPtr, nbytes As Integer) As BM_StatusTypeDef
    End Function
    '**
    '* @brief      Get current CAN status of the given channel.
    '* @param[in]  handle      Handle to the channel to operate on.
    '* @param[out] statusinfo  Detailed information of current CAN status  see BM_CanStatusInfoTypedef for details.
    '* @return     Current status code  see BM_StatusTypeDef for details.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_GetStatus(handle As IntPtr, ByRef statusinfo As BM_CanStatusInfoTypedef) As BM_StatusTypeDef
    End Function
    '**
    '* @brief      Get current local high precision device timestamp  in microseconds.
    '* @param[in]  handle     Handle to the channel to operate on.
    '* @param[out] timestamp  Timestamp value.
    '* @return     Operation status  see BM_StatusTypeDef for details.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_GetTimestamp(handle As IntPtr, ByRef timestamp As Integer) As BM_StatusTypeDef
    End Function

    ' /**
    ' * @brief      Set PTP timestamp in nano-seconds of the given channel.
    ' * @param[in]  handle  Handle to the channel to operate on.
    ' * @param[in]  ns      Expected PTP timestamp in nano-seconds, in local timezone, since 1970-1-1.
    ' * @return     Operation status, see BM_StatusTypeDef for details.
    ' */
    '//BMAPI BM_StatusTypeDef BM_SetPtpTime(BM_ChannelHandle handle, uint64_t ns);
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_SetPtpTime(handle As IntPtr, timestamp As ULong) As BM_StatusTypeDef
    End Function

    '/**
    ' * @brief      Set PTP timestamp synchronization mode.
    ' * @param[in]  handle  Handle to the channel to operate on.
    ' * @param[in]  mode    Expected PTP timestamp synchronization mode, see BM_PtpModeTypeDef for details.
    ' * @return     Operation status, see BM_StatusTypeDef for details.
    ' */
    'BMAPI BM_StatusTypeDef BM_SetPtpMode(BM_ChannelHandle handle, BM_PtpModeTypeDef mode);
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_SetPtpMode(handle As IntPtr, mode As BM_PtpModeTypeDef) As BM_StatusTypeDef
    End Function

    '   /**
    '* @brief     A platform/OS independent implementation to synchronize PTP timestamp with the host machine for single/multiple channels.
    '* @param[in] handles     An array of channel handles.
    '* @param[in] nhandles    Number of valid channel handles.
    '* @return    Operation status, see BM_StatusTypeDef for details.
    '*/
    'BMAPI BM_StatusTypeDef BM_SyncPtpTimes(BM_ChannelHandle Handles[], int nhandles);
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_SyncPtpTimes(channelhandles As IntPtr(), nHandles As Integer) As BM_StatusTypeDef
    End Function

    '/**
    '* @brief      Get PTP timestamp in nano-seconds of the host machine, which Is calling BMAPI.
    '* @return     Current PTP timestamp in nano-seconds, in local timezone, since 1970-1-1.
    '*/
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_GetHostPtpTime() As ULong
    End Function
    '/**
    ' * @brief      Get timestamp in nano-seconds of the given channel.
    ' * @param[in]  handle  Handle to the channel to operate on.
    ' * @param[out] ns      Current PTP timestamp in nano-seconds, in local timezone, since 1970-1-1.
    ' * @return     Operation status, see BM_StatusTypeDef for details.
    ' */
    '//BMAPI BM_StatusTypeDef BM_GetPtpTime(BM_ChannelHandle handle, uint64_t* ns);
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_GetPtpTime(handle As IntPtr, ByRef timestamp As ULong) As BM_StatusTypeDef
    End Function

    '/**
    '* @brief      Extract PTP timestamp from data (i.e. CAN message), this Is a helper function to ease application programming.
    '* @param[in]  handle     Handle to the channel to operate on. Could be NULL if the device supports hardware PTP.
    '*                        Otherwise you would need this handle to align device local timestamp with host machine's UTC time.
    '* @param[in]  data       The message data from which to extract an PTP timestamp.
    '* @param[out] timestamp  The extracted PTP timestamp.
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_GetDataPtpTimestamp(ByVal handle As IntPtr, ByRef data As BM_DataTypeDef, ByRef ptpTimestamp As ULong) As BM_StatusTypeDef
    End Function
    '**
    '* @brief      Set CAN mode option of the given channel.
    '* @param[in]  handle  Handle to the channel to operate on.
    '* @param[in]  mode    Expected CAN mode  see BM_CanModeTypeDef for details.
    '* @return     Operation status  see BM_StatusTypeDef for details.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_SetCanMode(handle As IntPtr, mode As BM_CanModeTypeDef) As BM_StatusTypeDef
    End Function
    '**
    '* @brief      Set terminal resistor option of the given channel.
    '* @param[in]  handle  Handle to the channel to operate on.
    '* @param[in]  tres    Expected terminal resistor  see BM_TerminalResistorTypeDef for details.
    '* @return     Operation status  see BM_StatusTypeDef for details.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_SetTerminalRegister(handle As IntPtr, tres As BM_TerminalResistorTypeDef) As BM_StatusTypeDef
    End Function
    '**
    '* @brief      Set bitrate option of the given channel.
    '* @param[in]  handle  Handle to the channel to operate on.
    '* @param[in]  bitrate Expected bitrate  see BM_BitrateTypeDef for details.
    '* @return     Operation status  see BM_StatusTypeDef for details.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_SetBitrate(hhandle As IntPtr, ByRef bitrate As BM_BitrateTypeDef) As BM_StatusTypeDef
    End Function
    '**
    '* @brief      Set TX tasks option of the given channel.
    '* @param[in]  handle    Handle to the channel to operate on.
    '* @param[in]  txtasks   An array of TX task information  see BM_TxTaskTypeDef for details.
    '* @param[in]  ntxtasks  Number of valid TX tasks in the txtasks array.
    '* @return     Operation status  see BM_StatusTypeDef for details.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_SetTxTasks(handle As IntPtr, txtasks As IntPtr, ntxtasks As Integer) As BM_StatusTypeDef
    End Function
    '**
    '* @brief      Set RX filters option of the given channel.
    '* @param[in]  handle      Handle to the channel to operate on.
    '* @param[in]  rxfilters   An array of RX filter information  see BM_RxFilterTypeDef for details.
    '* @param[in]  nrxfilters  Number of valid RX filters in the rxfilters array.
    '* @return     Operation status  see BM_StatusTypeDef for details.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_SetRxFilters(handle As IntPtr, rxfilters As IntPtr, nrxfilters As Integer) As BM_StatusTypeDef
    End Function
    '**
    '* @brief Get the platform'OS independent notification handle for the given channel  so that the application could wait for notifications later.
    '* @param[in]  handle        Handle to the channel that owns the notification handle.
    '* @param[out] notification  The platform'OS independent notification handle.
    '* @return     Operation status  see BM_StatusTypeDef for details.
    '* @note       By using notification handles in a background thread  it Is easy to implement an asynchronous message receiver as below:
    '* @code
    '*             channel = BM_OpenCan(...)
    '*             BM_GetNotification(channel  notification)
    '*             while (!exit) 
    '*               BM_WaitForNotifications(&notification  1  -1) '' Wait infinitely for new message notification.
    '*               BM_ReadCanMessage(...)
    '*             }
    '* @endcode
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_GetNotification(handle As IntPtr, ByRef notification As IntPtr) As BM_StatusTypeDef
    End Function

    '**
    '* @brief     A platform'OS independent implementation to wait for single'multiple notification handles.
    '* @param[in] handles     An array of channel notification handles.
    '* @param[in] nhandles    Number of valid notification handles.
    '* @param[in] ntimeoutms  This function will block current thread for ntimeoutms milliseconds if no notification Is received.
    '*                        Note this function will return immediately once a New notification Is received  the ntimeoutms param Is ignored in this normal case.
    '* @return    This function returns the index in handles array of the channel from which a New notification Is posted.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_WaitForNotifications(ByVal notifications() As IntPtr, nhandles As Integer, ntimeoutms As Integer) As Integer
    End Function
    '**
    '* @brief      Set offline logging configuration for current channel.
    '* @param[in]  handle    Handle to the channel to operate on.
    '* @param[in]  logging   Logging configuration, see BM_LoggingConfigTypeDef for details.
    '* @return     Operation status, see BM_StatusTypeDef for details.
    '* @note       All channels of a physical device share the same logging configuration, that Is, you simply need to select any channel (e.g. CH0) And configure logging once.
    '*/
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_SetLogging(handle As IntPtr, ByRef logging As BM_LoggingConfigTypeDef) As BM_StatusTypeDef
    End Function

    '**
    '* @brief      Get offline logging configuration for current channel.
    '* @param[in]  handle    Handle to the channel to operate on.
    '* @param[in]  logging   Logging configuration, see BM_LoggingConfigTypeDef for details.
    '* @return     Operation status, see BM_StatusTypeDef for details.
    '* @note       All channels of a physical device share the same logging configuration, that Is, you simply need to select any channel (e.g. CH0) And configure logging once.
    '*/
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_GetLogging(handle As IntPtr, ByRef logging As BM_LoggingConfigTypeDef) As BM_StatusTypeDef
    End Function

    '**
    '* @brief      Set offline replay configuration for current channel.
    '* @param[in]  handle    Handle to the channel to operate on.
    '* @param[in]  replay    Replay configuration, see BM_ReplayConfigTypeDef for details.
    '* @return     Operation status, see BM_StatusTypeDef for details.
    '* @note       All channels of a physical device share the same replay configuration, that Is, you simply need to select any channel (e.g. CH0) And configure replay once.
    '*/
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_SetReplay(handle As IntPtr, ByRef replay As BM_ReplayConfigTypeDef) As BM_StatusTypeDef
    End Function

    '**
    '* @brief      Set offline replay configuration for current channel.
    '* @param[in]  handle    Handle to the channel to operate on.
    '* @param[in]  replay    Replay configuration, see BM_ReplayConfigTypeDef for details.
    '* @return     Operation status, see BM_StatusTypeDef for details.
    '* @note       All channels of a physical device share the same replay configuration, that Is, you simply need to select any channel (e.g. CH0) And configure replay once.
    '*/
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_GetReplay(handle As IntPtr, ByRef replay As BM_ReplayConfigTypeDef) As BM_StatusTypeDef
    End Function

    '**
    '* @brief      Load configuration from off-line storage media for the given channels.
    '* @param[in]  handle    Handle to the channel to operate on.
    '* @param[in]  configmask   Device channel bit-mask, each bit indicates a channel, e.g. bit0=channel0, bit15=channel15.
    '* @return     Operation status, see BM_StatusTypeDef for details.
    '*/
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_LoadConfig(handle As IntPtr, configmask As UInt32) As BM_StatusTypeDef
    End Function

    '**
    '* @brief      Save configuration to off-line storage media for the given channels.
    '* @param[in]  handle    Handle to the channel to operate on.
    '* @param[in]  configmask   Device channel bit-mask, each bit indicates a channel, e.g. bit0=channel0, bit15=channel15.
    '* @return     Operation status, see BM_StatusTypeDef for details.
    '*/
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_SaveConfig(handle As IntPtr, configmask As UInt32) As BM_StatusTypeDef
    End Function

    '**
    '* @brief      Clear configuration in off-line storage media for the given channels.
    '* @param[in]  handle    Handle to the channel to operate on.
    '* @param[in]  configmask   Device channel bit-mask, each bit indicates a channel, e.g. bit0=channel0, bit15=channel15.
    '* @return     Operation status, see BM_StatusTypeDef for details.
    '*/
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_ClearConfig(handle As IntPtr, configmask As UInt32) As BM_StatusTypeDef
    End Function

    '* @brief      Translate error code to string  this Is a helper function to ease application programming.
    '* @param[in]  errorcode  The errorcode to be translated.
    '* @param[out] buffer     A caller-allocated string buffer to hold the translated string.
    '* @param[in]  nbytes     Number in bytes of the string buffer.
    '* @param[in]  language   Reserved  only English Is supported currently.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_GetErrorText(errorcode As BM_StatusTypeDef, buffer() As Byte, nbytes As Integer, language As UShort)
    End Function

    '**
    '* @brief      Translate data (i.e. CAN message) to string  this Is a helper function to ease application programming.
    '* @param[in]  data       The message data to be translated.
    '* @param[out] buffer     A caller-allocated string buffer to hold the translated string.
    '* @param[in]  nbytes     Number in bytes of the string buffer.
    '* @param[in]  language   Reserved  only English Is supported currently.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_GetDataText(ByRef data As BM_DataTypeDef, buffer() As Byte, nbytes As Integer, language As UShort)
    End Function

    '**
    '* @brief      Get library log level.
    '* @return     Current log level, all messages equal to Or less than this level are currently printed on debug console.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_GetLogLevel() As BM_LogLevelTypeDef
    End Function

    '**
    '* @brief      Set library log level.
    '* @param[in]  level       Target log level, all messages equal to Or less than this level will be printed on debug console.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_SetLogLevel(level As BM_LogLevelTypeDef)
    End Function

    '**
    '* @brief      Get library (*.dll|*.so) BMAPI version.
    '* @return     32-bit version code:
    '*             bit31-28 = major
    '*             bit27-24 = minor
    '*             bit23-16 = revision
    '*             bit15-00 = build
    '* @note       This API Is used to get the library version, 
    '*             in case that *.h Is mismatch with *.dll|*.so, use the macro BM_API_VERSION defined in bmapi.h to check consistency.
    '*
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_GetVersion() As Integer
    End Function

    '# /**
    '* @brief      Set LIN voltage option of the given channel.
    '* @param[in]  handle  Handle to the channel to operate on.
    '* @param[in]  tres    Expected LIN voltage, see BM_LinVoltageTypeDef for details.
    '* @return     Operation status, see BM_StatusTypeDef for details.
    '*/
    <DllImport("BMAPI.DLL", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function BM_SetLinVoltage(ByVal handle As IntPtr, ByVal voltage As BM_LinVoltageTypeDef) As BM_StatusTypeDef
    End Function
End Class



