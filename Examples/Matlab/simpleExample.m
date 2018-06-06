function simpleExample()
%SIMPLEEXAMPLE Simple TestMan example function.

%% Init
global server
cleanupObj = onCleanup(@cleanupFct);    % Just to shutdown the server on "Ctrl+C"
% TODO: Change path here!
testManPath = [pwd, '\TestManLib'];
addpath(testManPath);                   % Add path of library
dll_path = [testManPath, '\UDP-Communications.dll'];

%% What operating mode?
disp('Do you want to run receiver "rx" or transmitter "tx"?');
opMode = input('>', 's');

if opMode == 'tx'           % ### Transmitter ###
    
    %% Init
    softwareType = 2;       % Value in range [1,...,255]
    softwareId = 1;         % ID to distinguish software of same type. Value in range [1,...,254]

    %% Open TestMan server
    init_udp(softwareType, softwareId, dll_path)

    %% Send commands
    [result, return_message] = send_command('command', 'set' , 'value', '5')

    %% Send data
    % Define data
    key = 'myKey';
    value = 42;
    % Send data
    send_data(key, value);
    
    % Different syntax, multiple key-value pairs
    send_data(key, value, 'myKey2', 88);
    send_data('myVec', [1:10])

elseif opMode == 'rx'       % ### Receiver  ###
    
    %% Init
    softwareType = 2;       % Value in range [1,...,255]
    softwareId = 2;         % ID to distinguish software of same type. Value in range [1,...,254]

    %% Open server
    init_udp(softwareType, softwareId, dll_path)
    
    %% Infinite loop
    while(1)
        %% Poll TestMan server if data is available
        while(server.DataAvailable())

            % ### Receive packets ###
            packet = receive_packet();
            % 'packet' has many useful fields to identify it'S type:
            % E.g. sender type and id; Allows to search for data from
            % specific senders.
            senderType = packet.SenderType
            senderId = packet.SenderID
            % 'content' is a cell array with key-value pairs
            content = packet.Content
            % 'data' would contain double data e.g. from a TCP stream.
            % For non-TCP stream data it is empty.
            data = double(packet.Data)

        end
    end
else    % Unknown operating mode, wrong input string.
    disp('Error: Operating mode unknown!');
end



end

% Stop server on "Ctrl+C"
function cleanupFct()
    stop_udp();
    disp('Server terminated.')
end