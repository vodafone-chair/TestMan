/*
   TestmanCPP - C++ Implementation of the Testman Protocol

   Licensed under GPLv3

   (C) 2018, Maximilian Matthe, maximilian.matthe@ifn.et.tu-dresden.de
*/
#ifndef PACKET_H
#define PACKET_H

#include <vector>
#include <map>
#include <string>

#include "forwards.h"
#include "testman.h"

namespace testman {

  namespace detail {
    class StringMap {
    public:
      StringMap();
      StringMap(const std::map<std::string, std::string>& content);
      StringMap(std::initializer_list<std::pair<const std::string, std::string> >);

      const std::string& operator[](const std::string&) const;
      void set(const std::string& key, const std::string& value);
      bool has(const std::string& key) const;
      const std::string& get(const std::string& key) const;
      const std::string& get(const std::string& key, const std::string& def) const;

      std::map<std::string, std::string>::const_iterator begin() const;
      std::map<std::string, std::string>::const_iterator end() const;


    protected:
      std::map<std::string, std::string> _data;
    };

    std::vector<byte> sizeTo4Bytes(size_t s);
    size_t _4BytesToSize(const byte* start);
    size_t _4BytesToSize(const std::vector<byte>& v);
  }

  class InvalidPacket : std::runtime_error {
  };

  class RawPacket : public detail::StringMap {
  public:
    RawPacket();
    RawPacket(Identification id);
    RawPacket(Identification id, const detail::StringMap& content);

    std::vector<byte> encode() const;
    static RawPacket decode(const std::vector<byte>& bytes);

    static bool rawPacketsEqual(const RawPacket& lhs, const RawPacket& rhs);

    std::ostream& write(std::ostream& o) const {
      o << "Packet " << id << "\n";
      for(const auto& kv: _data)
	o << "    " << kv.first << ": " << kv.second << "\n";
      return o;
    }

  public:
    Identification id;
  };

  inline std::ostream& operator<<(std::ostream& o, const RawPacket& p) {
    p.write(o);
    return o;
  }




}
#endif /* PACKET_H */
