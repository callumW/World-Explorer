// File: MapGenerator.cs
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
using System.Collections;
using System.IO;

public enum DrawMode
{
	heightmap,
	mesh
};

public struct Map
{
	public float[,] heightMap;
	//public Vector3[,] normalMap;

	public Map(float[,] h_map /*, Vector3[,] norm_map*/)
	{
		heightMap = h_map;
		//normalMap = norm_map;
	}
};

public class MapGenerator : MonoBehaviour
{

	/* Height Map Vars */
	public string heightMapFileName;
	public int mapChunkSize;
	public float heightMultiplier;

    void Awake() {
        drawToPlane();
    }

	/**
	 * Generate the map from the height map
	 */
	public Map GenerateMap() {
		float[,] heightMap = HeightMapReader.ReadHeightMap(heightMapFileName);
		Map map = new Map(heightMap);
		return map;
	}

    /**
     * Draw the height to the plane
     */
    public void drawToPlane() {
        MapPlane plane = FindObjectOfType<MapPlane>();
        plane.DrawTexture(TextureGenerator.generateFromHeightMap(GenerateMap()));
    }

	/**
	 * Update the class
	 */
	void Update() {
        drawToPlane();
	}//End of Update


	/**
	 */
	void Start() {
        drawToPlane();
	}//End of Start

	/**
	 * Validate the class variables
	 */
	void OnValidate() {
		if (heightMapFileName.Equals("")) {
			print("Error: No height map was specified");
			heightMapFileName = Application.dataPath;
		}
		else if (!File.Exists(heightMapFileName)) {
			print("File: " + heightMapFileName + " does not exist!");
		}
		else {
			print("File: " + heightMapFileName + " exists!");
		}

		if (mapChunkSize <= 0) {
			mapChunkSize = 255;
		}
		if (heightMultiplier <= 0f) {
			heightMultiplier = 1f;
		}
			
	}//End of OnValidate
}
