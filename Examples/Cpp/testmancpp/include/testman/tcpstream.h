/*
   TestmanCPP - C++ Implementation of the Testman Protocol

   Licensed under GPLv3

   (C) 2018, Maximilian Matthe, maximilian.matthe@ifn.et.tu-dresden.de
*/
#ifndef TCPSTREAM_H
#define TCPSTREAM_H

#include <memory>
#include <vector>
#include <atomic>
#include <mutex>
#include <functional>

#include "testman/testman.h"
#include "testman/tcpstream.h"


namespace testman {
  class SocketFactory {
  public:
    virtual ~SocketFactory() = default;

    virtual std::shared_ptr<Socket> createSocket() = 0;
  };

  class Socket {
  public:
    virtual ~Socket() = default;
    virtual void connect(const std::string& ip, int port) = 0;
    virtual int listen() = 0;
    virtual void accept() = 0;

    virtual size_t available() = 0;
    virtual std::vector<byte> receive(size_t bytes) = 0;
    virtual void send(const std::vector<byte>& buffer) = 0;
  };

  class TCPStreamFactory {
  public:
    virtual ~TCPStreamFactory() = default;

    virtual std::shared_ptr<TCPStream> createStream() = 0;
  };


  class TCPStream {
  public:
    virtual ~TCPStream() = default;

    virtual void registerOnDataCallback(std::function<void(TCPStream*)>) = 0;
    virtual std::vector<unsigned char> getData() = 0;
    virtual void sendData(const std::vector<unsigned char>& data) = 0;

    virtual void connectToServer(const std::string& ip, int port) = 0;
    virtual int startServer() = 0;
    virtual void acceptConnection() = 0;
  };

  class TCPStreamImpl : public TCPStream {
  public:
    virtual ~TCPStreamImpl();

    virtual void registerOnDataCallback(std::function<void(TCPStream*)>);
    virtual std::vector<byte> getData();
    virtual void sendData(const std::vector<unsigned char>& data);

    virtual void connectToServer(const std::string& ip, int port);
    virtual int startServer();
    virtual void acceptConnection();

    virtual void writeData(const std::vector<byte>& data);
    virtual void stop();

    void setSocketFactory(std::shared_ptr<SocketFactory> socketFactory);
    void enableLogging(bool enable);

    const size_t PACKETSIZE=4096;

  private:
    void log(const std::string& msg);
    void ensureSocket();
    std::shared_ptr<SocketFactory> _socketFactory;

    void startControlLoop();
    void controlLoop();
    bool readDataFromStream();
    std::function<void(TCPStream*)> _onDataCallback;

    std::string read_message();
    void write_message(const std::string& msg);

    bool _doLogging = false;

    std::shared_ptr<Socket> _socket;

    std::vector<byte> _lastData;

    std::timed_mutex _controlMutex;
    std::atomic_bool _shouldStop;
    std::atomic_bool _stopped;
  };

}

#endif /* TCPSTREAM_H */
