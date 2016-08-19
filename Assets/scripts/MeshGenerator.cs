// File: MeshGenerator.cs
// Date: 2016-8-19
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

public static class MeshGenerator
{

    public static meshData GenerateMesh(float [,] map.heightMap, float heightMultiplier,
        int levelOfDetail)
    {
        int width = map.heightMap.GetLength(0);
        int height = map.heightMap.GetLength(1);

        int meshSimplificationIncrement = 1; //TODO: Change this

        int verticesPerLine = (width-1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData (verticiesPerLine, verticiesPerLine);
		int vertexIndex = 0;
		for (int y = 0; y < height; y += meshSimplificationIncrement)
        {
			for (int x = 0; x < width; x += meshSimplificationIncrement)
            {
				meshData.vertices [vertexIndex] = new Vector3 (topLeftX + x,
					heightCurve.Evaluate(map.heightMap[x, y]) * heightMultiplier,
					topLeftZ - y);

				meshData.uvs [vertexIndex] = new Vector2 (x / (float)width,
					y / (float)height);
					if (x < width - 1 && y < height - 1) {
					meshData.addTriangle(vertexIndex,
						vertexIndex+verticiesPerLine+1,
						vertexIndex+verticiesPerLine);
					meshData.addTriangle(vertexIndex+verticiesPerLine+1,
						vertexIndex, vertexIndex+1);
					}

				vertexIndex++;
			}
		}

		return meshData;
	}
}

public class MeshData
{
	public Vector3[] vertices;
	public int[] triangles;
	public Vector2[] uvs;

	int triangleIndex;

	public MeshData(int meshWidth, int meshHeight)
    {
		vertices = new Vector3[meshWidth * meshHeight];
		uvs = new Vector2[meshWidth*meshHeight];
		triangles = new int[(meshHeight - 1) * (meshWidth - 1) * 6];
	}

	public void addTriangle(int a, int b, int c)
    {
		triangles [triangleIndex] = a;
		triangles [triangleIndex + 1] = b;
		triangles [triangleIndex + 2] = c;
		triangleIndex += 3;
	}

	public Mesh CreateMesh()
    {
		Mesh mesh = new Mesh ();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.RecalculateNormals ();
		return mesh;
	}
}
