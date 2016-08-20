// File: HeightMapWriter.cs
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

using System;
using System.IO;
using UnityEngine;

public static class HeightMapWriter
{
	public static bool WriteFlatMap(string fileName, int width, int height)
	{
		Debug.Log("Writting file: " + fileName);
        FileMode fileMode;
        if (File.Exists(fileName))
        {
            //oh shit
            Debug.Log("FileName: " + fileName + " already exists!");
            Debug.Log("Overwritting file: " + fileName);
            fileMode = FileMode.Truncate;
        }
        else
        {
            fileMode = FileMode.CreateNew;
        }

		using (FileStream fs = new FileStream(fileName, fileMode)) {
			using (BinaryWriter w = new BinaryWriter(fs)) {
				/* write the width and height */
				w.Write(width);
				w.Write(height);
				w.Write(0f);
				w.Write(1f);

				int size = width * height;
				for (int i = 0; i < size; i++) {
					w.Write(0.7f);
				}
			}
		}

		return true;
	}
}

