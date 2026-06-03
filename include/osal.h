
/**
 * @file        osal.h
 * @brief       OS Abstraction Layer (OSAL) for Busmust SDK
 *              OS Abstraction Layer: Provides cross-platform (Windows/Linux) system interface encapsulation for Busmust SDK,
 *              including threads, synchronization, Socket, file operations, etc. It shields API differences across different operating systems
 *              to ensure cross-platform consistency of the SDK.
 * @author      Busmust Tech Co.,Ltd
 * @version     1.13.0.35
 * @copyright   Copyright (c) 2024 Busmust Tech Co.,Ltd. All rights reserved.
 *              Licensed under the Busmust SDK License Agreement.
 *              See LICENSE file in the SDK root directory for more information.
 */
#pragma once
#ifndef __BM_OSAL_H__
#define __BM_OSAL_H__
#define OSAL_ENABLE_SOCKET 1
#define OSAL_ENABLE_FILE 1

#if OSAL_ENABLE_SOCKET
#ifdef _MSC_VER
#define _WINSOCK_DEPRECATED_NO_WARNINGS
#include <winsock2.h>
#include <WS2tcpip.h>
#pragma comment(lib,"ws2_32.lib")
#else
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <errno.h>
#endif
#endif

#if OSAL_ENABLE_FILE
#ifdef _MSC_VER
#include <direct.h>
#else
#include <sys/stat.h>
#include <sys/types.h>
#endif
#endif

#if defined(__GNUC__) && !defined(_MSC_VER)
#include <pthread.h>
#include <unistd.h>
#include <sys/eventfd.h>
#include <poll.h>
#define CRITICAL_SECTION_IS_SPIN_LOCK
#define OFFSETOF(TYPE, MEMBER) ((size_t)&((TYPE*)0)->MEMBER)
#define ARRAYSIZE(a) (sizeof(a)/sizeof(a[0]))
#ifdef CRITICAL_SECTION_IS_SPIN_LOCK
#define CRITICAL_SECTION pthread_spinlock_t
#define InitializeCriticalSection(cs) pthread_spin_init(cs, PTHREAD_PROCESS_PRIVATE)
#define EnterCriticalSection(cs) pthread_spin_lock(cs)
#define LeaveCriticalSection(cs) pthread_spin_unlock(cs)
#else
#define CRITICAL_SECTION pthread_mutex_t
#define InitializeCriticalSection(cs) { pthread_mutexattr_t attr; pthread_mutexattr_init(&attr); pthread_mutexattr_settype(&attr, PTHREAD_MUTEX_RECURSIVE); pthread_mutex_init(cs, &attr); }
#define EnterCriticalSection(cs) pthread_mutex_lock(cs)
#define LeaveCriticalSection(cs) pthread_mutex_unlock(cs)
#endif
#define CreateEvent(attr, bManualReset, bInitialState, name) eventfd(((bInitialState) ? 1 : 0), 0)
#define SetEvent(evt) { uint64_t count = 1; size_t result=write(evt, &count, sizeof(count)); result = result; }
#define CloseEvent(evt) { close(evt); evt = 0; }
#define CloseSocket(socket) { close(socket); socket = INVALID_SOCKET; }
#define GetThread() pthread_self()
#define GetThreadId() GetThread()
#define StartThread(thread, func, arg) { pthread_create(&(thread), NULL, (func), (arg)); }
#define CloseThread(thread) { void* exitcode; pthread_join((pthread_t)(thread), &exitcode); (thread) = 0; }
typedef void* ThreadExitCode;
#define ExitCode_OK  ((ThreadExitCode)0)
#define Sleep(ms) usleep((ms)*1000)
#define NOTIFICATION_HANDLE int
#define INVALID_NOTIFICATION_HANDLE -1
#define THREAD_HANDLE pthread_t
#define INVALID_THREAD_HANDLE 
#define THREAD_ID THREAD_HANDLE
#define FALSE 0
#define SOCKET int 
#define INVALID_SOCKET -1
#define SOCKET_ERROR -1
#define __can_printf printf
#define GetTickCount() __bm_get_tick()
#define GetLastError() errno
//#include <libusb-1.0/libusb.h>
#elif defined(_MSC_VER)
#pragma warning(disable:4200)
//#include "libusb/libusb.h"
#define NOTIFICATION_HANDLE HANDLE
#define INVALID_NOTIFICATION_HANDLE NULL
#define THREAD_HANDLE HANDLE
#define INVALID_THREAD_HANDLE NULL
#define THREAD_ID DWORD
typedef DWORD ThreadExitCode;
#define ExitCode_OK 0
#define GetThread() GetCurrentThread()
#define GetThreadId() GetCurrentThreadId()
#define StartThread(thread, func, arg) { thread = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)(func), (arg), 0, NULL); SetThreadPriority(thread, THREAD_PRIORITY_TIME_CRITICAL); }
#define CloseEvent(evt) { CloseHandle(evt); evt = NULL; }
#define CloseSocket(socket) { closesocket(socket); socket = INVALID_SOCKET; }
#define CloseThread(thread) { CloseHandle(thread); thread = NULL; }
#ifndef ARRAYSIZE
#define ARRAYSIZE(a) (sizeof(a)/sizeof((a)[0]))
#endif
#ifndef OFFSETOF
#define OFFSETOF(TYPE, MEMBER) ((size_t)&((TYPE*)0)->MEMBER)
#endif
#endif
#define INVALID_TEF_LISTENER_SEQID (0xFFU)

#include <assert.h>
#include <stdio.h>
#include "bmapi.h"
#ifdef _MSC_VER
#define CREATE_HANDLES(handle) handle
#else 
#define CREATE_HANDLES(handle) &handle
#endif
#define WaitForNotification(handle, timeout) { BM_NotificationHandle handles = CREATE_HANDLES(handle); if (BM_WaitForNotifications(&handles, 1, (timeout)) != 0) { printf("ERROR: WaitForNotification(" #handle ") timeout.\n"); assert(0); } }
#endif
/* END OF FILE */
