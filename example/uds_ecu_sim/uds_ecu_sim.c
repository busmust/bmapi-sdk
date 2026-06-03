/**
 * @file        uds_ecu_sim.c
 * @brief       UDS ECU Simulator for HIL testing
 *
 * Simulates a UDS ECU server over ISOTP. Used with bmcli (tester) to
 * verify UDS diagnostic services end-to-end without a real ECU.
 *
 * Usage:
 *   uds_ecu_sim.exe --channel=0 --reqid=0x7E0 --respid=0x7E8
 *
 * The ECU listens for ISOTP requests on reqid and responds on respid.
 * Supports common UDS services: session control, ECU reset, security
 * access (seed/key with ~seed algorithm), read/write DID, DTC,
 * routine control, download/upload, tester present, etc.
 */
#ifdef __GNUC__
#include <unistd.h>
#elif defined(_MSC_VER)
#include <Windows.h>
#define usleep(us) Sleep((us)/1000)
#endif
#include <stdint.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include "bmapi.h"

/* ---- Configuration ---- */

static int   g_channel    = 0;
static uint32_t g_reqid   = 0x7E0;
static uint32_t g_respid  = 0x7E8;
static int   g_use_fd     = 0;
static int   g_verbose    = 0;
static int   g_mode_raw   = 0;  /* 0=BMAPI ISOTP (default), 1=raw CAN ISOTP */
static int   g_no_hw_isotp = 0; /* 1=disable HW ISOTP in BM_WriteIsotp */

/* ---- UDS Session State ---- */

static uint8_t  g_session_type = 1;
static int      g_security_level = 0;
static uint8_t  g_seed[4] = {0x12, 0x34, 0x56, 0x78};
static int      g_seed_requested = 0;
static uint8_t  g_dtc_setting_on = 1;
static uint32_t g_download_addr = 0;
static int      g_download_active = 0;
static uint8_t  g_download_data[65536];

/* ---- Helpers ---- */

static void hexprint(const uint8_t *data, int len)
{
    int i;
    for (i = 0; i < len; i++) printf("%02X ", data[i]);
}

/* ---- UDS Response Builders ---- */

static int make_pos_response(uint8_t sid, uint8_t *resp)
{
    resp[0] = sid + 0x40;
    return 1;
}

static int make_neg_response(uint8_t sid, uint8_t nrc, uint8_t *resp)
{
    resp[0] = 0x7F;
    resp[1] = sid;
    resp[2] = nrc;
    return 3;
}

#define NRC_SERVICE_NOT_SUPPORTED      0x11
#define NRC_SUB_FUNCTION_NOT_SUPPORTED 0x12
#define NRC_INCORRECT_MESSAGE_LENGTH   0x13
#define NRC_SECURITY_ACCESS_DENIED     0x35
#define NRC_INVALID_KEY                0x35
#define NRC_REQUEST_SEQUENCE_ERROR     0x24
#define NRC_REQUEST_OUT_OF_RANGE       0x31

/* ---- Service Handlers ---- */

static int handle_session_control(uint8_t *req, int reqlen, uint8_t *resp)
{
    if (reqlen < 2) return make_neg_response(req[0], NRC_INCORRECT_MESSAGE_LENGTH, resp);
    uint8_t subtype = req[1] & 0x7F;
    if (subtype == 0 || subtype > 3)
        return make_neg_response(req[0], NRC_SUB_FUNCTION_NOT_SUPPORTED, resp);
    g_session_type = subtype;
    g_security_level = 0;
    int n = make_pos_response(req[0], resp);
    resp[n] = subtype;
    return n + 1;
}

static int handle_ecu_reset(uint8_t *req, int reqlen, uint8_t *resp)
{
    if (reqlen < 2) return make_neg_response(req[0], NRC_INCORRECT_MESSAGE_LENGTH, resp);
    uint8_t subtype = req[1] & 0x7F;
    if (subtype == 0 || subtype > 3)
        return make_neg_response(req[0], NRC_SUB_FUNCTION_NOT_SUPPORTED, resp);
    int n = make_pos_response(req[0], resp);
    resp[n] = subtype;
    return n + 1;
}

static int handle_security_access(uint8_t *req, int reqlen, uint8_t *resp)
{
    if (reqlen < 2) return make_neg_response(req[0], NRC_INCORRECT_MESSAGE_LENGTH, resp);
    uint8_t subtype = req[1];

    if ((subtype & 0x01) == 1) {
        /* Request seed */
        g_seed_requested = 1;
        g_seed[0] ^= 0x5A; g_seed[1] ^= 0xA5;
        g_seed[2] ^= 0x3C; g_seed[3] ^= 0xC3;
        int n = make_pos_response(req[0], resp);
        resp[n] = subtype;
        memcpy(resp + n + 1, g_seed, 4);
        return n + 5;
    } else {
        /* Send key */
        if (!g_seed_requested)
            return make_neg_response(req[0], NRC_REQUEST_SEQUENCE_ERROR, resp);
        if (reqlen < 6)
            return make_neg_response(req[0], NRC_INCORRECT_MESSAGE_LENGTH, resp);
        int valid = 1;
        int i;
        for (i = 0; i < 4; i++) {
            if (req[2 + i] != (uint8_t)(~g_seed[i])) { valid = 0; break; }
        }
        g_seed_requested = 0;
        if (!valid)
            return make_neg_response(req[0], NRC_INVALID_KEY, resp);
        g_security_level = subtype - 1;
        int n = make_pos_response(req[0], resp);
        resp[n] = subtype;
        return n + 1;
    }
}

static int handle_tester_present(uint8_t *req, int reqlen, uint8_t *resp)
{
    if (reqlen < 2) return make_neg_response(req[0], NRC_INCORRECT_MESSAGE_LENGTH, resp);
    if (req[1] & 0x80) return 0; /* suppressPosRspMsgIndicationBit */
    int n = make_pos_response(req[0], resp);
    resp[n] = req[1];
    return n + 1;
}

static int handle_read_data_by_id(uint8_t *req, int reqlen, uint8_t *resp)
{
    if (reqlen < 3) return make_neg_response(req[0], NRC_INCORRECT_MESSAGE_LENGTH, resp);
    uint16_t did = ((uint16_t)req[1] << 8) | req[2];
    int n = make_pos_response(req[0], resp);
    int dlen = 0;

    switch (did) {
        case 0xF190: memcpy(resp+n+2, "WVWZZZ3CZWE123456", 17); dlen=17; break;
        case 0xF180: memcpy(resp+n+2, "SW_BOOT1", 8); dlen=8; break;
        case 0xF181: memcpy(resp+n+2, "SW_APP_V2", 9); dlen=9; break;
        case 0xF18C: memcpy(resp+n+2, "SN12345678", 10); dlen=10; break;
        case 0xF193: memcpy(resp+n+2, "HW_V1.0", 7); dlen=7; break;
        case 0xF194: memcpy(resp+n+2, "P/N_001", 7); dlen=7; break;
        case 0xF195: memcpy(resp+n+2, "V1.2.3", 6); dlen=6; break;
        default:
            return make_neg_response(req[0], NRC_REQUEST_OUT_OF_RANGE, resp);
    }
    resp[n] = (uint8_t)(did >> 8);
    resp[n+1] = (uint8_t)(did & 0xFF);
    return n + 2 + dlen;
}

static int handle_write_data_by_id(uint8_t *req, int reqlen, uint8_t *resp)
{
    if (reqlen < 4) return make_neg_response(req[0], NRC_INCORRECT_MESSAGE_LENGTH, resp);
    if (g_security_level == 0)
        return make_neg_response(req[0], NRC_SECURITY_ACCESS_DENIED, resp);
    int n = make_pos_response(req[0], resp);
    resp[n] = req[1];
    resp[n+1] = req[2];
    return n + 2;
}

static int handle_routine_control(uint8_t *req, int reqlen, uint8_t *resp)
{
    if (reqlen < 4) return make_neg_response(req[0], NRC_INCORRECT_MESSAGE_LENGTH, resp);
    if (req[1] == 0 || req[1] > 3)
        return make_neg_response(req[0], NRC_SUB_FUNCTION_NOT_SUPPORTED, resp);
    int n = make_pos_response(req[0], resp);
    resp[n] = req[1]; resp[n+1] = req[2]; resp[n+2] = req[3];
    return n + 3;
}

static int handle_request_download(uint8_t *req, int reqlen, uint8_t *resp)
{
    if (reqlen < 2) return make_neg_response(req[0], NRC_INCORRECT_MESSAGE_LENGTH, resp);
    if (g_security_level == 0)
        return make_neg_response(req[0], NRC_SECURITY_ACCESS_DENIED, resp);
    g_download_active = 1;
    g_download_addr = 0;
    if (g_verbose) printf("  Download start\n");
    int n = make_pos_response(req[0], resp);
    resp[n] = 0x20; resp[n+1] = 0x08; resp[n+2] = 0x00; /* maxBlock=0x0800 */
    return n + 3;
}

static int handle_transfer_data(uint8_t *req, int reqlen, uint8_t *resp)
{
    if (reqlen < 2) return make_neg_response(req[0], NRC_INCORRECT_MESSAGE_LENGTH, resp);
    if (!g_download_active)
        return make_neg_response(req[0], NRC_REQUEST_SEQUENCE_ERROR, resp);
    int datalen = reqlen - 2;
    if (datalen > 0 && g_download_addr + datalen <= (int)sizeof(g_download_data)) {
        memcpy(g_download_data + g_download_addr, req + 2, datalen);
        g_download_addr += datalen;
    }
    int n = make_pos_response(req[0], resp);
    resp[n] = req[1];
    return n + 1;
}

static int handle_transfer_exit(uint8_t *req, int reqlen, uint8_t *resp)
{
    (void)reqlen;
    if (!g_download_active)
        return make_neg_response(req[0], NRC_REQUEST_SEQUENCE_ERROR, resp);
    g_download_active = 0;
    if (g_verbose) printf("  Download complete: %u bytes\n", g_download_addr);
    return make_pos_response(req[0], resp);
}

static int handle_read_dtc(uint8_t *req, int reqlen, uint8_t *resp)
{
    if (reqlen < 2) return make_neg_response(req[0], NRC_INCORRECT_MESSAGE_LENGTH, resp);
    int n = make_pos_response(req[0], resp);
    switch (req[1]) {
        case 0x01: /* reportNumberOfDTCByStatusMask */
            resp[n]=0x01; resp[n+1]=0x01; resp[n+2]=0x00;
            resp[n+3]=0x00; resp[n+4]=0x03; /* 3 DTCs */
            return n+5;
        case 0x02: /* reportDTCByStatusMask */
            resp[n]=0x02; resp[n+1]=0x01;
            resp[n+2]=0x01; resp[n+3]=0x00; resp[n+4]=0x01;
            resp[n+5]=0x02; resp[n+6]=0x00; resp[n+7]=0x01;
            resp[n+8]=0x03; resp[n+9]=0x00; resp[n+10]=0x01;
            return n+11;
        case 0x0A: /* reportSupportedDTCs */
            resp[n]=0x0A; resp[n+1]=0x01;
            resp[n+2]=0x01; resp[n+3]=0x00; resp[n+4]=0x01;
            resp[n+5]=0x02; resp[n+6]=0x00; resp[n+7]=0x01;
            resp[n+8]=0x03; resp[n+9]=0x00; resp[n+10]=0x01;
            return n+11;
        case 0x0B: /* reportFirstTestFailedDTC */
        case 0x15: /* reportDTCWithPermanentStatus */
            resp[n]=req[1]; resp[n+1]=0x01;
            resp[n+2]=0x01; resp[n+3]=0x00; resp[n+4]=0x01;
            return n+5;
        default:
            return make_neg_response(req[0], NRC_SUB_FUNCTION_NOT_SUPPORTED, resp);
    }
}

static int handle_clear_dtc(uint8_t *req, int reqlen, uint8_t *resp)
{
    (void)req; (void)reqlen;
    return make_pos_response(0x14, resp);
}

static int handle_comm_control(uint8_t *req, int reqlen, uint8_t *resp)
{
    if (reqlen < 3) return make_neg_response(req[0], NRC_INCORRECT_MESSAGE_LENGTH, resp);
    int n = make_pos_response(req[0], resp);
    resp[n] = req[1]; resp[n+1] = req[2];
    return n + 2;
}

static int handle_control_dtc_setting(uint8_t *req, int reqlen, uint8_t *resp)
{
    if (reqlen < 2) return make_neg_response(req[0], NRC_INCORRECT_MESSAGE_LENGTH, resp);
    g_dtc_setting_on = (req[1] == 0x01) ? 1 : 0;
    int n = make_pos_response(req[0], resp);
    resp[n] = req[1];
    return n + 1;
}

static int handle_read_memory(uint8_t *req, int reqlen, uint8_t *resp)
{
    if (reqlen < 3) return make_neg_response(req[0], NRC_INCORRECT_MESSAGE_LENGTH, resp);
    int n = make_pos_response(req[0], resp);
    uint8_t len = req[reqlen - 1];
    int i;
    for (i = 0; i < len && n+i < 4000; i++)
        resp[n+i] = (uint8_t)(i & 0xFF);
    return n + len;
}

/* ---- UDS Dispatcher ---- */

static int process_uds_request(uint8_t *req, int reqlen, uint8_t *resp)
{
    if (reqlen < 1) return 0;
    if (g_verbose) { printf("  RX[%d]: ", reqlen); hexprint(req, reqlen); printf("\n"); }

    int r = 0;
    switch (req[0]) {
        case 0x10: r = handle_session_control(req, reqlen, resp); break;
        case 0x11: r = handle_ecu_reset(req, reqlen, resp); break;
        case 0x27: r = handle_security_access(req, reqlen, resp); break;
        case 0x22: r = handle_read_data_by_id(req, reqlen, resp); break;
        case 0x2E: r = handle_write_data_by_id(req, reqlen, resp); break;
        case 0x31: r = handle_routine_control(req, reqlen, resp); break;
        case 0x34: r = handle_request_download(req, reqlen, resp); break;
        case 0x36: r = handle_transfer_data(req, reqlen, resp); break;
        case 0x37: r = handle_transfer_exit(req, reqlen, resp); break;
        case 0x3E: r = handle_tester_present(req, reqlen, resp); break;
        case 0x19: r = handle_read_dtc(req, reqlen, resp); break;
        case 0x14: r = handle_clear_dtc(req, reqlen, resp); break;
        case 0x28: r = handle_comm_control(req, reqlen, resp); break;
        case 0x85: r = handle_control_dtc_setting(req, reqlen, resp); break;
        case 0x23: r = handle_read_memory(req, reqlen, resp); break;
        case 0x3D: r = handle_read_memory(req, reqlen, resp); break;
        default:
            r = make_neg_response(req[0], NRC_SERVICE_NOT_SUPPORTED, resp);
            break;
    }

    if (r > 0 && g_verbose) { printf("  TX[%d]: ", r); hexprint(resp, r); printf("\n"); }
    return r;
}

/* ---- ISOTP Callback (diagnostic) ---- */

static int g_diag_total_cb_calls = 0;

static uint8_t isotp_diag_callback(const BM_IsotpStatusTypeDef *status, uintptr_t userarg)
{
    g_diag_total_cb_calls++;
    if (g_verbose || status->ntotalbytes == 0) {
        printf("  [CB#%d] transferred=%u/%u fc=%u bs=%u stmin=%u\n",
               g_diag_total_cb_calls,
               status->ntransferredbytes, status->ntotalbytes,
               status->flowcontrol, status->blocksize, status->stmin);
    }
    return 0;
}

/* Drain and print all messages currently in rxq (diagnostic) */
static int drain_queue_print(BM_ChannelHandle ch)
{
    int count = 0;
    BM_DataTypeDef data;
    while (BM_Read(ch, &data) == BM_ERROR_OK) {
        count++;
        if (data.header.type & BM_CAN_FD_DATA) {
            BM_CanMessageTypeDef *msg = (BM_CanMessageTypeDef *)data.payload;
            uint32_t rxid = BM_GET_STD_MSG_ID(msg->id);
            int is_echo = (data.header.type & BM_ACK_DATA) ? 1 : 0;
            printf("  [Q%d] id=0x%03X echo=%d dlc=%d [%02X %02X %02X %02X %02X %02X %02X %02X]\n",
                   count, rxid, is_echo, msg->ctrl.rx.DLC,
                   msg->payload[0], msg->payload[1], msg->payload[2], msg->payload[3],
                   msg->payload[4], msg->payload[5], msg->payload[6], msg->payload[7]);
        } else {
            printf("  [Q%d] type=0x%02X len=%d\n", count, data.header.type, data.length);
        }
    }
    return count;
}

/* ---- Raw CAN ISOTP helpers ---- */

/* Send a CAN frame */
static BM_StatusTypeDef send_can(BM_ChannelHandle ch, uint32_t id,
                                  const uint8_t *payload, int len)
{
    BM_CanMessageTypeDef msg;
    memset(&msg, 0, sizeof(msg));
    BM_SET_STD_MSG_ID(msg.id, id);
    msg.ctrl.tx.DLC = 8;
    memcpy(msg.payload, payload, len);
    /* Pad remaining bytes */
    { int i; for (i = len; i < 8; i++) msg.payload[i] = 0xCC; }
    return BM_WriteCanMessage(ch, &msg, 0, 5000, NULL);
}

/* Receive one CAN frame with matching ID, with timeout (ms) */
static BM_StatusTypeDef recv_can_match(BM_ChannelHandle ch,
                                        BM_NotificationHandle notify,
                                        uint32_t match_id,
                                        uint8_t *out_payload, int timeout_ms)
{
    if (BM_WaitForNotifications(&notify, 1, timeout_ms) < 0)
        return BM_ERROR_BUSTIMEOUT;
    BM_DataTypeDef data;
    while (BM_Read(ch, &data) == BM_ERROR_OK) {
        if ((data.header.type & BM_CAN_FD_DATA) &&
            !(data.header.type & BM_ACK_DATA)) {
            BM_CanMessageTypeDef *msg = (BM_CanMessageTypeDef *)data.payload;
            uint32_t rxid = BM_GET_STD_MSG_ID(msg->id);
            if (rxid == match_id) {
                int dlc = msg->ctrl.rx.DLC;
                int plen = dlc < 8 ? dlc : 8;
                memcpy(out_payload, msg->payload, plen);
                return BM_ERROR_OK;
            }
        }
    }
    return BM_ERROR_BUSTIMEOUT;
}

/* Receive ISOTP message using raw CAN frames (avoids HW ISOTP state bug).
   Supports Single Frame and multi-frame (FF+CF) reception. */
static BM_StatusTypeDef recv_isotp_raw(BM_ChannelHandle ch,
                                        BM_NotificationHandle notify,
                                        uint32_t req_id,
                                        uint8_t *out, uint32_t *outlen,
                                        int timeout_ms)
{
    uint8_t frame[8];
    BM_StatusTypeDef err;

    /* Wait for first frame (SF or FF) */
    err = recv_can_match(ch, notify, req_id, frame, timeout_ms);
    if (err != BM_ERROR_OK) return err;

    uint8_t pci = frame[0] >> 4;
    if (pci == 0x0) {
        /* Single Frame: 0x0N <data> */
        int sf_len = frame[0] & 0x0F;
        if (sf_len == 0 || sf_len > 7) return BM_ERROR_BUSTIMEOUT;
        memcpy(out, frame + 1, sf_len);
        *outlen = (uint32_t)sf_len;
        return BM_ERROR_OK;
    } else if (pci == 0x1) {
        /* First Frame: 0x1N NN <6 bytes data> */
        uint16_t total = ((uint16_t)(frame[0] & 0x0F) << 8) | frame[1];
        if (total > *outlen) return BM_ERROR_BUSTIMEOUT;
        uint32_t copied = 0;
        memcpy(out + copied, frame + 2, 6);
        copied = 6;
        /* Send Flow Control */
        {
            uint8_t fc[8] = {0x30, 0x00, 0x00, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC};
            send_can(ch, g_respid, fc, 8);
        }
        /* Receive Consecutive Frames */
        int seq = 1;
        while (copied < total) {
            err = recv_can_match(ch, notify, req_id, frame, timeout_ms);
            if (err != BM_ERROR_OK) return err;
            if ((frame[0] >> 4) != 0x2) return BM_ERROR_BUSTIMEOUT;
            int chunk = (int)(total - copied);
            if (chunk > 7) chunk = 7;
            memcpy(out + copied, frame + 1, chunk);
            copied += chunk;
            seq++;
        }
        *outlen = total;
        return BM_ERROR_OK;
    }
    return BM_ERROR_BUSTIMEOUT;
}

/* Send ISOTP response using raw CAN frames.
   Uses Single Frame for ≤7 bytes, multi-frame otherwise. */
static BM_StatusTypeDef send_isotp_raw(BM_ChannelHandle ch,
                                        BM_NotificationHandle notify,
                                        uint32_t resp_id, uint32_t tester_id,
                                        const uint8_t *data, uint32_t len)
{
    if (len <= 7) {
        /* Single Frame */
        uint8_t frame[8];
        frame[0] = (uint8_t)(len & 0x0F);
        memcpy(frame + 1, data, len);
        { int i; for (i = (int)len + 1; i < 8; i++) frame[i] = 0xCC; }
        return send_can(ch, resp_id, frame, 8);
    }
    /* Multi-frame: FF + wait FC + CFs */
    {
        uint8_t frame[8];
        uint32_t total = len;
        uint32_t offset = 0;
        /* First Frame */
        frame[0] = 0x10 | (uint8_t)((total >> 8) & 0x0F);
        frame[1] = (uint8_t)(total & 0xFF);
        memcpy(frame + 2, data, 6);
        offset = 6;
        BM_StatusTypeDef err = send_can(ch, resp_id, frame, 8);
        if (err != BM_ERROR_OK) return err;
        /* Wait for Flow Control */
        uint8_t fc[8];
        err = recv_can_match(ch, notify, tester_id, fc, 5000);
        if (err != BM_ERROR_OK) return err;
        if ((fc[0] >> 4) != 0x3) return BM_ERROR_BUSTIMEOUT;
        /* Send Consecutive Frames */
        int seq = 1;
        while (offset < total) {
            int chunk = (int)(total - offset);
            if (chunk > 7) chunk = 7;
            frame[0] = 0x20 | (seq & 0x0F);
            memcpy(frame + 1, data + offset, chunk);
            { int i; for (i = chunk + 1; i < 8; i++) frame[i] = 0xCC; }
            err = send_can(ch, resp_id, frame, 8);
            if (err != BM_ERROR_OK) return err;
            offset += chunk;
            seq++;
        }
    }
    return BM_ERROR_OK;
}

static void parse_args(int argc, char **argv)
{
    int i;
    for (i = 1; i < argc; i++) {
        if (strncmp(argv[i], "--channel=", 10) == 0)
            g_channel = atoi(argv[i] + 10);
        else if (strncmp(argv[i], "--reqid=", 8) == 0)
            g_reqid = (uint32_t)strtol(argv[i] + 8, NULL, 0);
        else if (strncmp(argv[i], "--respid=", 9) == 0)
            g_respid = (uint32_t)strtol(argv[i] + 9, NULL, 0);
        else if (strcmp(argv[i], "--fd") == 0)
            g_use_fd = 1;
        else if (strcmp(argv[i], "--verbose") == 0 || strcmp(argv[i], "-v") == 0)
            g_verbose = 1;
        else if (strcmp(argv[i], "--mode=bmapi") == 0)
            g_mode_raw = 0;
        else if (strcmp(argv[i], "--mode=raw") == 0)
            g_mode_raw = 1;
        else if (strcmp(argv[i], "--no-hw-isotp") == 0)
            g_no_hw_isotp = 1;
        else if (strcmp(argv[i], "--help") == 0 || strcmp(argv[i], "-h") == 0) {
            printf("UDS ECU Simulator\n");
            printf("Usage: uds_ecu_sim [options]\n");
            printf("  --channel=N       Channel number (default: 0)\n");
            printf("  --reqid=ID        Request CAN ID (default: 0x7E0)\n");
            printf("  --respid=ID       Response CAN ID (default: 0x7E8)\n");
            printf("  --fd              Enable CAN FD mode\n");
            printf("  --mode=raw        Raw CAN + manual ISOTP (default)\n");
            printf("  --mode=bmapi      Use BM_ReadIsotp/BM_WriteIsotp\n");
            printf("  --no-hw-isotp     Disable HW ISOTP in BM_WriteIsotp\n");
            printf("  --verbose         Verbose output\n");
            exit(0);
        }
    }
}

/* ---- Main ---- */

int main(int argc, char **argv)
{
    BM_StatusTypeDef error;
    int msgcount = 0;

    parse_args(argc, argv);
    printf("UDS ECU Simulator\n");
    printf("  channel=%d  reqid=0x%03X  respid=0x%03X  fd=%d\n",
           g_channel, g_reqid, g_respid, g_use_fd);
    printf("  mode=%s  hw-isotp=%s\n",
           g_mode_raw ? "raw" : "bmapi",
           g_no_hw_isotp ? "disabled" : "enabled");
    printf("Press Ctrl+C to stop.\n\n");

    error = BM_Init();
    if (error) { printf("BM_Init failed: 0x%08X\n", error); return 1; }

    /* Enumerate channels */
    BM_ChannelInfoTypeDef chinfo[32];
    int nch = 32;
    error = BM_Enumerate(chinfo, &nch);
    if (error) { printf("BM_Enumerate failed: 0x%08X\n", error); goto cleanup; }
    printf("Found %d channels.\n", nch);
    if (g_channel >= nch) {
        printf("Error: channel %d not available (max %d)\n", g_channel, nch - 1);
        goto cleanup;
    }

    /* Open channel */
    BM_ChannelHandle channel;
    BM_BitrateTypeDef bitrate;
    memset(&bitrate, 0, sizeof(bitrate));
    bitrate.nbitrate = 500;
    bitrate.dbitrate = 2000;
    bitrate.nsamplepos = 75;
    bitrate.dsamplepos = 80;

    error = BM_OpenEx(&channel, &chinfo[g_channel],
                      BM_CAN_NORMAL_MODE, BM_TRESISTOR_120,
                      &bitrate, NULL, 0);
    if (error) {
        printf("BM_OpenEx failed: 0x%08X\n", error);
        goto cleanup;
    }
    printf("Channel %d opened.\n", g_channel);

    /* Get notification handle */
    BM_NotificationHandle notification;
    error = BM_GetNotification(channel, &notification);
    if (error) {
        printf("BM_GetNotification failed: 0x%08X\n", error);
        goto cleanup;
    }

    /* ---- BMAPI ISOTP mode (diagnostic) ---- */
    if (!g_mode_raw) {
        BM_IsotpConfigTypeDef isotp_cfg;
        memset(&isotp_cfg, 0, sizeof(isotp_cfg));
        isotp_cfg.version = 0x01;
        isotp_cfg.mode = BM_ISOTP_NORMAL_ECU;
        isotp_cfg.testerTimeout.a = 1000;
        isotp_cfg.testerTimeout.b = 1000;
        isotp_cfg.testerTimeout.c = 1000;
        isotp_cfg.ecuTimeout.a = 1000;
        isotp_cfg.ecuTimeout.b = 1000;
        isotp_cfg.ecuTimeout.c = 1000;
        isotp_cfg.paddingEnabled = 1;
        isotp_cfg.paddingValue = 0xCC;
        isotp_cfg.flowcontrol.hardwareIsotpDisabled = (uint8_t)g_no_hw_isotp;
        isotp_cfg.flowcontrol.blockSize = 0;
        isotp_cfg.flowcontrol.stmin = 0;
        isotp_cfg.callbackFunc = isotp_diag_callback;
        isotp_cfg.callbackUserarg = 0;

        /* Configure tester (send) template with request ID */
        BM_INIT_CAN_FD_DATA(isotp_cfg.testerDataTemplate, g_reqid, 8, 0, g_use_fd, g_use_fd, 0, 0, NULL);
        /* Configure ECU (recv) template with response ID */
        BM_INIT_CAN_FD_DATA(isotp_cfg.ecuDataTemplate, g_respid, 8, 0, g_use_fd, g_use_fd, 0, 0, NULL);

        printf("Waiting for UDS requests (BMAPI ISOTP, hw=%s)...\n",
               g_no_hw_isotp ? "disabled" : "enabled");

        while (1) {
            /* Pre-read: drain and print queue state */
            if (g_verbose) {
                printf("  [PRE-READ] Queue contents:\n");
                int nq = drain_queue_print(channel);
                if (nq == 0) printf("    (empty)\n");
            }

            uint8_t reqbuf[4096];
            uint8_t respbuf[4096];
            uint32_t nbytes = sizeof(reqbuf);

            printf("  [READ] Calling BM_ReadIsotp (cb_count was %d)...\n", g_diag_total_cb_calls);
            fflush(stdout);
            error = BM_ReadIsotp(channel, reqbuf, &nbytes, 10000, &isotp_cfg);
            printf("  [READ] Result=0x%08X nbytes=%u cb_count=%d\n", error, nbytes, g_diag_total_cb_calls);

            if (error == BM_ERROR_BUSTIMEOUT) {
                printf("  (timeout, retrying)\n");
                continue;
            }
            if (error != BM_ERROR_OK) {
                printf("  BM_ReadIsotp error: 0x%08X\n", error);
                usleep(500000);
                continue;
            }

            msgcount++;
            printf("[#%d] RX[%u]: ", msgcount, nbytes);
            hexprint(reqbuf, (int)nbytes);
            printf("\n");

            int resplen = process_uds_request(reqbuf, (int)nbytes, respbuf);
            if (resplen > 0) {
                printf("  [WRITE] Calling BM_WriteIsotp len=%d...\n", resplen);
                fflush(stdout);
                int old_cb = g_diag_total_cb_calls;
                error = BM_WriteIsotp(channel, respbuf, (uint32_t)resplen, 10000, &isotp_cfg);
                printf("  [WRITE] Result=0x%08X cb_count=%d->%d\n",
                       error, old_cb, g_diag_total_cb_calls);

                if (error != BM_ERROR_OK) {
                    printf("  BM_WriteIsotp error: 0x%08X\n", error);
                } else if (g_verbose) {
                    printf("  TX[%d]: ", resplen);
                    hexprint(respbuf, resplen);
                    printf("\n");
                }
            }
            printf("\n");
            fflush(stdout);
        }
        BM_Close(channel);
        goto cleanup_return;
    }

    /* ---- Raw CAN mode (default, stable) ---- */
    uint8_t reqbuf[4096];
    uint8_t respbuf[4096];

    printf("Waiting for UDS requests (raw CAN ISOTP)...\n");
    while (1) {
        uint32_t nbytes = sizeof(reqbuf);
        error = recv_isotp_raw(channel, notification, g_reqid,
                               reqbuf, &nbytes, 60000);
        if (error == BM_ERROR_QRCVEMPTY)
            continue;
        if (error == BM_ERROR_BUSTIMEOUT)
            continue;
        if (error != BM_ERROR_OK) {
            printf("recv_isotp_raw error: 0x%08X\n", error);
            usleep(100000);
            continue;
        }

        msgcount++;
        printf("[#%d] ", msgcount);
        int resplen = process_uds_request(reqbuf, (int)nbytes, respbuf);

        if (resplen > 0) {
            error = send_isotp_raw(channel, notification, g_respid, g_reqid,
                                   respbuf, (uint32_t)resplen);
            if (error != BM_ERROR_OK)
                printf("  send_isotp_raw error: 0x%08X\n", error);
            else if (g_verbose)
                printf("  TX[%d]: ", resplen), hexprint(respbuf, resplen), printf("\n");
        }
        printf("\n");
        fflush(stdout);
    }

    BM_Close(channel);
cleanup_return:
cleanup:
    BM_UnInit();
    printf("UDS ECU Simulator stopped. Processed %d requests.\n", msgcount);
    return 0;
}
