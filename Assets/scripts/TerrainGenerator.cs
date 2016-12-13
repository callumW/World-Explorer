using MapCollections;
using System.Collections;
using System.Collections.Generic;
using System;
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

public class Boundary
{
    private float gradient;
    private float perpendicular_gradient;
    private float intersect;
    private float domain_min, domain_max;
    private float range_min, range_max;

    public Boundary(float gradient, float intersect, float min, float max)
    {
        this.gradient = gradient;
        this.intersect = intersect;
        this.domain_min = min;
        this.domain_max = max;

        if (gradient < 0) {
            range_max = gradient * min + intersect;
            range_min = gradient * max + intersect;
        }
        else {
            range_min = gradient * min + intersect;
            range_max = gradient * max + intersect;
        }

        perpendicular_gradient = -(1.0f / gradient);
    }

    public float distance_to(float x, float y)
    {
        float perp_intersect = y - (perpendicular_gradient * x);
        float x_on_line = (perp_intersect - intersect) / (gradient - perpendicular_gradient);
        float y_on_line = (x_on_line * gradient) + intersect;

        return euclidian_distance(x_on_line, y_on_line, x, y);
    }

    public bool is_parallel(float x, float y)
    {
        bool x_in_range = x <= domain_max && x >= domain_min;
        bool y_in_range = y <= range_max && y >= range_min;
        return x_in_range && y_in_range;
    }

    float euclidian_distance(float x1, float y1, float x2, float y2) {
        float distanceX = x1 - x2;
        float distanceY = y1 - y2;

        return (float) Math.Sqrt((double) (distanceX*distanceX + distanceY*distanceY));
    }

    public override string ToString() {
        string mesg = "line: y = " + gradient + "x + " + intersect;
        mesg += "\nX in " + domain_min + " - " + domain_max;
        mesg += "\nY in " + range_min + " - " + range_max;
        return mesg;
    }
}

public class TectonicTerrainGenerator : TerrainGenerator
{
    private Voronoi voronoiGenerator;
    private Perlin perlinGenerator;
    private RidgedMultifractal fractalGenerator;
    private Dictionary<float, int[]> plates;
    private float[,] voronoi_map;
    private int width, height;
    private List<Boundary> boundaries;

    public TectonicTerrainGenerator ()
    {
        voronoiGenerator = new Voronoi();

        System.Random rnd = new System.Random((int) DateTime.Now.Ticks);

        voronoiGenerator.Seed = rnd.Next(int.MaxValue);
        //voronoiGenerator.Seed = 123141;
        voronoiGenerator.Frequency = 0.002;
        //voronoiGenerator.UseDistance = true;

        perlinGenerator = new Perlin();
        perlinGenerator.Seed = rnd.Next(int.MaxValue);
        perlinGenerator.Frequency = 0.006;
        perlinGenerator.Lacunarity = 5.5;
        perlinGenerator.Persistence = 0.15;

        fractalGenerator = new RidgedMultifractal();
        fractalGenerator.Seed = rnd.Next(int.MaxValue);
        //fractalGenerator.Lacunarity = 2.5;
        fractalGenerator.OctaveCount = 4;
        fractalGenerator.Frequency = 0.01;
        boundaries = new List<Boundary>();

    }

    public override float [,] GenerateMap(int width, int height)
    {
        this.width = width;
        this.height = height;
        plates = new Dictionary<float, int[]>();

        float [,] actualMap = new float[width, height];


        /** find boundaries ! **/
        voronoi_map = new float[width, height];


        boundaries.Add(new Boundary(1.0f, 0.0f, 0.0f, (float) width));
        boundaries.Add(new Boundary(-1.0f, height, 0.0f, (float) width));



        //Debug.Log("First Boundary: " + boundaries[0]);
        //Debug.Log("Second Boundary: " + boundaries[1]);

        ArrayList boundary_points = new ArrayList(); 
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                voronoi_map[x, y] = (float) voronoiGenerator.GetValue((float) x, (float) y, 0.5);

                if (!plates.ContainsKey(voronoi_map[x, y]))
                {
                    int[] temp = new int[2];

                    temp[0] = 1;
                    temp[1] = 1;

                    plates.Add(voronoi_map[x,y], temp);
                }
            }
        }

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {

                if (isBoundaryPoint(x, y)) {
                    boundary_points.Add(new Vector2(x, y));
                }
            }
        }

        //Debug.Log("Number of boundary points: " + boundary_points.Count);

        float h = 0.0f;
        float scale = 1.8f;
        float perlin_value = 0.0f;
        float fractal_value = 0.0f;
        float weight_coeff = 0.0f;
        float base_coeff = 0.1f;
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {

                fractal_value = get_fractal_value(x, y);
                perlin_value = get_perlin_value(x, y);

                weight_coeff = get_weight(x, y);

                h = fractal_value * weight_coeff + perlin_value * base_coeff;
                
                actualMap[x, y] = h * scale;
            }
        }

        return actualMap;
    }

    private float get_perlin_value(int x, int y)
    {
        return (float) ((perlinGenerator.GetValue((double) x, (double) y, 0.05) / 2.0 + 0.5));
    }

    private float get_fractal_value(int x, int y)
    {
        return (float) ((fractalGenerator.GetValue((double) x, (double) y, 0.5) / 2.0) + 0.5);
    }

    private float get_weight(int x, int y)
    {
        float range = 200.0f;

        int length = boundaries.Count;
        int counter = 0;
        float smallest_dist = -1.0f;
        do {
            
            if (boundaries[counter].is_parallel((float) x, (float) y)) {
                smallest_dist = boundaries[counter].distance_to((float) x, (float) y);
                break;
            }
            counter++;
        }
        while (counter < length);

        if (smallest_dist == -1.0f)
            return 0.0f;

        float cur_dist;
        counter++;

        if (x > width - 4)
            x = x;
        
        while (counter < length) {
            cur_dist = boundaries[counter].distance_to((float) x, (float) y);

            if (cur_dist < smallest_dist && boundaries[counter].is_parallel((float) x, (float) y)) {
                smallest_dist = cur_dist;
                //Debug.Log("I'm here!");
            }

            counter++;
        }

        if (smallest_dist > range)
            return 0.0f;

        return (range - smallest_dist) / range;
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
                else if (plates[voronoi_map[x, y]] != plates[voronoi_map[neighbourX, neighbourY]]) {
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
