/*
   TestmanCPP - C++ Implementation of the Testman Protocol

   Licensed under GPLv3

   (C) 2018, Maximilian Matthe, maximilian.matthe@ifn.et.tu-dresden.de
*/
#include <cassert>

#include "testman/packet.h"


namespace testman {
  RawPacket::RawPacket() : id(0,0) {
  }

  RawPacket::RawPacket(Identification id) : id(id) {
  }

  RawPacket::RawPacket(Identification id, const detail::StringMap& content)
    : RawPacket(id) {
    for(auto& kv: content)
      set(kv.first, kv.second);
  }


  std::vector<byte> RawPacket::encode() const {
    std::vector<byte> result;
    result.push_back(0x60);
    result.push_back(id.type);
    result.push_back(id.id);

    for(const auto& kv : _data) {
      result.push_back((byte)kv.first.size());
      std::copy(kv.first.begin(), kv.first.end(), std::back_inserter(result));

      std::vector<byte> size = detail::sizeTo4Bytes(kv.second.size());
      std::copy(size.begin(), size.end(), std::back_inserter(result));
      std::copy(kv.second.begin(), kv.second.end(), std::back_inserter(result));
      result.push_back(0);
    }

    return result;
  }

  RawPacket RawPacket::decode(const std::vector<byte>& bytes) {
    RawPacket result(Identification(bytes[1], bytes[2]));
    size_t cnt = 3;
    while (cnt < bytes.size()) {
      int ln1 = bytes[cnt]; cnt++;
      std::string key(bytes.begin() + cnt, bytes.begin() + cnt + ln1);
      cnt += ln1;

      int ln2 = detail::_4BytesToSize(&bytes[cnt]);
      cnt += 4;
      std::string value(bytes.begin() + cnt, bytes.begin() + cnt + ln2);
      cnt += ln2;
      cnt++;
      result.set(key, value);
    }
    return result;
  }

  bool RawPacket::rawPacketsEqual(const RawPacket& lhs, const RawPacket& rhs) {
    return lhs.id == rhs.id && lhs._data.size() == rhs._data.size() &&
      std::equal(lhs._data.begin(), lhs._data.end(),
		 rhs._data.begin());
  }

  namespace detail {
    StringMap::StringMap() {
    }

    StringMap::StringMap(const std::map<std::string, std::string>& content)
      : _data(content) {
    }

    StringMap::StringMap(std::initializer_list<std::pair<const std::string, std::string>> l)
      : _data(l) {
    }

    const std::string& StringMap::operator[](const std::string& key) const {
      return _data.at(key);
    }

    void StringMap::set(const std::string& key, const std::string& value) {
      _data[key]=value;
    }

    bool StringMap::has(const std::string& key) const {
      return _data.find(key) != _data.end();
    }

    const std::string& StringMap::get(const std::string& key) const {
      return _data.at(key);
    }

    const std::string& StringMap::get(const std::string& key, const std::string& def) const {
      if (has(key))
	return get(key);
      else
	return def;
    }

    std::map<std::string, std::string>::const_iterator StringMap::begin() const {
      return _data.begin();
    }

    std::map<std::string, std::string>::const_iterator StringMap::end() const {
      return _data.end();
    }


    std::vector<byte> sizeTo4Bytes(size_t s) {
      std::vector<byte> result;
      for(int i = 0; i < 4; i++) {
	result.push_back(s & 0xFF);
	s >>= 8;
      }
      return result;
    }

    size_t _4BytesToSize(const std::vector<byte>& v) {
      assert(v.size() == 4);
      return _4BytesToSize(&v[0]);
    }

    size_t _4BytesToSize(const byte* start) {
      size_t result = 0;
      for (int  i = 3; i >= 0; i--) {
	result <<= 8;
	result += start[i];
      }
      return result;
    }
  }
}
