/*
   TestmanCPP - C++ Implementation of the Testman Protocol

   Licensed under GPLv3

   (C) 2018, Maximilian Matthe, maximilian.matthe@ifn.et.tu-dresden.de
*/
#include <boost/asio.hpp>
#include <mutex>


#include "testman/testman.h"
#include "testman/stopwatch.h"
#include "testman/tcpstream.h"
#include "testman/socket.h"

namespace ba = boost::asio;
using boost::asio::ip::tcp;

namespace testman {
  namespace detail {
    class Semaphore {
    public:
      Semaphore (int count_ = 0)
        : count(count_) {}

      inline void notify()
      {
        std::unique_lock<std::mutex> lock(mtx);
        count++;
        cv.notify_one();
      }

      inline void wait()
      {
        std::unique_lock<std::mutex> lock(mtx);

        while(count == 0){
	  cv.wait(lock);
        }
        count--;
      }
      template <typename Duration>
      bool wait_for(const Duration duration)
      {
        std::unique_lock<std::mutex> lock(mtx);
        auto wait_for_it_success = cv.wait_for(lock, duration, [this]() { return count > 0; });
        return wait_for_it_success;
      }
      inline void reset()
      {
        if(mtx.try_lock())
	  {
            count=0;
            mtx.unlock();
	  }
        else
	  {
            throw std::runtime_error("reset Semaphore failed.");
	  }
      }

    private:
      std::mutex mtx;
      std::condition_variable cv;
      int count;
    };
  }
  class TCPSocket : public Socket {
  public:
    TCPSocket(ba::io_context&);

    virtual ~TCPSocket() = default;
    virtual void connect(const std::string& ip, int port);
    virtual int listen();
    virtual void accept();

    virtual size_t available();
    virtual std::vector<byte> receive(size_t bytes);
    virtual void send(const std::vector<byte>& buffer);

  private:
    ba::io_context& _io_context;
    std::unique_ptr<tcp::socket> _socket;
    std::unique_ptr<tcp::acceptor> _acceptor;

  };

  std::shared_ptr<Socket> TCPSocketFactory::createSocket() {
    return std::make_shared<TCPSocket>(_io_context);
  }

  TCPSocket::TCPSocket(ba::io_context& io_context)
    : _io_context(io_context)  {
	  _socket.reset(new tcp::socket(_io_context));
  }

  void TCPSocket::connect(const std::string& ip, int port) {
	  tcp::endpoint endpoint(ba::ip::address::from_string(ip), port);
	  _socket->connect(endpoint);
  }

  int TCPSocket::listen() {
    _acceptor.reset(new tcp::acceptor(_io_context, tcp::endpoint(tcp::v4(), 0)));
	int port = _acceptor->local_endpoint().port();
	return port;
  }

  void TCPSocket::accept() {

	  bool done = false;
	  boost::system::error_code err;
	  std::thread T([this, &done, &err]() {
		  _acceptor->accept(*_socket, err);
		  done = true;
		  // Danger! Need to accept with timeout, but this is somehow not supported.
		  // hence, we let the accept method run in a separete thread, and call cancel
		  // to the acceptor from the main thread once there is timeout. Moreover, we delete the acceptor object.
		  // However, we dont know, what happens with the accept call, (tests show it still does not return) and
		  // hence there's potential for a memory access violation, as the acceptor is deleted but still accept() is running.
	  });
	  T.detach();

	  StopWatch sw(500);  // dont need to wait long, as the accept should immediate return, since the ack-command has already been received before
	  while (!done) {
		  if (sw.isElapsed()) {
			  _acceptor->cancel();  // stop listening
			  _acceptor->close();
			  std::this_thread::sleep_for(std::chrono::milliseconds(100));
			  _acceptor.reset();
			  throw std::runtime_error("No client accepted while listening!");
		  }
		  std::this_thread::sleep_for(std::chrono::milliseconds(10));
	  }
	  if (err)
		  throw boost::system::system_error(err);
  }


  size_t TCPSocket::available() {
	  return _socket->available();
  }

  std::vector<byte> TCPSocket::receive(size_t bytes) {
	  std::vector<byte> result(bytes);
	  size_t rxBytes = _socket->receive(ba::buffer(&result[0], bytes));
	  result.resize(rxBytes);
	  return result;
  }

  void TCPSocket::send(const std::vector<byte>& buffer) {
	  _socket->send(ba::buffer(&buffer[0], buffer.size()));
  }



}
