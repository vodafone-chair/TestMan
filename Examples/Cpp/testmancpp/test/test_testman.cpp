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

#include "matchers.h"

TEST(timestamp, correctFormat) {
  std::string timestamp = testman::getTimestamp();
  ASSERT_EQ(timestamp.size(), 4+2+2 + 2+2+2+4);
}
