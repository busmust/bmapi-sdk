# BMAPI SDK development entry

## Choose the appropriate layer

- Use BUSMASTER for interactive bus inspection and to reproduce a BUSMASTER problem.
- Prefer BMCLI's existing commands/services for ordinary device automation and higher-level secondary development.
- Use BMAPI when the task explicitly needs the SDK, its examples/bindings, or direct library integration. A BMAPI library issue alone does not require exercising the entire SDK.
- Before SDK traffic, verify the device, driver, wiring, bitrate/sample points and termination in BUSMASTER or BMCLI. Reuse that known-good configuration; a new SDK program must not be used to prove its own environment is good.

## Find authoritative inputs

- `include/bmapi.h`: function signatures, return values and lifecycle; `include/bm_usb_def.h`: flags, structures and layouts.
- `doc/BMAPI软件开发指南.pdf`: programming model and examples; `doc/BMAPI.chm`: API reference. Current public headers take precedence when an older example/document differs.
- `README.md` / `README.zh-CN.md`: SDK layout, language entry points and downloads.
- `example/`: complete examples; keep the SDK directory structure intact when building one.
- `bin/` and `lib/`: match the process architecture and library revision. If headers, import libraries and DLLs do not match, obtain a matching official SDK package or contact BUSMUST support before continuing.

Keep a copy of your input, configuration and diagnostic output when asking for support.

## Read the guide before changing code

Before implementation, read the relevant sections of `doc/BMAPI软件开发指南.pdf`: initialization/open/close programming sequence, sending and receiving, notifications, buffers and threading. Use `doc/BMAPI.chm` for individual API details. Then build and run the nearest unchanged example below, and check the current public header signatures and structure layouts before adapting it.

## Start from the smallest matching example

| Need | Example |
|---|---|
| Receive and drain notifications | `example/bmapi_receive_only` |
| Send and handle status | `example/bmapi_transmit_only` |
| Several receiving channels | `example/multichannel_rx_cpp` |
| Minimal online ASC recording | `example/bmapi_asc_record` |
| Minimal external ASC playback | `example/bmapi_asc_replay` |
| Cyclic hardware tasks | `example/bmapi_cyclic_tx_task` |
| LIN | `example/bmapi_lin_txrx` |
| Python CAN / LIN / UDS | matching `example/python-can-*`, `python-lin-*`, `python-udsoncan-*` |

The ASC pair demonstrates ordinary BMAPI calls and standard file I/O. Read each example's README and its declared format/clock limits.

## Build and run

1. Open the example's `.sln` with the toolset declared by its `.vcxproj`; choose `Release` and the matching `Win32`/`x64` architecture. Do not silently substitute another target or treat Debug as Release acceptance.
2. For a `.pro` example, use an installed Qt kit matching its architecture/compiler. For Makefiles, inspect their library path and platform assumptions. Production/Linux-specific tools are not generic Windows examples.
3. Link against the matching import library and place the matching DLL/dependencies where the application will load them. Win32 uses `BMAPI.dll`/`BMAPI.lib`; the supplied x64 projects normally use `BMAPI64.dll`/`BMAPI64.lib`. For managed/Python code, check the runtime process bitness as well.
4. Confirm the DLL actually loaded by the executable. Compiling against a new header and loading an old DLL is not a valid test of the new source.
5. Run the unchanged, closest example first, then make the smallest change for the requested function. Preserve the command, input, actual result and environment when it fails.

### Linux development

Use the SDK package's matching Linux library (`bin/unix64/release/libbmapi64.so` on x86_64) and install the compiler for your architecture. Keep the SDK directory structure, then run `make` in an example that provides a Makefile. The two ASC examples support strict C99 builds. Set `LD_LIBRARY_PATH` to the absolute directory of the selected library when launching; it is not necessary to copy it into a system directory or change its permissions globally. Package contents vary: inspect the archive before assuming that BMAPI library implementation source is included; example-source compilation and rebuilding the library are different checks.

The current SDK's BMAPI libraries already statically link libusb. Applications use the BMAPI public headers and link only BMAPI; do not add libusb headers, include paths, or `-lusb` / `-lusb-1.0`. When upgrading a project from an older SDK, remove those legacy settings and use matching headers and libraries from the same SDK release.

### Example-specific prerequisites

- Qt CAN/LIN analyzers: build their `.pro` with a matching **32-bit** Qt kit for the supplied Win32 library. A shadow build is supported; deploy the kit's Qt runtime and the matching BMAPI DLL beside the executable. Their multi-select channel list initially checks all enumerated channels: deselect every unrelated device before opening.
- Legacy `python-can-3.3.4` and `python-lin-1.0.0` use `pkg_resources`; use an isolated environment with its provider (for example `setuptools<81`), plus the packages declared by their setup files. Do not mistake a missing Python dependency for a BMAPI failure.
- LabVIEW examples require LabVIEW and matching process/DLL bitness. Linux GPIO/SPI/production examples require their declared Linux host, board and wiring; building a Windows project file does not establish that environment.

## Programming and error handling

1. `BM_Init` → `BM_Enumerate` → explicitly select the intended channel(s) → `BM_OpenEx`.
2. Check every API return that controls the next operation. On partial initialization, close only successfully opened resources, then `BM_UnInit`.
3. Use `BM_GetNotification` / `BM_WaitForNotifications` to wait, then drain all selected receive queues until `BM_ERROR_QRCVEMPTY`. One notification can cover more than one frame or channel. Empty is a normal queue state.
4. For batch I/O, reset the input capacity and use the actual output count, including partial results. Do not assume capacity means completed frames.
5. Timeout zero on write is asynchronous queue admission; nonzero timeout is the blocking path. Do not equate submission, completion and an independent receiver observing the frame.
6. Keep default buffers unless the application specifically needs a different mode. Read `BM_SetBuffer`'s current device-wide semantics before switching; do not copy old buffer settings into unrelated code.
7. Use the current `BM_RecoverBusOff` contract for a confirmed Bus-Off state and check its return. Fix the wiring/configuration cause first. Recovery does not prove an uncertain send was absent or that retrying it is safe. Older firmware may use recovery traffic; do not perform it on an unauthorized bus.
8. Coordinate channel open/close/configuration with worker lifetimes. Never close or replace a handle while another thread uses it, or automatically select a different device after a failure.

## Validate the actual boundary

- Separate build/link, DLL load, API results, file semantics, on-wire behavior and GUI behavior. Success at one boundary does not prove the others.
- Check a normal case and the failures introduced by the change: invalid arguments, file I/O, unsupported records, partial API results, stopping and cleanup.
- Validate ASC output with an independent reader and playback with an independent receiver. Do not use only the new writer/reader round trip as the oracle.
- Use bounded traffic on the identified test circuit, and restore only state changed by the example. Never run production repair, firmware update, clear-configuration or ECU security examples as an ordinary smoke test.
- Review logs and cleanup results. Keep temporary tests/captures outside the SDK product files.
