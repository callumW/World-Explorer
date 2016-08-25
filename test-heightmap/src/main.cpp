/*
    File: main.cpp
    Author Callum Wilson

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

#include <iostream>
#include <string>

#include "Utils.h"
#include "Height_map.h"

int main(int argc, char** argv)
{
    if (!validate_input(argc, argv)) {
        std::cout << "Invalid input. Please see usage:" << std::endl;
        print_usage();
        return 1;
    }

    int width = 1205;
    int height = 1205;
    std::string file_name = "F:\\code\\World-Explorer\\Assets\\map2.hm";

	std::cout << "Note: the file size of your map will be: " << width * height * sizeof(float) + 2 * sizeof(int) + 2 * sizeof(float) << "Bytes" << std::endl;

    Height_map h_map{width, height};

    if (!h_map.write_out(file_name)) {
        std::cout << "Failed to write height map!" << std::endl;
    }
	else {
		std::cout << "Succesfully written height map!" << std::endl;
	}

	char c;
	std::cin >> c;

    return 0;
}
