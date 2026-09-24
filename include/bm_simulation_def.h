/**
 * @file bm_simulation_def.h
 * @brief BUSMUST table-driven CAN simulation schema (canonical definition).
 *
 * Records contain no host pointers, native enums, bitfields or implicit wire
 * padding. Bundle bytes are always little-endian; never memcpy native records
 * to a transport. Reserved fields must be zero. Indices are bundle-local and
 * object IDs are stable, nonzero identities. UINT32_MAX means no reference.
 */
#ifndef BM_SIMULATION_DEF_H
#define BM_SIMULATION_DEF_H

#include <stddef.h>
#include <stdint.h>

#define BM_SIM_SCHEMA_MAJOR 1U
#define BM_SIM_SCHEMA_MINOR 0U
#define BM_SIM_INVALID_INDEX UINT32_MAX
#define BM_SIM_BYTE_ORDER_LE 0x04030201U
#define BM_SIM_MAX_PAYLOAD 64U

#define BM_SIM_SECTION_MESSAGE 1U
#define BM_SIM_SECTION_PDU 2U
#define BM_SIM_SECTION_SIGNAL 3U
#define BM_SIM_SECTION_CODEC 4U
#define BM_SIM_SECTION_E2E 5U
#define BM_SIM_SECTION_TASK 6U
#define BM_SIM_SECTION_SOURCE 7U
#define BM_SIM_SECTION_STEP 8U
#define BM_SIM_SECTION_FAULT 9U
#define BM_SIM_SECTION_STRING 10U
#define BM_SIM_SECTION_BLOB 11U
#define BM_SIM_SECTION_ENUM 12U
#define BM_SIM_SECTION_VALUE 13U
#define BM_SIM_SECTION_PROVIDER 14U
#define BM_SIM_SECTION_OPTIONAL 0x80000000U

#define BM_SIM_CAN_IDE 1U
#define BM_SIM_CAN_FDF 2U
#define BM_SIM_CAN_BRS 4U
#define BM_SIM_SIGNAL_SIGNED 1U
#define BM_SIM_SIGNAL_MULTIPLEXOR 2U
#define BM_SIM_SIGNAL_MULTIPLEXED 4U
#define BM_SIM_SIGNAL_PROTECTED 8U
#define BM_SIM_INTEL 1U
#define BM_SIM_MOTOROLA 0U

#define BM_SIM_SOURCE_CONSTANT 1U
#define BM_SIM_SOURCE_STEP 2U
#define BM_SIM_SOURCE_SINE 3U
#define BM_SIM_SOURCE_SAWTOOTH 4U
#define BM_SIM_SOURCE_REPEAT 1U
#define BM_SIM_SOURCE_CLIP 2U
#define BM_SIM_SOURCE_REVERSE 4U

#define BM_SIM_FAULT_SUPPRESS 1U
#define BM_SIM_FAULT_PAYLOAD_XOR 2U
#define BM_SIM_FAULT_PAYLOAD_REPLACE 3U
#define BM_SIM_FAULT_COUNTER_FREEZE 4U
#define BM_SIM_FAULT_COUNTER_JUMP 5U
#define BM_SIM_FAULT_CHECKSUM_CORRUPT 6U
#define BM_SIM_FAULT_FRAME_DROP 7U
#define BM_SIM_FAULT_FRAME_REPEAT 8U
#define BM_SIM_FAULT_E2E_BYPASS 9U
#define BM_SIM_TRIGGER_SEQUENCE 1U
#define BM_SIM_TRIGGER_TIME 2U
#define BM_SIM_FAULT_ENABLED 1U

typedef struct {
    uint8_t magic[8]; /* "BMSIM\r\n\x1a" */
    uint16_t schema_major;
    uint16_t schema_minor;
    uint32_t byte_order;
    uint32_t feature_bits;
    uint32_t section_count;
    uint32_t section_offset;
    uint32_t total_length;
    uint32_t crc32; /* IEEE CRC-32, this field treated as zero */
    uint32_t name_offset;
    uint32_t reserved[6];
} BM_SimulationBundleHeaderTypeDef;

typedef struct {
    uint32_t type;
    uint32_t offset;
    uint32_t count;
    uint32_t stride;
} BM_SimulationSectionTypeDef;

typedef struct {
    uint32_t object_id;
    uint32_t can_id;
    uint32_t pdu_index;
    uint32_t name_offset;
    uint32_t default_cycle_us;
    uint32_t flags;
    uint32_t reserved[2];
} BM_MessageInfoTypeDef;

typedef struct {
    uint32_t object_id;
    uint32_t signal_first;
    uint32_t signal_count;
    uint32_t default_offset;
    uint32_t length;
    uint32_t e2e_index;
    uint32_t name_offset;
    uint32_t flags;
} BM_PduInfoTypeDef;

typedef struct {
    uint64_t default_raw;
    uint32_t object_id;
    uint32_t pdu_index;
    uint32_t name_offset;
    uint32_t codec_index;
    uint16_t start_bit; /* Original DBC start bit (MSB for Motorola) */
    uint16_t bit_length;
    uint8_t byte_order;
    uint8_t flags;
    uint16_t reserved;
} BM_SignalInfoTypeDef;

typedef struct {
    uint64_t factor_ieee64; /* IEEE-754 binary64 bits, control plane only */
    uint64_t offset_ieee64;
    uint64_t minimum_ieee64;
    uint64_t maximum_ieee64;
    uint32_t enum_first;
    uint32_t enum_count;
    uint32_t unit_offset;
    uint32_t mux_signal_index;
    uint32_t mux_value;
    uint32_t flags;
    uint32_t reserved[2];
} BM_SignalCodecInfoTypeDef;

typedef struct {
    uint32_t object_id;
    uint32_t provider_id;
    uint32_t profile_id;
    uint32_t variant;
    uint32_t config_offset;
    uint32_t config_size;
    uint32_t context_size;
    uint32_t owned_bits_offset; /* 64-byte payload ownership mask in blob */
} BM_E2EInfoTypeDef;

typedef struct {
    uint64_t cycle_us;
    uint64_t phase_us;
    uint64_t rounds; /* Accepted frames per run; 0 = unlimited */
    uint32_t object_id;
    uint32_t message_index;
    uint32_t e2e_index;
    uint32_t source_first;
    uint32_t source_count;
    uint32_t fault_first;
    uint32_t fault_count;
    uint32_t name_offset;
    uint32_t flags;
    uint32_t reserved;
} BM_TxTaskInfoTypeDef;

typedef struct {
    uint64_t period_us;
    uint64_t start_us;
    int64_t offset_raw;
    int64_t amplitude_raw;
    uint32_t object_id;
    uint32_t task_index;
    uint32_t signal_index;
    uint32_t kind;
    uint32_t phase_q32; /* One turn = 2^32 */
    uint32_t point_first;
    uint32_t point_count;
    uint32_t flags;
} BM_SignalSourceInfoTypeDef;

typedef struct {
    uint64_t time_us;
    uint64_t raw;
} BM_StepPointInfoTypeDef;

typedef struct {
    uint64_t raw;
    uint32_t label_offset;
    uint32_t reserved;
} BM_SignalEnumInfoTypeDef;

typedef struct {
    uint64_t raw;
    uint32_t task_index;
    uint32_t signal_index;
} BM_SignalValueInfoTypeDef;

/* Host plugin allowlist metadata. Portable firmware adapters may omit this
 * section only when no Task references its provider IDs. */
typedef struct {
    uint32_t provider_id;
    uint32_t name_offset;
    uint32_t path_offset;
    uint32_t flags;
    uint8_t sha256[32];
    uint32_t abi_major;
    uint32_t abi_minor;
    uint32_t timeout_us;
    uint32_t context_limit;
} BM_E2EProviderInfoTypeDef;

typedef struct {
    uint64_t begin;    /* Inclusive scheduled sequence/time (us) */
    uint64_t end;      /* Exclusive; UINT64_MAX = no end */
    uint64_t interval; /* 0 = one-shot at begin; else repeat */
    int64_t value;     /* Counter override/delta or checksum XOR */
    uint32_t object_id;
    uint32_t name_offset; /* Task ownership comes from Task fault span */
    uint32_t kind;
    uint32_t trigger_kind;
    uint32_t payload_offset;
    uint32_t byte_offset;
    uint32_t byte_length;
    uint32_t flags;
} BM_FaultRuleInfoTypeDef;

#define BM_SIM_ASSERT_(name, condition) typedef char name[(condition) ? 1 : -1]
BM_SIM_ASSERT_(bm_sim_header_size, sizeof(BM_SimulationBundleHeaderTypeDef) == 64);
BM_SIM_ASSERT_(bm_sim_section_size, sizeof(BM_SimulationSectionTypeDef) == 16);
BM_SIM_ASSERT_(bm_sim_message_size, sizeof(BM_MessageInfoTypeDef) == 32);
BM_SIM_ASSERT_(bm_sim_pdu_size, sizeof(BM_PduInfoTypeDef) == 32);
BM_SIM_ASSERT_(bm_sim_signal_size, sizeof(BM_SignalInfoTypeDef) == 32);
BM_SIM_ASSERT_(bm_sim_codec_size, sizeof(BM_SignalCodecInfoTypeDef) == 64);
BM_SIM_ASSERT_(bm_sim_e2e_size, sizeof(BM_E2EInfoTypeDef) == 32);
BM_SIM_ASSERT_(bm_sim_task_size, sizeof(BM_TxTaskInfoTypeDef) == 64);
BM_SIM_ASSERT_(bm_sim_source_size, sizeof(BM_SignalSourceInfoTypeDef) == 64);
BM_SIM_ASSERT_(bm_sim_step_size, sizeof(BM_StepPointInfoTypeDef) == 16);
BM_SIM_ASSERT_(bm_sim_enum_size, sizeof(BM_SignalEnumInfoTypeDef) == 16);
BM_SIM_ASSERT_(bm_sim_value_size, sizeof(BM_SignalValueInfoTypeDef) == 16);
BM_SIM_ASSERT_(bm_sim_provider_size, sizeof(BM_E2EProviderInfoTypeDef) == 64);
BM_SIM_ASSERT_(bm_sim_fault_size, sizeof(BM_FaultRuleInfoTypeDef) == 64);
BM_SIM_ASSERT_(bm_sim_signal_layout, offsetof(BM_SignalInfoTypeDef, start_bit) == 24);
BM_SIM_ASSERT_(bm_sim_task_layout, offsetof(BM_TxTaskInfoTypeDef, object_id) == 24);
BM_SIM_ASSERT_(bm_sim_source_layout, offsetof(BM_SignalSourceInfoTypeDef, object_id) == 32);
BM_SIM_ASSERT_(bm_sim_fault_layout, offsetof(BM_FaultRuleInfoTypeDef, object_id) == 32);
BM_SIM_ASSERT_(bm_sim_crc_layout, offsetof(BM_SimulationBundleHeaderTypeDef, crc32) == 32);
#undef BM_SIM_ASSERT_

#endif
