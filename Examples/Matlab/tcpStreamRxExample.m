function tcpStreamRxExample()
%TCPSTREAMRXEXAMPLE Example function for using TestMan as a TCP stream receiver.

%% Init
global server
cleanupObj = onCleanup(@cleanupFct);    % Just to shutdown the server on "Ctrl+C"
% TODO: Change path here!
testManPath = [pwd, '\TestManLib'];
addpath(testManPath);                   % Add path of library
dll_path = [testManPath, '\UDP-Communications.dll'];

% Init
myType = 100;       % Value in range [1,...,255]
myId = 2;           % ID to distinguish software of same type. Value in range [1,...,254]
txType = 100;       % Type of TCP transmitter
txId = 1;           % ID of TCP transmitter

%% Open server
init_udp(myType, myId, dll_path)
disp("Server started...(Press Ctrl+C to abort)")

received.packetCount = 0;
received.lostPacketCount = 0;

% Measure time duration
received.now = tic;


%% Receive data

% Receive TCP stream
while true
    % Poll if TCP data is available
    streamDataResult = stream_data_available(txType, txId);
    if streamDataResult
        % Get TCP stream data
        data = get_stream_data(txType, txId);
        % Print received TCP stream data
        fprintf("Length of data: %d\n", data.Length);
        fprintf("data[0]: %d\n", data(1));
        % Did we lose a packet?
        if data(1) ~= (received.packetCount+received.lostPacketCount)
            % Update number of lost packets
            received.lostPacketCount = data(1) - received.packetCount+1;
        end
        % Increase received packet counter
        received.packetCount = received.packetCount + 1;
        fprintf("Received packets: %d\n", received.packetCount)
        fprintf("Lost packets: %d\n", received.lostPacketCount)
        if mod(received.packetCount,10)==0 && received.packetCount > 1
            fprintf("100 MiB received in %f seconds.\n", toc(received.now))
            received.now = tic;
        end
    end
end


end

%% Stop server on "Ctrl+C"
function cleanupFct()
    stop_udp();
    disp('Server terminated.')
end