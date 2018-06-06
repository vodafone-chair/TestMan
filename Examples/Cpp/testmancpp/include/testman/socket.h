/*
   TestmanCPP - C++ Implementation of the Testman Protocol

   Licensed under GPLv3

   (C) 2018, Maximilian Matthe, maximilian.matthe@ifn.et.tu-dresden.de
*/
#ifndef SOCKET_H
#define SOCKET_H

#include <boost/asio.hpp>

#include "testman/tcpstream.h"

namespace ba = boost::asio;

namespace testman {

  class TCPSocketFactory : public SocketFactory {
  public:
    TCPSocketFactory(ba::io_context& io_context)
      : _io_context(io_context) {}

    virtual ~TCPSocketFactory() = default;

    virtual std::shared_ptr<Socket> createSocket();

  private:
    ba::io_context& _io_context;
  };
}

#endif /* SOCKET_H */
