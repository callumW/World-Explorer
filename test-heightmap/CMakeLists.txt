cmake_minimum_required (VERSION 2.8)

project (create_heightmap CXX)

file (GLOB SRC_FILES "src/*.cpp")
add_executable (create_heightmap ${SRC_FILES})

set(CMAKE_MODULE_PATH ${CMAKE_MODULE_PATH} "${CMAKE_SOURCE_DIR}/cmake/Modules/")
find_package (libnoise REQUIRED)

include_directories (${LIBNOISE_INCLUDE_DIR})
target_link_libraries (create_heightmap ${LIBNOISE_LIBRARY})
