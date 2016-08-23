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
using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Collections.Generic;

using MapCollections;

public enum DrawMode
{
    heightMap,
    mesh
};

public class MapGenerator : MonoBehaviour
{

    /* Height Map Vars */
    public string heightMapFileName;
    public const int mapChunkSize = 241;
    public float heightMultiplier;

    public DrawMode drawMode;

    [Range(0, 6)]
    public int editorPreviewLOD;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new
        Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new
        Queue<MapThreadInfo<MeshData>>();

    void Awake()
    {
        DrawToPlane();
    }

    /**
     * Generate the map from the height map
     */
    public MapData GenerateMap()
    {
        float[,] heightMap = HeightMapReader.ReadHeightMap(heightMapFileName);
        Color[] colMap = new Color[heightMap.GetLength(0) * heightMap.GetLength(1)];

        for (int i = 0; i < colMap.Length; i++)
        {
            colMap[i] = new Color(0f, 0f, 0f);
        }
        MapData map = new MapData(heightMap, colMap);
        return map;
    }

    public MapData GenerateMapData(Vector2 center)
    {
        return GenerateMap();
    }

    public void requestMapData(Vector2 center, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate {
            MapDataThread(center, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 center, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(center);
        lock (mapDataThreadInfoQueue) {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void requestMeshData(MapData mapData, Action<MeshData> callback, int lod)
    {
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, lod, callback);
        };
        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateMesh(mapData,
            heightMultiplier, lod);
        lock (meshDataThreadInfoQueue) {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    /**
     * Draw the height to the plane
     */
    public void DrawToPlane()
    {
        MapPlane plane = FindObjectOfType<MapPlane>();
        if (plane == null)
        {
            print("Error: Could not locate plane!");
            return;
        }

        Texture2D tex = TextureGenerator.generateFromHeightMap(GenerateMap());
        if (tex == null)
        {
            print("Null texture!!");
            return;
        }

        if (drawMode == DrawMode.heightMap)
        {
            plane.DrawTexture(TextureGenerator.generateFromHeightMap(
                GenerateMap()));
        }
        else if (drawMode == DrawMode.mesh)
        {
            plane.DrawMesh(MeshGenerator.GenerateMesh(GenerateMap(),
                heightMultiplier, editorPreviewLOD),
                TextureGenerator.generateFromHeightMap(GenerateMap()));
        }
    }

    /**
     * Update the class
     */
    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = 
                    mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.param);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = 
                    meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.param);
            }
        }
    }//End of Update


    /**
     */
    void Start()
    {
        DrawToPlane();
    }//End of Start

    /**
     * Validate the class variables
     */
    void OnValidate()
    {
        if (heightMapFileName.Equals(""))
        {
            print("Error: No height map was specified");
            heightMapFileName = Application.dataPath;
        }
        else if (!File.Exists(heightMapFileName))
        {
            print("File: " + heightMapFileName + " does not exist!");
            heightMapFileName = Application.dataPath;
        }
        else
        {
            print("File: " + heightMapFileName + " exists!");
        }
        /*
        if (mapChunkSize <= 0)
        {
            mapChunkSize = 255;
        }*/

        if (heightMultiplier <= 0f)
        {
            heightMultiplier = 1f;
        }

    }//End of OnValidate
}

public struct MapThreadInfo<T>
{
    public readonly Action<T> callback;
    public readonly T param;

    public MapThreadInfo(Action<T> callback, T param)
    {
        this.callback = callback;
        this.param = param;
    }
}
