CC ?= gcc
CFLAGS ?= -std=c99 -Wall -Wextra -O2
SDK := ../..
ifeq ($(OS),Windows_NT)
  LIBDIR ?= $(SDK)/lib/mingw32
  LIBNAME ?= bmapi
  EXE := .exe
  LDLIBS := -l$(LIBNAME)
else
  BITS := $(shell getconf LONG_BIT)
  MACHINE := $(shell uname -m)
  ifneq ($(filter arm% aarch64,$(MACHINE)),)
    PLATFORM := $(if $(filter 64,$(BITS)),raspbian64,linaro)
  else ifneq ($(filter i386 i486 i586 i686 x86_64 amd64,$(MACHINE)),)
    PLATFORM := unix$(BITS)
  else
    $(error Unsupported machine: $(MACHINE))
  endif
  LIBDIR ?= $(SDK)/bin/$(PLATFORM)/release
  LIBNAME := $(if $(filter 64,$(BITS)),bmapi64,bmapi)
  LDLIBS := -l$(LIBNAME)
endif

all: bmapi_asc_record$(EXE)
bmapi_asc_record$(EXE): bmapi_asc_record.c bmapi_asc_common.h
	$(CC) $(CFLAGS) -I"$(SDK)/include" bmapi_asc_record.c -L"$(LIBDIR)" $(LDLIBS) -o $@
clean:
	rm -f bmapi_asc_record bmapi_asc_record.exe
.PHONY: all clean
