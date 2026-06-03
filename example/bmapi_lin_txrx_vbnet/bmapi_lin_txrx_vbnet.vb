Imports System
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading.Tasks

Public Module Constants
    Public Const TEST_MSG_RX_TIMEOUT As Integer = 1000
    Public Const TEST_MSG_TX_TIMEOUT As Integer = 500
    Public Const TEST_CHECKSUM_TYPE As Integer = 1 ' Classic = 0, Enhance = 1
    Public Const TEST_MSG_COUNT As UInteger = 100
    Public Const TEST_MSG_ROUND As UInteger = 5
    Public Const TEST_MSG_CYCLE As UInteger = 10
    Public Const TEST_MSG_TX_ID As UInteger = &H3
    Public Const TEST_MSG_LENGTH As UInteger = &H8
End Module

Namespace BMAPILinBUS
    Public Module Program
        Private ReadOnly msgtypes As String() = {
            "lin_receive",
            "lin_master_write",
            "lin_master_read",
            "lin_slave_write"
        }

        Private ReadOnly sendtypes As String() = {
            "lin_bm_write",
            "lin_bm_txtask"
        }

        Public Function ConvertCanToLin(canMsg As BM_CanMessageTypeDef) As BM_LinMessageTypeDef
            Dim linCtrl As New BM_LinMessageCtrlTypeDef()
            linCtrl.AllBits = canMsg.ctrl
            Dim linId As UInteger = canMsg.id
            Dim linMsg As New BM_LinMessageTypeDef(linCtrl, linId)
            Array.Copy(canMsg.payload, linMsg.payload, linCtrl.DLC)
            Return linMsg
        End Function

        Public Enum LIN_MSG_TYPE
            LIN_RECEIVE = 0
            LIN_MASTER_WRITE
            LIN_MASTER_READ
            LIN_SLAVE_WRITE
        End Enum

        Public Enum LIN_SEND_TYPE
            LIN_BM_WRITE = 0
            LIN_BM_TXTASK
        End Enum

        ' 计算 PID
        Public Function CalculatePID(ByVal ucId As Byte) As UInteger
            Dim byteArray(7) As Byte
            For nIndex As Integer = 0 To 7
                byteArray(nIndex) = CByte((ucId >> nIndex) And 1)
            Next

            Dim chP0 As Byte = CByte(byteArray(0) Xor byteArray(1) Xor byteArray(2) Xor byteArray(4))
            Dim chP1 As Byte = CByte(Not (byteArray(1) + byteArray(3) + byteArray(4) + byteArray(5)))

            chP0 = CByte(chP0 << 6)
            chP1 = CByte(chP1 << 7)
            Return CByte(ucId + chP0 + chP1)
        End Function

        ' 计算校验和
        Public Function CalculateChecksum(ByVal xData() As Byte, ByVal xDataLen As Byte, ByVal pId As Byte) As Byte
            Dim sum As UShort = pId
            For i As Byte = 0 To xDataLen - 1
                sum += xData(i)
                While sum > &HFF
                    sum -= &HFF
                End While
            Next
            Return CByte((Not sum) And &HFF)
        End Function

        Public Sub printf_lin_msg(ByVal msg As BM_LinMessageTypeDef, ByVal nmessages As UInteger, ByVal n As UInteger, ByVal timestamp As UInteger, ByVal istx As Boolean)
            Dim direction As String = If(istx, "Tx", "Rx")
            Dim checksumType As String = If(msg.ctrl.ENHANCED_CHECKSUM, "Enhanced", "Classic")
            Console.WriteLine(
                $"LIN {direction} [{n + 1}/{nmessages}] @{timestamp}: id={msg.id:X2}, payload=[{msg.payload(0):X2}, {msg.payload(1):X2}, {msg.payload(2):X2}, {msg.payload(3):X2}, {msg.payload(4):X2}, {msg.payload(5):X2}, {msg.payload(6):X2}, {msg.payload(7):X2}] checksum = {msg.GetChecksum():x} checksumType = {checksumType}"
            )
        End Sub

        Public Function lin_receive(ByVal channel As IntPtr, ByVal notification As IntPtr, ByVal nmessages As UInteger) As Integer
            Dim status As BM_StatusTypeDef = BM_StatusTypeDef.BM_ERROR_OK
            Dim n As UInteger = 0
            Dim napifailures As UInteger = 0
            Dim nlostmsgs As UInteger = 0
            Dim nbadmsgs As UInteger = 0
            Dim notifications() As IntPtr = {notification}
            Console.WriteLine("Waiting for RX notification ...")

            Dim t1 As DateTime = DateTime.Now
            Dim bmData As BM_DataTypeDef = New BM_DataTypeDef()
            While n < nmessages
                If BMAPI.BM_WaitForNotifications(notifications, 1, Constants.TEST_MSG_RX_TIMEOUT) < 0 Then
                    Console.WriteLine($"Receive timeout, offset={n}.")
                End If
                Do
                    status = BMAPI.BM_Read(channel, bmData)
                    If status = BM_StatusTypeDef.BM_ERROR_OK Then
                        Dim linMessage As BM_LinMessageTypeDef = ConvertCanToLin(bmData.can)
                        printf_lin_msg(linMessage, nmessages, n, bmData.timestamp, False)
                        n += 1
                    End If
                Loop While status = BM_StatusTypeDef.BM_ERROR_OK

                If status <> BM_StatusTypeDef.BM_ERROR_QRCVEMPTY AndAlso status <> BM_StatusTypeDef.BM_ERROR_OK Then
                    napifailures += 1
                    Console.WriteLine($"\nFailed to receive message, error={CInt(status)}.")
                End If
            End While

            Dim t2 As DateTime = DateTime.Now
            Dim td As UInteger = CUInt((t2 - t1).TotalSeconds)
            Dim fps As Single = CSng(n) / CSng(td)

            Console.WriteLine("
================ RX STAT ================
Received messages: {0}
Lost messages: {1}
Bad messages: {2}
API Failures: {3}
Total time: {4} seconds
Frame rate: {5:F3} fps
=========================================
", n, nlostmsgs, nbadmsgs, napifailures, td, fps)

            Return CInt(n)
        End Function

        Public Function lin_transmit(ByVal channel As IntPtr, ByVal notification As IntPtr, ByVal nroundmessages As UInteger, ByVal nrounds As UInteger, ByVal cycle As UInteger, ByVal mode As UInteger, ByVal type As UInteger) As Integer
            Dim n As UInteger = 0
            Dim nbadmsgs As UInteger = 0
            Dim nmessages As UInteger = nroundmessages * nrounds
            Dim notifications() As IntPtr = {notification}
            Dim ctrl As BM_LinMessageCtrlTypeDef = New BM_LinMessageCtrlTypeDef()
            ctrl.DLC = CByte(Constants.TEST_MSG_LENGTH)
            ctrl.TRANSMIT = If(mode <> CUInt(LIN_MSG_TYPE.LIN_MASTER_READ), True, False)
            ctrl.ENHANCED_CHECKSUM = (Constants.TEST_CHECKSUM_TYPE = 1)
            Dim msg As BM_LinMessageTypeDef = New BM_LinMessageTypeDef(ctrl, Constants.TEST_MSG_TX_ID)
            Dim status As BM_StatusTypeDef = BM_StatusTypeDef.BM_ERROR_OK
            Dim bmData As BM_DataTypeDef = New BM_DataTypeDef()
            Dim pid As UInteger = 0

            Console.WriteLine($"Transmitting test message as {msgtypes(mode)} sendtype as {sendtypes(type)}...")

            Dim t1 As DateTime = DateTime.Now

            If type = CUInt(LIN_SEND_TYPE.LIN_BM_TXTASK) Then
                Dim txtask As BM_TxTaskTypeDef = New BM_TxTaskTypeDef() With {
                    .pattern = New Byte(31) {},
                    .e2e_reserved = New Byte(14) {},
                    .payload = New Byte(63) {},
                    .type = CByte(BM_TxTaskTypeTypeDef.BM_TXTASK_FIXED),
                    .cycle = If(mode = CInt(LIN_MSG_TYPE.LIN_SLAVE_WRITE), (0), (cycle)),
                    .nrounds = (nmessages),
                    .nmessages = (1)
                }
                txtask.id.id = Constants.TEST_MSG_TX_ID
                If Constants.TEST_CHECKSUM_TYPE = 1 AndAlso mode <> CInt(LIN_MSG_TYPE.LIN_MASTER_READ) Then
                    pid = CalculatePID(CByte(txtask.id.id))
                    txtask.flags = CByte(BM_LinMessageFlagsTypeDef.BM_LIN_MESSAGE_FLAGS_ENHANCED_CHECKSUM)
                End If

                If mode = CUInt(LIN_MSG_TYPE.LIN_MASTER_WRITE) OrElse mode = CUInt(LIN_MSG_TYPE.LIN_SLAVE_WRITE) Then
                    txtask.flags = 1
                    txtask.length = CByte(Constants.TEST_MSG_LENGTH)
                    For i As Integer = 0 To txtask.length - 1
                        txtask.payload(i) = CByte(i)
                    Next
                    Dim checksum As UInteger = CalculateChecksum(txtask.payload, 8, CByte(pid))
                    txtask.id.lin.CHECKSUM = CByte(checksum)
                End If
                Dim txtaskPtr As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(BM_TxTaskTypeDef)))
                Try
                    Marshal.StructureToPtr(txtask, txtaskPtr, True)
                    status = BMAPI.BM_SetTxTasks(channel, txtaskPtr, 1)
                Finally
                    Marshal.FreeHGlobal(txtaskPtr)
                End Try
                While n < nmessages
                        Dim incomingChannel As Integer = BMAPI.BM_WaitForNotifications(notifications, 1, Constants.TEST_MSG_TX_TIMEOUT)
                        If incomingChannel < 0 Then Continue While

                        While BMAPI.BM_Read(channel, bmData) = BM_StatusTypeDef.BM_ERROR_OK
                            Dim linMsg As BM_LinMessageTypeDef = ConvertCanToLin(bmData.can)
                            printf_lin_msg(linMsg, nmessages, n, bmData.timestamp, True)
                            n += 1
                        End While
                End While
            ElseIf mode <> CUInt(LIN_MSG_TYPE.LIN_SLAVE_WRITE) AndAlso type = CUInt(LIN_SEND_TYPE.LIN_BM_WRITE) Then
                For n = 0 To nmessages - 1
                    Dim timestamp As UInteger = 0

                    If Constants.TEST_CHECKSUM_TYPE = 1 Then
                        pid = CalculatePID(CByte(msg.id))
                    End If
                    msg.ctrl.DLC = 8
                    For i As Integer = 0 To msg.ctrl.DLC - 1
                        msg.payload(i) = CByte(n And &HFF)
                    Next
                    msg.ctrl.CHECKSUM = ((CalculateChecksum(msg.payload, msg.ctrl.DLC, CByte(pid)) And &HFF))

                    If BMAPI.BM_WriteLinMessage(channel, msg, 0, Constants.TEST_MSG_TX_TIMEOUT, timestamp) = BM_StatusTypeDef.BM_ERROR_OK Then
                        printf_lin_msg(msg, nmessages, n, timestamp, True)
                    End If
                Next
            End If

            Dim t2 As DateTime = DateTime.Now
            Dim td As UInteger = CUInt((t2 - t1).TotalSeconds)
            Dim fps As Single = CSng(n) / CSng(td)

            Console.WriteLine("
================ TX STAT ================
Transmitted messages: {0}
Bad messages: {1}
Total time: {2} seconds
Frame rate: {3:F3} fps
=========================================
", n, nbadmsgs, td, fps)

            Return CInt(n)
        End Function

        ' 主函数
        Public Sub Main(ByVal args() As String)
            Dim channel As IntPtr = IntPtr.Zero
            Dim notification As IntPtr = IntPtr.Zero
            Dim channelinfos(31) As BM_ChannelInfoTypeDef
            Dim nchannels As Integer = channelinfos.Length
            Dim channelid As Integer = If(args.Length > 0, Integer.Parse(args(0)), 0)
            Dim mode As String = If(args.Length > 1, args(1), "master-write-txtask")
            Dim exitcode As Integer = 0
            ' default LIN baudrate is 19200bps, feel free to change.
            Dim bitrate As BM_BitrateTypeDef = New BM_BitrateTypeDef() With {.nbitrate = 19200}
            Dim status As BM_StatusTypeDef = BM_StatusTypeDef.BM_ERROR_OK

            Console.WriteLine(
                vbCrLf &
                "========== BMAPI TEST PROGRAM ==========" & vbCrLf &
                "========= BUSMUST TECH Co.,Ltd. ========" & vbCrLf
            )

            ' Step1: Initialize BMAPI library before any other operation
            status = BMAPI.BM_Init()
            If status <> BM_StatusTypeDef.BM_ERROR_OK Then
                exitcode = 1
                GoTo ExitSub
            End If

            ' Step2: Enumerate connected device channels
            Dim ninfos As Integer = 32
            Dim infomem As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(GetType(BM_ChannelInfoTypeDef)) * ninfos)
            status = BMAPI.BM_EnumerateByCap(infomem, nchannels, CUShort(BM_CapabilityTypeDef.BM_LIN_CAP))
            If status <> BM_StatusTypeDef.BM_ERROR_OK Then
                exitcode = 2
                GoTo ExitSub
            End If
            For i As Integer = 0 To nchannels - 1
                Dim ptr As IntPtr = New IntPtr(infomem.ToInt64() + i * Marshal.SizeOf(GetType(BM_ChannelInfoTypeDef)))
                channelinfos(i) = CType(Marshal.PtrToStructure(ptr, GetType(BM_ChannelInfoTypeDef)), BM_ChannelInfoTypeDef)
            Next
            Marshal.FreeHGlobal(infomem)
            For i As Integer = 0 To nchannels - 1
                Console.WriteLine($"[{i}] {channelinfos(i).GetName()}")
            Next

            If channelid < nchannels Then
                ' Step3: Open the selected channel and configure baudrate
                Dim modeId As UInteger = If(mode.Contains("master"), CUInt(BM_LinModeTypeDef.BM_LIN_MASTER_MODE), CUInt(BM_LinModeTypeDef.BM_LIN_SLAVE_MODE))
                Console.WriteLine($"Opening channel {channelinfos(channelid).GetName()} in {If(modeId = CUInt(BM_LinModeTypeDef.BM_LIN_MASTER_MODE), "master", "slave")} mode ...")

                status = BMAPI.BM_OpenEx(
                    channel,
                    channelinfos(channelid),
                    CType(modeId, BM_CanModeTypeDef),
                    BM_TerminalResistorTypeDef.BM_TRESISTOR_PULLUP_1K,
                    bitrate,
                    IntPtr.Zero,
                    0
                )
                If status <> BM_StatusTypeDef.BM_ERROR_OK Then
                    exitcode = 3
                    GoTo ExitSub
                End If

                BMAPI.BM_SetLinVoltage(channel, BM_LinVoltageTypeDef.BM_LIN_VOLTAGE_12V_OUT)
                Console.WriteLine("Getting channel notification handle ..." & vbCrLf)
                status = BMAPI.BM_GetNotification(channel, notification)
                If status <> BM_StatusTypeDef.BM_ERROR_OK Then
                    exitcode = 4
                    GoTo ExitSub
                End If
                BMAPI.BM_ClearBuffer(channel)
                Task.Delay(100).Wait()

                ' Step4: TX (according to command line arguments)
                Dim nmessages As UInteger = If(args.Length > 2, CUInt(UInt32.Parse(args(2))), Constants.TEST_MSG_COUNT)
                Dim nrounds As UInteger = If(args.Length > 3, CUInt(UInt32.Parse(args(3))), Constants.TEST_MSG_ROUND)
                Dim ncyclems As UInteger = If(args.Length > 4, CUInt(UInt32.Parse(args(4))), Constants.TEST_MSG_CYCLE)

                Dim sendtype As UInteger = If(mode.Contains("-txtask"), CUInt(LIN_SEND_TYPE.LIN_BM_TXTASK), CUInt(LIN_SEND_TYPE.LIN_BM_WRITE))
                Dim linmode As UInteger = CUInt(LIN_MSG_TYPE.LIN_RECEIVE)

                If modeId = CUInt(BM_LinModeTypeDef.BM_LIN_MASTER_MODE) Then
                    If mode.Contains("write") Then
                        linmode = CUInt(LIN_MSG_TYPE.LIN_MASTER_WRITE)
                    ElseIf mode.Contains("read") Then
                        linmode = CUInt(LIN_MSG_TYPE.LIN_MASTER_READ)
                    End If
                Else
                    If mode.Contains("write") Then
                        linmode = CUInt(LIN_MSG_TYPE.LIN_SLAVE_WRITE)
                    End If
                End If

                If linmode = CUInt(LIN_MSG_TYPE.LIN_RECEIVE) Then
                    lin_receive(channel, notification, nmessages * nrounds)
                Else
                    lin_transmit(channel, notification, nmessages, nrounds, ncyclems, linmode, sendtype)
                End If

                ' Step5: Close the opened channel and release connected hardware
                Console.WriteLine("Closing channel ..." & vbCrLf)
                status = BMAPI.BM_Close(channel)
                If status <> BM_StatusTypeDef.BM_ERROR_OK Then
                    exitcode = 5
                    GoTo ExitSub
                End If
            Else
                Console.WriteLine($"Input Error: {nchannels} available devices, port {channelid} is invalid.")
                exitcode = 6
            End If

ExitSub:
            If status <> BM_StatusTypeDef.BM_ERROR_OK Then
                Dim buffer(255) As Byte
                BMAPI.BM_GetErrorText(status, buffer, 256, 0)
                Dim errorText As String = Encoding.UTF8.GetString(buffer).TrimEnd(vbNullChar)
                Console.WriteLine($"Error 0x{status:X8}: {errorText}")
            End If

            ' Step6: Cleanup library resource
            BMAPI.BM_UnInit()
            Console.WriteLine($"Exit {mode} app.")
        End Sub
    End Module
End Namespace