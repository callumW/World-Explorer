// File: HeightMapReader.cs
// Date: 2016-8-18
//
// COPYRIGHT (c) 2016 Callum Wilson callum.w@outlook.com
//
// MIT License
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using UnityEngine;
using System;
using System.IO;

public static class HeightMapReader
{

    /**
     * Read the specified height map file
     * \Prerequisite: The filename must not contain invalid characters.
     * \return: If successful A 2D array of floats coresponding to the values of the heightmap, if unsuccessful an array
     * with -1 as its only element.
     */
    public static float[,] ReadHeightMap(string heightMapFilename)
    {
        float[,] error = { {-1} };

        if (!File.Exists(heightMapFilename))
        {
            Debug.Log("File: " + heightMapFilename + " does not exist!");
            return error;
        }

        float[,] heightMap;
        int width, height;
        float min, max;

        using (FileStream fs = File.OpenRead(heightMapFilename))
        {
            using (BinaryReader br = new BinaryReader(fs))
            {
                /* get the width and height */
                width = br.ReadInt32();
                if (width <= 0)
                {
                    Debug.Log("Failed to read width of heightmap: " +
                        heightMapFilename);
                    return error;
                }

                height = br.ReadInt32();

                if (height <= 0)
                {
                    Debug.Log("Failed to read height of heightmap: " +
                        heightMapFilename);
                    return error;
                }

                min = br.ReadSingle();

                max = br.ReadSingle();
                if (min > max)
                {
                    float tmp = min;
                    min = max;
                    max = tmp;
                }

                heightMap = new float[width, height];
                float currentVal;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        currentVal = br.ReadSingle();

                        if (currentVal > max || currentVal < min)
                        {
                            Debug.Log("Value: " + currentVal + " out of bounds at: " + x + "," + y);
                            currentVal = (currentVal > max) ? max : min;	//truncate value
                            //TODO note: Should we increase bounds instead?
                        }
                        else
                        {
                            heightMap[x,y] = currentVal;
                        }
                    }
                }

                return heightMap;
            }
        }
    }
}
