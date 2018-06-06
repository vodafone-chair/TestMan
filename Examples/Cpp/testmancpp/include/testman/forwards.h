/*
   TestmanCPP - C++ Implementation of the Testman Protocol

   Licensed under GPLv3

   (C) 2018, Maximilian Matthe, maximilian.matthe@ifn.et.tu-dresden.de
*/
#ifndef FORWARDS_H
#define FORWARDS_H

namespace testman {
  class NetworkInterface;
  class RawPacket;
  class Server;

  class TCPStream;
  class TCPStreamImpl;
  class TCPStreamFactory;

  class Socket;
  class SocketImpl;
  class SocketFactory;

  namespace detail {
    class StringMap;
  }


  typedef unsigned char byte;
}

#endif /* FORWARDS_H */
