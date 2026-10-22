# Minimal BMAPI ASC playback / 精简外部 ASC 在线回放

Demonstrates ordinary BMAPI online transmission from an external ASC file.
There is no hardware Replay task, offline configuration or complete Logger.
Start with the SDK [development entry](../../AGENTS.md).

## Build and configure

Open `bmapi_asc_replay.sln`; build **Release**, x86 (Win32)/x64. Output is
`bin/<platform>/Release/`, including the matching SDK DLL copied by the project.
Obtain a matching official SDK package or contact BUSMUST support if the DLL,
import library and headers differ. Linux: `make`, then `sh run.sh --help`.
BMAPI already statically links libusb; use only the BMAPI public headers and
library. Remove legacy libusb headers/include paths and `-lusb` / `-lusb-1.0`
options when upgrading an old project.
MinGW uses the matching SDK import library/runtime via `LIBDIR`/`LIBNAME`.

Before traffic, verify the circuit in BUSMASTER/BMCLI and put its known-good
bitrate/sample point/resistor in `bmapi_asc_common.h`. Defaults are
500/2000 kbit/s, 75% sample points and resistor **disabled**.

## Check first, then play

```text
bmapi_asc_replay --check input.asc
bmapi_asc_replay --list
bmapi_asc_replay input.asc 0 2
```

`--check` validates the entire file without opening hardware. In the final
command, ASC channel 1 maps to current enumeration index 0, channel 2 to index 1.
Substitute the verified intended indices; indices are not persistent identities.
All records and mappings are checked before opening channels. Do not edit the
input during playback. Ctrl+C stops; incomplete playback returns nonzero.

## Deliberately small ASC subset

- Require `base hex timestamps absolute`: timestamp values are elapsed seconds,
  **not** wall-clock time. Relative/system timestamps and decimal-base files are
  rejected rather than interpreted as another clock domain.
- Classic CAN records: `time channel id Rx|Tx d length bytes...`; extended IDs
  have an `x` suffix; 0–8 data bytes.
- CAN FD: `time CANFD channel Rx|Tx id [single-symbolic-name] BRS ESI DLC length bytes...`;
  DLC is hex, actual byte length is decimal and must match. Eight optional integer
  Vector metadata columns may follow (or all may be absent).
- Normal date/header/comment/trigger-block markers are accepted. RTR, error
  frames, LIN, signal/event records and other formats are unsupported. Errors
  identify the line, and are not silently skipped as successful playback.
- Both Rx and Tx **data** records are sent once, in file order. Prepare the
  intended direction(s) in the input to avoid replaying both a frame and its echo.

The first nonzero timestamp retains its initial delay. Each frame uses the same
monotonic session baseline, avoiding accumulated per-frame sleep drift. Equal
timestamps retain file order; a past deadline sends the next record immediately.
This ordinary host-scheduled example does not promise hard real-time precision.
Default buffers and bounded blocking `BM_WriteCanMessage` are used. Send errors
stop with the line and successful-send count; recovery never retries an uncertain
frame or converts an incomplete run into success. Verify traffic independently.

Common `Start of measurement` markers emitted by ASCWriter are accepted as metadata after the required absolute header. They do not transmit a frame.
