# Changelog

## 1.13.6.40 [2025-05-15]

1. Added `BM_RecoverBusOff` API for active Bus-off recovery, avoiding the impact of periodic polling on other channels' transmit periods; added `BM_CAN_DISABLE_AUTO_BUSOFF_RECOVERY` mode flag to allow users to disable automatic recovery
2. Added `BM_HardwareLastMsgStat` API to get the hardware's latest transmitted message status
3. Python LIN library removed python-can dependency, bundled aenum module, added `store_task=False` hardware transmit task demo

## 1.13.5.39 [2025-01-23]

1. From this version onwards, CICD fully automated builds are used. A fifth digit (Build Number) will be appended to the four-digit version number (e.g., 1.13.5.39)
2. Fixed an issue where `BM_WriteIsotp` under CAN FD protocol (not CAN 2.0) sent non-conformant ISOTP messages when the payload length was smaller than the DLC capacity
3. Removed the dependency on libusb shared library! The `-lusb` linker option is no longer needed when compiling applications
4. Reorganized SDK folder contents, standardized naming, optimized startup scripts, etc. Contact technical support if you encounter any issues
5. Fixed an unexpected 32-bit timestamp rollover issue in the Python environment; timestamps now rely entirely on BMAPI.DLL

## 1.13.3.38 [2024-12-30]

1. **Critical BUGFIX!** Fixed a memory management error in the BMAPI base library introduced in September 2025, which caused long-running receive sessions to stop automatically, requiring a Disconnect/Connect cycle to resume reception
2. Added `ECHO` attribute to the `ctrl.tx` member of `BM_CanfdMessageTypeDef`. When set, even with blocking `BM_Write` calls, an additional ACK message is received for echo purposes

## 1.13.2.37 [2025-11-07]

1. Restored support for the MinGW compilation environment
2. Fixed runtime errors where `bmapi*.so` files for x86, ARM, and AArch64 platforms were missing libudev and other dynamic library symbols
3. Fixed a compatibility issue where second-generation products would hang on `BM_Close`
4. Improved python-can platform and version compatibility
5. Improved python-can Bus-off recovery logic and enhanced multi-threading support
6. Fixed a crash in `BM_Close` on Linux systems caused by uninitialized variables in certain environments
7. Optimized the self-recovery capability of third-generation devices during frequent Bus-off conditions by adjusting the USB transmit enable logic

## 1.13.1.35 [2025-09-09]

1. Added `BM_IsotpConfigTypeDef.flowcontrol.hardwareIsotpDisabled` option to disable hardware acceleration for ISOTP flow control; defaults to enabled if the hardware supports it
2. Fixed an issue where the CF sequence number was unexpectedly reset in `BM_WriteIsotp` during multi-frame flow control scenarios
3. Fixed python-can throwing an exception and aborting when no hardware device is plugged in, due to Virtual Channel not supporting PTP
4. Changed the timestamp returned by `BM_GetDataPtpTimestamp` from microseconds to nanoseconds, consistent with `BM_GetPtpTime` and `BM_GetHostPtpTime`; all examples updated accordingly
5. Improved message transmission efficiency and receive/transmit stability for third-generation devices

## 1.13.0.35 [2025-08-23]

1. Added comprehensive PTP-related APIs, with PTP functionality demonstrated in `can_analyzer_qt` and other examples (both second and third generation products support these APIs)
2. Optimized hardware ISOTP transmission efficiency in `BM_WriteIsotp`
3. `BM_WriteMultiple` and `BM_WriteMultipleCanMessage` now return the actual number of successfully sent messages via function parameters on failure
4. Optimized underlying receive/transmit parameters for third-generation devices, improving efficiency

## 1.12.3.34 [2025-07-16]

1. Added `sync_timestamp` demo to python-can and fixed a timestamp offset bug
2. Improved device naming in `BM_Enumerate`, e.g., unified naming for PCIe board multi-channel devices
3. Updated `BM_TxTaskTypeDef` definition while maintaining backward compatibility; added frame delay within the transmit period for LIN schedule table and other timing designs

## 1.12.2.33 [2025-06-24]

1. Improved compatibility of the new 64-bit UTC timestamp feature in all examples with older (pre-third-generation) devices
2. Fixed issues where Virtual Channel's two channels could not communicate with each other
3. Improved python-can timestamp synchronization precision

## 1.12.1.33 [2025-06-10]

1. Fixed several early issues with LIN-related APIs; LIN API is now production-ready
2. **New Feature!** Support for 64-bit UTC timestamps on third-generation hardware devices (in `BM_DataTailTypeDef`), demonstrated in multiple examples — no more timestamp rollover concerns
3. Added `recoverFromError` demo to python-can
4. Introduced the exclusive python-lin package for LIN bus control and message receive/transmit using Python, with a programming interface similar to python-can
5. Added thread priority tuning API on Linux for customers who need it; recommended to keep defaults for standard applications

## 1.12.0.32 [2025-04-07]

1. Official LIN support from this version; LIN-related APIs and examples are now stabilized
2. Support for GWR devices with more than 16 channels: introduced the `group` concept in `BM_DataTypeDef` alongside the existing `schn` and `dchn`. Each device supports up to 8 groups, each group has 16 channels, for a maximum of 128 channels per device
3. Added `BM_MapTimestamp` API to map the 32-bit hardware local timestamp in `BM_DataTypeDef` to a 64-bit UTC timestamp for long-duration data recording

## 1.11.0.31 [2025-03-03]

1. Added a series of APIs to support new LIN devices
2. Added a series of APIs to support new Ethernet-capable devices such as GWR, enabling remote device control and message receive/transmit over the network, with the ability to save configuration offline to a TF card
3. `BM_WriteIsotp` supports ultra-high-efficiency (near theoretical limit) ISOTP transmission; requires X2 or X4 devices with firmware version 2.4.12.x or later. Falls back to default software implementation if not supported (no compatibility issues)
4. Added `BM_CancelWrite` API, fixing a resource leak issue when using `send_periodic` in python-can
5. Added `bmapi_lin_txrx` example demonstrating LIN message receive/transmit with master and slave support
6. Added `lin_analyzer_qt` example demonstrating LIN message receive/transmit via Qt GUI with master and slave support
7. Added `bmapi_remote_connection` example for C++ and python-can, demonstrating remote control of GWR and other Ethernet-capable devices over the network
8. Added `loopback_test` example to python-can, demonstrating hardware self-loopback testing
9. Fixed inconsistency between `BM_WriteIsotp` and ISO 15765 CAN FD short-frame segmentation in certain corner cases
10. Fixed incorrect frame sequence number reset in `BM_WriteIsotp` after receiving an FC frame
11. Added a series of APIs supporting PTP time synchronization for new X2R/X4R/GWR devices
12. Optimized BMAPI maximum receive frame rate on Windows
13. Used `BM_SetTxTask` hardware transmit tasks in python-can to implement the `send_periodic` Python API, ensuring message period accuracy within 1 ms
14. Improved `BM_OpenEx` error codes: returns `BM_ERROR_ILLPARAMVAL` when channelinfo is empty or all zeros
15. Added two virtual channels that can communicate with each other; other features in progress
16. Fixed an issue where the CF request ID was incorrectly reset in `BM_WriteIsotp` across multiple FC scenarios
17. Fixed an issue where `BM_ReadIsotp` would reset the current session upon receiving another SF request (e.g., 3E 80)
18. Added automatic error recovery mechanism to `can_analyzer_qt` example, supporting USB device unplugging detection and Bus-off detection
19. Added internal implementation of `BM_NO_ACK` mode in BMAPI, enabling fast message transmission in asynchronous mode without processing transmit completion events. Note: in this mode, it is not possible to determine whether a message was successfully transmitted

## 1.10.2.30 [2024-03-04]

1. Added `multichannel_logger` example, additionally demonstrating how to record messages from multiple channels simultaneously into a single ASC file
2. Fixed a bug where `BM_ReadMultiple` and `BM_ReadMultipleCanMessage` could not retrieve the number of received messages via the `nmessages` parameter in certain scenarios
3. Improved return value definitions for `BM_ReadMultiple` and `BM_ReadMultipleCanMessage`: returns error code on exception; returns `BM_ERROR_BUSTIMEOUT` when the specified number of messages is not received within the specified time; otherwise returns `BM_ERROR_OK`
4. Eliminated extraneous debug output from `BM_OpenEx` on Linux
5. Fixed a crash caused by `BM_Enumerate` on Linux repeatedly enumerating already-opened devices (e.g., in python-can environments)
6. Fixed `multichannel_bmapi_demo.py` example throwing `BM_ERROR_QRCVEMPTY` exception after receiving messages

## 1.10.1.29 [2023-12-31]

1. Added `bmapi_cyclic_tx_task` example demonstrating how to use the BUSMUST-specific TXTASK API for periodic message transmission (e.g., network management messages)
2. Added `multichannel_isotp_txrx_cpp` example demonstrating multi-channel ISOTP (ISO 15765) communication
3. Added support for python-can 4.0.0 (theoretically supports higher 4.x.x versions)
4. Added python-can ECU UDS simulator example
5. Added several APIs to support offline configuration/recording/playback features

## 1.9.0.23 [2022-07-22]

1. Added support for VB.Net language framework
2. Restored support for MinGW compiler
3. Improved library import declarations for C#.Net and VB.Net
4. Fixed the C#.Net example not properly closing the hardware device when clicking Close

## 1.8.1.22 [2022-07-08]

1. Added `BM_ResetDevice` API for rebooting the entire USB device
2. Fixed `can_analyzer_csharp` example throwing an error when clicking Open with no channel selected
3. Fixed garbled characters at the end of channel names obtained via `BM_Enumerate` in C# programs

## 1.8.0.21 [2022-05-30]

1. Supported the latest 8-channel hardware device X8PI
2. Added several new APIs to support the latest offline recording/playback devices X4R, X2R
3. Added C++ multi-channel parallel receive example `multichannel_rx_cpp`
4. BMAPI partially refactored on Linux with significantly reduced CPU usage

## 1.7.1.20 [2022-02-16]

1. Supported Python 2.7.18
2. Python-can added `send_multiple` API and corresponding example
3. Updated raspbian64 platform binaries
4. Bundled aenum package with python-can
5. Updated udsoncan's `download_hex_file` example to support loading CANoe-format security DLLs

## 1.7.0.19

1. Qt/C#/PyQt examples now support multi-channel operation
2. Added `bmapi_receive_only` and `bmapi_transmit_only` examples
3. Fixed a hang issue when opening multiple channels simultaneously on Unix systems
4. Improved python-can multi-platform compatibility auto-detection

## 1.6.0.18

1. Fixed remote frames not being transmitted in real time
2. Python-can bus creation now allows specifying the sample point position
3. `BM_ReadIsotp` supports listening to both specified UDS Request ID and functional addressing address simultaneously
4. Fixed handle resource release issue with `BM_Reset` in multi-threaded environments

## 1.6.0.17 [Alpha]

1. Added `BM_GetVersion` API
2. Added `BM_GetLogLevel` and `BM_SetLogLevel` APIs

## 1.5.4.16 [2021-08-13]

1. Fixed a bug where `BM_OpenEx` failed to correctly configure X1PRO and X1 devices when receiving messages during initialization
2. Fixed `BM_ClearBuffer` recursive lock causing program blocking on Debian
3. Added `example.run.sh` launcher for udsoncan
4. Fixed incorrect sequence number in the first CF frame of `BM_WriteIsotp` and `BM_ReadIsotp`

---

For changes prior to version 1.5.4.16, see the changelist in the BUSMASTER installer package.
