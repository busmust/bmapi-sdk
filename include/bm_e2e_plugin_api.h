#ifndef BM_E2E_PLUGIN_API_H
#define BM_E2E_PLUGIN_API_H
#include <stdint.h>
#ifdef _WIN32
#define BM_E2E_CALL __cdecl
#ifdef BM_E2E_PLUGIN_EXPORT
#define BM_E2E_EXPORT __declspec(dllexport)
#else
#define BM_E2E_EXPORT
#endif
#else
#define BM_E2E_CALL
#define BM_E2E_EXPORT __attribute__((visibility("default")))
#endif
#ifdef __cplusplus
extern "C" {
#endif
#define BM_E2E_PLUGIN_ABI_MAJOR 1U
#define BM_E2E_PLUGIN_ABI_MINOR 0U
#define BM_E2E_CAP_PROTECT 1U
#define BM_E2E_CAP_CHECK 2U

/* Native in-process C ABI, not a serialized/wire structure.
 * All pointers are borrowed for one call. Plugins must not retain them or
 * start background work. Context is a caller-owned transactional byte image;
 * it may not contain pointers/handles. Protect has no authority to send. */
typedef struct {
    uint32_t struct_size, profile, variant, task_id;
    uint32_t message_id, config_size, context_size, payload_length;
    uint64_t sequence, scheduled_us;
    const uint8_t *config;
    uint8_t *context;
    uint8_t *payload;
} BM_E2EPluginCall;
typedef int32_t(BM_E2E_CALL *BM_E2EPluginOperation)(BM_E2EPluginCall *call);
/* Optional provider-global teardown after the last host reference unloads.
 * Native crash/hang isolation is not provided. Check returns 0 for valid, a
 * positive value for invalid (context may resynchronize), negative for error. */
typedef void(BM_E2E_CALL *BM_E2EPluginDestroy)(void);
typedef struct {
    uint32_t struct_size, abi_major, abi_minor, capabilities;
    uint32_t provider_id, provider_version, reserved[2];
    BM_E2EPluginOperation init, protect, check, reset;
    BM_E2EPluginDestroy destroy;
} BM_E2EPluginApi;
/* Export exactly BM_E2E_GetPluginApi. Return nonzero on unsupported ABI/size.
 * Set only fields which fit caller_size; never throw across the C boundary. */
typedef int32_t(BM_E2E_CALL *BM_E2EGetPluginApi)(uint32_t host_major, uint32_t caller_size,
                                                 BM_E2EPluginApi *api);
BM_E2E_EXPORT int32_t BM_E2E_CALL BM_E2E_GetPluginApi(uint32_t host_major, uint32_t caller_size,
                                                      BM_E2EPluginApi *api);
#ifdef __cplusplus
}
#endif
#endif
