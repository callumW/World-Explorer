using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * Raindrop Manager
 * Defines a manager class for raindrop agents.
 */
public class RaindropManager
{
	//List of Raindrops
	private List<RaindropAgent> agents;
	//List of visited coords
	private System.Random rndGenerator;

	int dropNumber;
	float[,] map;
	int width;
	int height;
	int seed;

	Action<Point> callback;

	public RaindropManager (int numOfRaindrops, ref float[,] map, int width, int height, int seed, Action<Point> callback)
	{
		dropNumber = numOfRaindrops;
		this.seed = seed;
		this.width = width;
		this.height = height;
		this.map = map;
		this.callback = callback;

		rndGenerator = new System.Random (seed);

		agents = new List<RaindropAgent> ();

		Debug.Log ("Creating Raindrops");
		int x, y;
		for (int i = 0; i < dropNumber; i++) {
			x = rndGenerator.Next (0, width);
			y = rndGenerator.Next (0, height);
			agents.Add( new RaindropAgent(ref map, width, height, x, y, 100));
		}
		Debug.Log ("Raindrops Created.");
	}

	public void simulate()
	{
		Debug.Log ("Running Simulaion");
		Point p;
		for (int i = 0; i < dropNumber; i++) {
			while (agents [i].isAlive ()) {
				p = agents [i].update ();
				Debug.Log ("Updating visited points");

				if (agents [i].stationary) {

				}
				callback (p);
			}
		}
	}
}


