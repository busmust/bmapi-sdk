Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Windows.Forms
Imports System.Net
Imports System.Net.NetworkInformation
Imports System.Collections.Generic
Partial Public Class MainForm
    Private rxMsgBuf As BM_CanMessageTypeDef
    Private txMsgBuf As BM_CanMessageTypeDef
    Private channelinfos(31) As BM_ChannelInfoTypeDef
    Private channelHandles(31) As IntPtr
    Private openedChannelCount As Integer
    Private nchannels As Integer 'Number Of enumerated channels
    Private channelSelected(31) As Boolean
    Private pendingTxMsgCount As Integer
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

    Public Sub MainForm()
        Array.Clear(channelHandles, 0, channelHandles.Length)
        Array.Clear(channelSelected, 0, channelSelected.Length)
        openedChannelCount = 0
        modeCombo.SelectedIndex = 0
        txTypeCombo.SelectedIndex = 0
        rxMsgBuf = New BM_CanMessageTypeDef()
        txMsgBuf = New BM_CanMessageTypeDef()
        BMAPI.BM_Init()
        GetAvailableHostIpList()
        enumerate()
        hostComboBox.SelectedIndex = 0
    End Sub

    Public Shared Function LocalTimeToNsSince1970() As ULong
        Dim localTime As DateTimeOffset = DateTimeOffset.Now
        Dim msSince1970 As Long = localTime.ToUnixTimeMilliseconds()
        Return (msSince1970) * 1000000UL
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
    Private Function enumerate()
        Dim succeed As Boolean = False

        Dim ip As Byte() = {192, 168, 41, 255}
        Dim remote As Boolean = ParseIpAddress(hostComboBox.Text, ip)
        channelComboBox.Items.Clear()
        Dim infosize As Integer = Marshal.SizeOf(GetType(BM_ChannelInfoTypeDef))
        Dim ninfos As Integer = 32
        Dim infomem As IntPtr
        infomem = Marshal.AllocHGlobal(ninfos * infosize)

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
            nchannels = ninfos
            Dim ncanchannel As Integer = 0
            For i As Integer = 0 To ninfos - 1
                Dim ptr As IntPtr = (infomem.ToInt64() + i * infosize)
                Dim channelInfo As BM_ChannelInfoTypeDef = Marshal.PtrToStructure(ptr, GetType(BM_ChannelInfoTypeDef))
                If (channelInfo.cap And BM_CapabilityTypeDef.BM_CAN_CAP) <> 0 Then
                    Dim name As String = channelInfo.GetName()
                    channelComboBox.AddItems(name)
                    channelinfos(ncanchannel) = channelInfo
                    ncanchannel += 1
                End If
            Next
        End If
        Marshal.FreeHGlobal(infomem)
        Return vbNull
    End Function

    Private Sub openButton_Click(sender As Object, e As EventArgs) Handles openButton.Click
        Dim success As Boolean = True
        Dim isOpen As Boolean = False
        Dim infomem As IntPtr
        Dim tres As BM_TerminalResistorTypeDef
        If openButton.Text = "Open" Then
            isOpen = True
        End If
        Dim ninfos As Integer = 32
        infomem = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(BM_ChannelInfoTypeDef)))
        For i As Integer = 0 To nchannels - 1
            If (isOpen And channelSelected(i)) Then
                Dim mode As BM_CanModeTypeDef = modeCombo.SelectedIndex
                If tresCheck.Checked Then
                    tres = BM_TerminalResistorTypeDef.BM_TRESISTOR_120
                Else
                    tres = BM_TerminalResistorTypeDef.BM_TRESISTOR_DISABLED
                End If

                Dim bitrate As BM_BitrateTypeDef = New BM_BitrateTypeDef()
                bitrate.nbitrate = Convert.ToUInt16(nominalBitrateEdit.Text)
                bitrate.dbitrate = Convert.ToUInt16(dataBitrateEdit.Text)
                bitrate.nsamplepos = Convert.ToByte(samplePosSpin.Value)
                bitrate.dsamplepos = Convert.ToByte(samplePosSpin.Value)
                Dim rxfilterMem As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(BM_RxFilterTypeDef)))
                Dim rxfilter As BM_RxFilterTypeDef = Marshal.PtrToStructure(rxfilterMem, GetType(BM_RxFilterTypeDef))
                If rxAdvancedFilterCheck.Checked Then
                    rxfilter.type = BM_RxFilterTypeTypeDef.BM_RXFILTER_ADVANCED
                Else
                    rxfilter.type = BM_RxFilterTypeTypeDef.BM_RXFILTER_BASIC
                End If
                rxfilter.SetIdFilter(Convert.ToUInt32(rxMaskEdit.Text, 16), Convert.ToUInt32(rxIdEdit.Text, 16), False)
                If (rxStandardCheck.Checked And Not (rxExtendedCheck.Checked)) Then
                    rxfilter.flags_mask = BM_MessageFlagsTypeDef.BM_MESSAGE_FLAGS_IDE
                    rxfilter.flags_value = 0
                ElseIf (Not (rxStandardCheck.Checked) And rxExtendedCheck.Checked) Then
                    rxfilter.flags_mask = BM_MessageFlagsTypeDef.BM_MESSAGE_FLAGS_IDE
                    rxfilter.flags_value = BM_MessageFlagsTypeDef.BM_MESSAGE_FLAGS_IDE
                Else
                    '/* Default as all enabled */
                    rxStandardCheck.Checked = True
                    rxExtendedCheck.Checked = True
                    rxfilter.flags_mask = 0
                    rxfilter.flags_value = 0
                End If

                If (rxAdvancedFilterCheck.Checked) Then
                    Dim j As Integer
                    Dim payload As String = rxDataMaskEdit.Text.Replace(" ", "").ToLower().Replace("x", "0")
                    While (j * 2 + 2 <= payload.Length)
                        rxfilter.payload_mask(j) = Convert.ToByte(payload.Substring(j * 2, 2), 16)
                        rxfilter.payload_value(j) = rxfilter.payload_mask(j)
                        j = j + 1
                    End While
                    System.Windows.Forms.MessageBox.Show("Advanced RX filters are currently unsupported.", "Warning")
                    rxAdvancedFilterCheck.Enabled = False
                End If
                Marshal.StructureToPtr(rxfilter, rxfilterMem, True)

                Dim openResult As BM_StatusTypeDef = BMAPI.BM_OpenEx(
                        channelHandles(openedChannelCount),
                        channelinfos(i),
                        mode,
                        tres,
                        bitrate,
                        rxfilterMem,
                        1
                    )

                If (openResult = BM_StatusTypeDef.BM_ERROR_OK) Then
                    Dim ns As ULong = LocalTimeToNsSince1970()
                    BMAPI.BM_SetPtpTime(channelHandles(openedChannelCount), ns)
                    sendComboBox.Items.Add(openedChannelCount + 1)
                    openedChannelCount = openedChannelCount + 1
                Else
                    System.Windows.Forms.MessageBox.Show("Failed to open channel.", "Error")
                    success = False
                End If
                BMAPI.BM_SetPtpMode(channelHandles(openedChannelCount), BM_PtpModeTypeDef.BM_PTP_INPUT_USB_SOF)
            Else
                If (Not (txButton.Text = "Transmit")) Then
                    txButton_Click()
                End If

                If (rxTimer.Enabled) Then
                    rxTimer.Stop()
                End If
            End If
        Next
        '// Sync PTP timestamp for all devices
        If Not (BMAPI.BM_SyncPtpTimes(channelHandles, openedChannelCount) = BM_StatusTypeDef.BM_ERROR_OK) Then
            Console.WriteLine("Failed to sync PTP timestamps.")
        End If
        If (isOpen And success And openedChannelCount > 0) Then
                sendComboBox.SelectedIndex = 0
                msgListView.Items.Clear()
                rxTimer = New System.Windows.Forms.Timer()
                rxTimer.Interval = 50
                rxTimer.Start()
                openButton.Text = "Close"
                rxIdEdit.Enabled = False
                rxMaskEdit.Enabled = False
                rxExtendedCheck.Enabled = False
                rxStandardCheck.Enabled = False
                'rxAdvancedFilterCheck.Enabled = false
                rxDataMaskEdit.Enabled = False
                modeCombo.Enabled = False
                tresCheck.Enabled = False
                nominalBitrateEdit.Enabled = False
                dataBitrateEdit.Enabled = False
                samplePosSpin.Enabled = False
                enumerateButton.Enabled = False
                txButton.Enabled = True
                channelComboBox.Enabled = False

            Else
                For j As Integer = 0 To openedChannelCount - 1
                    BMAPI.BM_Close(channelHandles(j))
                Next
                openedChannelCount = 0
                sendComboBox.Items.Clear()
                sendComboBox.Text = ""
                openButton.Text = "Open"
                rxIdEdit.Enabled = True
                rxMaskEdit.Enabled = True
                rxExtendedCheck.Enabled = True
                rxStandardCheck.Enabled = True
                'rxAdvancedFilterCheck.Enabled = true
                rxDataMaskEdit.Enabled = rxAdvancedFilterCheck.Checked
                modeCombo.Enabled = True
                tresCheck.Enabled = True
                nominalBitrateEdit.Enabled = True
                dataBitrateEdit.Enabled = True
                samplePosSpin.Enabled = True
                enumerateButton.Enabled = True
                txButton.Enabled = False
                channelComboBox.Enabled = True
            End If

    End Sub


    Private Function rxAdvancedFilterCheck_CheckedChanged()
        rxDataMaskEdit.Enabled = rxAdvancedFilterCheck.Checked
        Return vbNull
    End Function
    Private Sub enumerateButton_Click(sender As Object, e As EventArgs) Handles enumerateButton.Click
        enumerate()
    End Sub

    Private Function txButton_Click() Handles txButton.Click
        Dim success As Boolean = False
        Dim isTransmit As Boolean = False
        If txButton.Text = "Transmit" Then
            isTransmit = True
        End If
        Dim channelIndex As Integer
        channelIndex = sendComboBox.SelectedIndex
        If (channelHandles(channelIndex) = IntPtr.Zero) Then
            Return False
        End If
        If (isTransmit And channelIndex >= 0) Then
            Dim id As Integer = Convert.ToUInt32(txIdEdit.Text, 16)
            Dim length As Integer = Convert.ToUInt32(txLengthEdit.Text)
            Dim dlc As Integer = txMsgBuf.lengthToDLc(length)
            Dim DlcToDataBytes() As Integer = New Integer(15) {0, 1, 2, 3,
                                                           4, 5, 6, 7,
                                                           8, 12, 16, 20,
                                                           24, 32, 48, 64}
            Dim dlcToLength As Integer = DlcToDataBytes(dlc)
            If Not (length = dlcToLength) Then
                System.Windows.Forms.MessageBox.Show("According to ISO standard, CANFD message payload length could only be 0-8,12,16,20,24,32,48 or 64.", "Warning")
            End If

            txMsgBuf = New BM_CanMessageTypeDef()
            txMsgBuf.BM_CanMessageTypeDef(id, dlc, txExtendedCheck.Checked, False, txFdCheck.Checked, txFdCheck.Checked, False)
            Dim payload As String = txDataEdit.Text.Replace(" ", "")
            Dim j As Integer = 0
            While (j * 2 + 2 <= payload.Length)
                txMsgBuf.payload(j) = Convert.ToByte(payload.Substring(j * 2, 2), 16)
                j = j + 1
            End While
            success = True
        End If

        If (isTransmit And success) Then
            pendingTxMsgCount = Convert.ToUInt32(txCountEdit.Text)
            Dim cycle As Integer = Convert.ToInt32(txCycleEdit.Text)
            txTimer = New System.Windows.Forms.Timer()
            txTimer.Interval = cycle
            txTimer.Start()
            txButton.Text = "Stop"
            writePendingMessages()
        Else
            If (txTimer.Enabled) Then
                txTimer.Stop()
                pendingTxMsgCount = 0
                txButton.Text = "Transmit"
            End If
        End If
        Return vbNull
    End Function

    Public Shared Function GetUtcTimestamp(msgLen As UInteger, canMsg As BM_CanMessageTypeDef) As ULong
        Dim tailPtr As IntPtr = IntPtr.Zero
        Dim bufferPtr As IntPtr = IntPtr.Zero

        Try
            tailPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(BM_DataTailTypeDef)))

            Dim structSize As Integer = Marshal.SizeOf(Of BM_CanMessageTypeDef)()
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
    Private Function readPendingMessages()
        Dim msg As New BM_DataTypeDef()
        For i As Integer = 0 To openedChannelCount - 1
            If (channelHandles(i)) Then
                Dim timestamp As Integer = 0
                While BMAPI.BM_Read(Me.channelHandles(i), msg) = BM_StatusTypeDef.BM_ERROR_OK
                    Me.rxMsgBuf = msg.can
                    timestamp = msg.timestamp
                    Dim msgLen As UInteger = msg.can.GetLength()
                    Dim utcts As ULong = 0
                    If BMAPI.BM_GetDataPtpTimestamp(Me.channelHandles(i), msg, utcts) <> BM_StatusTypeDef.BM_ERROR_OK Then
                        'If failed to extract a PTP timestamp from data, use current time on host-machine.
                        utcts = BMAPI.BM_GetHostPtpTime()
                    End If
                    Dim istx As Boolean = msg.header.IsAckData()
                    DisplayMessage(i + 1, Me.rxMsgBuf, timestamp, utcts, istx)
                End While
            End If

        Next
        Return vbNull
    End Function

    Private Function writePendingMessages()
        Dim channelIndex As Integer = sendComboBox.SelectedIndex
        If channelIndex = -1 Then
            Return False
        End If
        If (channelHandles(channelIndex) = IntPtr.Zero) Then
            Return False
        End If
        If (channelIndex >= 0) Then
            If (pendingTxMsgCount > 0) Then
                Dim timestamp As Integer = 0
                Dim port As Integer = 0
                Dim ns As ULong = 0
                BMAPI.BM_GetPtpTime(channelHandles(channelIndex), ns)
                Dim status As BM_StatusTypeDef = BMAPI.BM_WriteCanMessage(channelHandles(channelIndex), txMsgBuf, port, 0, timestamp)
                If (status = BM_StatusTypeDef.BM_ERROR_OK) Then
                    pendingTxMsgCount = pendingTxMsgCount - 1
                    If (pendingTxMsgCount <= 0) Then
                        txButton_Click()
                    End If
                End If
            End If
        End If

        Return vbNull
    End Function

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
    Private Function DisplayMessage(i As Integer, canMsg As BM_CanMessageTypeDef, timestamp As Integer, utcts As ULong, tx As Boolean)
        Dim item As System.Windows.Forms.ListViewItem = New System.Windows.Forms.ListViewItem()
        Dim str As New StringBuilder(32)


        item.Text = Convert.ToString(timestamp * 0.000001F)
        If utcts > 0 Then
            TimevalToFormattedString(utcts, str, 32)
        Else
            str.Clear()
            str.Append("N/A")
        End If
        item.SubItems.Add(Convert.ToString(str))

        If tx Then
            item.SubItems.Add("TX")
        Else
            item.SubItems.Add("RX")
        End If

        item.SubItems.Add(Convert.ToString(i))
        item.SubItems.Add(Convert.ToString(canMsg.GetID(), 16).ToUpper())
        item.SubItems.Add(canMsg.GetTypeString())
        item.SubItems.Add(Convert.ToString(canMsg.GetLength()))
        item.SubItems.Add(canMsg.GetPayloadString())
        msgListView.Items.Add(item)
        msgListView.Items(msgListView.Items.Count - 1).EnsureVisible()
        Return vbNull
    End Function

    'Protected Function WndProc(m As System.Windows.Forms.Message)
    '    Const WM_DEVICECHANGE As Integer = &H219
    '    Try
    '        If (m.Msg = WM_DEVICECHANGE) Then
    '            If (openButton.Text = "Open") Then
    '                enumerate()
    '            End If

    '        End If


    '    Catch
    '        Dim ex As Exception = New Exception
    '        System.Windows.Forms.MessageBox.Show(ex.Message)
    '    End Try
    '    WndProc(m)
    '    Return vbNull
    'End Function


    ' See for details https//www.shuzhiduo.com/A/1O5EZB2Gd7/
    Private Sub channelComboBox_ItemClick(sender As Object, e As System.Windows.Forms.ItemCheckEventArgs) Handles channelComboBox.ItemClick
        channelSelected(e.Index) = Not channelSelected(e.Index)
    End Sub


    Private Sub txTimer_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles txTimer.Tick
        writePendingMessages()
    End Sub



    Private Sub rxTimer_Tick(sender As Object, e As EventArgs) Handles rxTimer.Tick
        readPendingMessages()
    End Sub
End Class
