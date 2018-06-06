/*
   TestmanCPP - C++ Implementation of the Testman Protocol

   Licensed under GPLv3

   (C) 2018, Maximilian Matthe, maximilian.matthe@ifn.et.tu-dresden.de
*/
#ifndef MATCHERS_H
#define MATCHERS_H

using testman::RawPacket;

static const std::string ANY = "-any-";

MATCHER_P(PacketIs, packet, "") {
  return RawPacket::rawPacketsEqual(arg, packet);
}

template<class Packet>
bool packetContains(const Packet& packet, const Packet& arg) {
  if (packet.id != arg.id)
    return false;
  for(auto& kv: packet) {
    if (!arg.has(kv.first))
      return false;
    if (kv.second != ANY && arg.get(kv.first) != kv.second)
      return false;
  }
  return true;
}

MATCHER_P(PacketContains, packet, "") {
  return packetContains(packet, arg);
}

MATCHER_P2(PacketContainsAndStores, packet, store, "") {
  *store = arg;
  return packetContains(packet, arg);
}


#endif /* MATCHERS_H */
