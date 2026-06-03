CC = gcc
CFLAGS := -Wall -O2 -DWIN32_LEAN_AND_MEAN -D__USE_MINGW_ANSI_STDIO

all: uds_ecu_sim.exe

uds_ecu_sim.exe: uds_ecu_sim.c
	$(CC) $(CFLAGS) uds_ecu_sim.c -I../../include -L../../lib/win32 -lBMAPI -lsetupapi -o uds_ecu_sim.exe

clean:
	rm -f uds_ecu_sim.exe uds_ecu_sim.o

run: uds_ecu_sim.exe
	PATH=../../bin/win32:$(PATH) ./uds_ecu_sim.exe --channel=0 --verbose
