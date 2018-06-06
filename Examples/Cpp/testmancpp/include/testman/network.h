/*
   TestmanCPP - C++ Implementation of the Testman Protocol

   Licensed under GPLv3

   (C) 2018, Maximilian Matthe, maximilian.matthe@ifn.et.tu-dresden.de
*/
#ifndef NETWORK_H
#define NETWORK_H

#include <functional>
#include <vector>
#include <memory>

#include "forwards.h"

namespace testman {

  class NetworkInterface {
  public:
    typedef std::function<void(const RawPacket&)> receiveFunc;
    virtual void send(const RawPacket& packet) = 0;
    virtual void registerOnReceive(receiveFunc);

    virtual std::vector<std::string> getIpAddresses() = 0;

    virtual void startReceiveLoop();

    virtual void distributeReceivedPacket(const RawPacket& packet);

    virtual std::shared_ptr<TCPStreamFactory> createStreamFactory() = 0;

  private:
    std::vector<receiveFunc> _receiveFuncs;
  };

  std::shared_ptr<NetworkInterface> createUdpInterface(const std::string& ipaddress, int port, int ttl);
}

#endif /* NETWORK_H */
