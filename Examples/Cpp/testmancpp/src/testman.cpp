/*
   TestmanCPP - C++ Implementation of the Testman Protocol

   Licensed under GPLv3

   (C) 2018, Maximilian Matthe, maximilian.matthe@ifn.et.tu-dresden.de
*/
#include <string>
#include <ctime>
#include <chrono>
#include <sstream>
#include <iomanip>

#include <boost/format.hpp>

#include "testman/testman.h"

namespace testman {
  std::string getTimestamp() {
    std::ostringstream oss;

    auto t = std::time(nullptr);
    auto tm = *std::localtime(&t);

    oss << std::put_time(&tm, "%Y%m%d%H%M%S");
    unsigned long long now = std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::system_clock::now().time_since_epoch()).count();
    oss << boost::str(boost::format("%04d") % (now % 1000));
    return oss.str();
  }
}
