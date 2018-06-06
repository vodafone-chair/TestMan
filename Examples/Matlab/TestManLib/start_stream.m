function [result] = start_stream(rxType, rxId)
% This function starts a TCP Stream to the TestMan Client which is specified by (rxType, rxId).

global server;

% Start TCP stream
result = server.startStream(rxType, rxId);

pause(0.01);
end