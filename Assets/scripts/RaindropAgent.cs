using System;
using UnityEngine;

/*
 * Point Class
 * 
 */ 
public struct Point
{
	public int x, y;

	public Point(int px, int py)
	{
		x = px;
		y = py;
	}
}
/*
 * Raindrop Agent
 * Defines an agent which is used to help define rivers, lakes, and other water features.
 * 
 */
public class RaindropAgent
{
	private float [,] map;
	private int width;
	private int height;

	private Point currentPoint;

	private int lifespan;
	private bool alive;

	//filling value?s
	private float fillValue = 0.0f;

	public RaindropAgent (ref float [,] map, int width, int height, int startX, int startY, int lifespan)
	{
		this.map = map;

		Debug.Assert (startX < width && startX >= 0);
		Debug.Assert (startY < height && startY >= 0);

		currentPoint.x = startX;
		currentPoint.y = startY;

		if (lifespan > 0) {
			this.lifespan = lifespan;
			alive = true;
		}
		else {
			lifespan = 0;
			alive = false;
		}
	}

	public Point update()
	{
		if (alive) {
			//Check for lowest neighbour
			//move to it
			Point lowestPoint;
			lowestPoint.x = 0;
			lowestPoint.y = 0;

			float currentHeight = 100000.0f;

			if (currentPoint.x > 0) {
				if (currentPoint.y > 0) {
					// Top left
					if (map [currentPoint.x - 1, currentPoint.y - 1] < currentHeight + fillValue) {
						lowestPoint.x = currentPoint.x - 1;
						lowestPoint.y = currentPoint.y - 1;
						currentHeight = map [lowestPoint.x, lowestPoint.y];
						fillValue = 0.0f;
					}
				}

				if (currentPoint.y < height - 1) {
					//bottom left
					if (map [currentPoint.x - 1, currentPoint.y + 1] < currentHeight + fillValue) {
						lowestPoint.x = currentPoint.x - 1;
						lowestPoint.y = currentPoint.y + 1;

						currentHeight = map [lowestPoint.x, lowestPoint.y];
						fillValue = 0.0f;
					}
					//middle left
					if (map [currentPoint.x - 1, currentPoint.y] < currentHeight + fillValue) {
						lowestPoint.x = currentPoint.x - 1;
						lowestPoint.y = currentPoint.y;

						currentHeight = map [lowestPoint.x, lowestPoint.y];
						fillValue = 0.0f;
					}
				}
			}

			if (currentPoint.x < width - 1) {
				//right middle
				if (map [currentPoint.x + 1, currentPoint.y] < currentHeight + fillValue) {
					lowestPoint.x = currentPoint.x + 1;
					lowestPoint.y = currentPoint.y;

					currentHeight = map [lowestPoint.x, lowestPoint.y];
					fillValue = 0.0f;
				}

				if (currentPoint.y > 0) {
					//top right
					if (map [currentPoint.x + 1, currentPoint.y - 1] < currentHeight + fillValue) {
						lowestPoint.x = currentPoint.x + 1;
						lowestPoint.y = currentPoint.y - 1;

						currentHeight = map [lowestPoint.x, lowestPoint.y];
						fillValue = 0.0f;
					}
				}

				if (currentPoint.y < height - 1) {
					//bottom right
					if (map [currentPoint.x + 1, currentPoint.y + 1] < currentHeight + fillValue) {
						lowestPoint.x = currentPoint.x + 1;
						lowestPoint.y = currentPoint.y + 1;

						currentHeight = map [lowestPoint.x, lowestPoint.y];
						fillValue = 0.0f;
					}
				}
			}

			if (currentPoint.y < width - 1) {
				//bottom
				if (map [currentPoint.x, currentPoint.y + 1] < currentHeight + fillValue) {
					lowestPoint.x = currentPoint.x;
					lowestPoint.y = currentPoint.y + 1;

					currentHeight = map [lowestPoint.x, lowestPoint.y];
					fillValue = 0.0f;
				}
			}

			if (currentPoint.y > 0) {
				//Top
				if (map [currentPoint.x, currentPoint.y - 1] < currentHeight + fillValue) {
					lowestPoint.x = currentPoint.x;
					lowestPoint.y = currentPoint.y - 1;

					currentHeight = map [lowestPoint.x, lowestPoint.y];
					fillValue = 0.0f;
				}
			}

			currentPoint.x = lowestPoint.x;
			currentPoint.y = lowestPoint.y;

			if ((currentPoint.x == width || currentPoint.x == 0) && (currentPoint.y == height || currentPoint.y == 0)) {
				alive = false;
			}

			--lifespan;
			if (lifespan <= 0)
				alive = false;
		}
		return currentPoint;
	}

	public bool isAlive()
	{
		return alive;
	}
}


