import zad1TCP
import zad1UDP
import time
import os

def test_udp():
    zad1UDP.run_udp_client("a", 50000, "127.0.0.1:50000", 0, "UDP", 12, "e", "PRZYPS")
    time.sleep(1)
    zad1UDP.run_udp_client("b", 50001, "127.0.0.1:50000", 0, "UDP", 8, "b", "22222")
    time.sleep(3)
    zad1UDP.run_udp_client("c", 50002, "127.0.0.1:50001", 1, "UDP", 8, "d", "3333333")
    time.sleep(4)
    zad1UDP.run_udp_client("d", 50003, "127.0.0.1:50001", 0, "UDP", 5, "a", "1111")

def test_tcp():
    zad1TCP.run_tcp_client("a", 50000, "127.0.0.1:50000", 0, "TCP", 12, "e", "PRZYPS")
    time.sleep(2)
    zad1TCP.run_tcp_client("b", 50001, "127.0.0.1:50000", 0, "TCP", 8, "b", "22222")
    time.sleep(3)
    zad1TCP.run_tcp_client("c", 50002, "127.0.0.1:50001", 0, "TCP", 8, "d", "3333333" )
    time.sleep(4)
    zad1TCP.run_tcp_client("d", 50003, "127.0.0.1:50002", 1, "TCP", 5, "a", "1111")

test_udp()

# os.system('python zad1UDP.py a 50000 127.0.0.1:50000 0 UDP 12 e PRZYPS')
# time.sleep(2)
# os.system('python zad1UDP.py b 50001 127.0.0.1:50000 0 UDP 8 b 22222')
# time.sleep(3)
# os.system('python zad1UDP.py c 50002 127.0.0.1:50001 1 UDP 8 d 3333333')
# time.sleep(4)
# os.system("python zad1UDP.py d 50003 127.0.0.1:50001 0 UDP 5 a 1111")