CC = g++
CFLAGS := -Wall -O3
LBITS := $(shell getconf LONG_BIT)
ifeq ($(LBITS),64)
	INCLUDE = ../../bin/unix64/release
	LIB = bmapi64
else
	INCLUDE = ../../bin/unix32/release
	LIB = bmapi
endif
ARCH := $(shell getconf LONG_BIT)

all: example

clean:
	rm -f bmapi_clearconfig.o bmapi_clearconfig

example:
	$(CC) $(CFLAGS) bmapi_clearconfig.cpp -I../../include -L$(INCLUDE) -L. -l$(LIB) -o bmapi_clearconfig

