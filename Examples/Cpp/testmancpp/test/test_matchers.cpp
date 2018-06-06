/*
   TestmanCPP - C++ Implementation of the Testman Protocol

   Licensed under GPLv3

   (C) 2018, Maximilian Matthe, maximilian.matthe@ifn.et.tu-dresden.de
*/
#include <iostream>
#include <gtest/gtest.h>
#include <gmock/gmock.h>

#include "testman/packet.h"

#include "matchers.h"

using ::testing::Not;

TEST(PacketMatcher, equality) {
  testman::Identification id(1,1);
  RawPacket packet(id, {{"key", "value"}});
  ASSERT_THAT(packet, PacketIs(RawPacket(id, {{"key","value"}})));
}

TEST(PacketMatcher, contains) {
  testman::Identification id(1,1);
  RawPacket packet(id, {{"key1", "value1"}, {"key2", "value2"}});
  ASSERT_THAT(packet, PacketContains(RawPacket(id,{{"key1", "value1"}})));
  ASSERT_THAT(packet, Not(PacketContains(RawPacket(id,{{"key1", "value2"}}))));
  ASSERT_THAT(packet, PacketContains(RawPacket(id,{{"key1", ANY}})));
}

TEST(PacketMatcher, storesPacket) {
  testman::Identification id(1,1);
  RawPacket packet1(id, {{"key1", "value1"}, {"key2", ANY}});
  RawPacket packet2(id, {{"key1", "value1"}, {"key2", "value3"}});

  RawPacket store;
  auto matcher(PacketContainsAndStores(packet1, &store));

  ASSERT_THAT(packet2, matcher);
  ASSERT_EQ(store.get("key2"), "value3");
}
