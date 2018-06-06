/*
   TestmanCPP - C++ Implementation of the Testman Protocol

   Licensed under GPLv3

   (C) 2018, Maximilian Matthe, maximilian.matthe@ifn.et.tu-dresden.de
*/
#include <iostream>
#include <gtest/gtest.h>
#include <gmock/gmock.h>

#include "testman/testman.h"
#include "testman/packet.h"
#include "testman/network.h"
#include "testman/server.h"
#include "testman/tcpstream.h"


#include "matchers.h"


using ::testing::_;
using ::testing::Not;
using ::testing::NiceMock;
using ::testing::InvokeWithoutArgs;
using ::testing::Invoke;
using ::testing::Eq;
using ::testing::DoAll;
using ::testing::Return;

using testman::RawPacket;
using testman::Server;
using testman::Identification;

class MockNetwork : public testman::NetworkInterface {
public:
  MOCK_METHOD1(send, void(const testman::RawPacket&));
  MOCK_METHOD1(registerOnReceive, void(testman::NetworkInterface::receiveFunc));
  MOCK_METHOD0(startReceiveLoop, void());
  MOCK_METHOD0(getIpAddresses, std::vector<std::string>());
  MOCK_METHOD0(createStreamFactory, std::shared_ptr<testman::TCPStreamFactory>());

  void base_registerOnReceive(testman::NetworkInterface::receiveFunc f) {
    NetworkInterface::registerOnReceive(f);
  }
};

class MockTCPStream : public testman::TCPStream {
public:
  MOCK_METHOD1(registerOnDataCallback, void(std::function<void(TCPStream*)>));

  MOCK_METHOD0(getData, std::vector<unsigned char>());
  MOCK_METHOD1(sendData, void(const std::vector<unsigned char>&));

  MOCK_METHOD2(connectToServer, void(const std::string&, int));
  MOCK_METHOD0(startServer, int());
  MOCK_METHOD0(acceptConnection, void());
};

class MockTCPStreamFactory : public testman::TCPStreamFactory {
public:
  MOCK_METHOD0(createStream, std::shared_ptr<testman::TCPStream>());

  std::shared_ptr<testman::TCPStream> stream;
};

ACTION_P2(AnswerWith, server, packet) {
  server->receiveRawPacket(packet);
}

ACTION_P2(AnswerWithTimestamp, server, packet) {
  RawPacket packet2(packet);
  packet2.set("timestamp", arg0.get("timestamp"));
  server->receiveRawPacket(packet2);
}

void addDestinationID(RawPacket& packet, Identification id) {
  packet.set("type", std::to_string(id.type));
  packet.set("id", std::to_string(id.id));
}

ACTION_P2(AckSimple, server, id) {
  RawPacket packet(id, {{"command", "ack"}});
  addDestinationID(packet, server->currentID());
  packet.set("timestamp", arg0.get("timestamp"));
  server->receiveRawPacket(packet);
}

MATCHER_P2(CmdReceived, server, cmd, "") {
  RawPacket p(server->currentID(), {{"command", "received"}});
  p.set("timestamp", cmd.get("timestamp"));
  return packetContains(p, arg);
}

MATCHER_P2(CmdExecuted, server, cmd, "") {
  RawPacket p(server->currentID(), {{"command", "executed"}});
  p.set("timestamp", cmd.get("timestamp"));
  return packetContains(p, arg);
}

MATCHER_P2(SimpleAck, server, destination, "") {
  RawPacket p(server->currentID(), {{"command", "ack"}});
  addDestinationID(p, destination);
  return packetContains(p, arg);
}

ACTION_P2(AckReception, server, id) {
  RawPacket p(id, {{"command", "receive"}});
  p.set("timestamp", arg0.get("timestamp"));
  addDestinationID(p, server->currentID());
  server->receiveRawPacket(p);
}

ACTION_P2(AckExecution, server, id) {
  RawPacket p(id, {{"command", "executed"}});
  p.set("timestamp", arg0.get("timestamp"));
  addDestinationID(p, server->currentID());
  server->receiveRawPacket(p);
}


TEST(TestmanServer, registersAtNetworkObject) {
  MockNetwork network;
  EXPECT_CALL(network,
	      registerOnReceive(_));

  testman::Server server(network, Identification(1,1));
}


TEST(TestmanServer, AssignsIdWhenNoAnswer) {
  NiceMock<MockNetwork> network;
  testman::Server server(network, Identification(100, 7));
  server.setTimeout(10);

  EXPECT_CALL(network,
	      send(PacketContains(RawPacket(Identification(255, 255),
					    {{"type", "100"},
						{"id", "7"}}))))
    .Times(3) // Three times to make sure no loss occured in the network
    .WillRepeatedly(AnswerWith(&server, RawPacket(Identification(1,1),
						  {{"version", "1.0.6659"}})));

  server.searchForID();

  ASSERT_THAT(server.currentID(), Eq(testman::Identification(100, 7)));
}

TEST(TestmanServer, AsksForNextIDIfIDIsAlreadyUsed) {
  NiceMock<MockNetwork> network;
  testman::Server server(network, Identification(100, 7));
  server.setTimeout(10);

  RawPacket version(Identification(100, 7), {{"version", "1.0"}});
  RawPacket idUsed(Identification(100, 7), {{"command", "ID not available"}});


  {
    ::testing::InSequence dummy;
    EXPECT_CALL(network,
		send(PacketContains(RawPacket(Identification(255, 255),
					      {{"type", "100"},
						  {"id", "7"}}))))
      .WillOnce(AnswerWith(&server, idUsed));

    EXPECT_CALL(network,
		send(PacketContains(RawPacket(Identification(255, 255),
					      {{"type", "100"},
						  {"id", "8"}}))))
      .Times(3)
      .WillRepeatedly(AnswerWith(&server, version));
  }
  server.searchForID();

  ASSERT_EQ(server.currentID(), Identification(100, 8));
}

TEST(TestmanServer, ProcessesMultipleSimultaneousAnswersInIDSearchForNextIDIfIDIsAlreadyUsed) {
  NiceMock<MockNetwork> network;
  testman::Server server(network, Identification(100, 7));
  server.setTimeout(10);

  RawPacket version(Identification(100, 7), {{"version", "1.0"}});
  RawPacket versionB(Identification(101, 7), {{"version", "1.0"}});
  RawPacket idUsed(Identification(100, 7), {{"command", "ID not available"}});


  {
    ::testing::InSequence dummy;
    EXPECT_CALL(network,
		send(PacketContains(RawPacket(Identification(255, 255),
					      {{"type", "100"},
						  {"id", "7"}}))))
      .WillOnce(DoAll(AnswerWith(&server, versionB),
		      AnswerWith(&server, idUsed),
		      AnswerWith(&server, versionB)));

    EXPECT_CALL(network,
		send(PacketContains(RawPacket(Identification(255, 255),
					      {{"type", "100"},
						  {"id", "8"}}))))
      .Times(3)
      .WillRepeatedly(AnswerWith(&server, version));
  }
  server.searchForID();

  ASSERT_EQ(server.currentID(), Identification(100, 8));
}

TEST(TestmanServer, ThrowsIfNoIdAtAllCanBeFound) {
  NiceMock<MockNetwork> network;
  testman::Server server(network, Identification(100, 7));
  server.setTimeout(1);

  {
    ::testing::InSequence dummy;
    for (int id = 7; id < 256+7; id++) {
      RawPacket idUsed(Identification(100, id%256), {{"command", "ID not available"}});
      RawPacket idRequest(Identification(255, 255), {{"type", "100"}, {"id", std::to_string(id%256)}});
      EXPECT_CALL(network,
		  send(PacketContains(idRequest)))
	.WillOnce(AnswerWith(&server, idUsed));
    }
  }
  ASSERT_THROW(server.searchForID(), testman::NoServerIdFound);
}

TEST(TestmanServer, AnswersWithIdUsedOnForeignIdRequest) {
  NiceMock<MockNetwork> network;
  testman::Server server(network, Identification(100,7));
  server.setTimeout(10);
  server.forceIdentification();

  RawPacket idUsed(Identification(100, 7), {{"command", "ID not available"}});
  RawPacket idRequest(Identification(255, 255), {{"type", "100"}, {"id", "7"}});

  EXPECT_CALL(network, send(PacketContains(idUsed)));

  server.receiveRawPacket(idRequest);
}

class TestmanServer_Command : public ::testing::Test {
public:
  TestmanServer_Command()
    : server(network, Identification(1,1)) {
    server.setTimeout(5);
    server.forceIdentification();
  }

  NiceMock<MockNetwork> network;
  testman::Server server;
};

TEST_F(TestmanServer_Command, ThrowsIfInvalidStateForSendCommand) {
  NiceMock<MockNetwork> network;
  testman::Server server(network, Identification(1, 1));
  server.setTimeout(5);  // 5ms timeout

  ASSERT_THROW(server.sendCommand({{"key", "value"}}), testman::InvalidServerState);
}

TEST_F(TestmanServer_Command, RepeatsCommandIfNoAckReceivedAndThrows) {
  RawPacket command(server.currentID(), {{"command", "doit"}, {"key", "value"}});
  RawPacket expect(server.currentID(),
		   {{"command", "doit"}, {"key", "value"}, {"packetnumber", ANY}, {"timestamp", ANY}});
  EXPECT_CALL(network, send(PacketContains(expect))).Times(3);

  ASSERT_THROW(server.sendCommand(command), testman::CommandNotAcknowledged);
}

TEST_F(TestmanServer_Command, sendsAckOnCommandReceivedAckThrowsIfNoExecuteAck) {
  Identification idOther(1,2);
  RawPacket command(server.currentID(),
		    {{"command", "doit"}, {"key", "value"}, {"timestamp", "1"}, {"packetnumber", "1"}});

  {
    ::testing::InSequence dummy;
    EXPECT_CALL(network, send(PacketContains(command)))
      .WillOnce(AckReception(&server, idOther));
    EXPECT_CALL(network, send(SimpleAck(&server, idOther)));
  }
  ASSERT_THROW(server.sendCommand(command), testman::CommandNotAcknowledged);
}

TEST_F(TestmanServer_Command, sendsAckOnImmediateCommandExecutedAck) {
  RawPacket command(server.currentID(),
		    {{"command", "doit"}, {"key", "value"}, {"timestamp", "1"}, {"packetnumber", "1"}});
  Identification idOther(1,2);
  {
    ::testing::InSequence dummy;
    EXPECT_CALL(network, send(PacketContains(command)))
      .WillOnce(AckExecution(&server, idOther));
    EXPECT_CALL(network, send(SimpleAck(&server, idOther)));
  }
  server.sendCommand(command);
}

TEST_F(TestmanServer_Command, sendsAckOnBothCommandExecutedAndReceivedAck) {
  RawPacket command(server.currentID(),
		    {{"command", "doit"}, {"key", "value"}, {"timestamp", "1"}, {"packetnumber", "1"}});
  Identification idOther(1,2);

  {
    ::testing::InSequence dummy;
    EXPECT_CALL(network, send(PacketContains(command)))
      .WillOnce(AckReception(&server, idOther));
    EXPECT_CALL(network, send(SimpleAck(&server, idOther)))
      .WillOnce(AckExecution(&server, idOther));
    EXPECT_CALL(network, send(SimpleAck(&server, idOther)));
  }
  server.sendCommand(command);
}

TEST_F(TestmanServer_Command, returnsTheReturnMessageIfAny) {
  RawPacket command(server.currentID(),
		    {{"command", "doit"}, {"key", "value"}, {"timestamp", "1"}, {"packetnumber", "1"}});
  Identification idOther(1,2);
  RawPacket cmdExecuted(idOther,
			{{"command", "executed"}, {"packetnumber", "10"}, {"timestamp", "1"}, {"message", "return-message"}});

  {
    ::testing::InSequence dummy;
    ON_CALL(network, send(PacketContains(command)))
      .WillByDefault(AnswerWith(&server, cmdExecuted));
  }
  ASSERT_EQ(server.sendCommand(command), "return-message");
}

TEST_F(TestmanServer_Command, sendAnswerExecutedSendsCorrectPacket3TimesWithNoAckAndThrows) {
  RawPacket answer(server.currentID(),
		   {{"command", "executed"}, {"timestamp", "12"}});

  EXPECT_CALL(network, send(PacketContains(answer))).Times(3);

  ASSERT_THROW(server.sendAnswer(testman::Answer::CMD_EXECUTED, "12"), testman::CommandNotAcknowledged);
}

TEST_F(TestmanServer_Command, sendAnswerReceivedSendsCorrectPacket3TimesWithNoAckAndThrows) {
  RawPacket answer(server.currentID(),
		   {{"command", "received"}, {"timestamp", "12"}});

  EXPECT_CALL(network, send(PacketContains(answer))).Times(3);

  ASSERT_THROW(server.sendAnswer(testman::Answer::CMD_RECEIVED, "12"), testman::CommandNotAcknowledged);
}

TEST_F(TestmanServer_Command, sendAnswerSendsCorrectPacketAcceptsAck) {
  RawPacket answer(server.currentID(),
		   {{"command", "received"}, {"timestamp", "12"}});
  RawPacket ack(Identification(1,2),
		{{"command", "ack"}, {"timestamp", "12"}, {"type", "1"}, {"id", "1"}});

  ON_CALL(network, send(PacketContains(answer)))
    .WillByDefault(AnswerWith(&server, ack));

  // just check if the correct answer is accepted
  server.sendAnswer(testman::Answer::CMD_RECEIVED, "12");
}

class TestmanServer_Callback : public ::testing::Test {
public:
  TestmanServer_Callback()
    : server(network, Identification(1,1),
	     [this](const std::string& name, const std::string& content, Identification id, const testman::RawPacket& p) {
	       callback.Call(name, content, id, p);
	     }) {
    server.forceIdentification();
    server.setTimeout(10);
  }

  NiceMock<MockNetwork> network;
  testman::Server server;
  testing::MockFunction<void(const std::string& name,
			     const std::string& content,
			     Identification id,
			     const testman::RawPacket&)> callback;
};

TEST_F(TestmanServer_Callback, callsCallbackWhenCommandIsReceived) {
  testman::Identification senderID(5,2);
  testman::RawPacket command(senderID, {{"command", "doit"}, {"timestamp", "1"}});

  EXPECT_CALL(callback, Call(Eq("new"), Eq("packet"), Eq(senderID), PacketContains(command)));

  server.receiveRawPacket(command);
}


TEST(TestmanServer, ignoresCommandsForOtherIDs) {
  NiceMock<MockNetwork> network;
  testman::Server server(network, Identification(1,1));
  server.setTimeout(10);
  server.forceIdentification();

  RawPacket getlocalip(Identification(2,5),
		       {{"command", "getlocalip"},
			   {"timestamp", "12"},
			     {"packetnumber", "1"},
			       {"type", "100"},
				 {"id", "100"}});

  EXPECT_CALL(network, send(_)).Times(0);
  server.receiveRawPacket(getlocalip);
}


TEST(TestmanServer, repliesCorrectlyTo_getlocalip) {
  NiceMock<MockNetwork> network;
  testman::Server server(network, Identification(1,1));
  server.setTimeout(10);
  server.forceIdentification();

  std::string ts = "12";
  Identification idOther(2,5);

  RawPacket getlocalip(idOther,
		       {{"command", "getlocalip"},
			   {"timestamp", ts},
			     {"packetnumber", "1"},
			       {"type", "1"},
				 {"id", "1"}});
  RawPacket response(server.currentID(),
		     {{"command", "response"},
			 {"answer", "getlocalip"},
			   {"type", "2"},
			     {"id", "5"},
			       {"0", "192.168.1.12:2000"}});

  std::vector<std::string> addressList{"192.168.1.12"};
  ON_CALL(network, getIpAddresses()).WillByDefault(Return(addressList));

  {
    ::testing::InSequence dummy;
    EXPECT_CALL(network, send(CmdReceived(&server, getlocalip)))
      .Times(1)
      .WillOnce(AckSimple(&server, idOther));
    EXPECT_CALL(network, send(PacketContains(response)))
      .Times(1)
      .WillOnce(AckExecution(&server, idOther));
    EXPECT_CALL(network, send(SimpleAck(&server, idOther)));
    EXPECT_CALL(network, send(CmdExecuted(&server, getlocalip)))
      .Times(1)
      .WillOnce(AckSimple(&server, idOther));
  }
  server.receiveRawPacket(getlocalip);
}

class TestmanServer_stream : public ::testing::Test {
public:
  TestmanServer_stream() : idOther(2,5), server(network, Identification(1,1)) {
    stream = std::make_shared<NiceMock<MockTCPStream>>();
    factory = std::make_shared<NiceMock<MockTCPStreamFactory>>();
    factory->stream = stream;

    ON_CALL(*factory, createStream()).WillByDefault(Return(stream));
    server.setStreamFactory(factory);

    server.setTimeout(10);
    server.forceIdentification();

    RawPacket starttcpclient_(idOther,
			     {{"command", "starttcpclient"},
			   {"timestamp", "12"},
			     {"ip", "1.2.3.4"},
			       {"port", "2000"},
				 {"packetnumber", "1"},
				   {"type", "1"},
				     {"id", "1"}});
    starttcpclient = starttcpclient_;

    ON_CALL(network, send(CmdReceived(&server, starttcpclient)))
      .WillByDefault(AckSimple(&server, idOther));

    ON_CALL(network, send(CmdExecuted(&server, starttcpclient)))
      .WillByDefault(AckSimple(&server, idOther));

  }

  NiceMock<MockNetwork> network;
  testman::Server server;

  Identification idOther;
  RawPacket starttcpclient;

  std::shared_ptr<MockTCPStream> stream;
  std::shared_ptr<MockTCPStreamFactory> factory;

};

TEST_F(TestmanServer_stream, processes_starttcpclient) {
  {
    ::testing::InSequence dummy;
    EXPECT_CALL(network, send(CmdReceived(&server, starttcpclient)))
      .Times(1)
      .WillOnce(AckSimple(&server, idOther));

    EXPECT_CALL(*stream, connectToServer(starttcpclient.get("ip"),
					 std::stoi(starttcpclient.get("port"))));

    EXPECT_CALL(network, send(CmdExecuted(&server, starttcpclient)))
      .Times(1)
      .WillOnce(AckSimple(&server, idOther));
  }
  server.receiveRawPacket(starttcpclient);
}

TEST_F(TestmanServer_stream, starttcpclient_storesStream) {
  server.receiveRawPacket(starttcpclient);
  ASSERT_EQ(stream.get(), server.getStream(idOther));
}

TEST_F(TestmanServer_stream, stopStream_removesStream) {
  server.receiveRawPacket(starttcpclient);
  ASSERT_EQ(stream.get(), server.getStream(idOther));

  server.stopStream(idOther);

  ASSERT_THROW(server.getStream(idOther), std::out_of_range);
}

TEST_F(TestmanServer_stream, startStream_sendsGetLocalIP) {
  std::vector<std::string> addressList{"1.2.3.4"};
  ON_CALL(network, getIpAddresses()).WillByDefault(Return(addressList));

  RawPacket starttcpclient(server.currentID(),
			   {{"command", "starttcpclient"},
			       {"ip", "1.2.3.4"},
				 {"port", "2000"}});
  addDestinationID(starttcpclient, idOther);

  {
    ::testing::InSequence dummy;
    EXPECT_CALL(*stream, startServer())
      .WillOnce(Return(2000));
    EXPECT_CALL(network, send(PacketContains(starttcpclient)))
      .WillOnce(AckExecution(&server, idOther));
    EXPECT_CALL(network, send(SimpleAck(&server, idOther)));
    EXPECT_CALL(*stream, acceptConnection()).Times(1);
  }
  server.startStream(idOther);
}
