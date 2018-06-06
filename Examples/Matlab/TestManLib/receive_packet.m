function [packet] = receive_packet()
    %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    %get the Handle to the Server%
    %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    global server
    
    wait_for_packet = 1;      
    show_server_messages = 1;
    
    
    %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    %get the Handle to the Server%
    %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%    
    while(wait_for_packet == 1) 
       %Ask DLL if data available
       if (server.data_available())
           [packet] = get_packet();
           %Handle server messages if requested
           if ((show_server_messages == 1) && (packet.isServerMessage == 1))
               %Notification about new TCP stream data
               if (strcmpi(packet.Content{2},'new_data') == 1)                   
                   %Get it
                   [packet.Data, double] = receive_data();
                   if (isempty(packet.Data))
                       continue;
                   end
                   if (double == 0)
                        packet.BinaryData = 1;
                   else
                        packet.DoubleData = 1;
                   end
                   break;
               end
               %Show server message
               disp(char(packet.Content));
           else               
               break;
           end
       end
       pause(0.1);
    end
    
end

function [packet] = get_packet()

    global server;

    %Copy the whole packet as a struct, will thrown an unintresting warning
    warning('off','all');
    packet = struct(server.getPacket());
    warning('on','all');
    %individually convert selected DataTypes (string and string array)
    packet.Type = char(packet.Type);
    packet.Command = char(packet.Command);
    packet.Timestamp = char(packet.Timestamp);
    packet.Content = cell(packet.StringArray);
    packet.BinaryData = 0;
    packet.DoubleData = 0;
    packet.Data = [];

end

function [data, double] = receive_data() 

    global server;
    data = [];
    double = -1;
    %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    %Daten über einen Stream erhalten               %
    %A application streamed some data to us over TCP%
    %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    try
        while(1)
            %Data already received?
            [available, Type, ID] = server.stream_data_available();
            if (available)                
                bytes = server.getStreamData(Type, ID);
                %Try to convert into double array if not you get the bytes
                if isempty(bytes) 
                else           
                    try 
                        data = server.GetDoubles(bytes);
                        double = 1;
                    catch err
                        disp(err.message);
                        data = uint8(bytes);
                        double = 0;
                    end
                end 
                break;
            end
            pause(0.1);
        end
    catch err
       disp(['Could not get double array! ' err.message]); 
    end

end