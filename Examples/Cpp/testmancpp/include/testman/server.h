/*
   TestmanCPP - C++ Implementation of the Testman Protocol

   Licensed under GPLv3

   (C) 2018, Maximilian Matthe, maximilian.matthe@ifn.et.tu-dresden.de
*/
#ifndef SERVER_H
#define SERVER_H

#include <mutex>
#include <condition_variable>
#include <deque>
#include <map>
#include <functional>

#include "forwards.h"
#include "testman.h"




namespace testman {
  namespace detail {
    class Event {
    public:
      bool wait_for(int ms);
      void notify();

    private:
      bool fired = false;
      std::mutex m;
      std::condition_variable cv;
    };
  }

  class NoServerIdFound : public std::runtime_error {
  public:
    NoServerIdFound() : std::runtime_error("All IDs have been tried") {}
  };

  class InvalidServerState : public std::runtime_error {
  public:
    InvalidServerState() : std::runtime_error("The Server is in an invalid state for this operation") {}
  };

  class CommandNotAcknowledged : public std::runtime_error {
  public:
    CommandNotAcknowledged(const detail::StringMap& cmd);
  };

  class WaitingPacket : public detail::Event {
  public:
  WaitingPacket(const std::string& timestamp)
    : timestamp(timestamp), receiverID(0,0) {}
    std::string timestamp;
    Identification receiverID;
    bool cmdWasReceived = false;
    bool cmdWasExecuted = false;
    std::string message;
    int timeout = 1000;
  };

  enum Answer {
    CMD_RECEIVED,
    CMD_EXECUTED,
  };
  inline std::string answerToString(Answer type) {
    switch(type) {
    case CMD_RECEIVED:
      return "received";
    case CMD_EXECUTED:
      return "executed";
    default:
      throw std::runtime_error("Unknown answer type!");
      break;
    }
  }


  class Server {
  public:
    typedef std::function<void(const std::string&, const std::string&, Identification, const RawPacket&)> CallbackFunction;
    Server(NetworkInterface& network, Identification requestedID, CallbackFunction callback=CallbackFunction());
    void setStreamFactory(std::shared_ptr<TCPStreamFactory> streamFactory);

    void searchForID();
    void forceIdentification();

    void setTimeout(int ms);

    void receiveRawPacket(const RawPacket& packet);
    Identification currentID() const;

    void sendData(const detail::StringMap& data);
    std::string sendCommand(const detail::StringMap& data);
    void sendAnswer(Answer type, const std::string& timestamp, const std::string& message="", int timout=-1);

    void startStream(Identification destination);
    TCPStream* getStream(Identification destination);
    void stopStream(Identification destination);


  private:
    enum State {
      CONSTRUCTED,
      LOOKING_FOR_ID,
      RECEIVING,
    };

    struct removeWaitingPacketAtScopeExit {
      removeWaitingPacketAtScopeExit(Server* server, const std::string& timestamp)
      : server(server), timestamp(timestamp) {}

      ~removeWaitingPacketAtScopeExit() {
	auto it = server->_waitingPackets.find(timestamp);
	if (it != server->_waitingPackets.end())
	  server->_waitingPackets.erase(it);
      }

      Server* server;
      std::string timestamp;

    };
    friend struct removeWaitingPacketAtScopeExit;

    void createEvent(const std::string& name, const std::string& content, Identification senderID);
    void createEvent(const std::string& name, const std::string& content, Identification senderID, const RawPacket& packet);

    void searchForID(Identification wish);
    void setIdentification(Identification id);
    bool rxPacketsIndicateIdIsUsed(Identification requestedID);
    bool notifyWaitingPacket(const RawPacket& packet);
    bool processIdSearchPacket(const RawPacket& packet);
    bool processCommandPacket(const RawPacket& packet);
    std::string addCommandTimestamp(RawPacket& packet);
    void sendSimpleAck(const std::string& timestamp, Identification receiverID);

    void ensureServerState(State state);
    bool destinationMatchesOurs(const RawPacket& packet, bool allowZeroAll=true);

    bool process_getlocalip(const RawPacket& packet);
    bool process_starttcpclient(const RawPacket& packet);
    bool process_starttcpserver(const RawPacket& packet);
    bool process_tcpreconnect(const RawPacket& packet);


    Identification findRemoteId(TCPStream* stream);

    void showException(const std::exception& e);
    void showError(const std::string& msg);
    void showWarning(const std::string& msg);
    void showHint(const std::string& msg);

    std::shared_ptr<TCPStream> createStream();

    NetworkInterface& _network;
    Identification _currentID;
    Identification _requestedID;
    CallbackFunction _packetCallback;

    int _timeout_ms;

    detail::Event _rxPacketEvent;
    std::deque<std::shared_ptr<RawPacket>> _lastRxPackets;

    std::map<std::string, std::shared_ptr<WaitingPacket> > _waitingPackets;

    std::shared_ptr<TCPStreamFactory> _streamFactory;
    std::map<Identification, std::shared_ptr<TCPStream>> _streams;

    State _serverState;
  };
}

#endif /* SERVER_H */
