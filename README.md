# BMAPI SDK

BMAPI (Busmust Device Communication Application Programming Interface) SDK by Busmust Co., Ltd.

The BMAPI SDK provides a collection of runtime libraries and examples for real-time connection of applications to CAN/LIN buses via Busmust USB devices.

## Supported Platforms

- Windows XP/7/8/10/11 x86 (32-bit/64-bit)
- Linux x86 (32-bit/64-bit)
- ARM32 (e.g., Raspbian 32-bit)
- ARM64 (e.g., Raspbian 64-bit)
- LoongArch64

## Supported Devices

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

## Quick Start

**Please read the documentation before getting started:**

1. `doc/BMAPI软件开发指南.pdf` — Complete API usage guide (read this first)
2. `doc/BMAPI.chm` — API reference manual for quick lookup during development
3. `doc/Application Programming Sequence.png` — Standard programming sequence diagram

### Standard Programming Sequence

Every application follows this basic flow:

```
1. BM_Init()                    — Initialize the BMAPI library
2. BM_Enumerate()               — Enumerate connected device channels
3. BM_OpenEx(&channel, ...)     — Open a channel with mode/termination/baudrate/filter config
4. BM_GetNotification()         — Get the channel's notification handle
5. Loop:
   BM_WaitForNotifications()    — Wait for data/event notifications
   BM_Read()                    — Read all messages from the queue (until BM_ERROR_QRCVEMPTY)
   BM_WriteCanMessage()         — Send messages
6. BM_Close(channel)            — Close the channel
7. BM_UnInit()                  — Cleanup library resources
```

## SDK Structure

| Directory | Description |
|-----------|-------------|
| `doc/` | Documentation and reference materials |
| `include/` | Header files: `bmapi.h`, `bm_usb_def.h`, `osal.h` |
| `example/` | Examples in multiple languages |

## Examples

### Basic C/C++ Examples (Start Here)

| Example | Description |
|---------|-------------|
| `bmapi_transmit_only/` | Minimal CAN FD **transmit** example |
| `bmapi_receive_only/` | Minimal CAN FD **receive** example |
| `bmapi_lin_txrx/` | LIN transmit/receive (master and slave) |

### Advanced C/C++ Examples

| Example | Description |
|---------|-------------|
| `bmapi_test/` | API basics and loopback testing |
| `bmapi_dualthread_txrx/` | Dual-thread async transmit/receive |
| `multichannel_rx_cpp/` | High-efficiency multi-channel parallel receive |
| `multichannel_isotp_txrx_cpp/` | Multi-channel ISOTP (ISO 15765) + UDS |
| `bmapi_cyclic_tx_task/` | Periodic message transmission (TXTASK API) |
| `hw_replay_demo/` | Hardware sequence playback (3rd-gen devices only) |
| `nobuffer_txrx/` | Zero-buffer ultra-low-latency mode (3rd-gen devices only) |
| `bmapi_remote_connection/` | Remote connection to GWR Ethernet devices |

### GUI Applications

| Example | Language | Description |
|---------|----------|-------------|
| `can_analyzer_qt/` | C++/Qt | Full-featured CAN analyzer |
| `can_analyzer_csharp/` | C# | Full-featured CAN analyzer (WinForms) |
| `can_analyzer_pyqt/` | Python/Qt | Full-featured CAN analyzer |
| `can_analyzer_vb.net/` | VB.NET | Full-featured CAN analyzer |
| `lin_analyzer_qt/` | C++/Qt | Full-featured LIN analyzer |

## Key API Notes

- **`BM_Read` is polling-based**: After each `BM_WaitForNotification`, you must drain the queue by calling `BM_Read` in a loop (until it returns `BM_ERROR_QRCVEMPTY`), since one notification may correspond to multiple messages.
- **`BM_Write` timeout**: timeout=0 means async mode (no TEF consumed); timeout≠0 means blocking mode (waits for ACK).
- **Device exclusivity**: Each physical device can only be opened by one process at a time.
- **Thread safety**: `WriteXXX`/`ReadXXX` functions are thread-safe; `OpenEx` and `SetXXX` functions are **not**.
- **BUSOFF recovery**: See the `recoverFromError()` function in `bmapi_transmit_only`.

## Python Users

See our companion packages for Python development:
- **[python-can](https://github.com/busmust/python-can)** — BUSMUST interface for the python-can library
- **[python-udsoncan](https://github.com/busmust/python-udsoncan)** — BUSMUST interface for the udsoncan library
- **[python-lin](https://github.com/busmust/python-lin)** — BUSMUST LIN bus interface for Python

## Downloads

Pre-built binaries (DLL/SO/LIB) for all platforms are available on the [Releases](https://github.com/busmust/bmapi-sdk/releases) page.

## Support

For technical support, search for "霸码科技" on WeChat, follow our official account, and click "技术支持" to chat directly with our engineers.

## License

See [LICENSE](LICENSE) for details. This SDK is licensed for use exclusively with BUSMUST hardware devices.
