function [result] = send_data(varargin)

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
result = server.send_data(data);

end