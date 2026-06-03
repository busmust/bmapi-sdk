# BMAPI SDK

BMAPI（Busmust Device Communication Application Programming Interface）是霸码科技（Busmust Co., Ltd.）提供的设备通信应用编程接口。

BMAPI SDK 包含一系列运行时库和例程，支持应用程序通过 Busmust USB 设备实时连接 CAN/LIN 总线。

## 支持平台

- Windows XP/7/8/10/11 x86（32位/64位）
- Linux x86（32位/64位）
- ARM32（如 Raspbian 32位）
- ARM64（如 Raspbian 64位）
- 龙芯 LoongArch64

## 支持设备

- BM-USB-CANFD-X1PRO
- BM-USB-CANFD-X1
- BM-USB-CAN-C1PRO
- BM-USB-CAN-C1
- BM-USB-CAN-X2
- BM-USB-CAN-X4
- BM-USB-CAN-X2R
- BM-USB-CAN-X4R
- BM-USB-CAN-L1
- BM-USB-CAN-GWR

## 快速开始

**开发前请务必先阅读文档：**

1. `doc/BMAPI软件开发指南.pdf` — 完整的 API 使用指南（**入门必读**）
2. `doc/BMAPI.chm` — CHM 格式 API 参考手册，函数调用时快速查阅
3. `doc/Application Programming Sequence.png` — 标准应用编程序列流程图

### 标准编程序列

每个应用程序都遵循以下基本流程：

```
1. BM_Init()                    — 初始化 BMAPI 库
2. BM_Enumerate()               — 枚举已连接设备通道
3. BM_OpenEx(&channel, ...)     — 打开通道，配置模式、终端电阻、波特率、过滤器
4. BM_GetNotification()         — 获取通道的通知句柄
5. 循环:
   BM_WaitForNotifications()    — 等待新数据/事件通知
   BM_Read()                    — 读空队列中所有报文（直到返回 BM_ERROR_QRCVEMPTY）
   BM_WriteCanMessage()         — 发送报文
6. BM_Close(channel)            — 关闭通道
7. BM_UnInit()                  — 清理库资源
```

## SDK 结构

| 目录 | 说明 |
|------|------|
| `doc/` | 文档和参考资料 |
| `include/` | 头文件：`bmapi.h`、`bm_usb_def.h`、`osal.h` |
| `example/` | 各语言例程 |

## 例程清单

### 基础 C/C++ 例程（入门首选）

| 例程 | 说明 |
|------|------|
| `bmapi_transmit_only/` | 最基础的 CAN FD **发送**例程 |
| `bmapi_receive_only/` | 最基础的 CAN FD **接收**例程 |
| `bmapi_lin_txrx/` | LIN 收发例程（支持主机/从机） |

### 进阶 C/C++ 例程

| 例程 | 说明 |
|------|------|
| `bmapi_test/` | API 基础用法与对发测试 |
| `bmapi_dualthread_txrx/` | 双线程高效异步收发 |
| `multichannel_rx_cpp/` | 高效率多通道并行接收 |
| `multichannel_isotp_txrx_cpp/` | 多通道 ISOTP（ISO 15765）+ UDS |
| `bmapi_cyclic_tx_task/` | 周期性报文发送（TXTASK API） |
| `hw_replay_demo/` | 硬件序列播放（仅第三代硬件支持） |
| `nobuffer_txrx/` | 无缓冲模式极低延迟交互（仅第三代硬件支持） |
| `bmapi_remote_connection/` | 远程连接 GWR 网关设备 |

### 完整上位机

| 例程 | 语言 | 说明 |
|------|------|------|
| `can_analyzer_qt/` | C++/Qt | 完整 CAN 分析仪上位机 |
| `can_analyzer_csharp/` | C# | 完整 CAN 分析仪上位机（WinForms） |
| `can_analyzer_pyqt/` | Python/Qt | 完整 CAN 分析仪上位机 |
| `can_analyzer_vb.net/` | VB.NET | 完整 CAN 分析仪上位机 |
| `lin_analyzer_qt/` | C++/Qt | 完整 LIN 分析仪上位机 |

## 关键 API 注意事项

- **`BM_Read` 是轮询式**：每次 `BM_WaitForNotification` 通知后，必须通过 `BM_Read` 循环将队列**读空**（直到返回 `BM_ERROR_QRCVEMPTY`），因为一个通知可能对应多条报文
- **`BM_Write` 超时机制**：超时=0 为异步模式（不消耗 TEF），超时≠0 为阻塞模式（消耗 TEF 等待 ACK）
- **设备独占**：一个物理设备只能被一个进程打开
- **多线程**：`WriteXXX`/`ReadXXX` 支持多线程，但 `OpenEx` 和 `SetXXX` **不支持**
- **BUSOFF 恢复**：参考 `bmapi_transmit_only` 例程中的 `recoverFromError()` 函数

## Python 开发者

配套 Python 开发包：
- **[python-can](https://github.com/busmust/python-can)** — BUSMUST 的 python-can 接口
- **[python-udsoncan](https://github.com/busmust/python-udsoncan)** — BUSMUST 的 udsoncan 接口
- **[python-lin](https://github.com/busmust/python-lin)** — BUSMUST 的 Python LIN 总线接口

### Python 开发注意事项

1. 请先插入 BUSMUST 设备再运行 Python 脚本，否则会抛出加载 BMAPI 失败的异常
2. 一个物理设备只能被一个进程打开，如果打开通道失败，请检查是否有其他进程占用
3. 在 Ubuntu 下请使用 `sudo` 权限执行脚本或启动器，否则驱动会报错提示无权限访问设备
4. 确保对应的 python-can 文件夹已正确添加至 `PYTHONPATH`，否则 `bmcan` 会被识别为无效的 bus 类型

## 下载

各平台预编译二进制文件（DLL/SO/LIB）请前往 [Releases](https://github.com/busmust/bmapi-sdk/releases) 页面下载。

## 技术支持

微信搜索"霸码科技"，关注公众号后点击"技术支持"即可与技术人员一对一聊天获得深度技术支持。

## 许可证

详见 [LICENSE](LICENSE)。本 SDK 仅限与 BUSMUST 硬件设备配合使用。
