# 更新日志

## 1.13.7.40 [2026-06-21]

1. 修复背景 CAN/CAN FD 流量下 ISO-TP 超时处理问题，提升诊断通信中软件 ISO-TP 的稳定性。
2. 修复 BMAPI Python 绑定中 TX task 结构体以及 python-can、udsoncan 使用的 ISO-TP 收发路径兼容性问题。

## 1.13.6.40 [2025-05-15]

1. 新增 `BM_RecoverBusOff` 接口，主动恢复 Bus-off 状态，避免周期性轮询对其他通道发送周期的影响；增加 `BM_CAN_DISABLE_AUTO_BUSOFF_RECOVERY` 模式标志，允许用户禁用自动恢复
2. 新增 `BM_HardwareLastMsgStat` 接口，获取硬件最新发送报文状态
3. Python LIN 库去除 python-can 依赖，补充 aenum 模块，新增 `store_task=False` 硬件发送任务 demo

## 1.13.5.39 [2025-01-23]

1. 从本版本开始使用 CICD 全自动构建，将在四位版本号（例如 1.13.5.39）基础上追加第五位版本号代表构建号（Build Number）
2. 修复 CANFD 协议（而不是 CAN2.0）下，`BM_WriteIsotp` 在报文负载长度小于 DLC 可承载长度时，发送的报文不符合 ISOTP 约定的问题
3. 移除对 libusb 动态库的依赖！今后在编译应用程序的时候不再需要 `-lusb` 这个链接选项了
4. 整理 SDK 文件夹内容，规范化命名，优化一些启动脚本等，如有困扰欢迎联系技术支持
5. 修复 Python 环境下使用 32 位时间戳意外翻转的问题，现在时间戳获取完全依赖 BMAPI.DLL

## 1.13.3.38 [2024-12-30]

1. **关键 BUGFIX！** 修复 2025 年 9 月新引入的，由于 BMAPI 基础库中的内存管理错误导致的长时间接收挂机自动停止，需要 Disconnect 再 Connect 才能恢复接收的问题
2. `BM_CanfdMessageTypeDef` 的 `ctrl.tx` 成员新增 `ECHO` 属性，当置位此属性时，即使阻塞式调用 `BM_Write`，也可以额外收到一条 ACK 报文，用于回显

## 1.13.2.37 [2025-11-07]

1. 恢复对 mingw 编译环境的支持
2. 修复 x86 以及 arm 以及 aarch64 平台下，提供的 bmapi*.so 文件缺失 libudev 等动态库符号的运行时错误
3. 修复第二代产品调用 `BM_Close` 会卡死的兼容性问题
4. 优化 python-can 的平台和版本兼容性
5. 优化 python-can 的 busoff recovery 逻辑，提升多线程支持能力
6. 修复 Linux 系统由于变量未初始化导致的在部分环境下 `BM_Close` 崩溃问题
7. 通过调整 USB 发送使能逻辑，优化第三代设备在频繁 busoff 环境下的发送自恢复能力

## 1.13.1.35 [2025-09-09]

1. 添加 `BM_IsotpConfigTypeDef.flowcontrol.hardwareIsotpDisabled` 选项，用于禁用 ISOTP 流控的硬件加速功能，默认不会禁用（如果硬件支持）
2. 修复 `BM_WriteIsotp` 函数在需要多帧流控的情况下，CF SEQ 被意外重置的问题
3. 修复 python-can 在未插入硬件设备时，由于 Virtual Channel 不支持 PTP 而导致抛出异常中止运行的问题
4. `BM_GetDataPtpTimestamp` 函数返回的时间戳从微秒改为纳秒，从而与 `BM_GetPtpTime` 以及 `BM_GetHostPtpTime` 保持一致，各个例程也做了同样的修改
5. 提升第三代设备的报文发送效率和收发稳定性

## 1.13.0.35 [2025-08-23]

1. 全面添加 PTP 相关 API，并在 can_analyzer_qt 等多个例程中演示 PTP 功能（第二代和第三代产品均可调用相关 API）
2. 优化 `BM_WriteIsotp` 的硬件 ISOTP 发送效率
3. `BM_WriteMultiple` 和 `BM_WriteMultipleCanMessage` 在调用失败时，可以通过函数参数获取到实际成功发送了多少条
4. 优化对第三代设备的底层收发参数，提高收发效率

## 1.12.3.34 [2025-07-16]

1. python-can 增加 `sync_timestamp` demo，并修复时间戳偏移 bug
2. 优化 `BM_Enumerate` 在枚举设备时的命名，例如将 PCIE 板卡的多个通道统一命名
3. 更新 `BM_TxTaskTypeDef` 的定义并保持与历史版本兼容，添加了发送周期内的 frame delay，用于 LIN 调度表等定时设计

## 1.12.2.33 [2025-06-24]

1. 优化各个例程中新增的 64 位 UTC 时间戳功能与第三代以前的旧设备的兼容性
2. 修复 Virtual Channel 两个通道之间无法对发等问题
3. 优化 python-can 的时间戳同步精度

## 1.12.1.33 [2025-06-10]

1. 修复 LIN 相关 API 的若干早期问题，现在 LIN API 已经可以用于量产环境
2. **新特性！** 支持第三代硬件设备的 64 位 UTC 时间戳（位于 `BM_DataTailTypeDef`），并在多个例程中演示如何获取 UTC 时间戳，从此以后再也不用担心时间戳翻转问题
3. python-can 增加 `recoverFromError` demo
4. 独家推出 python-lin 软件包，可用 Python 语言来进行 LIN 总线的控制和报文收发，编程接口类似 python-can
5. 在 Linux 上添加 thread priority tuning API 供有需要的客户使用，常规应用下建议保持默认值

## 1.12.0.32 [2025-04-07]

1. 此版本开始正式支持 LIN 功能，与 LIN 相关的 API 和例程初步固化
2. 对通道数超出 16 的 GWR 设备的支持：`BM_DataTypeDef` 中，在原来的 `schn` 和 `dchn` 的基础上，引入 group 的概念，每个设备最多支持 8 个 group，每个 group 固定为 16 个 channel，进而每个设备最多支持 128 个通道
3. 添加 `BM_MapTimestamp` 新 API，用于将 `BM_DataTypeDef` 中的 32 位硬件本地时间戳映射为 64 位 UTC 时间戳，方便长时间采集记录

## 1.11.0.31 [2025-03-03]

1. 新增系列 API，实现对新款 LIN 设备的支持
2. 新增系列 API，实现对 GWR 等支持以太网的新款设备的支持，可以通过网络进行设备远程控制和报文收发等，可以将配置信息离线保存到 TF 卡中
3. `BM_WriteIsotp` 支持超高效率（接近理论极限）的 ISOTP 发送操作，需要 X2 或者 X4 设备，且需要 2.4.12.x 或者更高版本的固件，否则无法生效（但仍可以使用默认软件实现正常发送，不存在兼容性问题）
4. 新增 `BM_CancelWrite` API，从而修复 python-can 环境下使用 `send_periodic` 函数时，资源泄露的问题
5. 新增 `bmapi_lin_txrx` 例程，演示如何进行 LIN 报文收发，支持主机和从机
6. 新增 `lin_analyzer_qt` 例程，演示如何通过 Qt 图形界面进行 LIN 报文收发，支持主机和从机
7. C++ 和 python-can 新增 `bmapi_remote_connection` 例程，演示如何通过网络远程控制 GWR 等支持以太网的新款设备
8. python-can 新增 `loopback_test` 例程，演示如何使用硬件进行自发自收
9. 修复 `BM_WriteIsotp` 与 ISO15765 在某些 corner case 下的 CANFD 短包分包方式不一致的问题
10. 修复 `BM_WriteIsotp` 在收到 FC 帧之后帧序号被错误重置的问题
11. 新增系列 API，支持新款 X2R/X4R/GWR 设备的授时功能
12. 优化 Windows 操作系统下的 BMAPI 极限接收帧率
13. 在 python-can 中使用 `BM_SetTxTask` 硬件发送任务来实现 `send_periodic` 这个 Python API，保证报文的周期精度在 1ms 以内
14. 优化 `BM_OpenEx` 的错误码，当 channelinfo 为空或者全零时，返回 `BM_ERROR_ILLPARAMVAL`
15. 新增两个虚拟通道，可以实现虚拟通道之间的互相收发，其他功能完善中
16. 修复 `BM_WriteIsotp` 函数在编码 CF 请求 id 时，在多个 FC 的场景下，id 被错误重置的问题
17. 修复 `BM_ReadIsotp` 函数在一个会话中收到另外一个 SF 请求（例如 3E 80）导致当前会话被重置的问题
18. `can_analyzer_qt` 例程添加自动错误恢复机制，支持 USB 设备拔出检测以及 busoff 检测
19. BMAPI 内部新增对 `BM_NO_ACK` 模式的实现，从而支持在异步模式下快速发送报文而无需处理发送完成事件，请注意在这种模式下无法得知报文是否发送成功

## 1.10.2.30 [2024-03-04]

1. 新增 `multichannel_logger` 例程，额外演示了如何同时将多个通道捕获的报文录制到一个 ASC 文件中
2. 修复 `BM_ReadMultiple` 和 `BM_ReadMultipleCanMessage` 在某些场景下无法通过 nmessages 参数获取已经接收到的报文数量的 bug
3. 优化 `BM_ReadMultiple` 和 `BM_ReadMultipleCanMessage` 的返回值定义
4. 消除 `BM_OpenEx` 在 Linux 的额外调试打印信息
5. 修复 `BM_Enumerate` 在 Linux 下反复多次枚举时会重复枚举已经打开的设备，导致程序概率性崩溃的问题
6. 修复 `multichannel_bmapi_demo.py` 例程收到报文后抛出 `BM_ERROR_QRCVEMPTY` 异常的问题

## 1.10.1.29 [2023-12-31]

1. 新增 `bmapi_cyclic_tx_task` 例程，演示如何使用 BUSMUST 特有的 TXTASK API 来进行周期性报文发送
2. 新增 `multichannel_isotp_txrx_cpp` 例程，演示多通道 ISOTP（ISO15765）通信
3. 新增对 python-can-4.0.0 的支持（理论上支持更高的 4.x.x 版本）
4. 新增 python-can ECU UDS 模拟器例程
5. 新增若干 API，用于支持离线配置/记录/回放相关功能

## 1.9.0.23 [2022-07-22]

1. 新增对 VB.Net 语言框架的支持
2. 恢复对 Mingw 编译器的支持
3. 优化 C#.Net 以及 VB.Net 的若干库导入声明
4. 修复 C#.Net 的 example 点击 Close 无法真正关闭对应的硬件设备的问题

## 1.8.1.22 [2022-07-08]

1. 新增 `BM_ResetDevice` API，用于重启整个 USB 设备
2. 修复 `can_analyzer_csharp` 例程在未选中任何通道时点击 Open 按钮时报错的问题
3. 修复 C# 程序通过 `BM_Enumerate` 获取到的通道名称尾部有乱码的问题

## 1.8.0.21 [2022-05-30]

1. 支持最新的八通道硬件设备 X8PI
2. 增加若干新的 API，支持最新的支持离线报文录制播放的设备 X4R、X2R
3. 新增 C++ 多通道并行接收例程 `multichannel_rx_cpp`
4. BMAPI 在 Linux 上部分重构，CPU 占用率大幅降低

## 1.7.1.20 [2022-02-16]

1. 支持 Python 2.7.18
2. Python-can 新增 `send_multiple` API 以及对应的 example
3. 更新 raspbian64 平台的各个二进制文件
4. 在 python-can 中自带了 aenum package
5. 更新 udsoncan 的 `download_hex_file` 例程，现已支持加载 CANoe 格式的 security DLL

## 1.7.0.19

1. Qt/C#/pyqt 示例程序从此版本起支持多通道操作
2. 新增 `bmapi_receive_only` 和 `bmapi_transmit_only` 例程
3. 修复 Unix 系统下同时打开多通道时的卡死问题
4. 优化 python-can 对多平台兼容性的自适应能力

## 1.6.0.18

1. 修复远程帧无法实时发送的问题
2. Python-can 创建 bus 对象时允许指定采样点位置
3. `BM_ReadIsotp` 支持同时侦听指定的 UDS Request ID 和功能寻址地址
4. 修复 `BM_Reset` 在多线程环境的句柄资源释放问题

## 1.6.0.17 [Alpha]

1. 新增 `BM_GetVersion` 接口
2. 新增 `BM_GetLogLevel` 和 `BM_SetLogLevel` 接口

## 1.5.4.16 [2021-08-13]

1. 针对 X1PRO 和 X1 设备修正了 `BM_OpenEx` 时正在接收报文导致设备无法正确配置的 bug
2. 修正 Debian 下 `BM_ClearBuffer` 递归锁导致程序阻塞的问题
3. udsoncan 增加 `example.run.sh` 启动器
4. 修正 `BM_WriteIsotp` 和 `BM_ReadIsotp` 的首个 CF 帧的序列号不对的问题

---

1.5.4.16 以前的历史版本，变更记录位于 BUSMASTER 上位机安装包的 changelist 中。
