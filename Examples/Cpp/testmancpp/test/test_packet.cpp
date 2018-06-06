/*
   TestmanCPP - C++ Implementation of the Testman Protocol

   Licensed under GPLv3

   (C) 2018, Maximilian Matthe, maximilian.matthe@ifn.et.tu-dresden.de
*/
#include <iostream>
#include <gtest/gtest.h>

#include "testman/packet.h"

using namespace testman;

TEST(RawPacket, canConstruct) {
  RawPacket p2(Identification(1, 2));
  ASSERT_EQ(p2.id.type, 1);
  ASSERT_EQ(p2.id.id, 2);

  RawPacket p3(Identification(2,3),{{"key", "value"}});
  ASSERT_EQ(p3.id.type, 2);
  ASSERT_EQ(p3.id.id, 3);
  ASSERT_TRUE(p3.has("key"));
  ASSERT_EQ(p3.get("key"), "value");
}

TEST(RawPacket, encodesHeaderAndIDs) {
  RawPacket p(Identification(2, 100));
  std::vector<byte> encoded = p.encode();

  ASSERT_EQ(encoded.size(), 3);
  ASSERT_EQ(encoded[0], 0x60);
  ASSERT_EQ(encoded[1], 2);
  ASSERT_EQ(encoded[2], 100);
}

TEST(RawPacket, encodesData)
{
  RawPacket p(Identification(2, 100));
  p.set("key1", "value1");
  p.set("key2", "value2");

  std::vector<byte> encoded = p.encode();
  ASSERT_EQ(encoded.size(), 3+
	    1+strlen("key1")+4+strlen("value1")+1 +
	    1+strlen("key2")+4+strlen("value2")+1);
  ASSERT_EQ(encoded[3], 4);
  ASSERT_EQ(encoded[4], 'k');
  ASSERT_EQ(encoded[7], '1');
  ASSERT_EQ(encoded[8], 6);
  ASSERT_EQ(encoded[9], 0);
  ASSERT_EQ(encoded[10], 0);
  ASSERT_EQ(encoded[11], 0);
  ASSERT_EQ(encoded[12], 'v');
}

TEST(RawPacket, decodesEncoded) {
  RawPacket p(Identification(2, 100));
  p.set("key1", "value1");
  p.set("key2", "value2");

  std::vector<byte> encoded = p.encode();

  RawPacket p2(RawPacket::decode(encoded));
  ASSERT_TRUE(RawPacket::rawPacketsEqual(p, p2));
}


TEST(PacketMap, elementAccess) {
  testman::detail::StringMap p;

  p.set("element", "value");
  p.set("element2", "value2");

  ASSERT_EQ(p["element"], "value");
  ASSERT_EQ(p["element2"], "value2");

  ASSERT_THROW(p["non-existing"], std::out_of_range);
}

TEST(PacketMap, get) {
  testman::detail::StringMap p;
  p.set("element1", "value");

  ASSERT_FALSE(p.has("element2"));
  ASSERT_TRUE(p.has("element1"));
  ASSERT_EQ(p.get("element1"), "value");
  ASSERT_EQ(p.get("element2", "default"), "default");
  ASSERT_THROW(p.get("element2"), std::out_of_range);
}

TEST(PacketMap, foreach) {
  testman::detail::StringMap p;
  p.set("element1", "value");
  for(auto kv : p) {
    ASSERT_EQ(kv.first, "element1");
    ASSERT_EQ(kv.second, "value");
  }

}

TEST(Conversion, sizeTo4Bytes) {
  std::vector<byte> bytes;
  bytes = detail::sizeTo4Bytes(0);
  ASSERT_EQ(detail::sizeTo4Bytes(0),
	    std::vector<byte>({0,0,0,0}));

  ASSERT_EQ(detail::sizeTo4Bytes(1),
	    std::vector<byte>({1,0,0,0}));

  ASSERT_EQ(detail::sizeTo4Bytes(256),
	    std::vector<byte>({0,1,0,0}));

  ASSERT_EQ(detail::sizeTo4Bytes(257),
	    std::vector<byte>({1,1,0,0}));
}

TEST(Conversion, _4BytesToSize) {
  ASSERT_EQ(detail::_4BytesToSize(std::vector<byte>({0,0,0,0})), 0);
  ASSERT_EQ(detail::_4BytesToSize(std::vector<byte>({1,0,0,0})), 1);
  ASSERT_EQ(detail::_4BytesToSize(std::vector<byte>({0,1,0,0})), 256);
  ASSERT_EQ(detail::_4BytesToSize(std::vector<byte>({1,1,0,0})), 257);
}
