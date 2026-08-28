using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Globalization;

namespace can_analyzer_csharp
{
    public partial class MainForm : Form
    {
        private BM_CanMessageTypeDef txMsgBuf;
        private BM_ChannelInfoTypeDef[] channelinfos = new BM_ChannelInfoTypeDef[32];
        private IntPtr[] channelHandles = new IntPtr[32];
        private uint openedChannelCount;
        private System.Windows.Forms.Timer rxTimer;
        private System.Windows.Forms.Timer txTimer;
        int nchannels = 0;////Number of enumerated channels
        private bool[] channelSelected = new bool[32];
        private uint pendingTxMsgCount;
        private const int MaxDisplayedMessages = 5000;
        private const int MaxXpMessagesPerTimerTick = 256;
        private readonly List<ReceivedMessage> messageHistory = new List<ReceivedMessage>();

        private sealed class ReceivedMessage
        {
            public int Channel;
            public BM_CanMessageTypeDef CanMessage;
            public uint Timestamp;
            public ulong UtcTimestamp;
            public bool IsTx;
        }

        public MainForm()
        {
            InitializeComponent();
            msgListView.VirtualMode = true;
            msgListView.RetrieveVirtualItem += msgListView_RetrieveVirtualItem;
            Array.Clear(channelHandles, 0, channelHandles.Length);
            Array.Clear(channelSelected, 0, channelSelected.Length);
            this.openedChannelCount = 0;
            this.modeCombo.SelectedIndex = 0;
            this.txTypeCombo.SelectedIndex = 0;
            this.txMsgBuf = new BM_CanMessageTypeDef();
            BMAPI.BM_Init();

            GetAvailableHostIpList();
            enumerate();
            hostComboBox.SelectedIndex = 0;
        }

        public static ulong LocalTimeToNsSince1970()
        {
            // .NET 4.0 lacks DateTimeOffset.ToUnixTimeMilliseconds() (added in 4.6); compute manually
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long ticks = DateTime.UtcNow.Ticks - epoch.Ticks;
            ulong msSince1970 = (ulong)(ticks / TimeSpan.TicksPerMillisecond);

            return msSince1970 * 1000000;
        }

        private static bool IsWindowsXpOrEarlier()
        {
            OperatingSystem operatingSystem = Environment.OSVersion;
            return operatingSystem.Platform == PlatformID.Win32NT &&
                operatingSystem.Version.Major < 6;
        }

        private readonly HashSet<string> selectedChannelNames = new HashSet<string>(StringComparer.Ordinal);
        private static readonly uint[] ValidCanFdLengths = new uint[]
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 12, 16, 20, 24, 32, 48, 64
        };

        public static bool ParseIpAddress(string ipString, ref byte[] ipArray)
        {
            string[] parts = ipString.Split(new[] { '.' }, StringSplitOptions.None);
            if (parts.Length != 4)
            {
                return false;
            }
            for (int i = 0; i < 4; i++)
            {
                int value;
                if (!int.TryParse(parts[i], out value))
                {
                    return false;
                }
                if (value < 0 || value > 255)
                {
                    return false;
                }
                ipArray[i] = (byte)value;
            }
            return true;
        }

        private void ShowInputError(string message)
        {
            MessageBox.Show(this, message, "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static bool TryParseHexUInt32(string text, uint maximum, out uint value)
        {
            value = 0;
            string normalized = text == null ? String.Empty : text.Trim();
            if (normalized.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring(2);
            }

            return normalized.Length > 0 &&
                uint.TryParse(normalized, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value) &&
                value <= maximum;
        }

        private static bool IsValidCanFdLength(uint length)
        {
            return Array.IndexOf(ValidCanFdLengths, length) >= 0;
        }

        private static bool IsChannelLossStatus(BM_StatusTypeDef status)
        {
            uint rawStatus = (uint)status;
            uint handleStatus = rawStatus & (uint)BM_StatusTypeDef.BM_ERROR_ILLHANDLE;
            return handleStatus == (uint)BM_StatusTypeDef.BM_ERROR_ILLHW ||
                handleStatus == (uint)BM_StatusTypeDef.BM_ERROR_ILLNET ||
                handleStatus == (uint)BM_StatusTypeDef.BM_ERROR_ILLCLIENT ||
                (rawStatus & (uint)BM_StatusTypeDef.BM_ERROR_INITIALIZE) != 0 ||
                (rawStatus & (uint)BM_StatusTypeDef.BM_ERROR_ILLOPERATION) != 0;
        }

        private bool TryBuildOpenConfiguration(out BM_BitrateTypeDef bitrate, out BM_RxFilterTypeDef[] rxfilters)
        {
            bitrate = new BM_BitrateTypeDef();
            rxfilters = null;
            BM_RxFilterTypeDef filterTemplate = new BM_RxFilterTypeDef
            {
                payload_mask = new byte[8],
                payload_value = new byte[8]
            };

            ushort nominalBitrate;
            ushort dataBitrate;
            if (!ushort.TryParse(nominalBitrateEdit.Text, out nominalBitrate) || nominalBitrate == 0)
            {
                ShowInputError("Nominal bitrate must be a positive integer no greater than 65535 kbit/s.");
                return false;
            }
            if (!ushort.TryParse(dataBitrateEdit.Text, out dataBitrate) || dataBitrate == 0)
            {
                ShowInputError("Data bitrate must be a positive integer no greater than 65535 kbit/s.");
                return false;
            }

            bool acceptStandard = rxStandardCheck.Checked;
            bool acceptExtended = rxExtendedCheck.Checked;
            if (!acceptStandard && !acceptExtended)
            {
                acceptStandard = true;
                acceptExtended = true;
                rxStandardCheck.Checked = true;
                rxExtendedCheck.Checked = true;
            }

            uint maximumRxId = acceptExtended ? 0x1FFFFFFFU : 0x7FFU;
            uint rxId;
            uint rxMask;
            if (!TryParseHexUInt32(rxIdEdit.Text, maximumRxId, out rxId))
            {
                ShowInputError(acceptExtended
                    ? "RX ID must be hexadecimal from 0 to 1FFFFFFF."
                    : "Standard RX ID must be hexadecimal from 0 to 7FF.");
                return false;
            }
            if (!TryParseHexUInt32(rxMaskEdit.Text, maximumRxId, out rxMask))
            {
                ShowInputError(acceptExtended
                    ? "RX mask must be hexadecimal from 0 to 1FFFFFFF."
                    : "Standard RX mask must be hexadecimal from 0 to 7FF.");
                return false;
            }
            if (acceptStandard && acceptExtended && (rxId > 0x7FF || rxMask > 0x7FF))
            {
                ShowInputError("RX ID and mask must not exceed 7FF when both standard and extended frames are accepted.");
                return false;
            }

            bitrate.nbitrate = nominalBitrate;
            bitrate.dbitrate = dataBitrate;
            bitrate.nsamplepos = Convert.ToByte(samplePosSpin.Value);
            bitrate.dsamplepos = Convert.ToByte(samplePosSpin.Value);

            filterTemplate.type = rxAdvancedFilterCheck.Checked
                ? (byte)BM_RxFilterTypeTypeDef.BM_RXFILTER_ADVANCED
                : (byte)BM_RxFilterTypeTypeDef.BM_RXFILTER_BASIC;

            if (rxAdvancedFilterCheck.Checked)
            {
                string payload = rxDataMaskEdit.Text.Replace(" ", "").ToLowerInvariant().Replace('x', '0');
                if (payload.Length > 16 || payload.Length % 2 != 0)
                {
                    ShowInputError("Advanced RX filter data must contain at most 8 complete hexadecimal bytes.");
                    return false;
                }
                for (int i = 0; i < payload.Length / 2; i++)
                {
                    byte value;
                    if (!byte.TryParse(payload.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
                    {
                        ShowInputError("Advanced RX filter data contains an invalid hexadecimal byte.");
                        return false;
                    }
                    filterTemplate.payload_mask[i] = value;
                    filterTemplate.payload_value[i] = value;
                }
            }

            List<BM_RxFilterTypeDef> filters = new List<BM_RxFilterTypeDef>(2);
            if (acceptStandard)
            {
                BM_RxFilterTypeDef standardFilter = filterTemplate;
                standardFilter.SetIdFilter(rxMask, rxId, false);
                standardFilter.flags_mask = (byte)BM_MessageFlagsTypeDef.BM_MESSAGE_FLAGS_IDE;
                standardFilter.flags_value = 0;
                filters.Add(standardFilter);
            }
            if (acceptExtended)
            {
                BM_RxFilterTypeDef extendedFilter = filterTemplate;
                extendedFilter.SetIdFilter(rxMask, rxId, true);
                extendedFilter.flags_mask = (byte)BM_MessageFlagsTypeDef.BM_MESSAGE_FLAGS_IDE;
                extendedFilter.flags_value = (byte)BM_MessageFlagsTypeDef.BM_MESSAGE_FLAGS_IDE;
                filters.Add(extendedFilter);
            }
            if (filters.Count == 1)
            {
                filters.Add(new BM_RxFilterTypeDef
                {
                    type = (byte)BM_RxFilterTypeTypeDef.BM_RXFILTER_INVALID,
                    payload_mask = new byte[8],
                    payload_value = new byte[8]
                });
            }
            rxfilters = filters.ToArray();
            return true;
        }

        private bool TryBuildTransmitMessage(out BM_CanMessageTypeDef message, out uint count, out int cycle)
        {
            message = new BM_CanMessageTypeDef();
            count = 0;
            cycle = 0;

            uint maximumId = txExtendedCheck.Checked ? 0x1FFFFFFFU : 0x7FFU;
            uint id;
            if (!TryParseHexUInt32(txIdEdit.Text, maximumId, out id))
            {
                ShowInputError(txExtendedCheck.Checked
                    ? "Extended CAN ID must be hexadecimal from 0 to 1FFFFFFF."
                    : "Standard CAN ID must be hexadecimal from 0 to 7FF.");
                return false;
            }

            uint length;
            if (!uint.TryParse(txLengthEdit.Text, out length) ||
                (txFdCheck.Checked ? !IsValidCanFdLength(length) : length > 8))
            {
                ShowInputError(txFdCheck.Checked
                    ? "CAN FD length must be 0-8, 12, 16, 20, 24, 32, 48, or 64 bytes."
                    : "Classic CAN length must be from 0 to 8 bytes.");
                return false;
            }

            if (!uint.TryParse(txCountEdit.Text, out count) || count == 0)
            {
                ShowInputError("Count must be a positive integer.");
                return false;
            }
            if (!int.TryParse(txCycleEdit.Text, out cycle) || cycle <= 0)
            {
                ShowInputError("Cycle must be a positive integer in milliseconds.");
                return false;
            }

            string payload = txDataEdit.Text.Replace(" ", "");
            if (payload.Length % 2 != 0)
            {
                ShowInputError("Payload must contain complete hexadecimal bytes.");
                return false;
            }
            uint payloadLength = (uint)(payload.Length / 2);
            if (payloadLength > length)
            {
                ShowInputError("Payload contains more bytes than the declared length.");
                return false;
            }

            uint dlc = message.lengthToDlc(length);
            message = new BM_CanMessageTypeDef(
                id,
                dlc,
                txExtendedCheck.Checked,
                false,
                txFdCheck.Checked,
                txFdCheck.Checked,
                false);
            for (int i = 0; i < payload.Length / 2; i++)
            {
                byte value;
                if (!byte.TryParse(payload.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
                {
                    ShowInputError("Payload contains an invalid hexadecimal byte.");
                    return false;
                }
                message.payload[i] = value;
            }

            return true;
        }
        public void GetAvailableHostIpList()
        {
            List<string> allIps = new List<string>();
            NetworkInterface[] interfaceList = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface interfaceItem in interfaceList)
            {
                bool isLoopback = IsLoopbackInterface(interfaceItem);

                if (interfaceItem.Supports(NetworkInterfaceComponent.IPv4) &&
                    !isLoopback &&
                    !interfaceItem.GetPhysicalAddress().ToString().StartsWith("005056") &&
                    interfaceItem.Name.IndexOf("vmware", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    UnicastIPAddressInformationCollection unicastAddresses = interfaceItem.GetIPProperties().UnicastAddresses;

                    foreach (UnicastIPAddressInformation entry in unicastAddresses)
                    {
                        string ip = entry.Address.ToString();
                        if (!ip.StartsWith("fe80::"))
                        {
                            allIps.Add(ip);
                        }
                    }
                }
            }
            allIps.Add("127.0.0.1");

            List<string> uniqueIps = new List<string>(new HashSet<string>(allIps));
            for (int i = 0; i < uniqueIps.Count; i++)
            {
                if (uniqueIps[i] != "127.0.0.1")
                {
                    int lastDotIndex = uniqueIps[i].LastIndexOf('.');
                    if (lastDotIndex != -1)
                    {
                        string prefix = uniqueIps[i].Substring(0, lastDotIndex + 1);
                        uniqueIps[i] = prefix + "255";
                    }
                }

                hostComboBox.Items.Add(uniqueIps[i]);
            }
        }
        private bool IsLoopbackInterface(NetworkInterface iface)
        {
            UnicastIPAddressInformationCollection unicastAddresses = iface.GetIPProperties().UnicastAddresses;
            foreach (UnicastIPAddressInformation entry in unicastAddresses)
            {
                if (entry.Address.ToString() == "127.0.0.1")
                {
                    return true;
                }
            }
            return false;
        }

        private static void RetainEnumeratedChannelSelections(
            HashSet<string> selectedNames,
            HashSet<string> enumeratedNames)
        {
            selectedNames.IntersectWith(enumeratedNames);
        }

        private void enumerate()
        {
            bool succeed = false;
            byte[] ip = { 192, 168, 41, 255 };
            bool remote = ParseIpAddress(hostComboBox.Text, ref ip);
            channelComboBox.Items.Clear();
            Array.Clear(channelSelected, 0, channelSelected.Length);
            Array.Clear(channelinfos, 0, channelinfos.Length);
            nchannels = 0;
            int infoSize = Marshal.SizeOf(typeof(BM_ChannelInfoTypeDef));
            int nInfos = 32;
            IntPtr infoMem = Marshal.AllocHGlobal(nInfos * infoSize);
            if (remote)
            {
                if (BMAPI.BM_EnumerateRemote(infoMem, ref nInfos, ip, 100) == BM_StatusTypeDef.BM_ERROR_OK)
                {
                    succeed = true;
                }
            }
            else
            {
                if (BMAPI.BM_EnumerateByCap(infoMem, ref nInfos, (ushort)BM_CapabilityTypeDef.BM_CAN_CAP) == BM_StatusTypeDef.BM_ERROR_OK)
                {
                    succeed = true;
                }
            }

            if (succeed)
            {
                int ncanchannel = 0;
                HashSet<string> enumeratedChannelNames = new HashSet<string>(StringComparer.Ordinal);
                for (int i = 0; i < nInfos; i++)
                {
                    IntPtr ptr = IntPtr.Add(infoMem, i * infoSize);
                    BM_ChannelInfoTypeDef channelInfo = (BM_ChannelInfoTypeDef)Marshal.PtrToStructure(ptr, typeof(BM_ChannelInfoTypeDef));
                    if ((channelInfo.cap & (ushort)BM_CapabilityTypeDef.BM_CAN_CAP) != 0)
                    {
                        channelinfos[ncanchannel] = channelInfo;
                        string name = channelInfo.GetName();
                        enumeratedChannelNames.Add(name);
                        channelComboBox.Items.Add(name);
                        bool keepSelected = selectedChannelNames.Contains(name);
                        channelSelected[ncanchannel] = keepSelected;
                        channelComboBox.SetItemChecked(ncanchannel, keepSelected);
                        ncanchannel++;
                    }
                }
                nchannels = ncanchannel;
                RetainEnumeratedChannelSelections(selectedChannelNames, enumeratedChannelNames);
            }

            Marshal.FreeHGlobal(infoMem);
        }


        private void openButton_Click(object sender, EventArgs e)
        {
            bool isOpen = this.openButton.Text == "Open";
            if (!isOpen)
            {
                CloseOpenedChannels();
                return;
            }

            bool hasSelection = false;
            for (int i = 0; i < nchannels; i++)
            {
                hasSelection |= channelSelected[i];
            }
            if (!hasSelection)
            {
                return;
            }

            BM_BitrateTypeDef bitrate;
            BM_RxFilterTypeDef[] rxfilters;
            if (!TryBuildOpenConfiguration(out bitrate, out rxfilters))
            {
                return;
            }

            bool enablePtpSynchronization = !IsWindowsXpOrEarlier();
            for (int i = 0; i < nchannels; i++)
            {
                if (channelSelected[i])
                {
                    BM_CanModeTypeDef mode = (BM_CanModeTypeDef)modeCombo.SelectedIndex;
                    BM_TerminalResistorTypeDef tres = tresCheck.Checked ? BM_TerminalResistorTypeDef.BM_TRESISTOR_120 : BM_TerminalResistorTypeDef.BM_TRESISTOR_DISABLED;
                    int rxfilterSize = Marshal.SizeOf(typeof(BM_RxFilterTypeDef));
                    IntPtr rxfilterMem = Marshal.AllocHGlobal(rxfilterSize * rxfilters.Length);
                    if (rxAdvancedFilterCheck.Checked)
                    {
                        MessageBox.Show("Advanced RX filters are currently unsupported.", "Warning");
                        rxAdvancedFilterCheck.Enabled = false;
                    }
                    for (int filterIndex = 0; filterIndex < rxfilters.Length; filterIndex++)
                    {
                        Marshal.StructureToPtr(rxfilters[filterIndex], IntPtr.Add(rxfilterMem, filterIndex * rxfilterSize), false);
                    }

                    BM_StatusTypeDef openResult;
                    try
                    {
                        openResult = BMAPI.BM_OpenEx(
                            ref channelHandles[this.openedChannelCount],
                            ref channelinfos[i],
                            mode,
                            tres,
                            ref bitrate,
                            rxfilterMem,
                            rxfilters.Length
                        );
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(rxfilterMem);
                    }

                    if (openResult != BM_StatusTypeDef.BM_ERROR_OK ||
                        this.channelHandles[this.openedChannelCount] == IntPtr.Zero)
                    {
                        if (this.channelHandles[this.openedChannelCount] != IntPtr.Zero)
                        {
                            BMAPI.BM_Close(this.channelHandles[this.openedChannelCount]);
                            this.channelHandles[this.openedChannelCount] = IntPtr.Zero;
                        }
                        MessageBox.Show("Failed to open channel: " + openResult, "Error");
                        CloseOpenedChannels();
                        return;
                    }

                    if (enablePtpSynchronization)
                    {
                        ulong ns = LocalTimeToNsSince1970();
                        BMAPI.BM_SetPtpTime(channelHandles[this.openedChannelCount], ns);
                        BMAPI.BM_SetPtpMode(channelHandles[this.openedChannelCount], BM_PtpModeTypeDef.BM_PTP_INPUT_USB_SOF);
                    }
                    this.sendComboBox.Items.Add(this.openedChannelCount + 1);
                    this.openedChannelCount++;
                }
            }
            // Sync PTP timestamp for all devices
            if (enablePtpSynchronization && openedChannelCount > 0 &&
                BMAPI.BM_SyncPtpTimes(channelHandles, (int)openedChannelCount) != BM_StatusTypeDef.BM_ERROR_OK)
            {
                Console.WriteLine("Failed to sync PTP timestamps.");
            }
            if (this.openedChannelCount > 0)
            {
                sendComboBox.SelectedIndex = 0;
                ClearMessageHistory();
                rxTimer = new System.Windows.Forms.Timer();
                rxTimer.Interval = 50;
                rxTimer.Tick += new EventHandler(this.readPendingMessages);
                rxTimer.Start();
                openButton.Text = "Close";
                rxIdEdit.Enabled = false;
                rxMaskEdit.Enabled = false;
                rxExtendedCheck.Enabled = false;
                rxStandardCheck.Enabled = false;
                //rxAdvancedFilterCheck.Enabled = false;
                rxDataMaskEdit.Enabled = false;
                modeCombo.Enabled = false;
                tresCheck.Enabled = false;
                nominalBitrateEdit.Enabled = false;
                dataBitrateEdit.Enabled = false;
                samplePosSpin.Enabled = false;
                enumerateButton.Enabled = false;
                txButton.Enabled = true;
                channelComboBox.Enabled = false;
            }
        }

        private void StopTransmission()
        {
            if (txTimer != null)
            {
                txTimer.Stop();
                txTimer.Tick -= this.writePendingMessages;
                txTimer.Dispose();
                txTimer = null;
            }
            pendingTxMsgCount = 0;
            txButton.Text = "Transmit";
        }

        private void CloseOpenedChannels()
        {
            StopTransmission();
            if (rxTimer != null)
            {
                rxTimer.Stop();
                rxTimer.Tick -= this.readPendingMessages;
                rxTimer.Dispose();
                rxTimer = null;
            }

            for (int i = 0; i < channelHandles.Length; i++)
            {
                if (channelHandles[i] != IntPtr.Zero)
                {
                    BMAPI.BM_Close(channelHandles[i]);
                    channelHandles[i] = IntPtr.Zero;
                }
            }
            openedChannelCount = 0;
            sendComboBox.Items.Clear();
            sendComboBox.Text = "";
            openButton.Text = "Open";
            rxIdEdit.Enabled = true;
            rxMaskEdit.Enabled = true;
            rxExtendedCheck.Enabled = true;
            rxStandardCheck.Enabled = true;
            rxDataMaskEdit.Enabled = rxAdvancedFilterCheck.Checked;
            modeCombo.Enabled = true;
            tresCheck.Enabled = true;
            nominalBitrateEdit.Enabled = true;
            dataBitrateEdit.Enabled = true;
            samplePosSpin.Enabled = true;
            enumerateButton.Enabled = true;
            txButton.Enabled = false;
            channelComboBox.Enabled = true;
        }

        private bool TryGetUnavailableOpenedChannel(out int channelNumber, out BM_StatusTypeDef failureStatus)
        {
            channelNumber = 0;
            failureStatus = BM_StatusTypeDef.BM_ERROR_OK;
            for (int i = 0; i < openedChannelCount; i++)
            {
                if (channelHandles[i] == IntPtr.Zero)
                {
                    continue;
                }

                BM_CanStatusInfoTypedef statusInfo = new BM_CanStatusInfoTypedef();
                BM_StatusTypeDef status = BMAPI.BM_GetStatus(channelHandles[i], ref statusInfo);
                if (IsChannelLossStatus(status))
                {
                    channelNumber = i + 1;
                    failureStatus = status;
                    return true;
                }
            }
            return false;
        }

        private void rxAdvancedFilterCheck_CheckedChanged(object sender, EventArgs e)
        {
            this.rxDataMaskEdit.Enabled = this.rxAdvancedFilterCheck.Checked;
        }

        private void enumerateButton_Click(object sender, EventArgs e)
        {
            enumerate();
        }

        private void txButton_Click(object sender, EventArgs e)
        {
            bool isTransmit = this.txButton.Text == "Transmit";
            if (!isTransmit)
            {
                StopTransmission();
                return;
            }

            int channelIndex = this.sendComboBox.SelectedIndex;
            if (channelIndex < 0 || this.channelHandles[channelIndex] == IntPtr.Zero)
            {
                ShowInputError("Select an open channel before transmitting.");
                return;
            }

            BM_CanMessageTypeDef message;
            uint count;
            int cycle;
            if (!TryBuildTransmitMessage(out message, out count, out cycle))
            {
                return;
            }

            txMsgBuf = message;
            pendingTxMsgCount = count;
            txTimer = new System.Windows.Forms.Timer();
            txTimer.Interval = cycle;
            txTimer.Tick += new EventHandler(this.writePendingMessages);
            txTimer.Start();
            txButton.Text = "Stop";
            writePendingMessages(null, null);
        }
        public static ulong GetUtcTimestamp(uint msgLen, BM_CanMessageTypeDef canMsg)
        {
            IntPtr tailPtr = IntPtr.Zero;
            IntPtr bufferPtr = IntPtr.Zero;

            try
            {
                tailPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BM_DataTailTypeDef)));

                int structSize = Marshal.SizeOf(typeof(BM_CanMessageTypeDef));
                bufferPtr = Marshal.AllocHGlobal(structSize);
                Marshal.StructureToPtr(canMsg, bufferPtr, false);

                int offset = (int)(msgLen - BMAPI.BM_DATA_TAIL_SIZE);
                int bytesToCopy = Math.Min(structSize - offset, Marshal.SizeOf(typeof(BM_DataTailTypeDef)));

                for (int j = 0; j < bytesToCopy; j++)
                {
                    byte value = Marshal.ReadByte(bufferPtr, j + offset);
                    Marshal.WriteByte(tailPtr, j, value);
                }

                BM_DataTailTypeDef data = (BM_DataTailTypeDef)Marshal.PtrToStructure(tailPtr, typeof(BM_DataTailTypeDef));
                return data.GetUtcTimestamp();
            }
            finally
            {
                if (tailPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(tailPtr);
                }

                if (bufferPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(bufferPtr);
                }
            }
        }

        private void readPendingMessages(object source, EventArgs e)
        {
            if (this.openedChannelCount == 0)
            {
                return;
            }

            List<ReceivedMessage> batch = new List<ReceivedMessage>();
            uint channelPending = this.openedChannelCount >= 32
                ? uint.MaxValue
                : (1U << (int)this.openedChannelCount) - 1U;
            int maxMessagesThisTick = IsWindowsXpOrEarlier()
                ? MaxXpMessagesPerTimerTick
                : int.MaxValue;
            BM_DataTypeDef msg = new BM_DataTypeDef();

            while (channelPending != 0 && batch.Count < maxMessagesThisTick)
            {
                for (int channelOffset = 0;
                    channelOffset < this.openedChannelCount && batch.Count < maxMessagesThisTick;
                    channelOffset++)
                {
                    uint channelBit = 1U << channelOffset;
                    if ((channelPending & channelBit) == 0)
                    {
                        continue;
                    }

                    if (this.channelHandles[channelOffset] == IntPtr.Zero)
                    {
                        channelPending &= ~channelBit;
                        continue;
                    }

                    BM_StatusTypeDef status = BMAPI.BM_Read(this.channelHandles[channelOffset], ref msg);
                    if (status == BM_StatusTypeDef.BM_ERROR_OK)
                    {
                        ulong utcts = 0;
                        if (BMAPI.BM_GetDataPtpTimestamp(this.channelHandles[channelOffset], ref msg, ref utcts) != BM_StatusTypeDef.BM_ERROR_OK)
                        {
                            // If failed to extract a PTP timestamp from data, use current time on host-machine.
                            utcts = BMAPI.BM_GetHostPtpTime();
                        }

                        batch.Add(new ReceivedMessage
                        {
                            Channel = channelOffset + 1,
                            CanMessage = CloneCanMessage(msg.can),
                            Timestamp = msg.timestamp,
                            UtcTimestamp = utcts,
                            IsTx = msg.header.IsAckData()
                        });
                    }
                    else if (status == BM_StatusTypeDef.BM_ERROR_QRCVEMPTY)
                    {
                        channelPending &= ~channelBit;
                    }
                    else
                    {
                        channelPending &= ~channelBit;
                        Console.Error.WriteLine(
                            "BM_Read failed on channel {0}: {1}",
                            channelOffset + 1,
                            status);
                    }
                }
            }

            AppendMessageBatch(batch);
        }

        private static BM_CanMessageTypeDef CloneCanMessage(BM_CanMessageTypeDef source)
        {
            BM_CanMessageTypeDef copy = source;
            if (source.payload != null)
            {
                copy.payload = (byte[])source.payload.Clone();
            }
            return copy;
        }

        private void ClearMessageHistory()
        {
            messageHistory.Clear();
            msgListView.VirtualListSize = 0;
            msgListView.Invalidate();
        }

        private void AppendMessageBatch(IList<ReceivedMessage> batch)
        {
            if (batch.Count == 0)
            {
                return;
            }

            int firstIncoming = Math.Max(0, batch.Count - MaxDisplayedMessages);
            int incomingCount = batch.Count - firstIncoming;
            int removeCount = Math.Max(
                0,
                messageHistory.Count + incomingCount - MaxDisplayedMessages);
            if (removeCount > 0)
            {
                messageHistory.RemoveRange(0, removeCount);
            }

            for (int i = firstIncoming; i < batch.Count; i++)
            {
                messageHistory.Add(batch[i]);
            }

            msgListView.VirtualListSize = messageHistory.Count;
            msgListView.Invalidate();
            msgListView.EnsureVisible(messageHistory.Count - 1);
        }

        private void writePendingMessages(object source, EventArgs e)
        {
            int channelIndex = this.sendComboBox.SelectedIndex;
            if (channelIndex >= 0 && this.channelHandles[channelIndex] != IntPtr.Zero)
            {
                if (this.pendingTxMsgCount > 0)
                {
                    uint timestamp = 0;
                    uint port = 0;

                    BM_StatusTypeDef status = BMAPI.BM_WriteCanMessage(this.channelHandles[channelIndex], ref this.txMsgBuf, port, 0, ref timestamp);
                    if (status == BM_StatusTypeDef.BM_ERROR_OK)
                    {
                        if (--pendingTxMsgCount <= 0)
                        {
                            txButton_Click(null, null);
                        }
                    }
                    else
                    {
                        StopTransmission();
                        if (IsChannelLossStatus(status))
                        {
                            CloseOpenedChannels();
                            MessageBox.Show(
                                this,
                                "The transmit channel is no longer available (" + status + "). Re-enumerate the device and open it again.",
                                "Channel disconnected",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                        }
                        else
                        {
                            MessageBox.Show(this, "Failed to transmit message: " + status, "Transmit failed");
                        }
                    }
                }
            }
        }
        public static void TimevalToFormattedString(ulong utcTimestamp, StringBuilder str, int capacity)
        {
            long seconds = (long)(utcTimestamp / 1000000000);
            long microseconds = (long)(utcTimestamp % 1000000000 / 1000);

            DateTime dateTime;
            try
            {
                dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                           .AddSeconds(seconds)
                           .ToLocalTime();
            }
            catch (ArgumentOutOfRangeException)
            {
                str.Append("Invalid");
                return;
            }

            string formatted = dateTime.ToString("yyyy-MM-dd HH:mm:ss.");

            if (str.Capacity < capacity)
                str.Capacity = capacity;

            str.Clear();
            str.Append(formatted);
            str.Append(microseconds.ToString("D6"));
        }
        private void msgListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex < 0 || e.ItemIndex >= messageHistory.Count)
            {
                e.Item = new ListViewItem();
                return;
            }

            e.Item = CreateMessageItem(messageHistory[e.ItemIndex]);
        }

        private static ListViewItem CreateMessageItem(ReceivedMessage received)
        {
            StringBuilder str = new StringBuilder(32);
            if (received.UtcTimestamp > 0)
            {
                TimevalToFormattedString(received.UtcTimestamp, str, 32);
            }
            else
            {
                str.Clear();
                str.Append("N/A");
            }
            ListViewItem item = new ListViewItem();
            item.Text = Convert.ToString(received.Timestamp * 1E-6f);
            item.SubItems.Add(Convert.ToString(str));
            item.SubItems.Add(received.IsTx ? "TX" : "RX");
            item.SubItems.Add(Convert.ToString(received.Channel));
            item.SubItems.Add(Convert.ToString(received.CanMessage.GetID(), 16).ToUpper());
            item.SubItems.Add(received.CanMessage.GetTypeString());
            item.SubItems.Add(Convert.ToString(received.CanMessage.GetLength()));
            item.SubItems.Add(received.CanMessage.GetPyloadString());
            return item;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            CloseOpenedChannels();
            BMAPI.BM_UnInit();
            base.OnFormClosing(e);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_DEVICECHANGE = 0x219;
            try
            {
                if (m.Msg == WM_DEVICECHANGE)
                {
                    if (openButton.Text == "Open")
                    {
                        enumerate();
                    }
                    else
                    {
                        int channelNumber;
                        BM_StatusTypeDef failureStatus;
                        if (TryGetUnavailableOpenedChannel(out channelNumber, out failureStatus))
                        {
                            selectedChannelNames.Clear();
                            CloseOpenedChannels();
                            MessageBox.Show(
                                this,
                                "Channel " + channelNumber + " is no longer available (" + failureStatus + "). Re-enumerate the device and open it again.",
                                "Channel disconnected",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            base.WndProc(ref m);
        }

        // See for details: https://www.shuzhiduo.com/A/1O5EZB2Gd7/
        private void channelComboBox_ItemClick(object sender, ItemCheckEventArgs e)
        {
            if (e.Index >= 0 && e.Index < nchannels && e.Index < channelSelected.Length)
            {
                bool isChecked = e.NewValue == CheckState.Checked;
                channelSelected[e.Index] = isChecked;
                string channelName = channelinfos[e.Index].GetName();
                if (isChecked)
                {
                    selectedChannelNames.Add(channelName);
                }
                else
                {
                    selectedChannelNames.Remove(channelName);
                }
            }
        }
    }
}
