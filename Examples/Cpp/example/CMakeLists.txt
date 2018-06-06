
include_directories(${CMAKE_SOURCE_DIR}/testmancpp/include)
include_directories(${Boost_INCLUDE_DIRS})
link_directories(${Boost_LIBRARY_DIRS})

add_executable(packetSniffer udpMulticastReceiver.cpp)
target_link_libraries(packetSniffer testmancpp)

add_executable(exampleTestman testman_example.cpp)
target_link_libraries(exampleTestman testmancpp)
