﻿// File: Map.cs
// Date: 2016-8-22
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
using System.Collections.Generic;
using MapCollections;

public class Map : MonoBehaviour {

    private const float scale = 1f;

    private const float moveDelay = 25f;
    private const float sqrMoveDelay = moveDelay * moveDelay;

    private static MapGenerator mapGen;

    public LODData[] lods;
    public static float maxViewDst;

    private int chunkSize;
    private int chunksVisibleInViewDst;

    private static Vector2 cameraPosition;
    private Vector2 cameraPositionOld;
    public Material mapMaterial;

    Dictionary<Vector2, MapChunk> mapChunkDictionary = new Dictionary<Vector2,
        MapChunk>();

    static List<MapChunk> chunksVisibleLastUpdate = new List<MapChunk>();

	// Use this for initialization
	void Start () 
    {
        mapGen = FindObjectOfType<MapGenerator>();

        maxViewDst = lods[lods.Length - 1].minViewableDistance;
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
        cameraPosition = new Vector2(transform.position.x, transform.position.z);
        cameraPositionOld = cameraPosition;
        UpdateVisibleChunks();
	}

    void UpdateVisibleChunks()
    {
        for (int i = 0; i < chunksVisibleLastUpdate.Count; i++)
        {
            chunksVisibleLastUpdate[i].SetVisible(true);
        }
        chunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(cameraPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(cameraPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDst;
            yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst;
                xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX +
                   xOffset, currentChunkCoordY + yOffset);
            
                if (mapChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    mapChunkDictionary[viewedChunkCoord].UpdateMapChunk();
                }
                else
                {
                    mapChunkDictionary.Add(viewedChunkCoord,
                        new MapChunk(viewedChunkCoord, chunkSize, lods, 
                            transform, mapMaterial));
                }
            }
        }
    }

	
	// Update is called once per frame
	void Update ()
    {
        cameraPosition.x = transform.position.x;
        cameraPosition.y = transform.position.z;
        if ((cameraPositionOld - cameraPositionOld).sqrMagnitude > sqrMoveDelay)
        {
            cameraPositionOld = cameraPosition;
            UpdateVisibleChunks();
        }
	}

    public class MapChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MapData mapData;
        bool mapDataReceived;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        public LODData[] lods;
        public LODMesh[] lodMeshes;

        int prevLodIndex = -1;

        public MapChunk(Vector2 pos, int size, LODData[] lods, Transform parent,
            Material material)
        {
            this.lods = lods;
            position = pos * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 posV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Map Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;

            meshObject.transform.position = posV3 * scale;
            meshObject.transform.parent = null;
            meshObject.transform.localScale = Vector3.one * scale;
            meshObject.transform.rotation = Quaternion.identity;
            meshObject.transform.localRotation = Quaternion.identity;

            SetVisible(false);

            lodMeshes = new LODMesh[this.lods.Length];
            for (int i = 0; i < lodMeshes.Length; i++)
            {
                lodMeshes[i] = new LODMesh(lods[i].lod, UpdateMapChunk);
            }

            mapGen.requestMapData(position, OnMapDataReceived);
        }

        void OnMapDataReceived(MapData md)
        {
            mapData = md;
            mapDataReceived = true;
            Texture2D tex = TextureGenerator.generateFromColorMap(
                mapData.colorMap, MapGenerator.mapChunkSize, 
                MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = tex;
            UpdateMapChunk();
        }

        public void UpdateMapChunk()
        {
            if (mapDataReceived)
            {
                float dstFromEdge = Mathf.Sqrt(bounds.SqrDistance(cameraPosition));
                bool visible = dstFromEdge <= maxViewDst;

                if (visible)
                {
                    int lodIndex = 0;
                    for (int i = 0; i < lods.Length; i++)
                    {
                        if (dstFromEdge > lods[i].minViewableDistance)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (lodIndex != prevLodIndex)
                    {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            prevLodIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.requestMesh(mapData);
                        }
                    }
                    chunksVisibleLastUpdate.Add(this);
                }
                SetVisible(visible);
            }
        }

        public void SetVisible(bool v)
        {
            meshObject.SetActive(v);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }

    [System.Serializable]
    public struct LODData
    {
        public int lod;
        public float minViewableDistance;
    }

    public class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;

        public LODMesh(int lod, System.Action callback)
        {
            this.lod = lod;
            updateCallback = callback;
        }

        void OnMeshReceived(MeshData meshData)
        {
            hasMesh = true;
            mesh = meshData.ToMesh();
            updateCallback();
        }

        public void requestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGen.requestMeshData(mapData, OnMeshReceived, lod);
        }
    }
}