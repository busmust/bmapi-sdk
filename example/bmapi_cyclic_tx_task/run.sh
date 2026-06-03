export LD_LIBRARY_PATH=.:../../bin/unix64/release:$(LD_LIBRARY_PATH)
export LD_LIBRARY_PATH=.:../../bin/unix64/debug:$(LD_LIBRARY_PATH)
./bmapi_cyclic_tx_task 0 rx 1000 &
sleep 1
./bmapi_cyclic_tx_task 1 tx 100000 1000
