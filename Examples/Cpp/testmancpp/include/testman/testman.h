/*
   TestmanCPP - C++ Implementation of the Testman Protocol

   Licensed under GPLv3

   (C) 2018, Maximilian Matthe, maximilian.matthe@ifn.et.tu-dresden.de
*/
#ifndef TESTMAN_H
#define TESTMAN_H

#include <iostream>

#include "forwards.h"

namespace testman {
  struct Identification {
    Identification(byte type, byte id)
      : id(id), type(type) {}

    byte id;
    byte type;

    operator std::string() { return "(" + std::to_string(type) + ", " + std::to_string(id) + ")"; }
  };

  inline bool operator==(const Identification& lhs, const Identification& rhs) {
    return lhs.id == rhs.id && lhs.type == rhs.type;
  }

  inline bool operator!=(const Identification& lhs, const Identification& rhs) {
    return !(lhs == rhs);
  }

  inline bool operator<(const Identification& lhs, const Identification& rhs) {
    if (lhs.type == rhs.type)
      return lhs.id < rhs.id;
    else
      return lhs.type < rhs.type;
  }


  inline std::ostream& operator<<(std::ostream& o, const Identification& id) {
    o << "(type: " << (int)id.type << ", id: " << (int)id.id << ")";
    return o;
  }

  std::string getTimestamp();

}

#endif /* TESTMAN_H */
