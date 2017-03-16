using System;
using UnityEngine;
using System.Collections.Generic;

public class RainSimulator
{
	private float [,] map;
	private List<Point> visitedPoints;

	private RaindropManager manager;
	public RainSimulator (ref float [,] map, int width, int height, int seed)
	{
		this.map = map;
		manager = new RaindropManager (1, ref map, width, height, seed, addVisit);
		visitedPoints = new List<Point> ();

		Debug.Log ("Initialised RainSimulator");
		manager.simulate ();
		applyVisits ();
	}

	public void applyVisits()
	{
		foreach (Point p in visitedPoints) {
			map [p.x, p.y] -= 1.0f;
		}
	}

	public void addVisit(Point p)
	{
		Debug.Log ("Adding a visit");
		visitedPoints.Add (p);
	}
}


