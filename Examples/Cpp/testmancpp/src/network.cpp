/*
   TestmanCPP - C++ Implementation of the Testman Protocol

   Licensed under GPLv3

   (C) 2018, Maximilian Matthe, maximilian.matthe@ifn.et.tu-dresden.de
*/
#include <set>
#include <iostream>
#include <thread>
#include <mutex>

#include <boost/asio.hpp>

#include "testman/network.h"
#include "testman/packet.h"
#include "testman/socket.h"

namespace ba = boost::asio;

namespace testman {
  class DefaultTCPStreamFactory : public TCPStreamFactory {
  public:
    DefaultTCPStreamFactory(ba::io_context& io_context)
      : _io_context(io_context) {
      _socketFactory = std::make_shared<TCPSocketFactory>(io_context);
    }

    virtual std::shared_ptr<TCPStream> createStream() {
      std::shared_ptr<TCPStreamImpl> stream = std::make_shared<TCPStreamImpl>();
      stream->setSocketFactory(_socketFactory);
      stream->enableLogging(true);
      return stream;
    }

  private:
    ba::io_context& _io_context;
    std::shared_ptr<SocketFactory> _socketFactory;
  };


  void NetworkInterface::distributeReceivedPacket(const RawPacket& packet) {
    std::thread T([this, packet] () {
	for(auto& e: _receiveFuncs)
	  e(packet);
      });
    T.detach();
  }

  void NetworkInterface::registerOnReceive(NetworkInterface::receiveFunc f) {
    _receiveFuncs.push_back(f);
  }

  void NetworkInterface::startReceiveLoop() {
  }

  typedef std::vector<byte> BytePacket;

  class PacketFilter {
  public:
    void logPacket(const BytePacket& packet);
    bool acceptPacket(const BytePacket& packet);

  private:
    std::set<BytePacket> _loggedPackets;
    std::mutex _logMutex;
  };

  void PacketFilter::logPacket(const BytePacket& packet) {
    std::lock_guard<std::mutex> g(_logMutex);
    _loggedPackets.insert(packet);
  }

  bool PacketFilter::acceptPacket(const BytePacket& data) {
    // Check, if the packet is in the toIgnore-List, and throw it away.
    // If not, we can accept the packet
    std::lock_guard<std::mutex> g(_logMutex);

    auto i = _loggedPackets.find(data);
    if (i != _loggedPackets.end()) {
      _loggedPackets.erase(i);
      return false;
    }
    else
      return true;
  }


  class UdpNetwork : public NetworkInterface {
  public:
    UdpNetwork(const std::string& ipaddress, int port, int ttl);

    void startReceiveLoop();
    void send(const RawPacket& packet);
    std::vector<std::string> getIpAddresses();

    std::shared_ptr<TCPStreamFactory> createStreamFactory();

  private:
    void setupSockets(const std::string& ipaddress, int port, int ttl);
    void receiveLoop_impl();

    std::unique_ptr<ba::io_context> _io_context;
    std::unique_ptr<ba::ip::udp::socket> _rxSocket;
    std::unique_ptr<ba::ip::udp::endpoint> _txEndpoint;

    PacketFilter _packetFilter;
  };

  std::shared_ptr<NetworkInterface> createUdpInterface(const std::string& ipaddress, int port, int ttl) {
    return std::make_shared<UdpNetwork>(ipaddress, port, ttl);
  }

  UdpNetwork::UdpNetwork(const std::string& ipaddress, int port, int ttl) {
    _io_context.reset(new ba::io_context());
    setupSockets(ipaddress, port, ttl);
  }

  std::shared_ptr<TCPStreamFactory> UdpNetwork::createStreamFactory() {
    return std::make_shared<DefaultTCPStreamFactory>(*_io_context);
  }

  void UdpNetwork::setupSockets(const std::string& ipaddress, int port, int ttl) {
    using ba::ip::udp;
    _rxSocket.reset(new udp::socket(*_io_context));
    udp::endpoint listen_endpoint(udp::v4(), port);

    _rxSocket->open(listen_endpoint.protocol());
    _rxSocket->set_option(udp::socket::reuse_address(true));
    _rxSocket->bind(listen_endpoint);
    _rxSocket->set_option(ba::ip::multicast::join_group(ba::ip::address_v4::from_string(ipaddress)));

    _txEndpoint.reset(new udp::endpoint(ba::ip::address_v4::from_string(ipaddress), port));
  }

  void UdpNetwork::startReceiveLoop() {
    std::thread T(std::bind(&UdpNetwork::receiveLoop_impl, this));
    T.detach();
  }

  void UdpNetwork::receiveLoop_impl() {
    using ba::ip::udp;

    while(true) {
      testman::byte data[64000];
      udp::endpoint sender_endpoint;
      size_t packet_length = this->_rxSocket->receive_from(ba::buffer(data, 64000), sender_endpoint);
      try {
	std::vector<testman::byte> bytes(data, data + packet_length);
	if (_packetFilter.acceptPacket(bytes)) {
	  testman::RawPacket p = testman::RawPacket::decode(bytes);
	  distributeReceivedPacket(p);
	}
      }
      catch(InvalidPacket) {
      }
    }
  }

  void UdpNetwork::send(const RawPacket& packet) {
    std::vector<testman::byte> data = packet.encode();
    _packetFilter.logPacket(data);
    _rxSocket->send_to(ba::buffer(&data[0], data.size()), *_txEndpoint);
  }

  std::vector<std::string> UdpNetwork::getIpAddresses() {
    using boost::asio::ip::tcp;
    using boost::asio::ip::udp;

    std::vector<std::string> addresses;

    tcp::resolver resolver(*_io_context);
    std::string hostname = boost::asio::ip::host_name();
    tcp::resolver::query query(boost::asio::ip::host_name(), "");
    tcp::resolver::iterator it = resolver.resolve(query);

    while(it != tcp::resolver::iterator()) {
      boost::asio::ip::address addr = (it++)->endpoint().address();
      std::string addr_str = addr.to_string();

      // use only local addresses
      if (addr_str.find("192.168.") == 0)
	addresses.push_back(addr_str);
      if (addr_str.find("141.76") == 0)
	addresses.push_back(addr_str);
    }

    #ifdef __linux__
    // On Linux, additionally try to connect to google and find the IP of the local socket
    try {
      udp::resolver   resolver(*_io_context);
      udp::resolver::query query(udp::v4(), "google.com", "");
      udp::resolver::iterator endpoints = resolver.resolve(query);
      udp::endpoint ep = *endpoints;
      udp::socket socket(*_io_context);
      socket.connect(ep);
      boost::asio::ip::address addr = socket.local_endpoint().address();
      std::string addr_str = addr.to_string();
      if (std::find(addresses.begin(), addresses.end(), addr_str) == addresses.end())
	addresses.push_back(addr_str);
      // std::cout << "My IP according to google is: " << addr.to_string() << std::endl;
    } catch (std::exception& e){
      std::cerr << "Could not deal with socket. Exception: " << e.what() << std::endl;
    }
    #endif

    return addresses;
  }
}
