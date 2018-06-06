function [tcpStreamData] = get_stream_data(rxType, rxId)
% This function returns 'tcpStreamData' from the TestMan Client which is specified
% by (rxType, rxId) which has arrived over a TCP stream.

global server;

% New TCP stream data available?
[tcpStreamData] = server.getStreamData(rxType, rxId);

pause(0.01);
end