import time
import struct
import numpy as np

from Server import UDP_Server

def main():
    # Init
    myType = 100
    myId = 2;
    txType = 100
    txId = 1
    
    # Initialize metric variables
    packetCount=0
    lostPacketCount=0

    # Start the testman server
    S = UDP_Server(ip="224.5.6.7", port=50000, ttl=1, type_=myType, id_=myId)
    print("Server started...(Press Ctrl+C to abort)")
    
    # Measure time duration
    now = time.time()
    last = time.time()
    
    # Receive TCP streams
    while True:
        # Poll if TCP data is available
        streamDataResult = S.stream_data_available(txType, txId)
        if streamDataResult:
            # Get TCP stream data
            data = S.get_stream_data(txType, txId)
            # Print received TCP stream data
            print("Length of data: ", len(data))
            print("data[0:9]: ", data[0:9])
            # Did we loose a packet?
            if data[0] != (packetCount+lostPacketCount):
                # Update number of lost packets
                lostPacketCount = data[0] - packetCount+1
            # Increase received packet counter
            packetCount+=1
            print("Received packets: ", packetCount)
            print("Lost packets: ", lostPacketCount)
            if packetCount%10==0 and packetCount > 1:
                now=time.time()
                print("100 MiB received in :", now-last , "seconds")
                last = now

if __name__ == '__main__':
    main()
