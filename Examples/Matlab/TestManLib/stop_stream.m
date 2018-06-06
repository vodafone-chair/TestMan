function stop_stream(rxType, rxId)
% This function stops the TCP Stream to the TestMan Client which is specified by (rxType, rxId).

global server;

% Stop TCP stream
server.stopStream(rxType, rxId);

pause(0.01);
end