/*
   TestmanCPP - C++ Implementation of the Testman Protocol

   Licensed under GPLv3

   (C) 2018, Maximilian Matthe, maximilian.matthe@ifn.et.tu-dresden.de
*/
#include <iostream>
#include <gtest/gtest.h>
#include <gmock/gmock.h>

#include <thread>

#include <chrono>
using namespace std::chrono_literals;

#include "testman/testman.h"
#include "testman/network.h"
#include "testman/tcpstream.h"
#include "testman/packet.h"

#include "matchers.h"

using ::testing::_;
using ::testing::Not;
using ::testing::NiceMock;
using ::testing::InvokeWithoutArgs;
using ::testing::Invoke;
using ::testing::Eq;
using ::testing::DoAll;
using ::testing::Return;
using ::testing::ReturnPointee;

using testman::Socket;
using testman::SocketFactory;
using testman::TCPStreamImpl;

class MockSocket : public Socket {
public:
  MOCK_METHOD2(connect, void(const std::string& ip, int port));
  MOCK_METHOD0(listen, int());
  MOCK_METHOD0(accept, void());

  MOCK_METHOD0(available, size_t());
  MOCK_METHOD1(receive, std::vector<testman::byte>(size_t bytes));
  MOCK_METHOD1(send, void(const std::vector<testman::byte>&));
};

class MockSocketFactory : public SocketFactory {
public:
  virtual std::shared_ptr<Socket> createSocket() {
    return socket;
  }

  std::shared_ptr<MockSocket> socket;
};

class TCPStream_comms : public ::testing::Test {
public:
  std::shared_ptr<MockSocket> socket;
  std::shared_ptr<MockSocketFactory> factory;
  TCPStreamImpl stream;

  TCPStream_comms() {
    socket = std::make_shared<NiceMock<MockSocket>>();
    factory = std::make_shared<MockSocketFactory>();
    factory->socket = socket;

    stream.setSocketFactory(factory);
  }

  std::vector<testman::byte> createMsg(const std::string& msg) {
    std::vector<testman::byte> result(stream.PACKETSIZE);
    std::fill(result.begin(), result.end(), 0);
    std::copy(msg.begin(), msg.end(), result.begin());
    return result;
  }

};

TEST_F(TCPStream_comms, callsSocketConnectUpon_connectToServer) {
  EXPECT_CALL(*socket, connect("1.2.3.4", 2000));

  stream.connectToServer("1.2.3.4", 2000);
  stream.stop();
}


TEST_F(TCPStream_comms, repliesWithCheckToCheckMessage) {
  auto checkMsg(createMsg("check"));

  {
    ::testing::InSequence dummy;
    EXPECT_CALL(*socket, available())
      .Times(1)
      .WillOnce(Return(stream.PACKETSIZE));
    EXPECT_CALL(*socket, receive(stream.PACKETSIZE))
      .Times(1)
      .WillOnce(Return(checkMsg));

    EXPECT_CALL(*socket, send(checkMsg))
      .Times(1);
  }

  stream.connectToServer("1.2.3.4", 2000);
  stream.stop();
}

TEST_F(TCPStream_comms, newData_correctReception) {

  int numAvail = stream.PACKETSIZE;
  ON_CALL(*socket, available())
    .WillByDefault(ReturnPointee(&numAvail));

  std::vector<testman::byte> data;
  for(testman::byte i = 0; i < 42; i++)
    data.push_back(i);

  {
    ::testing::InSequence dummy;
    EXPECT_CALL(*socket, receive(stream.PACKETSIZE))
      .Times(1)
      .WillOnce(Return(createMsg("new_data")));
    EXPECT_CALL(*socket, send(createMsg("start"))).Times(1);
    EXPECT_CALL(*socket, receive(stream.PACKETSIZE))
      .Times(1)
      .WillOnce(Return(createMsg("42")));
    EXPECT_CALL(*socket, send(createMsg("ok"))).Times(1);
    EXPECT_CALL(*socket, receive(42)).Times(1)
      .WillOnce(Return(data));

    EXPECT_CALL(*socket, send(createMsg("true"))).Times(1)
      .WillOnce(InvokeWithoutArgs([&numAvail]() {numAvail=0;}));
  }
  stream.connectToServer("1.2.3.4", 2000);
  std::this_thread::sleep_for(1000ms);
  ASSERT_EQ(stream.getData(), data);
  stream.stop();
}

TEST_F(TCPStream_comms, newData_longPacketCorrectReception) {
  int numAvail = stream.PACKETSIZE;
  ON_CALL(*socket, available())
    .WillByDefault(ReturnPointee(&numAvail));

  std::vector<testman::byte> data;
  for(int i = 0; i < stream.PACKETSIZE+42; i++)
    data.push_back(i % 7);
  std::vector<testman::byte> data1(data.begin(), data.begin()+stream.PACKETSIZE);
  std::vector<testman::byte> data2(data.begin()+stream.PACKETSIZE, data.end());

  {
    ::testing::InSequence dummy;
    EXPECT_CALL(*socket, receive(stream.PACKETSIZE))
      .WillOnce(Return(createMsg("new_data")));
    EXPECT_CALL(*socket, send(createMsg("start")));
    EXPECT_CALL(*socket, receive(stream.PACKETSIZE))
      .WillOnce(Return(createMsg(std::to_string(data.size()))));
    EXPECT_CALL(*socket, send(createMsg("ok"))).Times(1);
    EXPECT_CALL(*socket, receive(stream.PACKETSIZE))
      .WillOnce(Return(data1));
    EXPECT_CALL(*socket, receive(42))
      .WillOnce(Return(data2));
    EXPECT_CALL(*socket, send(createMsg("true")))
      .WillOnce(InvokeWithoutArgs([&numAvail]() {numAvail=0;}));
  }
  stream.connectToServer("1.2.3.4", 2000);
  std::this_thread::sleep_for(100ms);
  ASSERT_EQ(stream.getData(), data);
  stream.stop();
}

TEST_F(TCPStream_comms, writeData) {
  int numAvail = stream.PACKETSIZE;
  ON_CALL(*socket, available())
    .WillByDefault(ReturnPointee(&numAvail));

  std::vector<testman::byte> data;
  for(int i = 0; i < stream.PACKETSIZE+42; i++)
    data.push_back(i % 7);
  std::vector<testman::byte> data1(data.begin(), data.begin()+stream.PACKETSIZE);
  std::vector<testman::byte> data2(data.begin()+stream.PACKETSIZE, data.end());

  {
    ::testing::InSequence dummy;
    EXPECT_CALL(*socket, send(createMsg("new_data")));
    EXPECT_CALL(*socket, receive(stream.PACKETSIZE))
      .WillOnce(Return(createMsg("start")));
    EXPECT_CALL(*socket, send(createMsg(std::to_string(data.size()))));
    EXPECT_CALL(*socket, receive(stream.PACKETSIZE))
      .WillOnce(Return(createMsg("ok")));
    EXPECT_CALL(*socket, send(data1));
    EXPECT_CALL(*socket, send(data2));
    EXPECT_CALL(*socket, receive(stream.PACKETSIZE))
      .WillOnce(Return(createMsg("true")));
  }

  //  stream.connectToServer("1.2.3.4", 2000);
  stream.writeData(data);
}

TEST_F(TCPStream_comms, startServer_callsListen) {
  EXPECT_CALL(*socket, listen()).Times(1)
    .WillOnce(Return(2000));

  ASSERT_EQ(stream.startServer(), 2000);
}

TEST_F(TCPStream_comms, acceptConnection_callsAcceptAndSendsCheck) {
  ON_CALL(*socket, listen()).WillByDefault(Return(2000));
  stream.startServer();

  {
    ::testing::InSequence dummy;

    EXPECT_CALL(*socket, accept());

    EXPECT_CALL(*socket, send(createMsg("check")));
    EXPECT_CALL(*socket, available()).WillOnce(Return(stream.PACKETSIZE));
    EXPECT_CALL(*socket, receive(stream.PACKETSIZE)).WillOnce(Return(createMsg("check")));

    EXPECT_CALL(*socket, available()).WillRepeatedly(Return(0)); // block the control loop to not read anything
  }

  stream.acceptConnection();
}
