#!/usr/bin/python

AVERAGE_LATENCY = (33, 66, 100, 200, 500, 1000, 2000)
LATENCY_JITTER = (10, 20, 30, 50)
PACKET_LOSS_RATE = (0, 5, 10, 15, 20, 25)

for al in AVERAGE_LATENCY:
    for lj in LATENCY_JITTER:
        for plr in PACKET_LOSS_RATE:
            print("{}ms / {}% / {}%".format(al, lj, plr))