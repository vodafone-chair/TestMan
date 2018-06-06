/*
   TestmanCPP - C++ Implementation of the Testman Protocol

   Licensed under GPLv3

   (C) 2018, Maximilian Matthe, maximilian.matthe@ifn.et.tu-dresden.de
*/
#include <chrono>
#include <thread>
#include <string>
#include <iostream>


#include <testman/network.h>
#include <testman/server.h>
#include <testman/packet.h>
#include <testman/tcpstream.h>
#include <testman/stopwatch.h>

using namespace std::chrono_literals;

testman::Server* g_server;

/*
This is the callback function for the server. The callback function is called by Testman, whenever some event happens.
Parameters:
  name:
    "hint", "warn", "error": denotes some information from the server class. Variable content contains the message.
    "new" : denotes that new data has arrived (either a packet or TCP streaming data).
  content:
    Denotes the type of the new data:
    "packet" : a new packet has arrived, which is given as the last parameter
    "data"   : new TCP stream data is available. Get the data from the server class with an extra call.
  id: the (type, id) struct describing the origin of the message (i.e. the remote sender of the data)
  packet : the received packet in case of a packet
 */
void callback(const std::string& name, const std::string& content, testman::Identification id, const testman::RawPacket& packet) {
  try {
    if (name == "hint" || name == "warn" || name == "error") {
      // Information from the server; just display it to the console.
      std::cerr << "   " << name << ": " << content << std::endl;
    }
    else if(name == "new") {
      // New data has arrived (either packet or streaming)
      if (content == "data") {
	// it's TCP streaming data. For example, use "Send File" from
	// the TestSuite to send some streaming data which will be
	// dumped to console here. Make sure, that a stream has been
	// setup from either the TestSuite or from this program.
	std::cout << "New data from id " << id << std::endl;
	std::cout << "Received data:" << std::endl;

	// Obtain the stream and get the data. After the call to getData(), the data is removed from the stream.
	// i.e. call getData() only once for each received data
	std::vector<testman::byte> data = g_server->getStream(id)->getData();
	for(char c: data)
	  std::cout << c;
	std::cout << std::endl;
      }
      if (content == "packet") {
	// a new packet has arrived
	std::cout << "    New packet: " << std::endl;
	std::cout << packet << std::endl;  // dump the packet to console

	if(packet.has("command") && packet.has("timestamp")) {
	  // if the packet is a command, we check if it's for us (i.e. if the "type" and "id" fields match our id)
	  if (packet.get("type", "") == std::to_string(g_server->currentID().type) &&
	      packet.get("id", "") == std::to_string(g_server->currentID().id)) {
	    // the command is for us! Just confirm with two answers (one for command received, one for command executed)
	    std::cout << "Sending answer to command..." << packet.get("command") << std::endl;
	    g_server->sendAnswer(testman::Answer::CMD_RECEIVED,
				 packet.get("timestamp"));
	    std::this_thread::sleep_for(500ms);
	    // add an answer message, which will be the result of the sendCommand call at the remote
	    // (using the return message is optional)
	    g_server->sendAnswer(testman::Answer::CMD_EXECUTED,
				 packet.get("timestamp"),
				 "This is the answer message");
	  }
	}
      }
    }
    else {
      // unexpected
      std::cout << "Unknown event: " << name << ", " << content << std::endl;
    }
  }
  catch(std::exception& e) {
    // just catch any error here before it propagates back into the server
    std::cout << "Error: " << e.what() << std::endl;
  }
}

void example_sendCommand();
void example_sendData();
void example_openStream();
void example_sendStreamData();


/*
The main function creates the UDP server and establishes a connection to the Testman network.
Then, it enters an interactive loop to try out different commands.
 */
int main(int argc, char *argv[])
{
  // 1. create the UDP network interface, given multicast ip, port and TTL
  std::shared_ptr<testman::NetworkInterface> network = testman::createUdpInterface("224.5.6.7", 50000, 1);

  // 2. Let the network create a TCPStream factory, which will be used by the Server
  std::shared_ptr<testman::TCPStreamFactory> streamFactory = network->createStreamFactory();

  // 3. (optional): Print local IP addresses of the server
  std::cout << "IP addresses" << std::endl;
  auto a = network->getIpAddresses();
  for(auto x: a)
    std::cout << x << std::endl;
  if (a.size() == 0) {
    std::cout << "Could not get IP address!" << std::endl;
    return 1;
  }

  // 4. start the network receive loop. This spawns an extra thread which runs in the background to receive
  // UDP packets.
  std::cout << "Starting network..." << std::endl;
  network->startReceiveLoop();

  // 5. Start the Server with the desired ID (the actually used ID might be different if the ID is already in use)
  //    Also, supply the pointer to the callback function.
  testman::Server server(*network, testman::Identification(255, 1), callback);

  // 6. Let the server know the TCPStream factory, such that it can spawn TCPStreams
  server.setStreamFactory(streamFactory);

  g_server = &server;  // store server object globally to be accessed from outside of main

  // 7. Perform an ID search to identify the server in the network.
  std::cout << "Searching for ID..." << std::endl;
  server.searchForID();

  // 8. Finished. The Server is now fully setup and ready to be used.

  std::cout << "Current ID: " << server.currentID() << std::endl;
  if (server.currentID() != testman::Identification(255,2)) {
    std::cerr << "Error: Did not get the expected (255,2) identification. Is the Testsuite running with id (255,1)?" << std::endl;
    return 1;
  }


  /// Prepare the interactive loop
  // jump-table mapping commands to the example functions
  std::string line;
  std::map<std::string, std::function<void()> > examples;
  examples["command"] = example_sendCommand;
  examples["data"] = example_sendData;
  examples["openstream"] = example_openStream;
  examples["streamdata"] = example_sendStreamData;

  std::cout << "Initialization done. Entering interactive loop" << std::endl;

  while(true) {
    std::cout << "Please choose one of\n";
    for(auto& kv: examples)
      std::cout << "   " << kv.first << std::endl;
    std::cout << "Your choice? (exit with Ctrl-C) ";
    std::getline(std::cin, line);
    auto it = examples.find(line);
    if (it != examples.end()) {
      try {
	it->second();  // call the example function
      }
      catch(std::exception& e) {
	std::cout << "Error: " << e.what() << std::endl;
      }
    }
    else
      std::cout << "!!! " << line << " is not a valid command!" << std::endl;
    std::cout << "===================================" << std::endl;
  }

  return 0;
}

/*
sendCommand example: A command packet is a packet where the server
requests an acknowledgement that the packet was received at the remote
server. If no ack is received, the command is sent repeatedly. The
sendCommand functions returns only, when the ack has been
received. Also, it returns the (optional) return message for the
command.

A command packet must have a field "command" and might have "type" and
"id" as the destination address.
 */
void example_sendCommand() {
  std::cout << "Example: sendCommand" << std::endl;
  std::string response = g_server->sendCommand({{"command", "hi"}, {"type", "255"}, {"id", "1"}});
  std::cout << "Command response: " << response << std::endl;;
}

/*
sendData example: Simply send a single packet, without waiting for ack
or repetition. The sendData command does not allow return messages.
 */
void example_sendData() {
  std::cout << "Example: sendData" << std::endl;
  g_server->sendData({{"key1", "value1"}, {"key2", "value2"}});
}

/*
Open a TCP stream to a remote server. With this stream, data can be streamed both from and to the remote.
 */
void example_openStream() {
  std::cout << "Example: openStream" << std::endl;
  g_server->startStream({255, 1}); // start stream to id(255, 1)
}

/*
Stream some example data to the remote. In the TestSuite program, a file-save dialog will pop up to save the received bytes.
 */
void example_sendStreamData() {
  std::cout << "Example: sendStreamData" << std::endl;
  std::cout << "Going to send over some data" << std::endl;
  std::string content = "this is the tex... enaditure naiud enraiune arduit enarui eaui";
  g_server->getStream({ 255, 1 })->sendData({ content.begin(), content.end() });
}
