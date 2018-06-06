function [result, return_message] = send_command(varargin)

if (mod(nargin, 2) == 1)
    error('The number of arguments has to be even! Abort!')
end

%Convert cell array to .NET string
data = NET.createArray('System.String',nargin);

for i=0:nargin - 1
    if (isnumeric(varargin{i+1})) 
        varargin{i+1} = num2str(varargin{i+1});
    end    
    data.Set(i, varargin{i+1});    
end

global server;
[result, return_message] = server.send_command(data);
if (result == 0) 
    warning('Command not successfully send.');
end

pause(0.01);
end