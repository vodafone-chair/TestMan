/*
   TestmanCPP - C++ Implementation of the Testman Protocol

   Licensed under GPLv3

   (C) 2018, Maximilian Matthe, maximilian.matthe@ifn.et.tu-dresden.de
*/
#include <iostream>
#include <string>

#include <boost/scope_exit.hpp>
#include <boost/algorithm/string.hpp>

#include "testman/packet.h"
#include "testman/network.h"
#include "testman/server.h"
#include "testman/tcpstream.h"
#include "testman/stopwatch.h"

using std::to_string;

namespace testman {
  namespace detail {
    void Event::notify() {
      std::lock_guard<std::mutex> lk(m);
      fired = true;
      cv.notify_one();
    }

    bool Event::wait_for(int ms) {
      std::unique_lock<std::mutex> lk(m);
      bool result = cv.wait_for(lk, std::chrono::milliseconds(ms), [this]{return fired;});
      if (result)
	fired = false;
      return result;
    }
  }

  CommandNotAcknowledged::CommandNotAcknowledged(const detail::StringMap& cmd)
    : std::runtime_error("The command was not received or executed (timestamp " + cmd.get("timestamp", "unknown") + ")") {};

  void addDestinationID(detail::StringMap& packet, Identification id) {
    packet.set("type", std::to_string(id.type));
    packet.set("id", std::to_string(id.id));
  }


  Server::Server(NetworkInterface& network, Identification requestedID, CallbackFunction callback)
    : _network(network), _currentID(255, 255), _requestedID(requestedID),
      _timeout_ms(3000), _serverState(State::CONSTRUCTED), _packetCallback(callback)
  {
    using namespace std::placeholders;

    network.registerOnReceive(std::bind(&Server::receiveRawPacket, this, _1));
  }

  Identification Server::currentID() const {
    return _currentID;
  }

  void Server::setTimeout(int ms) {
    _timeout_ms = ms;
  }

  void Server::forceIdentification() {
    _currentID = _requestedID;
    _serverState = State::RECEIVING;
  }

  void Server::setIdentification(Identification id) {
    _currentID = id;
    _serverState = State::RECEIVING;
    showHint(std::string("Setting Identification to ") + std::string(id));
  }

  void Server::showException(const std::exception& e) {
    showError(e.what());
  }

  void Server::showError(const std::string& msg) {
    std::cerr << "Server(" << this << ") ERROR: " << msg << std::endl;
    createEvent("error", msg, Identification(0,0));
  }

  void Server::showWarning(const std::string& msg) {
    std::cerr << "Server(" << this << ") WARN : " << msg << std::endl;
    createEvent("warn", msg, Identification(0,0));
  }

  void Server::showHint(const std::string& msg) {
    std::cerr << "Server(" << this << ") HINT : " << msg << std::endl;
    createEvent("hint", msg, Identification(0,0));
  }

  void Server::createEvent(const std::string& name, const std::string& content, Identification id, const RawPacket& p) {
    if (_packetCallback) {
      _packetCallback(name, content, id, p);
    }
  }

  void Server::createEvent(const std::string& name, const std::string& content, Identification id) {
    createEvent(name, content, id, RawPacket());
  }

  void Server::searchForID() {
    searchForID(_requestedID);
  }

  void Server::ensureServerState(State state) {
    if (_serverState != state)
      throw InvalidServerState();
  }

  void Server::searchForID(Identification requestedID) {
    _serverState = State::LOOKING_FOR_ID;
    Identification originalRequest(requestedID);
    for (int i = 0; i < 3; i++) {
      sendData({{"id", to_string(requestedID.id)}, {"type", to_string(requestedID.type)}});
      StopWatch sw(_timeout_ms);

      while(!sw.isElapsed()) {
	_rxPacketEvent.wait_for(_timeout_ms+5);
	if (rxPacketsIndicateIdIsUsed(requestedID)) {
	  // The id is already used by somebody else
	  // try another one and reset the trials counter
	  requestedID.id = (requestedID.id + 1) % 256;
	  // std::cout << "ID was used. Now trying " << requestedID << std::endl;
	  if (requestedID.id == originalRequest.id)
	    throw NoServerIdFound();
	  i = -1;
	  break;
	}
      }
    }
    setIdentification(requestedID);
  }

  bool Server::rxPacketsIndicateIdIsUsed(Identification requestedID) {
    while (_lastRxPackets.size() > 0) {
      auto packet = _lastRxPackets.front();
      _lastRxPackets.pop_front();

      if (packet->id == requestedID) {
	_lastRxPackets.clear();
	return true;
      }
    }
    return false;
  }

  void Server::receiveRawPacket(const RawPacket& packet) {
    try {
      switch(_serverState) {
      case State::LOOKING_FOR_ID:
	_lastRxPackets.push_back(std::make_shared<RawPacket>(packet));
	_rxPacketEvent.notify();
	break;
      case State::RECEIVING: {
	if (processIdSearchPacket(packet)) { }
	else if (processCommandPacket(packet)) { }
	else if (notifyWaitingPacket(packet)) { }
	else {
	  createEvent("new", "packet", packet.id, packet);
	}
      }
      }
    }
    catch(std::exception& e) {
      showException(e);
    }
  }

  bool Server::destinationMatchesOurs(const RawPacket& packet, bool allowZeroAll) {
    if (!packet.has("type") || !packet.has("id"))
      return false;
    Identification dest{byte(std::stoi(packet.get("type"))), byte(std::stoi(packet.get("id")))};

    if (dest == currentID())
      return true;
    if (allowZeroAll && dest == Identification(0,0))
      return true;
    return false;
  }

  bool Server::processIdSearchPacket(const RawPacket& packet) {
    if (packet.id == Identification(255,255)) {
      if(destinationMatchesOurs(packet, false)) {
	sendData({{"command", "ID not available"}});
      }
      return true;  // return indicating the packet was processed
    }
    return false;   // return indicating packet was not processed
  }


  bool Server::processCommandPacket(const RawPacket& packet) {
    using namespace boost::algorithm;

    if (!destinationMatchesOurs(packet, true))
      return false;

    if (!packet.has("timestamp"))
      return false;
    std::string command = to_lower_copy(packet.get("command", ""));

    if (command == "getlocalip") {
      return process_getlocalip(packet);
    }
    else if (command == "starttcpclient") {
      return process_starttcpclient(packet);
    }
    else if (command == "tcpreconnect") {
      return process_tcpreconnect(packet);
    }
    else if (command == "starttcpserver") {
      return process_tcpreconnect(packet);
    }
    return false;
  }

  bool Server::process_getlocalip(const RawPacket& packet) {
    sendAnswer(Answer::CMD_RECEIVED, packet.get("timestamp"));

    std::vector<std::string> ipAddresses = _network.getIpAddresses();
    if (ipAddresses.size() == 0)
      throw std::runtime_error("Could not get any IP address of the server");

    detail::StringMap cmdData{{"command", "response"}, {"answer", "getlocalip"}};
    addDestinationID(cmdData, packet.id);
    for(size_t i = 0; i < ipAddresses.size(); i++) {
      cmdData.set(std::to_string(i), ipAddresses[i] + ":2000");
    }
    sendCommand(cmdData);

    sendAnswer(Answer::CMD_EXECUTED, packet.get("timestamp"));
    return true;
  }

  bool Server::process_starttcpclient(const RawPacket& packet) {
    sendAnswer(Answer::CMD_RECEIVED, packet.get("timestamp"));

    std::shared_ptr<TCPStream> stream = createStream();
    stream->connectToServer(packet.get("ip"), std::stoi(packet.get("port")));
    _streams[packet.id] = stream;

    sendAnswer(Answer::CMD_EXECUTED, packet.get("timestamp"));
    return true;
  }

  bool Server::process_tcpreconnect(const RawPacket& packet) {
    showWarning("TCPReconnect request not implemented. Ignoring.");
    return true;
  }

  bool Server::process_starttcpserver(const RawPacket& packet) {
    showWarning("startTCPServer request not implemented. Ignoring.");
    return true;
  }

  std::shared_ptr<TCPStream> Server::createStream() {
    std::shared_ptr<TCPStream> stream = _streamFactory->createStream();
    stream->registerOnDataCallback([this] (TCPStream* s) {
	Identification idRemote(findRemoteId(s));
	createEvent("new", "data", idRemote);
      });
    return stream;
  }

  TCPStream* Server::getStream(Identification destination) {
    return _streams.at(destination).get();
  }

  void Server::stopStream(Identification destination) {
    _streams.erase(_streams.find(destination));
  }

  void Server::startStream(Identification destination) {
    std::shared_ptr<TCPStream> stream = createStream();
    int port = stream->startServer();
    _streams[destination] = stream;

    auto addresses = _network.getIpAddresses();
    if (addresses.size() == 0)
      throw std::runtime_error("Could not get any IP address of the server");
    RawPacket starttcpclient(currentID(),
			     {{"command", "starttcpclient"},
				 {"ip", addresses[0]},
				   {"port", std::to_string(port)}});
    addDestinationID(starttcpclient, destination);
    sendCommand(starttcpclient);

    stream->acceptConnection();
    _streams[destination] = stream;
  }

  Identification Server::findRemoteId(TCPStream* stream) {
    for(auto it: _streams) {
      if (it.second.get() == stream)
	return it.first;
    }
    throw std::runtime_error("ID for stream not found!");
  }


  bool Server::notifyWaitingPacket(const RawPacket& packet)
  {
    if (packet.has("timestamp") && packet.has("command")) {
      std::string timestamp = packet.get("timestamp");
      auto it = _waitingPackets.find(timestamp);
      if (it != _waitingPackets.end()) {
	std::shared_ptr<WaitingPacket> evt = it->second;
	evt->receiverID = packet.id;
	if(packet.get("command") == "received")
	  evt->cmdWasReceived = true;
	if(packet.get("command") == "executed") {
	  evt->cmdWasReceived = true;
	  evt->cmdWasExecuted = true;
	}
	if(packet.has("message"))
	  evt->message = packet.get("message");
	evt->notify();
	return true;    // return true to indicate we used the packet
      }
      else {
	return false;   // return false to indicate we did not use the packet
      }
    }
    return false;       // return false to indicate we did not use the packet
  }

  void Server::sendData(const detail::StringMap& data) {
    RawPacket p(_currentID, data);
    if (!p.has("packetnumber"))
      p.set("packetnumber", getTimestamp());
    _network.send(p);
  }

  std::string Server::addCommandTimestamp(RawPacket& p) {
    std::string timestamp = getTimestamp();
    if (!p.has("timestamp"))
      p.set("timestamp", timestamp);
    else
      timestamp = p["timestamp"];
    if (!p.has("packetnumber"))
      p.set("packetnumber", timestamp);
    return timestamp;
  }

  std::string Server::sendCommand(const detail::StringMap& data) {
    ensureServerState(State::RECEIVING);

    RawPacket p(_currentID, data);
    std::string timestamp = addCommandTimestamp(p);

    std::shared_ptr<WaitingPacket> packetEvent = std::make_shared<WaitingPacket>(timestamp);
    _waitingPackets[timestamp] = packetEvent;
    removeWaitingPacketAtScopeExit dummy(this, timestamp);

    bool receiveAck = false;
    bool cmdExecuted = false;
    for(int i = 0; i < 3; i++) {
      _network.send(p);

      // wait for receive or execute ack
      if(packetEvent->wait_for(_timeout_ms)) {
	// the packet was received at the opposite side; reply with ack.
	cmdExecuted = packetEvent->cmdWasExecuted;
	sendSimpleAck(timestamp, packetEvent->receiverID);
	receiveAck = true;
	break;
      }
    }
    if(!receiveAck)
      throw CommandNotAcknowledged(p);

    if(!cmdExecuted) {
      // Need to wait for command execution flag in addition
      if(packetEvent->wait_for(packetEvent->timeout) || packetEvent->cmdWasExecuted) {
	sendSimpleAck(timestamp, packetEvent->receiverID);
      }
      else {
	throw CommandNotAcknowledged(p);
      }
    }

    return packetEvent->message;
  }

  void Server::sendAnswer(Answer type, const std::string& timestamp, const std::string& message, int timeout) {
    std::string cmd = answerToString(type);
    detail::StringMap data({{"command", cmd}, {"timestamp", timestamp}});

    std::shared_ptr<WaitingPacket> packetEvent = std::make_shared<WaitingPacket>(timestamp);
    _waitingPackets[timestamp] = packetEvent;
    removeWaitingPacketAtScopeExit dummy(this, timestamp);

    for(int i = 0; i < 3; i++) {
      sendData(data);

      if(packetEvent->wait_for(_timeout_ms)) {
	return;
      }
    }
    // We have not returned, i.e. not received an ack
    throw CommandNotAcknowledged(data);
  }

  void Server::sendSimpleAck(const std::string& timestamp, Identification receiverID) {
    detail::StringMap data({{"timestamp", timestamp}, {"command", "ack"}});
    data.set("id", std::to_string(receiverID.id));
    data.set("type", std::to_string(receiverID.type));
    sendData(data);
  }

  void Server::setStreamFactory(std::shared_ptr<TCPStreamFactory> streamFactory) {
    _streamFactory = streamFactory;
  }


}
