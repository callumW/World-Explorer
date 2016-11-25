using MapCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;
using LibNoise.Generator;
using LibNoise.Operator;
using System;

public abstract class TerrainGenerator 
{
    abstract public float [,] GenerateMap(int width, int height);
}

public class PerlinTerrainGenerator : TerrainGenerator
{
    private Perlin perlin;

    public PerlinTerrainGenerator()
    {
        perlin = new Perlin();
        perlin.Seed = (int) DateTime.Now.Ticks;
        Debug.Log("Seed: " + perlin.Seed);
        perlin.Frequency = 0.006;
        perlin.Lacunarity = 5.5;
        perlin.Persistence = 0.15;
    }

    public override float [,] GenerateMap(int width, int height)
    {
        float [,] map = new float[width, height];
        float xCoord = 0.0f;
        float yCoord = 0.0f;

        //System.Random prng = new System.Random(124324234);
        //float xOffset = prng.Next(-100000, 100000);
        //float yOffset = prng.Next(-100000, 100000);

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                xCoord = x;
                yCoord = y;
                map[x, y] = (float) perlin.GetValue((float) x, (float) y, 0.5);
            }
        }

        return map;
    }

}

public class TectonicTerrainGenerator : TerrainGenerator
{
    private Voronoi voronoiGenerator;
    private Dictionary<float, int[]> plates;
    private float[,] map;
    private int width, height;
    public TectonicTerrainGenerator ()
    {
        voronoiGenerator = new Voronoi();
        voronoiGenerator.Seed = (int) DateTime.Now.Ticks;
        voronoiGenerator.Frequency = 0.002;
    }

    public override float [,] GenerateMap(int width, int height)
    {
        this.width = width;
        this.height = height;
        plates = new Dictionary<float, int[]>();

        map = new float[width, height];
        float [,] actualMap = new float[width, height];
        float xCoord = 0.0f;
        float yCoord = 0.0f;

        //System.Random prng = new System.Random(124324234);
        //float xOffset = prng.Next(-100000, 100000);
        //float yOffset = prng.Next(-100000, 100000);

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                xCoord = x;
                yCoord = y;
                map[x, y] = (float) voronoiGenerator.GetValue((float) x, (float) y, 0.5);

                if (!plates.ContainsKey(map[x, y]))
                {
                    int[] temp = new int[2];

                    temp[0] = 1;
                    temp[1] = 1;

                    plates.Add(map[x,y], temp);
                }
            }
        }

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                //actualMap[x, y] = 0.0f;
                if (isBoundaryPoint(x, y)) {
                    actualMap[x, y] = 1.0f;
                }
                else {
                    actualMap[x, y] = 0.0f;
                }
            }
        }

        Debug.Log("Number of plates: " + plates.Count);
        return actualMap;
    }

    private bool isBoundaryPoint(int x, int y)
    {
        for (int i = -1; i < 2; i++) {
            for (int j=-1; j < 2; j++) {
                
                int neighbourX = x + i;
                int neighbourY = y + j;

                if (i == 0 && j == 0) {

                }
                else if (neighbourX < 0 || neighbourY < 0 || neighbourX >= width
                    || neighbourY >= height) {
                
                }
                else if (plates[map[x, y]] != plates[map[neighbourX, neighbourY]]) {
                    return true;
                }

            }
        }

        return false;
    }

}
