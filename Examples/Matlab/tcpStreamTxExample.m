function tcpStreamTxExample()
%TCPSTREAMTXEXAMPLE Example function for using TestMan as a TCP stream transmitter.

%% Init
global server
cleanupObj = onCleanup(@cleanupFct);    % Just to shutdown the server on "Ctrl+C"
% TODO: Change path here!
testManPath = [pwd, '\TestManLib'];
addpath(testManPath);                   % Add path of library
dll_path = [testManPath, '\UDP-Communications.dll'];

% Init
myType = 100;       % Value in range [1,...,255]
myId = 1;           % ID to distinguish software of same type. Value in range [1,...,254]
rxType = 100;       % Type of TCP transmitter
rxId = 2;           % ID of TCP transmitter

% Create random data of size 10 MiB
rand_buffer = uint8(randi([0 255], 1, 10*1024^2));

%% Open server
init_udp(myType, myId, dll_path)
disp("Server started...(Press Ctrl+C to abort)")

%% Transmit data
% If possible use this command
result = start_stream(rxType, rxId);
% Alternatively, use this command. It tells the TCP stream receiver to start the stream.
%result = send_command("starttcpserver", rxType, rxId);

% This seems a good idea to give the receiver some time
pause(2)

if result == true
    disp("TCP request succeeded!")
else
    disp("TCP request failed!")
end

transmit.count = 0;
transmit.lost = 0;
lastTx  = tic;

% Send 100 rand_buffer over the TCP stream
while transmit.count < 100
    % Print message for every 100 MiB sent
    if transmit.count>1 && mod(transmit.count,10)==0
        fprintf("transmit.count: %d\n", transmit.count);
        fprintf("100 MiB Sent in %f seconds\n", toc(lastTx));
        lastTx = tic;
    end

    % Update first element of rand_buffer to contain current counter
    rand_buffer(1) = mod(transmit.count,256);
    fprintf("rand_buffer[0]: %d\n", rand_buffer(1));

    % Send stream will send data over a TCP stream
    result = send_stream(rxType, rxId, rand_buffer);
    if result == true
        disp("Data transfer succeeded!")
    else
        disp("Data transfer failed!")
        transmit.lost = transmit.lost + 1
    end
    transmit.count = transmit.count + 1;
    % This seems to be necessary
    pause(0.1)
    
end

% Send status (just for fun)
send_data('status', 'Python closing');

% Close the TCP stream
stop_stream(rxType, rxId)

% Stop the server
stop_udp();
disp("Server terminated.");


end

%% Stop server on "Ctrl+C"
function cleanupFct()
    stop_udp();
    disp('Server terminated.')
end