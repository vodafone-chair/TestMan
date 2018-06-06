function [result] = send_stream(rxType, rxId, data)
% This function sends 'data' over a TCP Stream to the TestMan Client which is specified by (rxType, rxId).

global server;

% Send data over TCP stream
result = server.write(data, rxType, rxId);

pause(0.01);
end