using MapCollections;
using System.Collections;
using System.Collections.Generic;
using System;
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
        perlin.Seed = (int) DateTime.Now.Ticks;
        Debug.Log("Seed: " + perlin.Seed);
        perlin.Frequency = 0.006;
        perlin.Lacunarity = 5.5;
        perlin.Persistence = 0.15;
    }

    public override float [,] GenerateMap(int width, int height)
    {
        float [,] map = new float[width, height];

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
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

        bool y_in_range;
        if (range_max == range_min) {
            y_in_range = true;
        }
        else {
            y_in_range = y <= range_max && y >= range_min;
        }

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

    private int width, height;
    private List<Boundary> boundaries;

    public TectonicTerrainGenerator ()
    {
        voronoiGenerator = new Voronoi();

        System.Random rnd = new System.Random((int) DateTime.Now.Ticks);

        voronoiGenerator.Seed = rnd.Next(int.MaxValue);
        voronoiGenerator.Frequency = 0.002;

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

        float [,] map = new float[width, height];


        /** find boundaries ! **/


        boundaries.Add(new Boundary(1.0f, 0.0f, 0.0f, (float) width));

        boundaries.Add(new Boundary(-1.0f, height, 0.0f, (float) width));



        //Debug.Log("First Boundary: " + boundaries[0]);
        //Debug.Log("Second Boundary: " + boundaries[1]);

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
                
                map[x, y] = h * scale;
            }
        }

        return map;
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
        
        while (counter < length) {
            cur_dist = boundaries[counter].distance_to((float) x, (float) y);

            if (cur_dist < smallest_dist && boundaries[counter].is_parallel((float) x, (float) y)) {
                smallest_dist = cur_dist;
            }

            counter++;
        }

        if (smallest_dist > range)
            return 0.0f;

        return (range - smallest_dist) / range;
    }
}
