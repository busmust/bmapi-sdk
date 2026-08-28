Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Windows.Forms
Imports System.Net
Imports System.Net.NetworkInformation
Imports System.Collections.Generic
Imports System.Globalization
Partial Public Class MainForm
    Private txMsgBuf As BM_CanMessageTypeDef
    Private channelinfos(31) As BM_ChannelInfoTypeDef
    Private channelHandles(31) As IntPtr
    Private openedChannelCount As Integer
    Private nchannels As Integer 'Number Of enumerated channels
    Private channelSelected(31) As Boolean
    Private ReadOnly selectedChannelNames As New HashSet(Of String)(StringComparer.Ordinal)
    Private pendingTxMsgCount As UInteger
    Private Const MaxDisplayedMessages As Integer = 5000
    Private Const MaxXpMessagesPerTimerTick As Integer = 256
    Private Const XpUsbCloseSettleMilliseconds As Integer = 500
    Private Shared ReadOnly ValidCanFdLengths As UInteger() = {
        0UI, 1UI, 2UI, 3UI, 4UI, 5UI, 6UI, 7UI, 8UI,
        12UI, 16UI, 20UI, 24UI, 32UI, 48UI, 64UI
    }
    Private ReadOnly messageHistory As New List(Of ReceivedMessage)()

    Private Class ReceivedMessage
        Public Channel As Integer
        Public CanMessage As BM_CanMessageTypeDef
        Public Timestamp As Integer
        Public UtcTimestamp As ULong
        Public IsTx As Boolean
    End Class

    Public Function ParseIpAddress(ipString As String, ByRef ipArray() As Byte) As Boolean
        Dim parts() As String = ipString.Split("."c)
        If parts.Length <> 4 Then
            Return False
        End If
        For i As Integer = 0 To 3
            Dim value As Integer
            If Not Integer.TryParse(parts(i), value) Then
                Return False
            End If
            If value < 0 OrElse value > 255 Then
                Return False
            End If
            ipArray(i) = CByte(value)
        Next
        Return True
    End Function

    Public Sub New()
        InitializeComponent()
        msgListView.VirtualMode = True
        AddHandler msgListView.RetrieveVirtualItem, AddressOf msgListView_RetrieveVirtualItem
        Array.Clear(channelHandles, 0, channelHandles.Length)
        Array.Clear(channelSelected, 0, channelSelected.Length)
        openedChannelCount = 0
        modeCombo.SelectedIndex = 0
        txTypeCombo.SelectedIndex = 0
        txMsgBuf = New BM_CanMessageTypeDef()
        BMAPI.BM_Init()
        GetAvailableHostIpList()
        enumerate()
        hostComboBox.SelectedIndex = 0
    End Sub

    Public Shared Function LocalTimeToNsSince1970() As ULong
        Dim epoch As New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        Dim ticks As Long = DateTime.UtcNow.Ticks - epoch.Ticks
        Dim msSince1970 As ULong = CULng(ticks \ TimeSpan.TicksPerMillisecond)
        Return msSince1970 * 1000000UL
    End Function

    Private Shared Function IsWindowsXpOrEarlier() As Boolean
        Dim operatingSystem As OperatingSystem = Environment.OSVersion
        Return operatingSystem.Platform = PlatformID.Win32NT AndAlso
            operatingSystem.Version.Major < 6
    End Function


    Private Sub ShowInputError(message As String)
        MessageBox.Show(
            Me,
            message,
            "Invalid input",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning)
    End Sub

    Private Shared Function TryParseHexUInt32(
        text As String,
        maximum As UInteger,
        ByRef value As UInteger) As Boolean

        value = 0
        Dim normalized As String = If(text, String.Empty).Trim()
        If normalized.StartsWith("0x", StringComparison.OrdinalIgnoreCase) Then
            normalized = normalized.Substring(2)
        End If

        Return normalized.Length > 0 AndAlso
            UInteger.TryParse(
                normalized,
                NumberStyles.AllowHexSpecifier,
                CultureInfo.InvariantCulture,
                value) AndAlso
            value <= maximum
    End Function

    Private Shared Function IsValidCanFdLength(length As UInteger) As Boolean
        Return Array.IndexOf(ValidCanFdLengths, length) >= 0
    End Function

    Private Shared Function IsChannelLossStatus(status As BM_StatusTypeDef) As Boolean
        Dim rawStatus As UInteger = CUInt(status)
        Dim handleStatus As UInteger = rawStatus And CUInt(BM_StatusTypeDef.BM_ERROR_ILLHANDLE)
        Return handleStatus = CUInt(BM_StatusTypeDef.BM_ERROR_ILLHW) OrElse
            handleStatus = CUInt(BM_StatusTypeDef.BM_ERROR_ILLNET) OrElse
            handleStatus = CUInt(BM_StatusTypeDef.BM_ERROR_ILLCLIENT) OrElse
            (rawStatus And CUInt(BM_StatusTypeDef.BM_ERROR_INITIALIZE)) <> 0 OrElse
            (rawStatus And CUInt(BM_StatusTypeDef.BM_ERROR_ILLOPERATION)) <> 0
    End Function

    Private Function TryBuildOpenConfiguration(
        ByRef bitrate As BM_BitrateTypeDef,
        ByRef rxfilter As BM_RxFilterTypeDef) As Boolean

        bitrate = New BM_BitrateTypeDef()
        rxfilter = New BM_RxFilterTypeDef() With {
            .payload_mask = New Byte(7) {},
            .payload_value = New Byte(7) {}
        }

        Dim nominalBitrate As UShort
        Dim dataBitrate As UShort
        If Not UShort.TryParse(nominalBitrateEdit.Text, nominalBitrate) OrElse
            nominalBitrate = 0 Then
            ShowInputError("Nominal bitrate must be a positive integer no greater than 65535 kbit/s.")
            Return False
        End If
        If Not UShort.TryParse(dataBitrateEdit.Text, dataBitrate) OrElse
            dataBitrate = 0 Then
            ShowInputError("Data bitrate must be a positive integer no greater than 65535 kbit/s.")
            Return False
        End If

        Dim acceptStandard As Boolean = rxStandardCheck.Checked
        Dim acceptExtended As Boolean = rxExtendedCheck.Checked
        If Not acceptStandard AndAlso Not acceptExtended Then
            acceptStandard = True
            acceptExtended = True
            rxStandardCheck.Checked = True
            rxExtendedCheck.Checked = True
        End If

        Dim maximumRxId As UInteger = If(acceptExtended, &H1FFFFFFFUI, &H7FFUI)
        Dim rxId As UInteger
        Dim rxMask As UInteger
        If Not TryParseHexUInt32(rxIdEdit.Text, maximumRxId, rxId) Then
            ShowInputError(If(
                acceptExtended,
                "RX ID must be hexadecimal from 0 to 1FFFFFFF.",
                "Standard RX ID must be hexadecimal from 0 to 7FF."))
            Return False
        End If
        If Not TryParseHexUInt32(rxMaskEdit.Text, maximumRxId, rxMask) Then
            ShowInputError(If(
                acceptExtended,
                "RX mask must be hexadecimal from 0 to 1FFFFFFF.",
                "Standard RX mask must be hexadecimal from 0 to 7FF."))
            Return False
        End If

        bitrate.nbitrate = nominalBitrate
        bitrate.dbitrate = dataBitrate
        bitrate.nsamplepos = Convert.ToByte(samplePosSpin.Value)
        bitrate.dsamplepos = Convert.ToByte(samplePosSpin.Value)

        rxfilter.type = CByte(If(
            rxAdvancedFilterCheck.Checked,
            BM_RxFilterTypeTypeDef.BM_RXFILTER_ADVANCED,
            BM_RxFilterTypeTypeDef.BM_RXFILTER_BASIC))
        rxfilter.SetIdFilter(
            CInt(rxMask),
            CInt(rxId),
            rxId > &H7FFUI OrElse rxMask > &H7FFUI)
        If acceptStandard AndAlso Not acceptExtended Then
            rxfilter.flags_mask = CByte(BM_MessageFlagsTypeDef.BM_MESSAGE_FLAGS_IDE)
            rxfilter.flags_value = 0
        ElseIf Not acceptStandard AndAlso acceptExtended Then
            rxfilter.flags_mask = CByte(BM_MessageFlagsTypeDef.BM_MESSAGE_FLAGS_IDE)
            rxfilter.flags_value = CByte(BM_MessageFlagsTypeDef.BM_MESSAGE_FLAGS_IDE)
        End If

        If rxAdvancedFilterCheck.Checked Then
            Dim payload As String = rxDataMaskEdit.Text.Replace(" ", "").ToLowerInvariant().Replace("x"c, "0"c)
            If payload.Length > 16 OrElse payload.Length Mod 2 <> 0 Then
                ShowInputError("Advanced RX filter data must contain at most 8 complete hexadecimal bytes.")
                Return False
            End If
            For i As Integer = 0 To payload.Length \ 2 - 1
                Dim value As Byte
                If Not Byte.TryParse(
                    payload.Substring(i * 2, 2),
                    NumberStyles.AllowHexSpecifier,
                    CultureInfo.InvariantCulture,
                    value) Then
                    ShowInputError("Advanced RX filter data contains an invalid hexadecimal byte.")
                    Return False
                End If
                rxfilter.payload_mask(i) = value
                rxfilter.payload_value(i) = value
            Next
        End If

        Return True
    End Function

    Private Function TryBuildTransmitMessage(
        ByRef message As BM_CanMessageTypeDef,
        ByRef count As UInteger,
        ByRef cycle As Integer) As Boolean

        message = New BM_CanMessageTypeDef()
        count = 0
        cycle = 0

        Dim maximumId As UInteger = If(txExtendedCheck.Checked, &H1FFFFFFFUI, &H7FFUI)
        Dim id As UInteger
        If Not TryParseHexUInt32(txIdEdit.Text, maximumId, id) Then
            ShowInputError(If(
                txExtendedCheck.Checked,
                "Extended CAN ID must be hexadecimal from 0 to 1FFFFFFF.",
                "Standard CAN ID must be hexadecimal from 0 to 7FF."))
            Return False
        End If

        Dim length As UInteger
        If Not UInteger.TryParse(txLengthEdit.Text, length) OrElse
            If(txFdCheck.Checked, Not IsValidCanFdLength(length), length > 8UI) Then
            ShowInputError(If(
                txFdCheck.Checked,
                "CAN FD length must be 0-8, 12, 16, 20, 24, 32, 48, or 64 bytes.",
                "Classic CAN length must be from 0 to 8 bytes."))
            Return False
        End If

        If Not UInteger.TryParse(txCountEdit.Text, count) OrElse count = 0 Then
            ShowInputError("Count must be a positive integer.")
            Return False
        End If
        If Not Integer.TryParse(txCycleEdit.Text, cycle) OrElse cycle <= 0 Then
            ShowInputError("Cycle must be a positive integer in milliseconds.")
            Return False
        End If

        Dim payload As String = txDataEdit.Text.Replace(" ", "")
        If payload.Length Mod 2 <> 0 Then
            ShowInputError("Payload must contain complete hexadecimal bytes.")
            Return False
        End If
        Dim payloadLength As UInteger = CUInt(payload.Length \ 2)
        If payloadLength > length Then
            ShowInputError("Payload contains more bytes than the declared length.")
            Return False
        End If

        Dim dlc As Integer = message.lengthToDLc(CInt(length))
        message.BM_CanMessageTypeDef(
            CInt(id),
            dlc,
            txExtendedCheck.Checked,
            False,
            txFdCheck.Checked,
            txFdCheck.Checked,
            False)
        For i As Integer = 0 To payload.Length \ 2 - 1
            Dim value As Byte
            If Not Byte.TryParse(
                payload.Substring(i * 2, 2),
                NumberStyles.AllowHexSpecifier,
                CultureInfo.InvariantCulture,
                value) Then
                ShowInputError("Payload contains an invalid hexadecimal byte.")
                Return False
            End If
            message.payload(i) = value
        Next

        Return True
    End Function

    Public Sub GetAvailableHostIpList()
        Dim allIps As New List(Of String)()
        Dim interfaceList As NetworkInterface() = NetworkInterface.GetAllNetworkInterfaces()

        For Each interfaceItem As NetworkInterface In interfaceList
            Dim isLoopback As Boolean = IsLoopbackInterface(interfaceItem)
            If interfaceItem.Supports(NetworkInterfaceComponent.IPv4) AndAlso
           Not isLoopback AndAlso
           Not interfaceItem.GetPhysicalAddress().ToString().StartsWith("005056") AndAlso
           Not interfaceItem.Name.IndexOf("vmware", StringComparison.OrdinalIgnoreCase) >= 0 Then
                Dim unicastAddresses As UnicastIPAddressInformationCollection = interfaceItem.GetIPProperties().UnicastAddresses

                For Each entry As UnicastIPAddressInformation In unicastAddresses
                    Dim ip As String = entry.Address.ToString()
                    If Not ip.StartsWith("fe80::") Then
                        allIps.Add(ip)
                    End If
                Next
            End If
        Next
        allIps.Add("127.0.0.1")
        Dim uniqueIps As New List(Of String)(New HashSet(Of String)(allIps))
        For i As Integer = 0 To uniqueIps.Count - 1
            If uniqueIps(i) <> "127.0.0.1" Then
                Dim lastDotIndex As Integer = uniqueIps(i).LastIndexOf(".")
                If lastDotIndex <> -1 Then
                    Dim prefix As String = uniqueIps(i).Substring(0, lastDotIndex + 1)
                    uniqueIps(i) = prefix & "255"
                End If
            End If
            hostComboBox.Items.Add(uniqueIps(i))
        Next
    End Sub

    Private Function IsLoopbackInterface(iface As NetworkInterface) As Boolean
        Dim unicastAddresses As UnicastIPAddressInformationCollection = iface.GetIPProperties().UnicastAddresses
        For Each entry As UnicastIPAddressInformation In unicastAddresses
            If entry.Address.ToString() = "127.0.0.1" Then
                Return True
            End If
        Next
        Return False
    End Function
    Private Sub enumerate()
        Dim succeed As Boolean = False

        Dim ip As Byte() = {192, 168, 41, 255}
        Dim remote As Boolean = ParseIpAddress(hostComboBox.Text, ip)
        channelComboBox.Items.Clear()
        Array.Clear(channelSelected, 0, channelSelected.Length)
        Array.Clear(channelinfos, 0, channelinfos.Length)
        nchannels = 0
        Dim infosize As Integer = Marshal.SizeOf(GetType(BM_ChannelInfoTypeDef))
        Dim ninfos As Integer = 32
        Dim infomem As IntPtr = Marshal.AllocHGlobal(ninfos * infosize)

        Try
            If remote Then
                If BMAPI.BM_EnumerateRemote(infomem, ninfos, ip, 100) = BM_StatusTypeDef.BM_ERROR_OK Then
                    succeed = True
                End If
            Else
                If BMAPI.BM_EnumerateByCap(infomem, ninfos, CType(BM_CapabilityTypeDef.BM_CAN_CAP, UShort)) = BM_StatusTypeDef.BM_ERROR_OK Then
                    succeed = True
                End If
            End If

            If succeed Then
                Dim ncanchannel As Integer = 0
                For i As Integer = 0 To ninfos - 1
                    Dim ptr As IntPtr = New IntPtr(infomem.ToInt64() + CLng(i) * infosize)
                    Dim channelInfo As BM_ChannelInfoTypeDef = DirectCast(
                        Marshal.PtrToStructure(ptr, GetType(BM_ChannelInfoTypeDef)),
                        BM_ChannelInfoTypeDef)
                    If (channelInfo.cap And BM_CapabilityTypeDef.BM_CAN_CAP) <> 0 Then
                        Dim name As String = channelInfo.GetName()
                        channelComboBox.Items.Add(name)
                        channelinfos(ncanchannel) = channelInfo
                        Dim keepSelected As Boolean = selectedChannelNames.Contains(name)
                        channelSelected(ncanchannel) = keepSelected
                        channelComboBox.SetItemChecked(ncanchannel, keepSelected)
                        ncanchannel += 1
                    End If
                Next
                nchannels = ncanchannel
            End If
        Finally
            Marshal.FreeHGlobal(infomem)
        End Try
    End Sub

    Private Sub openButton_Click(sender As Object, e As EventArgs) Handles openButton.Click
        Dim isOpen As Boolean = openButton.Text = "Open"
        If Not isOpen Then
            CloseOpenedChannels()
            Return
        End If

        Dim hasSelection As Boolean = False
        For i As Integer = 0 To nchannels - 1
            hasSelection = hasSelection OrElse channelSelected(i)
        Next
        If Not hasSelection Then
            Return
        End If

        Dim bitrate As BM_BitrateTypeDef
        Dim rxfilter As New BM_RxFilterTypeDef()
        If Not TryBuildOpenConfiguration(bitrate, rxfilter) Then
            Return
        End If

        Dim enablePtpSynchronization As Boolean = Not IsWindowsXpOrEarlier()
        For i As Integer = 0 To nchannels - 1
            If channelSelected(i) Then
                Dim mode As BM_CanModeTypeDef = CType(modeCombo.SelectedIndex, BM_CanModeTypeDef)
                Dim tres As BM_TerminalResistorTypeDef = If(
                    tresCheck.Checked,
                    BM_TerminalResistorTypeDef.BM_TRESISTOR_120,
                    BM_TerminalResistorTypeDef.BM_TRESISTOR_DISABLED)
                Dim rxfilterMem As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(BM_RxFilterTypeDef)))
                If rxAdvancedFilterCheck.Checked Then
                    MessageBox.Show("Advanced RX filters are currently unsupported.", "Warning")
                    rxAdvancedFilterCheck.Enabled = False
                End If
                Marshal.StructureToPtr(rxfilter, rxfilterMem, False)

                Dim openResult As BM_StatusTypeDef
                Try
                    openResult = BMAPI.BM_OpenEx(
                        channelHandles(openedChannelCount),
                        channelinfos(i),
                        mode,
                        tres,
                        bitrate,
                        rxfilterMem,
                        1
                    )
                Finally
                    Marshal.FreeHGlobal(rxfilterMem)
                End Try

                If openResult <> BM_StatusTypeDef.BM_ERROR_OK OrElse
                    channelHandles(openedChannelCount) = IntPtr.Zero Then
                    MessageBox.Show("Failed to open channel: " & openResult.ToString(), "Error")
                    CloseOpenedChannels()
                    Return
                End If

                If enablePtpSynchronization Then
                    Dim ns As ULong = LocalTimeToNsSince1970()
                    BMAPI.BM_SetPtpTime(channelHandles(openedChannelCount), ns)
                    BMAPI.BM_SetPtpMode(channelHandles(openedChannelCount), BM_PtpModeTypeDef.BM_PTP_INPUT_USB_SOF)
                End If
                sendComboBox.Items.Add(openedChannelCount + 1)
                openedChannelCount += 1
            End If
        Next

        If enablePtpSynchronization AndAlso openedChannelCount > 0 AndAlso
            BMAPI.BM_SyncPtpTimes(channelHandles, openedChannelCount) <> BM_StatusTypeDef.BM_ERROR_OK Then
            Console.WriteLine("Failed to sync PTP timestamps.")
        End If
        If openedChannelCount > 0 Then
            sendComboBox.SelectedIndex = 0
            ClearMessageHistory()
            rxTimer = New System.Windows.Forms.Timer()
            rxTimer.Interval = 50
            rxTimer.Start()
            openButton.Text = "Close"
            rxIdEdit.Enabled = False
            rxMaskEdit.Enabled = False
            rxExtendedCheck.Enabled = False
            rxStandardCheck.Enabled = False
            rxDataMaskEdit.Enabled = False
            modeCombo.Enabled = False
            tresCheck.Enabled = False
            nominalBitrateEdit.Enabled = False
            dataBitrateEdit.Enabled = False
            samplePosSpin.Enabled = False
            enumerateButton.Enabled = False
            txButton.Enabled = True
            channelComboBox.Enabled = False
        End If
    End Sub

    Private Sub StopTransmission()
        If txTimer IsNot Nothing Then
            txTimer.Stop()
            txTimer.Dispose()
            txTimer = Nothing
        End If
        pendingTxMsgCount = 0
        txButton.Text = "Transmit"
    End Sub

    Private Sub CloseOpenedChannels()
        Dim isWindowsXp As Boolean = IsWindowsXpOrEarlier()
        Dim hadOpenedHandle As Boolean = False
        For i As Integer = 0 To channelHandles.Length - 1
            If channelHandles(i) <> IntPtr.Zero Then
                hadOpenedHandle = True
                Exit For
            End If
        Next
        Dim waitForXpUsbRelease As Boolean = isWindowsXp AndAlso hadOpenedHandle
        StopTransmission()
        If rxTimer IsNot Nothing Then
            rxTimer.Stop()
            rxTimer.Dispose()
            rxTimer = Nothing
        End If

        If isWindowsXp Then
            For i As Integer = 0 To channelHandles.Length - 1
                If channelHandles(i) <> IntPtr.Zero Then
                    BMAPI.BM_ClearBuffer(channelHandles(i))
                End If
            Next
            For i As Integer = 0 To channelHandles.Length - 1
                If channelHandles(i) <> IntPtr.Zero Then
                    BMAPI.BM_SetCanMode(channelHandles(i), BM_CanModeTypeDef.BM_CAN_OFF_MODE)
                End If
            Next
        End If

        For i As Integer = 0 To channelHandles.Length - 1
            If channelHandles(i) <> IntPtr.Zero Then
                BMAPI.BM_Close(channelHandles(i))
                channelHandles(i) = IntPtr.Zero
            End If
        Next
        If waitForXpUsbRelease Then
            System.Threading.Thread.Sleep(XpUsbCloseSettleMilliseconds)
        End If

        openedChannelCount = 0
        sendComboBox.Items.Clear()
        sendComboBox.Text = ""
        openButton.Text = "Open"
        rxIdEdit.Enabled = True
        rxMaskEdit.Enabled = True
        rxExtendedCheck.Enabled = True
        rxStandardCheck.Enabled = True
        rxDataMaskEdit.Enabled = rxAdvancedFilterCheck.Checked
        modeCombo.Enabled = True
        tresCheck.Enabled = True
        nominalBitrateEdit.Enabled = True
        dataBitrateEdit.Enabled = True
        samplePosSpin.Enabled = True
        enumerateButton.Enabled = True
        txButton.Enabled = False
        channelComboBox.Enabled = True
    End Sub

    Private Function TryGetUnavailableOpenedChannel(
        ByRef channelNumber As Integer,
        ByRef failureStatus As BM_StatusTypeDef) As Boolean

        channelNumber = 0
        failureStatus = BM_StatusTypeDef.BM_ERROR_OK
        For i As Integer = 0 To openedChannelCount - 1
            If channelHandles(i) = IntPtr.Zero Then
                Continue For
            End If

            Dim statusInfo As New BM_CanStatusInfoTypedef()
            Dim status As BM_StatusTypeDef = BMAPI.BM_GetStatus(channelHandles(i), statusInfo)
            If IsChannelLossStatus(status) Then
                channelNumber = i + 1
                failureStatus = status
                Return True
            End If
        Next
        Return False
    End Function


    Private Sub rxAdvancedFilterCheck_CheckedChanged(
        sender As Object,
        e As EventArgs) Handles rxAdvancedFilterCheck.CheckedChanged

        rxDataMaskEdit.Enabled = rxAdvancedFilterCheck.Checked
    End Sub

    Private Sub enumerateButton_Click(sender As Object, e As EventArgs) Handles enumerateButton.Click
        enumerate()
    End Sub

    Private Sub txButton_Click(sender As Object, e As EventArgs) Handles txButton.Click
        Dim isTransmit As Boolean = txButton.Text = "Transmit"
        If Not isTransmit Then
            StopTransmission()
            Return
        End If

        Dim channelIndex As Integer = sendComboBox.SelectedIndex
        If channelIndex < 0 OrElse channelHandles(channelIndex) = IntPtr.Zero Then
            ShowInputError("Select an open channel before transmitting.")
            Return
        End If

        Dim message As New BM_CanMessageTypeDef()
        Dim count As UInteger
        Dim cycle As Integer
        If Not TryBuildTransmitMessage(message, count, cycle) Then
            Return
        End If

        txMsgBuf = message
        pendingTxMsgCount = count
        txTimer = New System.Windows.Forms.Timer()
        txTimer.Interval = cycle
        txTimer.Start()
        txButton.Text = "Stop"
        writePendingMessages()
    End Sub

    Public Shared Function GetUtcTimestamp(msgLen As UInteger, canMsg As BM_CanMessageTypeDef) As ULong
        Dim tailPtr As IntPtr = IntPtr.Zero
        Dim bufferPtr As IntPtr = IntPtr.Zero

        Try
            tailPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(BM_DataTailTypeDef)))

            Dim structSize As Integer = Marshal.SizeOf(GetType(BM_CanMessageTypeDef))
            bufferPtr = Marshal.AllocHGlobal(structSize)
            Marshal.StructureToPtr(canMsg, bufferPtr, False)

            Dim offset As Integer = CInt(msgLen - BMAPI.BM_DATA_TAIL_SIZE)
            Dim bytesToCopy As Integer = Math.Min(structSize - offset, Marshal.SizeOf(GetType(BM_DataTailTypeDef)))

            For j As Integer = 0 To bytesToCopy - 1
                Dim value As Byte = Marshal.ReadByte(bufferPtr, j + offset)
                Marshal.WriteByte(tailPtr, j, value)
            Next
            Dim data As BM_DataTailTypeDef = DirectCast(Marshal.PtrToStructure(tailPtr, GetType(BM_DataTailTypeDef)), BM_DataTailTypeDef)
            Return data.GetUtcTimestamp()

        Finally
            If tailPtr <> IntPtr.Zero Then
                Marshal.FreeHGlobal(tailPtr)
            End If

            If bufferPtr <> IntPtr.Zero Then
                Marshal.FreeHGlobal(bufferPtr)
            End If
        End Try
    End Function
    Private Sub readPendingMessages()
        If openedChannelCount = 0 Then
            Return
        End If

        Dim batch As New List(Of ReceivedMessage)()
        Dim channelPending As UInteger
        If openedChannelCount >= 32 Then
            channelPending = UInteger.MaxValue
        Else
            channelPending = (1UI << openedChannelCount) - 1UI
        End If
        Dim maxMessagesThisTick As Integer = If(
            IsWindowsXpOrEarlier(),
            MaxXpMessagesPerTimerTick,
            Integer.MaxValue)
        Dim msg As New BM_DataTypeDef()

        While channelPending <> 0 AndAlso batch.Count < maxMessagesThisTick
            For channelOffset As Integer = 0 To openedChannelCount - 1
                If batch.Count >= maxMessagesThisTick Then
                    Exit For
                End If
                Dim channelBit As UInteger = 1UI << channelOffset
                If (channelPending And channelBit) = 0 Then
                    Continue For
                End If

                If channelHandles(channelOffset) = IntPtr.Zero Then
                    channelPending = channelPending And Not channelBit
                    Continue For
                End If

                Dim status As BM_StatusTypeDef = BMAPI.BM_Read(Me.channelHandles(channelOffset), msg)
                If status = BM_StatusTypeDef.BM_ERROR_OK Then
                    Dim utcts As ULong = 0
                    If BMAPI.BM_GetDataPtpTimestamp(Me.channelHandles(channelOffset), msg, utcts) <> BM_StatusTypeDef.BM_ERROR_OK Then
                        'If failed to extract a PTP timestamp from data, use current time on host-machine.
                        utcts = BMAPI.BM_GetHostPtpTime()
                    End If

                    Dim canCopy As BM_CanMessageTypeDef = msg.can
                    If msg.can.payload IsNot Nothing Then
                        canCopy.payload = DirectCast(msg.can.payload.Clone(), Byte())
                    End If

                    batch.Add(New ReceivedMessage With {
                        .Channel = channelOffset + 1,
                        .CanMessage = canCopy,
                        .Timestamp = msg.timestamp,
                        .UtcTimestamp = utcts,
                        .IsTx = msg.header.IsAckData()
                    })
                ElseIf status = BM_StatusTypeDef.BM_ERROR_QRCVEMPTY Then
                    channelPending = channelPending And Not channelBit
                Else
                    channelPending = channelPending And Not channelBit
                    Console.Error.WriteLine(
                        "BM_Read failed on channel {0}: {1}",
                        channelOffset + 1,
                        status)
                    If IsChannelLossStatus(status) Then
                        AppendMessageBatch(batch)
                        CloseOpenedChannels()
                        MessageBox.Show(
                            Me,
                            "Channel " & (channelOffset + 1).ToString() &
                                " is no longer available (" & status.ToString() &
                                "). Re-enumerate the device and open it again.",
                            "Channel disconnected",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)
                        Return
                    End If
                End If
            Next
        End While

        AppendMessageBatch(batch)
    End Sub

    Private Sub ClearMessageHistory()
        messageHistory.Clear()
        msgListView.VirtualListSize = 0
        msgListView.Invalidate()
    End Sub

    Private Sub AppendMessageBatch(batch As IList(Of ReceivedMessage))
        If batch.Count = 0 Then
            Return
        End If

        Dim firstIncoming As Integer = Math.Max(0, batch.Count - MaxDisplayedMessages)
        Dim incomingCount As Integer = batch.Count - firstIncoming
        Dim removeCount As Integer = Math.Max(
            0,
            messageHistory.Count + incomingCount - MaxDisplayedMessages)
        If removeCount > 0 Then
            messageHistory.RemoveRange(0, removeCount)
        End If

        For i As Integer = firstIncoming To batch.Count - 1
            messageHistory.Add(batch(i))
        Next

        msgListView.VirtualListSize = messageHistory.Count
        msgListView.Invalidate()
        msgListView.EnsureVisible(messageHistory.Count - 1)
    End Sub

    Private Sub writePendingMessages()
        Dim channelIndex As Integer = sendComboBox.SelectedIndex
        If channelIndex < 0 OrElse channelHandles(channelIndex) = IntPtr.Zero Then
            Return
        End If

        If pendingTxMsgCount > 0 Then
            Dim timestamp As UInteger = 0
            Dim port As UInteger = 0
            Dim status As BM_StatusTypeDef = BMAPI.BM_WriteCanMessage(
                channelHandles(channelIndex),
                txMsgBuf,
                port,
                0,
                timestamp)
            If status = BM_StatusTypeDef.BM_ERROR_OK Then
                pendingTxMsgCount -= 1UI
                If pendingTxMsgCount = 0 Then
                    StopTransmission()
                End If
            Else
                StopTransmission()
                If IsChannelLossStatus(status) Then
                    CloseOpenedChannels()
                    MessageBox.Show(
                        Me,
                        "The transmit channel is no longer available (" & status.ToString() &
                            "). Re-enumerate the device and open it again.",
                        "Channel disconnected",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning)
                Else
                    MessageBox.Show(Me, "Failed to transmit message: " & status.ToString(), "Transmit failed")
                End If
            End If
        End If
    End Sub

    Public Shared Sub TimevalToFormattedString(utcTimestamp As ULong, str As StringBuilder, capacity As Integer)
        Dim seconds As Long = CLng(utcTimestamp \ 1000000000)
        Dim microseconds As Long = CLng(utcTimestamp Mod 1000000000 \ 1000)

        Dim dateTime As DateTime
        Try
            dateTime = New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) _
               .AddSeconds(seconds) _
               .ToLocalTime()
        Catch ex As ArgumentOutOfRangeException
            str.Append("Invalid")
            Return
        End Try

        Dim formatted As String = dateTime.ToString("yyyy-MM-dd HH:mm:ss.")

        If str.Capacity < capacity Then
            str.Capacity = capacity
        End If

        str.Clear()
        str.Append(formatted)
        str.Append(microseconds.ToString("D6"))
    End Sub
    Private Sub msgListView_RetrieveVirtualItem(
        sender As Object,
        e As RetrieveVirtualItemEventArgs)

        If e.ItemIndex < 0 OrElse e.ItemIndex >= messageHistory.Count Then
            e.Item = New ListViewItem()
            Return
        End If

        e.Item = CreateMessageItem(messageHistory(e.ItemIndex))
    End Sub

    Private Shared Function CreateMessageItem(received As ReceivedMessage) As ListViewItem
        Dim item As System.Windows.Forms.ListViewItem = New System.Windows.Forms.ListViewItem()
        Dim str As New StringBuilder(32)


        item.Text = Convert.ToString(received.Timestamp * 0.000001F)
        If received.UtcTimestamp > 0 Then
            TimevalToFormattedString(received.UtcTimestamp, str, 32)
        Else
            str.Clear()
            str.Append("N/A")
        End If
        item.SubItems.Add(Convert.ToString(str))

        If received.IsTx Then
            item.SubItems.Add("TX")
        Else
            item.SubItems.Add("RX")
        End If

        item.SubItems.Add(Convert.ToString(received.Channel))
        item.SubItems.Add(Convert.ToString(received.CanMessage.GetID(), 16).ToUpper())
        item.SubItems.Add(received.CanMessage.GetTypeString())
        item.SubItems.Add(Convert.ToString(received.CanMessage.GetLength()))
        item.SubItems.Add(received.CanMessage.GetPayloadString())
        Return item
    End Function

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        CloseOpenedChannels()
        BMAPI.BM_UnInit()
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Sub WndProc(ByRef message As Message)
        Const WM_DEVICECHANGE As Integer = &H219
        Try
            If message.Msg = WM_DEVICECHANGE Then
                If openButton.Text = "Open" Then
                    enumerate()
                Else
                    Dim channelNumber As Integer
                    Dim failureStatus As BM_StatusTypeDef
                    If TryGetUnavailableOpenedChannel(channelNumber, failureStatus) Then
                        CloseOpenedChannels()
                        MessageBox.Show(
                            Me,
                            "Channel " & channelNumber.ToString() &
                                " is no longer available (" & failureStatus.ToString() &
                                "). Re-enumerate the device and open it again.",
                            "Channel disconnected",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)
                    End If
                End If
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
        MyBase.WndProc(message)
    End Sub


    ' See for details https//www.shuzhiduo.com/A/1O5EZB2Gd7/
    Private Sub channelComboBox_ItemClick(sender As Object, e As System.Windows.Forms.ItemCheckEventArgs) Handles channelComboBox.ItemClick
        If e.Index >= 0 AndAlso e.Index < nchannels AndAlso e.Index < channelSelected.Length Then
            Dim isChecked As Boolean = e.NewValue = CheckState.Checked
            channelSelected(e.Index) = isChecked
            Dim channelName As String = channelinfos(e.Index).GetName()
            If isChecked Then
                selectedChannelNames.Add(channelName)
            Else
                selectedChannelNames.Remove(channelName)
            End If
        End If
    End Sub


    Private Sub txTimer_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles txTimer.Tick
        writePendingMessages()
    End Sub



    Private Sub rxTimer_Tick(sender As Object, e As EventArgs) Handles rxTimer.Tick
        readPendingMessages()
    End Sub
End Class
