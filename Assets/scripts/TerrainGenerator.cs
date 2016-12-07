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
    private Perlin perlinGenerator;
    private Dictionary<float, int[]> plates;
    private float[,] map;
    private int width, height;
    public TectonicTerrainGenerator ()
    {
        voronoiGenerator = new Voronoi();


        //voronoiGenerator.Seed = (int) DateTime.Now.Ticks;
        voronoiGenerator.Seed = 123141;
        voronoiGenerator.Frequency = 0.002;
        //voronoiGenerator.UseDistance = true;

        perlinGenerator = new Perlin();
        perlinGenerator.Seed = voronoiGenerator.Seed;
        perlinGenerator.Frequency = 0.006;
        perlinGenerator.Lacunarity = 5.5;
        perlinGenerator.Persistence = 0.15;
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
        /*
        ArrayList boundary_points = new ArrayList(); 
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

        voronoiGenerator.UseDistance = true;

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                //actualMap[x, y] = 0.0f;

                if (isBoundaryPoint(x, y)) {
                    boundary_points.Add(new Vector2(x, y));
                }
                //actualMap[x, y] = (float) (voronoiGenerator.GetValue((float) x, (float) y, 0.5)); // + perlinGenerator.GetValue( (float) x, (float) y, 0.5));
            }
        }
        */
        //Debug.Log("Number of boundary points: " + boundary_points.Count);

        float h = 0.0f;
        float scale = 1.8f;
        float gradient_level = 0.0f;
        float gradient_curve = 0.0f;
        float perlin_value = 0.0f;
        float weight_coeff = 0.0f;
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {

                perlin_value = get_perlin_value(x, y);

                weight_coeff = get_weight(x, y);

                h = perlin_value * weight_coeff;

                if (h < 0.0f)
                    h = 0.0f;
                else if (h > 1.0f)
                    h = 1.0f;
                
                actualMap[x, y] = h * scale;
            }
        }

        //Debug.Log("Number of plates: " + plates.Count);
        return actualMap;
    }

    private float get_perlin_value(int x, int y)
    {
        return (float) Math.Abs(perlinGenerator.GetValue((double) x, (double) y, 0.05));
    }

    private float get_weight(int x, int y)
    {
        float range = 100.0f;
        //return 1.0f;
        /*
        if (x > 10 && x < width - 10) {
            if (y > 10 && y < height - 10) {
                return 0.0f;
            }
        }
        */
        if (x == 100 || y == 100 || x == width - 100 || y == height - 100)
            return 1.0f;
        
        float left_dist = Math.Abs(x - 100);
        float right_dist = Math.Abs(width - 100 - x);
        float top_dist = Math.Abs(y - 100);
        float bottom_dist = Math.Abs(height - 100 - y);
        float smallest_dist = left_dist;

        if (smallest_dist > right_dist)
            smallest_dist = right_dist;
        if (smallest_dist > top_dist)
            smallest_dist = top_dist;
        if (smallest_dist > bottom_dist)
            smallest_dist = bottom_dist;

        if (smallest_dist > range)
            return 0.1f;
        
        return (100.0f - smallest_dist) / 100.0f;
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

    float euclidian_distance(int x1, int y1, int x2, int y2) {
        int distanceX = x1 - x2;
        int distanceY = y1 - y2;

        return (float) Math.Sqrt((double) (distanceX*distanceX + distanceY*distanceY));
    }

}
