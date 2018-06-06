import time
import numpy as np

from Server import UDP_Server
from signalslot import Slot

# rand_buffer of size 10 MiB
rand_buffer = np.random.randint(0,255,10*pow(1024,2),dtype="uint8")

def main():
    # Init
    myType = 100
    myId = 1
    rxType = 100
    rxId = 2
    
    # Start the testman server
    S = UDP_Server(ip="224.5.6.7", port=50000, ttl=1, type_=myType, id_=myId)
    print("Server started...(Press Ctrl+C to abort)")

    # If possible use this command
    result = S.start_stream(rxType, rxId)
    # Alternatively, use this command. It tells the TCP stream receiver to start the stream.
    #result = S.send_command("starttcpserver", type=rxType,id=rxId)
    
    # This seems a good idea to give the receiver some time
    time.sleep(0.5)

    if result == True:
        print("TCP request succeeded!")
    else:
         print("TCP request failed!")

    count = 0
    lostCount = 0
    last   = time.time()

    # Send 100 rand_buffer over the TCP stream
    while count < 100:
        # Print message for every 100 MiB sent
        if count>1 and count%10==0:
            now=time.time()
            print("Count: ", count)
            print("100 MiB Sent in :",now-last , "seconds")
            last = now
            
        # Update first element of rand_buffer to contain current counter
        rand_buffer[0] = count%256;
        print("rand_buffer[0]: ", rand_buffer[0])
        
        # This is not a good idea, maybe it won't transmit the full buffer
        #S.send_command("set phy", data=rand_buffer)
        # This is intended for small data, maybe it won't transmit the whole buffer
        #S.send_data(data=rand_buffer)
        
        # Send stream will send data over a TCP stream
        result = S.send_stream(rxType, rxId, data=rand_buffer)
        if result == True:
            print("Data transfer succeeded!")
        else:
            print("Data transfer failed!")
            lostCount += 1
            print("Packets lost: ", lostCount)
        count += 1
        # This seems to be necessary
        time.sleep(0.1) 

    # Send some data
    S.send_data(status="Python closing")

    # Close the TCP stream
    S.stop_stream(rxType, rxId)

    # Stop the server
    S.stop()


if __name__ == '__main__':
    main()
