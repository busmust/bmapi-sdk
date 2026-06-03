using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
namespace BMAPILinBus
{
    public class Program
    {
        static readonly string[] msgtypes =
        {
        "lin_receive",
        "lin_master_write",
        "lin_master_read",
        "lin_slave_write"
        };


        static readonly string[] sendtypes =
        {
        "lin_bm_write",
        "lin_bm_txtask"
    };
        public enum LIN_MSG_TYPE
        {
            LIN_RECEIVE = 0,
            LIN_MASTER_WRITE,
            LIN_MASTER_READ,
            LIN_SLAVE_WRITE
        }

        public enum LIN_SEND_TYPE
        {
            LIN_BM_WRITE = 0,
            LIN_BM_TXTASK
        }
        public static class Constants
        {
            public const int TEST_MSG_RX_TIMEOUT = 1000;
            public const int TEST_MSG_TX_TIMEOUT = 500;
            public const int TEST_CHECKSUM_TYPE = 1;// classic = 0 enhance = 1
            public const uint TEST_MSG_COUNT = 1000;
            public const uint TEST_MSG_ROUND = 50;
            public const uint TEST_MSG_CYCLE = 10;
            public const uint TEST_MSG_TX_ID = 0x3;
            public const uint TEST_MSG_LENGTH = 0x8;
        }
        // 计算 PID
        public static uint CalculatePID(byte ucId)
        {
            byte[] byteArray = new byte[8];
            for (int nIndex = 0; nIndex < 8; nIndex++)
            {
                byteArray[nIndex] = (byte)((ucId >> nIndex) & 1);
            }

            byte chP0 = (byte)(byteArray[0] ^ byteArray[1] ^ byteArray[2] ^ byteArray[4]);
            byte chP1 = (byte)(~(byteArray[1] + byteArray[3] + byteArray[4] + byteArray[5]));

            chP0 = (byte)(chP0 << 6);
            chP1 = (byte)(chP1 << 7);
            return (byte)(ucId + chP0 + chP1);
        }

        // 计算校验和
        public static byte CalculateChecksum(byte[] xData, byte xDataLen, byte pId)
        {
            ushort sum = pId;
            for (byte i = 0; i < xDataLen; i++)
            {
                sum += xData[i];
                while (sum > 0xFF) sum -= 0xFF;
            }
            return (byte)((~sum) & 0xFF);
        }
        public static void printf_lin_msg(BM_LinMessageTypeDef msg, uint nmessages, uint n, uint timestamp, bool istx)
        {
            string direction = istx ? "Tx" : "Rx";
            string checksumType = msg.ctrl.ENHANCED_CHECKSUM ? "Enhanced" : "Classic";
            Console.WriteLine(
                $"LIN {direction} [{n + 1}/{nmessages}] @{timestamp}: id={msg.id:X2}, payload=[{msg.payload[0]:X2}, {msg.payload[1]:X2}, {msg.payload[2]:X2}, {msg.payload[3]:X2}, {msg.payload[4]:X2}, {msg.payload[5]:X2}, {msg.payload[6]:X2}, {msg.payload[7]:X2}] checksum = {msg.GetChecksum():x} checksumType = {checksumType}"
            );
        }
        public static int lin_receive(IntPtr channel, IntPtr notification, uint nmessages)
        {
            BM_StatusTypeDef error = BM_StatusTypeDef.BM_ERROR_OK;
            uint n = 0;
            uint napifailures = 0;
            uint nlostmsgs = 0;
            uint nbadmsgs = 0;
            IntPtr[] notifications = new IntPtr[] { notification };
            Console.WriteLine("Waiting for RX notification ...");

            DateTime t1 = DateTime.Now;
            BM_DataTypeDef bmData = new BM_DataTypeDef();
            while (n < nmessages)
            {
                if (BMAPI.BM_WaitForNotifications(notifications, 1, Constants.TEST_MSG_RX_TIMEOUT) < 0)
                {
                    Console.WriteLine($"Receive timeout, offset={n}.");
                }
                IntPtr lin = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BM_CanMessageTypeDef)));
                try
                {

                    while ((error = BMAPI.BM_Read(channel, ref bmData)) == BM_StatusTypeDef.BM_ERROR_OK)
                    {
                        Marshal.StructureToPtr(bmData.can, lin, true);
                        BM_LinMessageTypeDef linMessage = (BM_LinMessageTypeDef)Marshal.PtrToStructure(lin, typeof(BM_LinMessageTypeDef));
                        printf_lin_msg(linMessage, nmessages, n, bmData.timestamp, false);
                        n++;
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(lin);
                }

                if (error != BM_StatusTypeDef.BM_ERROR_QRCVEMPTY && error != BM_StatusTypeDef.BM_ERROR_OK)
                {
                    napifailures++;
                    Console.WriteLine($"\nFailed to receive message, error={(int)error}.");
                }
            }

            DateTime t2 = DateTime.Now;
            uint td = (uint)(t2 - t1).TotalSeconds;
            float fps = ((float)n) / ((float)td);

            Console.WriteLine(@"
================ RX STAT ================
Received messages: {0}
Lost messages: {1}
Bad messages: {2}
API Failures: {3}
Total time: {4} seconds
Frame rate: {5:F3} fps
=========================================
", n, nlostmsgs, nbadmsgs, napifailures, td, fps);

            return (int)n;
        }
        public static int lin_transmit(IntPtr channel, IntPtr notification, uint nroundmessages, uint nrounds, uint cycle, uint mode, uint type)
        {
            uint n = 0;
            uint nbadmsgs = 0;
            uint nmessages = nroundmessages * nrounds;
            IntPtr[] notifications = new IntPtr[] { notification };
            BM_LinMessageCtrlTypeDef ctrl = new BM_LinMessageCtrlTypeDef();
            ctrl.DLC = (byte)Constants.TEST_MSG_LENGTH;
            ctrl.TRANSMIT = (mode != (uint)LIN_MSG_TYPE.LIN_MASTER_READ ? true : false);
            ctrl.ENHANCED_CHECKSUM = Constants.TEST_CHECKSUM_TYPE == 1;
            BM_LinMessageTypeDef msg = new BM_LinMessageTypeDef(ctrl, Constants.TEST_MSG_TX_ID);
            BM_StatusTypeDef error = BM_StatusTypeDef.BM_ERROR_OK;
            BM_DataTypeDef bmData = new BM_DataTypeDef();
            uint pid = 0;

            Console.WriteLine($"Transmitting test message as {msgtypes[mode]} sendtype as {sendtypes[type]}...");

            DateTime t1 = DateTime.Now;

            if (type == (uint)LIN_SEND_TYPE.LIN_BM_TXTASK)
            {
                BM_TxTaskTypeDef txtask = new BM_TxTaskTypeDef
                {
                    pattern = new byte[32],
                    e2e_reserved = new byte[15],
                    payload = new byte[64],
                    type = (byte)BM_TxTaskTypeTypeDef.BM_TXTASK_FIXED,
                    cycle = (mode == (int)LIN_MSG_TYPE.LIN_SLAVE_WRITE) ? (ushort)0 : (ushort)cycle,
                    nrounds = (ushort)nmessages,
                    nmessages = (ushort)1,
                };
                txtask.id.id = Constants.TEST_MSG_TX_ID;
                if (Constants.TEST_CHECKSUM_TYPE == 1 && mode != (int)LIN_MSG_TYPE.LIN_MASTER_READ)
                {
                    pid = CalculatePID((byte)txtask.id.id);
                    txtask.flags |= (byte)(BM_LinMessageFlagsTypeDef.BM_LIN_MESSAGE_FLAGS_ENHANCED_CHECKSUM); // 假设 BM_LIN_MESSAGE_FLAGS_ENHANCED_CHECKSUM 为 1
                }

                if (mode == (uint)LIN_MSG_TYPE.LIN_MASTER_WRITE || mode == (uint)LIN_MSG_TYPE.LIN_SLAVE_WRITE)
                {
                    txtask.flags = 1;
                    txtask.length = (byte)Constants.TEST_MSG_LENGTH;
                    for (int i = 0; i < txtask.length; i++)
                    {
                        txtask.payload[i] = (byte)i;
                    }
                    uint checksum = CalculateChecksum(txtask.payload, 8, (byte)pid);
                    txtask.id.lin.CHECKSUM = (byte)checksum;
                }
                IntPtr txtaskPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BM_TxTaskTypeDef)));
                try
                {
                    Marshal.StructureToPtr(txtask, txtaskPtr, true);
                    error = BMAPI.BM_SetTxTasks(channel, txtaskPtr, 1);
                }
                finally
                {
                    Marshal.FreeHGlobal(txtaskPtr);
                }
                IntPtr lin = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BM_LinMessageTypeDef)));
                try
                {
                    while (n < nmessages)
                    {
                        int incomingChannel = BMAPI.BM_WaitForNotifications(notifications, 1, Constants.TEST_MSG_TX_TIMEOUT);
                        if (incomingChannel < 0) continue;

                        while (BMAPI.BM_Read(channel, ref bmData) == BM_StatusTypeDef.BM_ERROR_OK)
                        {
                            Marshal.StructureToPtr(bmData.can, lin, true);
                            BM_LinMessageTypeDef linMessage = (BM_LinMessageTypeDef)Marshal.PtrToStructure(lin, typeof(BM_LinMessageTypeDef));
                            printf_lin_msg(linMessage, nmessages, n, bmData.timestamp, true);
                            n++;
                        }
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(lin);
                }
            }
            else if (mode != (uint)LIN_MSG_TYPE.LIN_SLAVE_WRITE && type == (uint)LIN_SEND_TYPE.LIN_BM_WRITE)
            {
                for (n = 0; n < nmessages; n++)
                {
                    uint timestamp = 0;

                    if (Constants.TEST_CHECKSUM_TYPE == 1)
                    {
                        pid = CalculatePID((byte)msg.id);
                    }
                    for (int i = 0; i < msg.ctrl.DLC; i++)
                    {
                        msg.payload[i] = (byte)(n & 0xFF);
                    }

                    msg.ctrl.CHECKSUM = (byte)((CalculateChecksum(msg.payload, msg.ctrl.DLC, (byte)pid) & 0xFF));

                    if (BMAPI.BM_WriteLinMessage(channel, ref msg, 0, Constants.TEST_MSG_TX_TIMEOUT, ref timestamp) == BM_StatusTypeDef.BM_ERROR_OK)
                    {
                        printf_lin_msg(msg, nmessages, n, timestamp, true);
                    }
                }
            }

            DateTime t2 = DateTime.Now;
            uint td = (uint)(t2 - t1).TotalSeconds;
            float fps = ((float)n) / ((float)td);

            Console.WriteLine(@"
================ TX STAT ================
Transmitted messages: {0}
Bad messages: {1}
Total time: {2} seconds
Frame rate: {3:F3} fps
=========================================
", n, nbadmsgs, td, fps);

            return (int)n;
        }

        // 主函数
        public static void Main(string[] args)
        {
            IntPtr channel = IntPtr.Zero;
            IntPtr notification = IntPtr.Zero;
            BM_ChannelInfoTypeDef[] channelinfos = new BM_ChannelInfoTypeDef[32];
            int nchannels = channelinfos.Length;
            int channelid = args.Length > 0 ? int.Parse(args[0]) : 0;
            string mode = args.Length > 1 ? args[1] : "master-write-txtask";
            int exitcode = 0;
            /* default LIN baudrate is 19200bps, feel free to change. */
            BM_BitrateTypeDef bitrate = new BM_BitrateTypeDef { nbitrate = 19200 };
            BM_StatusTypeDef error = BM_StatusTypeDef.BM_ERROR_OK;

            Console.WriteLine(
                "\n" +
                "========== BMAPI TEST PROGRAM ==========\n" +
                "========= BUSMUST TECH Co.,Ltd. ========\n"
            );

            /* Step1: Initialize BMAPI library before any other operation */
            error = BMAPI.BM_Init();
            if (error != BM_StatusTypeDef.BM_ERROR_OK)
            {
                exitcode = 1;
                goto Exit;
            }

            /* Step2: Enumerate connected device channels */
            int ninfos = 32;
            IntPtr infomem = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BM_ChannelInfoTypeDef)) * ninfos);
            error = BMAPI.BM_EnumerateByCap(infomem, ref nchannels, (ushort)BM_CapabilityTypeDef.BM_LIN_CAP);
            if (error != BM_StatusTypeDef.BM_ERROR_OK)
            {
                exitcode = 2;
                goto Exit;
            }
            for (int i = 0; i < nchannels; i++)
            {
                IntPtr ptr = (IntPtr)(infomem.ToInt64() + i * Marshal.SizeOf(typeof(BM_ChannelInfoTypeDef)));
                channelinfos[i] = (BM_ChannelInfoTypeDef)Marshal.PtrToStructure(ptr, typeof(BM_ChannelInfoTypeDef));
            }
            Marshal.FreeHGlobal(infomem);
            for (int i = 0; i < nchannels; i++)
            {
                Console.WriteLine($"[{i}] {channelinfos[i].GetName()}");
            }

            if (channelid < nchannels)
            {
                /* Step3: Open the selected channel and configure baudrate */
                uint modeId = mode.Contains("master") ? (uint)BM_LinModeTypeDef.BM_LIN_MASTER_MODE : (uint)BM_LinModeTypeDef.BM_LIN_SLAVE_MODE;
                Console.WriteLine($"Opening channel {channelinfos[channelid].GetName()} in {(modeId == (uint)BM_LinModeTypeDef.BM_LIN_MASTER_MODE ? "master" : "slave")} mode ...");

                error = BMAPI.BM_OpenEx(
                    ref channel,
                    ref channelinfos[channelid],
                    (BM_CanModeTypeDef)modeId,
                    BM_TerminalResistorTypeDef.BM_TRESISTOR_PULLUP_1K,
                    ref bitrate,
                    IntPtr.Zero,
                    0
                    );
                if (error != BM_StatusTypeDef.BM_ERROR_OK)
                {
                    exitcode = 3;
                    goto Exit;
                }
                BMAPI.BM_SetLinVoltage(channel, BM_LinVoltageTypeDef.BM_LIN_VOLTAGE_12V_OUT);
                Console.WriteLine("Getting channel notification handle ...\n");
                error = BMAPI.BM_GetNotification(channel, ref notification);
                if (error != BM_StatusTypeDef.BM_ERROR_OK)
                {
                    exitcode = 4;
                    goto Exit;
                }
                BMAPI.BM_ClearBuffer(channel);
                Task.Delay(100);

                /* Step4: TX (according to command line arguments) */
                uint nmessages = args.Length > 2 ? uint.Parse(args[2]) : Constants.TEST_MSG_COUNT;
                uint nrounds = args.Length > 3 ? uint.Parse(args[3]) : Constants.TEST_MSG_ROUND;
                uint ncyclems = args.Length > 4 ? uint.Parse(args[4]) : Constants.TEST_MSG_CYCLE;

                uint sendtype = mode.Contains("-txtask") ? (uint)LIN_SEND_TYPE.LIN_BM_TXTASK : (uint)LIN_SEND_TYPE.LIN_BM_WRITE;
                uint linmode = (uint)LIN_MSG_TYPE.LIN_RECEIVE;

                if (modeId == (uint)BM_LinModeTypeDef.BM_LIN_MASTER_MODE)
                {
                    if (mode.Contains("write"))
                    {
                        linmode = (uint)LIN_MSG_TYPE.LIN_MASTER_WRITE;
                    }
                    else if (mode.Contains("read"))
                    {
                        linmode = (uint)LIN_MSG_TYPE.LIN_MASTER_READ;
                    }
                }
                else
                {
                    if (mode.Contains("write"))
                    {
                        linmode = (uint)LIN_MSG_TYPE.LIN_SLAVE_WRITE;
                    }
                }

                if (linmode == (uint)LIN_MSG_TYPE.LIN_RECEIVE)
                {
                    lin_receive(channel, notification, nmessages * nrounds);
                }
                else
                {
                    lin_transmit(channel, notification, nmessages, nrounds, ncyclems, linmode, sendtype);
                }

                /* Step5: Close the opened channel and release connected hardware */
                Console.WriteLine("Closing channel ...\n");
                error = BMAPI.BM_Close(channel);
                if (error != BM_StatusTypeDef.BM_ERROR_OK)
                {
                    exitcode = 5;
                    goto Exit;
                }
            }
            else
            {
                Console.WriteLine($"Input Error: {nchannels} available devices, port {channelid} is invalid.");
                exitcode = 6;
            }

        Exit:
            if (error != BM_StatusTypeDef.BM_ERROR_OK)
            {
                char[] buffer = new char[256];
                BMAPI.BM_GetErrorText(error, buffer, 256, 0);
                Console.WriteLine($"Error 0x{error:X8}: {Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(buffer)).TrimEnd('\0')}");
            }

            /* Step6: Cleanup library resource */
            BMAPI.BM_UnInit();
            Console.WriteLine($"Exit {mode} app.");
        }
    }
}