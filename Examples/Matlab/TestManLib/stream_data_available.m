function [result] = stream_data_available(rxType, rxId)
% This function checks if 'data' from the TestMan Client which is specified
% by (rxType, rxId) has arrived over a TCP stream.

global server;

% New TCP stream data available?
[result, dataRxType, dataRxId] = server.StreamDataAvailable();

if result
    % Compare Type and Id
    if rxType~=dataRxType || rxId~=dataRxId
        result = false;
    end
end

pause(0.01);
end