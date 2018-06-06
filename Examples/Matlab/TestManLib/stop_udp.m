function stop_udp() 
    global server

    server.stop_udp();
    server = [];
end