/*
    File: Height_map.h
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
#ifndef HEIGHT_MAP_H
#define HEIGHT_MAP_H

#include <string>
#include <vector>

struct Point {
    float height;
    //Add color info later?
};

class Height_map {
public:
    Height_map(int w, int h);

    bool write_out(const std::string& file_name);
private:
    int width, height;
    std::vector<Point> map;
};

float get_height(float x, float y, float z = 0.5f);

template<class T> char* as_bytes(T& i);
char* float_to_byte(float f);

#endif
