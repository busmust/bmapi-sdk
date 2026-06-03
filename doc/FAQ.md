# Frequently Asked Questions

## Q1: BM_Write always times out — what could be the reasons?

A timeout means the message could not be sent to the bus, or it was sent but no acknowledgment was received (this is a fundamental CAN protocol mechanism, handled automatically by the hardware). There are many possible causes. We recommend first using the verified BUSMASTER application to open the same port with the same baud rate and other configuration parameters, try sending, and observe whether it succeeds. If it doesn't, adjust the configuration parameters. Once you've verified successful transmission in BUSMASTER, return to your development environment and use the same configuration parameters to initialize the channel.

## Q2: After a timeout, subsequent sends always fail with BUSOFF

The same advice applies — first ensure stable communication using BUSMASTER, then try in your custom application. Additionally, you can refer to the `recoverFromError()` function in the `bmapi_transmit_only` example to learn how to recover from a BUSOFF fault and continue sending/receiving.

## Q3: Are BMAPI functions thread-safe?

All `WriteXXX` and `ReadXXX` functions support multi-threading. However, `BM_OpenEx` and all `SetXXX` functions do **not** support multi-threading.

## Q4: Is there a limit on how many CAN messages BM_WriteMultipleCanMessage can send at once?

The internal transmit buffer is 1 MB, which can hold approximately 25,000 messages.
