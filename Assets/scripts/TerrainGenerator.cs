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

public abstract class Line
{
    public abstract float get_distance(float x, float y);
}

public class VerticalLine : Line
{
    private float x_value;
    private float minY, maxY;
    public VerticalLine(float x, float minY, float maxY)
    {
        x_value = x;
        this.minY = minY;
        this.maxY = maxY;
    }

    
    public override float get_distance(float x, float y)
    {
        if (x >= minY && x <= maxY)
            return Math.Abs(x - x_value);
        else
            return -1.0f;
    }
}

public class HorizontalLine : Line
{
    private float y_value;
    private float minX, maxX;

    public HorizontalLine(float y, float minX, float maxX )
    {
        y_value = y;
        this.minX = minX;
        this.maxX = maxX;
    }

    public override float get_distance(float x, float y)
    {
        if (x >= minX && x <= maxX)
            return Math.Abs(y - y_value);
        else
            return -1.0f;
    }
}

public class LinearLine : Line
{
    private float intersect;
    private float gradient;
    private float minX, maxX;
    private float minY, maxY;

    private float perpendicular;
    private float minRangeIntersect;
    private float maxRangeIntersect;

    public LinearLine(float minX, float minY,
        float maxX, float maxY)
    {
        // Need to calculate the perpendicular gradient, the minimum range line
        // and the maximum range line

        gradient = (maxY - minY) / (maxX - minX);
        intersect = maxY - gradient * maxX;
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;

        perpendicular = -1.0f / gradient;

        minRangeIntersect = minY - perpendicular * minX;
        maxRangeIntersect = maxY - perpendicular * maxX;
    }

    public override float get_distance(float x, float y)
    {
        // First we need to check we are within the domain of the line
        // Then we need to calculate the point on the line which is closest
        // to the point given.
        // Finally we calculate the euclidian distance between those two points

        

        if ( y >= minRangeIntersect + perpendicular * x) {
            if ( y <= maxRangeIntersect + perpendicular * x) {
                float perpendicularIntersect = y - perpendicular * x;
                float closestX = (perpendicularIntersect - intersect) / (gradient - perpendicular);
                float closestY = perpendicularIntersect + perpendicular * closestX;

                return (float) Math.Sqrt((closestX - x) * (closestX - x) + (closestY - y) * (closestY -y));
            }
            else
            {
                //we are above the maxrange line therefore maxX, maxY is the
                //closest point

                return (float)Math.Sqrt((x - maxX) * (x - maxX) + (y - maxY) * (y - maxY));
            }
        }
        else
        {
            //we are below the minimum bounding line therefore minX, minY is
            // the closest point.

            return (float) Math.Sqrt((x - minX) * (x - minX) + (y - minY) * (y - minY));
        }

        return -1.0f;

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
    private Perlin baseGroundGenerator;
    private RidgedMultifractal fractalGenerator;
    private int seed;

    private int width, height;
    private List<Line> boundaries;

    //private TectonicFault faultLine;
    private Lithosphere lithosphere;

    public TectonicTerrainGenerator ()
    {
        voronoiGenerator = new Voronoi();
        seed = (int)DateTime.Now.Ticks;
        System.Random rnd = new System.Random(seed);

        voronoiGenerator.Seed = rnd.Next(int.MaxValue);
        voronoiGenerator.Frequency = 0.002;

        perlinGenerator = new Perlin();
        perlinGenerator.Seed = rnd.Next(int.MaxValue);
        perlinGenerator.Frequency = 0.006;
        perlinGenerator.Lacunarity = 5.5;
        perlinGenerator.Persistence = 0.15;

        baseGroundGenerator = new Perlin();
        baseGroundGenerator.Seed = rnd.Next(int.MaxValue);
        baseGroundGenerator.Frequency = 0.001;
        baseGroundGenerator.Persistence = 0.05;

        fractalGenerator = new RidgedMultifractal();
        fractalGenerator.Seed = rnd.Next(int.MaxValue);
        //fractalGenerator.Lacunarity = 2.5;
        fractalGenerator.OctaveCount = 4;
        fractalGenerator.Frequency = 0.01;
        boundaries = new List<Line>();

    }

    public override float [,] GenerateMap(int width, int height)
    {
        
        this.width = width;
        this.height = height;

        float [,] map = new float[width, height];

        //faultLine = new TectonicFault(width, height);
        Debug.Log("Newing Lithosphere");
        lithosphere = new Lithosphere (width, height, seed);

        /** find boundaries ! **/

        /*
        boundaries.Add(new LinearLine(0.0f, 0.0f, (float) width, (float) height));

        boundaries.Add(new LinearLine((float) width, 0.0f, 0.0f, (float) height));

        boundaries.Add(new HorizontalLine(height / 2.0f, 0.0f, (float) width));
        boundaries.Add(new VerticalLine(width / 2.0f, 0.0f, (float) height));
        */


        //Debug.Log("First Boundary: " + boundaries[0]);
        //Debug.Log("Second Boundary: " + boundaries[1]);

        float h = 0.0f;
        float scale = 1.8f;
        float perlin_value = 0.0f;
        float base_value = 0.0f;
        float fractal_value = 0.0f;
        float weight_coeff = 0.0f;
        float base_coeff = 0.9f;
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {

                fractal_value = get_fractal_value(x, y);
                perlin_value = get_perlin_value(x, y);
                base_value = get_base_perlin_value(x, y);

                weight_coeff = get_weight(x, y);

                h = fractal_value * weight_coeff + perlin_value * base_coeff + base_value;
                
                map[x, y] = h * scale;
            }
        }

        return map;
    }

    private float get_perlin_value(int x, int y)
    {
        return (float) ((perlinGenerator.GetValue((double) x, (double) y, 0.05) / 2.0 + 0.5));
    }

    private float get_base_perlin_value(int x, int y)
    {
        return (float) ((baseGroundGenerator.GetValue((double) x, (double) y, 0.05) / 2.0 + 0.5));
    }

    private float get_fractal_value(int x, int y)
    {
        return (float) ((fractalGenerator.GetValue((double) x, (double) y, 0.5) / 2.0) + 0.5);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private float get_weight(int x, int y)
    {
        float range = 200.0f;
        float smallest_dist = -1.0f;
        bool distance_found = false;
        int numFaults = lithosphere.getNumberOfFaults ();
        for (int i = 0; i < numFaults; i++) {
            TectonicFault faultLine = lithosphere.getFault (i);

            int length = faultLine.length ();
            int counter = 0;
            if (!distance_found)
            {
                smallest_dist = faultLine.get(counter).get_distance((float)x, (float)y);
                distance_found = true;
            }

            /*
            do {
                
                if (faultLine.get (counter).get_distance ((float)x, (float)y) != -1.0f) {
                    smallest_dist = faultLine.get (counter).get_distance ((float)x, (float)y);
                    break;
                }
                counter++;
            } while (counter < length);

            if (smallest_dist == -1.0f)
                return 0.0f;
                */

            float cur_dist;
            counter++;
            
            while (counter < length) {
                cur_dist = faultLine.get (counter).get_distance ((float)x, (float)y);

                if (cur_dist < smallest_dist && cur_dist != -1.0f) {
                    smallest_dist = cur_dist;
                }

                counter++;
            }
        }

        if (smallest_dist > range)
            return 0.0f;

        return (range - smallest_dist) / range;
    }
}

public class TectonicFault 
{
    private List<Line> lines;
    //Type of boundary?
    //range?

    private System.Random rndGenerator;

    public TectonicFault(int width, int height, int startX, int startY, int seed)
    {
        Debug.Log("Creating Fault: \nStartX = " + startX + " StartY = " + startY
            + "\nSeed: " + seed);
        lines = new List<Line>();
        rndGenerator = new System.Random(seed);
        int length = 100;

        int baseAngle = 0;

        if (startX == 0) {
            baseAngle = 0;
        } else if (startX == width) {
            baseAngle = 270;
        } else if (startY == 0) {
            baseAngle = 90;
        } else if (startY == height) {
            baseAngle = 180;
        }

        Debug.Log("\nBase Angle: " + baseAngle);
        //int baseAngle = getAngle(45, 135);
        int minAngle = baseAngle - 45;
        int maxAngle = baseAngle + 45;
        int currentAngle;

        int endX, endY;

        bool end = false;
        while (!end) {
            currentAngle = getAngle(minAngle, maxAngle);

            endX = (int)(startX + length * System.Math.Cos(currentAngle * (System.Math.PI / 180)));

            endY = (int)(startY + length * System.Math.Sin(currentAngle * (System.Math.PI / 180)));

            if (endX < 0) {
                endX = 0;
                end = true;
            }
            else if (endX > width) {
                endX = width;
                end = true;
            }

            if (endY < 0) {
                endY = 0;
                end = true;
            }
            else if (endY > height) {
                endY = height;
                end = true;
            }

            Debug.Assert(endX != startX || endY != startY);
            //Debug.Assert(endY != startY);

            if (endX == startX && endY == startY) {
                Debug.Log("X: " + startX + " Y: " + startY);
                Debug.Log("base angle: " + baseAngle + " Current angle: " + currentAngle);
            }

            float gradient = 0.0f;
            if (endX != startX)
                gradient = (endY - startY) / (endX - startX);

            if (gradient < 0.1f && gradient > -0.1f)
            {
                //Debug.Log("")
                lines.Add(new HorizontalLine(startY, startX, endX));
                endY = startY;
            }
            else if (gradient > 0.9f && gradient < -0.9f)
            {
                lines.Add(new VerticalLine(startX, startY, endY));
                endX = startX;
            }
            else
            {
                lines.Add(new LinearLine(startX, startY, endX, endY));
            }

            startX = endX;
            startY = endY;
        }
    }

    private int getAngle(int min, int max)
    {
        return rndGenerator.Next(min, max);
    }

    public Line get(int i) {
        return lines[i];
    }

    public int length() {
        return lines.Count;
    }
    
    public void print()
    {
        //for ()
    }

}

public class Lithosphere
{
    private List<TectonicFault> faults;
    private int numberOfFaults;

    private System.Random rndGenerator;
    public Lithosphere(int width, int height, int seed)
    {
        Debug.Log ("Generating Lithosphere");
        rndGenerator = new System.Random (seed);
        faults = new List<TectonicFault> ();
        numberOfFaults = rndGenerator.Next (3) + 1;

        Debug.Log ("Generating " + numberOfFaults + " Faults.");
        
        int side = 0;
        int startX = 0;
        int startY = 0;
        for (int i = 0; i < numberOfFaults; i++) {
            side = rndGenerator.Next (3);


            switch (side) {
            case 0:
                //left side
                startX = 0;
                startY = rndGenerator.Next (height / 3, height * 2 / 3);
                break;
            case 1:
                //top side
                startX = rndGenerator.Next (width / 3, width * 2 / 3);
                startY = 0;
                break;
            case 2:
                //right side
                startX = width;
                startY = rndGenerator.Next (height / 3, height * 2 / 3);
                break;
            case 3:
                //bottom side
                startX = rndGenerator.Next (width /3, width * 2 / 3);
                startY = height;
                break;
            default:
                startX = 0;
                startY = height / 2;
                break;
            }

            faults.Add(new TectonicFault(width, height, startX, startY, rndGenerator.Next()));

        }
        
        //faults.Add(new TectonicFault(width, height, 1000, 0, seed));
        //faults.Add(new TectonicFault(width, height, 0, 1000, seed));
        //numberOfFaults = 2;
    }

    public int getNumberOfFaults() {
        return numberOfFaults;
    }

    public TectonicFault getFault(int i) {
        if (i >= 0 && i < numberOfFaults) {
            return faults [i];
        } else {
            throw new Exception ();
        }
    }

    public void printLines()
    {
        for (int i = 0; i < numberOfFaults; i++)
        {
            faults[i].print();
        }
    }
}
