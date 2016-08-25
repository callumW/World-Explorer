/*
    File: Height_map.cpp
    Author: Callum Wilson

COPYRIGHT (c) 2016 Callum Wilson callum.w@outlook.com

MIT License

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#include "Height_map.h"
#include "module\perlin.h"


#include <iostream>
#include <fstream>
#include <ios>

float get_height(float x, float y, float z)
{
    static noise::module::Perlin generator;
	generator.SetLacunarity(3.0f);
	double val = generator.GetValue(x, y, z) / 2.0f + 0.5f;
	float f = static_cast<float> (val);
	if (f < 0.0f) {
		//This should not happen
		//std::cout << "out of bounds value detected and corrected";
		f = 0.0f;
	}
	else if (f > 1.0f) {
		//This should not happen
		//std::cout << "out of bounds value detected and corrected";
		f = 1.0f;
	}
	return f;
}

Height_map::Height_map(int w, int h)
    :width{w}, height{h}
{
	std::cout << "Generating heightmap" << std::endl;
    for (int y = 0; y < height; y++) {
        for (int x = 0; x < width; x++) {
            map.push_back(Point{get_height(0.004f * x, 0.004f * y)});
        }
    }
	std::cout << "Height map generated" << std::endl;
}

bool Height_map::write_out(const std::string& file_name)
{
    std::ofstream file_out {file_name, std::ios_base::out|std::ios_base::binary|
        std::ios_base::trunc};

    if (!file_out) {
        std::cout << "Could not open file: " << file_name << std::endl;
        return false;
    }

    std::cout << "Writing to file: " << file_name << std::endl;

    /** Write the header **/
    file_out.write(as_bytes(width), sizeof(int));
    file_out.write(as_bytes(height), sizeof(int));
	float min = 0.0f;
	float max = 1.0f;
    file_out.write(as_bytes(min), sizeof(float));
    file_out.write(as_bytes(max), sizeof(float));

    /** write Data **/
    for (Point p : map) {
        file_out.write(as_bytes(p.height), sizeof(float));
    }

    return true;
}

template<class T> char* as_bytes(T& i)
{
	void* addr = &i;	// get the address of the first byte
						// of memory used to store the object
	return static_cast<char*>(addr); // treat that memory as bytes
}

//little-endian
char* float_to_byte(float f)
{
	uint32_t val = (uint32_t) f;

	char buf[sizeof(float)];

	buf[3] = (val >> 24) & 0xFF;
	buf[2] = (val >> 16) & 0xFF;
	buf[1] = (val >> 8) & 0xFF;
	buf[0] =  val & 0xFF;

	return buf;
}
