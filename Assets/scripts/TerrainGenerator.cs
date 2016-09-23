using MapCollections;
using System.Collections;
using UnityEngine;
using LibNoise;
using LibNoise.Generator;
using LibNoise.Operator;

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
        perlin.Frequency = 0.006;
        perlin.Lacunarity = 5.5;
        perlin.Persistence = 0.15;

    }
    public override float [,] GenerateMap(int width, int height)
    {
        float [,] map = new float[width, height];
        float xCoord = 0.0f;
        float yCoord = 0.0f;

        System.Random prng = new System.Random(124324234);
        float xOffset = prng.Next(-100000, 100000);
        float yOffset = prng.Next(-100000, 100000);

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                xCoord = x + xOffset;
                yCoord = y + yOffset;
                map[x, y] = (float) perlin.GetValue((float) x, (float) y, 0.5);
            }
        }

        return map;
    }

}
