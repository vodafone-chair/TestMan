SET(testmancpp_SRC
  src/packet.cpp
  src/network.cpp
  src/server.cpp
  src/testman.cpp
  src/tcpstream.cpp
  src/socket.cpp
  )

SET(testmancpp_HDR
  include/testman/forwards.h
  include/testman/packet.h
  include/testman/network.h
  include/testman/server.h
  include/testman/stopwatch.h
  include/testman/tcpstream.h
  )

SET(testmancpp_test_SRC
  test/test_packet.cpp
  test/test_server.cpp
  test/test_matchers.cpp
  test/test_testman.cpp
  test/test_tcpstream.cpp
  test/test_main.cpp)

include_directories(${CMAKE_SOURCE_DIR}/gtest)
include_directories(include)

include_directories(${Boost_INCLUDE_DIRS})
link_directories(${Boost_LIBRARY_DIRS})

add_library(testmancpp
  ${testmancpp_SRC}
  ${testmancpp_HDR}
)

add_executable(testman_unittest
  ${testmancpp_test_SRC}
)
target_link_libraries(testman_unittest testmancpp gtest)

if(UNIX)
  target_link_libraries(testmancpp pthread ${Boost_LIBRARIES})
endif()
