function init_udp(type, id, dll_path)

    %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    %This variable combination is like a unique ip adresse assigned to an %
    %application                                                          %
    %The communication class checks that and assigns a new ID if necessary%
    %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

    
    
    %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    %Load the DLL into MATLAB     %
    %Enter a LOCAL full path only!%
    %IT IS NOT POSSIBLE TO UN-    %
    %OR RELOAD THE DLL!           %
    %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    disp(['Load DLL from ' dll_path]);    
    NET.addAssembly(dll_path);

    %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    %get the Handle to the Server%
    %and make it global          %
    %IT IS NOT POSSIBLE TO STOP  %
    %THE CALLBACK! (i dont know  %
    %how to do it)               %
    %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    global server 
    
    try
        if (server.udp_active() == 1)
           server.clear_packet_list();
           disp('Server already running... Abort!'); 
           return
        end
    catch
        
    end
    
    server = UDP_Communications.UDP_Server();   
    
    %% Some advanced initialization options
    % Debugging output is print to command line
    server.debug = 0;
    % Filter for a certain IP address range
    server.adapter_filter(false);

    %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    %Init the Communication                                                                         %
    %string ipadress, int port, int ttl, byte type, byte id, [bool wait = false, string filter = ""]%
    %wait = true: Execution is blocked until the initialization is finished.                        %
    %wait = false: The following (Matlab) commands are executed, but before using the server the    %
    %return value of server.initComplete() should be checked.                                       %
    %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    server.init_udp('224.5.6.7', 50000, 1, type, id, true);
    
    % Give Matlab some more time
    pause(0.1);
end