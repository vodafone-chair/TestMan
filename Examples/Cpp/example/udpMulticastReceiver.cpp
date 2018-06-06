/*
   TestmanCPP - C++ Implementation of the Testman Protocol

   Licensed under GPLv3

   (C) 2018, Maximilian Matthe, maximilian.matthe@ifn.et.tu-dresden.de
*/
#include <vector>
#include <iostream>
#include <string>
#include <boost/asio.hpp>
#include "boost/bind.hpp"

#include <testman/packet.h>

const int multicast_port = 50000;

using boost::asio::ip::tcp;

void listAddresses(boost::asio::io_context& io_context) {
  tcp::resolver resolver(io_context);
  tcp::resolver::query query(boost::asio::ip::host_name(), "");
  tcp::resolver::iterator it = resolver.resolve(query);

  std::cout << "Available network addresses:" << std::endl;
  while(it != tcp::resolver::iterator()) {
    boost::asio::ip::address addr = (it++)->endpoint().address();
    std::cout << addr.to_string() << std::endl;
  }
}

class receiver
{
public:
  receiver(boost::asio::io_context& io_context,
      const boost::asio::ip::address_v4& listen_address,
      const boost::asio::ip::address_v4& multicast_address)
    : socket_(io_context)
  {
    // Create the socket so that multiple may be bound to the same address.
    boost::asio::ip::udp::endpoint listen_endpoint(
        listen_address, multicast_port);
    socket_.open(listen_endpoint.protocol());
    socket_.set_option(boost::asio::ip::udp::socket::reuse_address(true));

    socket_.bind(listen_endpoint);

    // Join the multicast group.
    socket_.set_option(boost::asio::ip::multicast::join_group(multicast_address));

    std::cout << "Waiting for packets to dump..." << std::endl;

    socket_.async_receive_from(
        boost::asio::buffer(data_, max_length), sender_endpoint_,
        boost::bind(&receiver::handle_receive_from, this,
          boost::asio::placeholders::error,
          boost::asio::placeholders::bytes_transferred));
  }

  void handle_receive_from(const boost::system::error_code& error,
      size_t bytes_recvd)
  {
    if (!error)
    {
      std::cout << "From IP: " << sender_endpoint_.address() << std::endl << "  ";
      // std::cout.write(data_, bytes_recvd);
      std::cout << std::endl;

      try {
	std::vector<testman::byte> bytes(data_, data_ + bytes_recvd);
	testman::RawPacket p = testman::RawPacket::decode(bytes);
	std::cout << p << std::endl;
      }
      catch(testman::InvalidPacket&) {
	std::cout << "Invalid Packet" << std::endl;
      }


      socket_.async_receive_from(
          boost::asio::buffer(data_, max_length), sender_endpoint_,
          boost::bind(&receiver::handle_receive_from, this,
            boost::asio::placeholders::error,
            boost::asio::placeholders::bytes_transferred));
    }
    else
      std::cout << error << std::endl;
  }

private:
  boost::asio::ip::udp::socket socket_;
  boost::asio::ip::udp::endpoint sender_endpoint_;
  enum { max_length = 64000 };
  char data_[max_length];
};

int main(int argc, char* argv[])
{
  try
  {
    boost::asio::io_context io_context;
    listAddresses(io_context);

    receiver r(io_context,
	       boost::asio::ip::make_address("0.0.0.0").to_v4(),
	       boost::asio::ip::make_address("224.5.6.7").to_v4());
    io_context.run();
  }
  catch (std::exception& e)
  {
    std::cerr << "Exception: " << e.what() << "\n";
  }

  return 0;
}
