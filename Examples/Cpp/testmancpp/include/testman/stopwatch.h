/*
   TestmanCPP - C++ Implementation of the Testman Protocol

   Licensed under GPLv3

   (C) 2018, Maximilian Matthe, maximilian.matthe@ifn.et.tu-dresden.de
*/
#ifndef STOPWATCH_H
#define STOPWATCH_H

#include <chrono>
#include <thread>

namespace testman {
  class StopWatch {
  public:
    StopWatch(int ms) : duration(ms) {
      reset();
    };

    void reset() {
      start = std::chrono::steady_clock::now();
    }

    bool isElapsed() const {
      auto elapsed = std::chrono::steady_clock::now();
      if (elapsed - start >= duration)
	return true;
      return false;
    };

    void waitForElapse() {
      auto remaining = start + duration - std::chrono::steady_clock::now();
      std::this_thread::sleep_for(remaining);
    }

  private:
    std::chrono::milliseconds duration;
    std::chrono::steady_clock::time_point start;
  };

}

#endif /* STOPWATCH_H */
