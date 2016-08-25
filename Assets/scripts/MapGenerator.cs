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
    public string heightMapName;
    public string chunkedMapName;
    string heightMapFileName;
    string chunkedMapFileName;
    public const int mapChunkSize = 241;
    public float heightMultiplier;

    public DrawMode drawMode;

    public Biome[] biomes;

    [Range(0, 6)]
    public int editorPreviewLOD;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new
        Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new
        Queue<MapThreadInfo<MeshData>>();

    void Awake()
    {
        //DrawToPlane();
    }

    /**
     * Generate the map from the height map
     */
    public MapData GenerateMap()
    {
        float[,] heightMap = HeightMapReader.ReadHeightMap(heightMapFileName);
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Color[] colMap = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int biomeIndex;
                for (biomeIndex = 0; biomeIndex < biomes.Length; biomeIndex++)
                {
                    if (biomes[biomeIndex].minHeight > heightMap[x, y])
                    {
                        break;
                    }
                }
                colMap[y * width + x] = (biomeIndex == 0) ? biomes[0].color : 
                    biomes[biomeIndex - 1].color;
            }
        }
        MapData map = new MapData(heightMap, colMap);
        return map;
    }

    /**
     * Generate the map from the height map
     */
    public MapData[,] GenerateChunkedMap()
    {
        float[,] heightMap = HeightMapReader.ReadHeightMap(chunkedMapFileName);
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        int mapWidthChunks = width / mapChunkSize;
        int mapHeightChunks = height / mapChunkSize;

        MapData[,] maps = new MapData[mapWidthChunks, mapHeightChunks];



        for (int chunkY = 0; chunkY < mapHeightChunks; chunkY++)
        {
            int endYIndex = (chunkY + 1) * mapChunkSize;
            int startYIndex = chunkY * mapChunkSize;

            for (int chunkX = 0; chunkX < mapWidthChunks; chunkX++)
            {
                int endXIndex = (chunkX + 1) * mapChunkSize;
                int startXIndex = chunkX * mapChunkSize;

                Color[] colMap = new Color[mapChunkSize * mapChunkSize];
                float[,] chunkHeightMap = new float[mapChunkSize, mapChunkSize];


                int localY = 0;
                for (int y = startYIndex; y < endYIndex; y++)
                {
                    int localX = 0;
                    for (int x = startXIndex; x < endXIndex; x++)
                    {
                        chunkHeightMap[localX, localY] = heightMap[x, y];
                        int biomeIndex;
                        for (biomeIndex = 0; biomeIndex < biomes.Length; biomeIndex++)
                        {
                            if (biomes[biomeIndex].minHeight > heightMap[x, y])
                            {
                                break;
                            }
                        }
                        colMap[localY * mapChunkSize + localX] = 
                            (biomeIndex == 0) ? biomes[0].color : 
                                biomes[biomeIndex - 1].color;
                        localX++;
                    }
                    localY++;
                }
                maps[chunkX, chunkY] = new MapData(chunkHeightMap, colMap);
            }
        }
        return maps;
    }

    public MapData GenerateMapData(Vector2 center)
    {
        return GenerateMap();
    }

    public MapData GenerateMapData(Vector2 center, int chunkX, int chunkY)
    {
        return GenerateChunkedMap()[chunkX, chunkY];
    }

    public void requestMapData(Vector2 center, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate {
            MapDataThread(center, callback);
        };

        new Thread(threadStart).Start();
    }

    public void requestMapData(Vector2 center, int chunkX, int chunkY, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate {
            MapDataThread(center, chunkX, chunkY, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 center, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(center);
        lock (mapDataThreadInfoQueue) {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, 
                mapData));
        }
    }

    void MapDataThread(Vector2 center, int chunkX, int chunkY, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(center, chunkX, chunkY);
        lock (mapDataThreadInfoQueue) {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, 
                mapData));
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
        OnValidate();
        //heightMapFileName = Application.dataPath + "/" + heightMapName;
        DrawToPlane();
    }//End of Start

    /**
     * Validate the class variables
     */
    void OnValidate()
    {
        if (heightMapName.Equals(""))
        {
            print("Error: No height map was specified");
            //heightMapName = Application.dataPath;
        }
        else
        {
            heightMapFileName = Application.dataPath + "/" + heightMapName;
            print("heightmap path: " + heightMapFileName);
        }

        if (heightMapName.Equals(""))
        {
            print("Error: No height map was specified");
            //heightMapName = Application.dataPath;
        }
        else
        {
            chunkedMapFileName = Application.dataPath + "/" + chunkedMapName;
            print("heightmap path: " + chunkedMapFileName);
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
