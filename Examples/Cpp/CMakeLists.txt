cmake_minimum_required(VERSION 3.0)
project(testmancpp)

if (CMAKE_CXX_COMPILER_ID STREQUAL "GNU")
  set (CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++14 -fpermissive")
endif ()

set(CMAKE_CXX_STANDARD 14)

if (WIN32)
  find_package(Boost 1.60.0 REQUIRED)  # rely on MSVC autolinking feature to link the libraries
  SET(Boost_LIBRARY_DIRS "C:/local/boost_1_66_0/lib64-msvc-14.1")
else()
  find_package(Boost 1.60.0 REQUIRED COMPONENTS system)  # explicitely specify the boost lib
endif(WIN32)


add_subdirectory(gtest)
add_subdirectory(testmancpp)
add_subdirectory(example)

message(STATUS "Boost dir (should not be empty): ${Boost_LIBRARY_DIRS}")