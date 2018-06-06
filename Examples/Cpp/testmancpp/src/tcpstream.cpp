/*
   TestmanCPP - C++ Implementation of the Testman Protocol

   Licensed under GPLv3

   (C) 2018, Maximilian Matthe, maximilian.matthe@ifn.et.tu-dresden.de
*/
#include <iostream>
#include <cassert>
#include <thread>
#include <chrono>
#include <string>
#include <algorithm>

#include "testman/stopwatch.h"
#include "testman/server.h"
#include "testman/tcpstream.h"

using namespace std::literals::chrono_literals;

namespace testman {
  TCPStreamImpl::~TCPStreamImpl() {
    try {
      stop();
    }
    catch(std::exception& e) {
    }
  }

  void TCPStreamImpl::registerOnDataCallback(std::function<void(TCPStream*)> callback) {
    _onDataCallback = callback;
  }

  void TCPStreamImpl::connectToServer(const std::string& ip, int port) {
    ensureSocket();

    _socket->connect(ip, port);
    startControlLoop();
  }

  int TCPStreamImpl::startServer() {
    ensureSocket();

    int port = _socket->listen();
	log("Listening on Port " + std::to_string(port));
	return port;
  }

  void TCPStreamImpl::acceptConnection() {
	  log("Waiting for client to connect");
    _socket->accept();

    write_message("check");
    if(read_message() != "check")
      throw std::runtime_error("Client is not responding to the check message!");
    startControlLoop();
  }

  void TCPStreamImpl::startControlLoop() {
    _shouldStop = false;
    _stopped = false;
    std::thread T(std::bind(&TCPStreamImpl::controlLoop, this));
    T.detach();
  }

  void TCPStreamImpl::log(const std::string& msg) {
    if (_doLogging) {
      std::cout << "stream: " << msg << std::endl;
    }
  }

  void TCPStreamImpl::enableLogging(bool enable) {
    _doLogging = enable;
  }

  std::string TCPStreamImpl::read_message() {

    StopWatch timeout(100);
    while (_socket->available() < PACKETSIZE) {
      std::this_thread::sleep_for(10ms);
      if (timeout.isElapsed())
	return "";
    }

    std::vector<byte> packet = _socket->receive(PACKETSIZE);
    std::string msg(packet.begin(), std::find(packet.begin(), packet.end(), 0));
    log(std::to_string((int)this) + " <--- " + msg);
    return msg;
  }

  void TCPStreamImpl::write_message(const std::string& msg) {
    log(std::to_string((int)this) + " ---> " + msg);
    std::vector<testman::byte> packet(PACKETSIZE);
    std::fill(packet.begin(), packet.end(), 0);
    std::copy(msg.begin(), msg.end(), packet.begin());
    _socket->send(packet);
  }

  void TCPStreamImpl::stop() {
      if(_stopped)
          return;
    _shouldStop = true;

    StopWatch timeout(1000);
    while(!_stopped) {
      std::this_thread::sleep_for(10ms);
      if (timeout.isElapsed())
	throw std::runtime_error("Thread did not close!");
    }
  }

  void TCPStreamImpl::controlLoop() {
    _stopped = false;
    try {
      do {
	std::this_thread::sleep_for(10ms);
	std::lock_guard<std::timed_mutex> lock(_controlMutex);

	std::string msg = read_message();
	if (msg == "new_data") {
	  if(readDataFromStream()) {
	    if (_onDataCallback) _onDataCallback(this);
	  }
	}
	else if (msg == "check") {
	  write_message(msg);
	}
      } while(!_shouldStop);
      _stopped = true;
    }
    catch(std::exception& e) {
      _stopped = true;
	  std::cerr << "TCPStream " << this << " " << e.what() << "\n   Stopping control thread" << std::endl;
    }
  }

  bool TCPStreamImpl::readDataFromStream() {
    write_message("start");
    std::string strLength = read_message();
    int length = 0;
    try {
      length = std::stoi(strLength);
    } catch(std::invalid_argument e) {
      throw std::runtime_error("Could not get the length of expected stream data");
    }

    write_message("ok");
    _lastData.clear();
    _lastData.reserve(length);
    int bytesLeft = length;
    while(bytesLeft > 0) {
      int bytesToRead = std::min<int>(PACKETSIZE, bytesLeft);
      std::vector<byte> bytes = _socket->receive(bytesToRead);
      std::copy(bytes.begin(), bytes.end(), std::back_inserter(_lastData));
      bytesLeft -= bytes.size();
    }
    write_message("true");
    return true;
  }

  void TCPStreamImpl::writeData(const std::vector<byte>& data) {
    ensureSocket();

    if(!_controlMutex.try_lock_for(1000ms))
      throw std::runtime_error("Could not acquire exclusive lock for stream sending");
    std::lock_guard<std::timed_mutex> lock(_controlMutex, std::adopt_lock);

    write_message("new_data");
    if (read_message() != "start")
      throw std::runtime_error("Other side not ready to receive data");
    write_message(std::to_string(data.size()));
    if (read_message() != "ok")
      throw std::runtime_error("Other side could not decode data length");

    int bytesLeft = data.size();
    const byte* dataPtr = &data[0];
    while(bytesLeft > 0) {
      int bytesToSend = std::min<int>(bytesLeft, PACKETSIZE);
      std::vector<byte> part(dataPtr, dataPtr + bytesToSend);

      _socket->send(part);

      dataPtr += bytesToSend;
      bytesLeft -= bytesToSend;
    }
    if (read_message() != "true") {
      throw std::runtime_error("Receiver side did not receive all the data");
    }
  }

  void TCPStreamImpl::ensureSocket() {
    assert(_socketFactory);
	if(!_socket)
		_socket = _socketFactory->createSocket();
  }

  std::vector<byte> TCPStreamImpl::getData() {
    std::vector<byte> result;
    result.swap(_lastData);
    return result;
  }

  void TCPStreamImpl::sendData(const std::vector<byte>& data) {
    writeData(data);
  }

  void TCPStreamImpl::setSocketFactory(std::shared_ptr<SocketFactory> factory) {
    _socketFactory = factory;
  }
}
