# Minimal BMAPI ASC recorder / 精简 ASC 录制

Direct BMAPI calls and standard file I/O demonstrate multi-channel CAN/CAN FD
recording. No complete Logger, BAF, BLF, PCAP, background worker or device logging
configuration is needed. Start with the SDK [development entry](../../AGENTS.md).

## Build

Keep the SDK directory layout. Open `bmapi_asc_record.sln`, select **Release**
and x86 (Win32)/x64, then build. Output: `bin/<platform>/Release/`.
Link and load the matching BMAPI DLL/import-library pair. The project copies
the architecture-specific DLL from the SDK; obtain a matching official SDK package or contact BUSMUST support if they differ.

Linux: `make`, then `sh run.sh --help`. Install the appropriate compiler and
provide the matching BMAPI runtime. BMAPI already statically links libusb;
application code needs only the BMAPI public headers and library. Remove legacy
libusb headers/include paths and `-lusb` / `-lusb-1.0` options when upgrading an old project.
MinGW: use the matching `LIBDIR`/`LIBNAME` and runtime, for example `make` with
the supplied mingw32 SDK libraries. A build does not prove hardware behavior.

## Run

1. Verify the exact circuit in BUSMASTER/BMCLI first. Set the known-good bitrate,
   sample point and resistor in `bmapi_asc_common.h` and rebuild. Defaults are
   500/2000 kbit/s, 75% sample points, resistor **disabled**.
2. Run `bmapi_asc_record --list`. Indices belong to this current enumeration;
   do not reuse an index as a durable device identity.
3. Select a contiguous range of intended CAN channels (maximum 32 in this demo):

```text
bmapi_asc_record capture.asc 0 2 5
```

This example selects enumeration indices 0 and 1, maps them to ASC channels 1
and 2, and records for five seconds. Substitute your actual verified indices.
It refuses to overwrite an existing file. The duration deadline or Ctrl+C starts
final draining; Ctrl+C uses the time the main loop first observes the request.

## Format and time

- `base hex / timestamps absolute`; Classic CAN and CAN FD **data** frames,
  standard/extended IDs, FDF/BRS/ESI, DLC and actual byte length.
- Elapsed seconds are derived from BMAPI device receive timestamps, anchored
  to the capture session with host/device timestamp queries. Counters are sampled
  during idle periods to extend 32-bit wrap. This is not a PTP synchronization or
  cross-device accuracy demonstration; queued pre-baseline frames are retained at zero.
- Records are written in queue-service order. Across channels they can have
  overlapping/out-of-order timestamps; no global physical ordering is promised.
- RTR/unsupported input, receive errors/overrun, or file I/O errors stop with a
  nonzero result. A partial file is retained and explicitly reported incomplete.
  Final draining uses batches of at most 64 frames per channel and a 2-second
  limit. Frames at or before the stop cutoff are retained; later timestamps are
  drained but excluded. Success requires an entire post-stop sweep of empty
  selected queues with no reported error. Continuous traffic or a drain timeout
  yields a nonzero, incomplete result. This covers frames admitted to the SDK
  receive queues; it does not prove that the physical source never lost a frame.

The API sequence remains visible in the C file: initialize/open, obtain
notifications, wait, drain every selected queue, write, close and uninitialize.
Validate the ASC file with an independent reader and known input traffic.
