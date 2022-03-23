#!/usr/bin/env python3
from sys import exit
import time
import numpy as np
from strictfire import StrictFire

import helpers
import socket_handler

def run(HOST='127.0.0.1', PORT=25001, time_step=0.1):
    np.set_printoptions(precision=2, suppress=True)

    s = socket_handler.init_socket(HOST, PORT)

    def handler(signal_received, frame):
        # Handle any cleanup here
        print('SIGINT or CTRL-C detected. Exiting gracefully')
        socket_handler.stop(s)
        exit(0)
    # signal(SIGINT, handler)

    while True:
        pub = {'clock': 0, 'vel_cmd': (0, 0, 0), 'sim_control': 'i'}
        # send a few packet to be sure it is launched
        for _ in range(5):
            to_send = helpers.publish_all(pub)
            raw = socket_handler.send_and_receive(s, to_send)
            pub = helpers.do_step(time_step, pub)
        while True:
            # making the raw string to send from the dict
            to_send = helpers.publish_all(pub)
            # sending and receiving raw data
            raw = socket_handler.send_and_receive(s, to_send)
            # getting dict from raw data
            dico = helpers.raw_data_to_dict(raw)
            if False:
                print(dico)
            speed, rot = (1, 0)
            pub['vel_cmd'] = (speed, 0, np.rad2deg(rot))
            # doing a step
            pub = helpers.do_step(time_step, pub)
    socket_handler.stop(s)
    time.sleep(1)


if __name__ == "__main__":
    StrictFire(run)
