using UnityEditor;
using UnityEngine;


public class Intersections : ScriptableObject
{
	//From http://totologic.blogspot.se/2014/01/accurate-point-in-triangle-test.html
	//p is the testpoint, and the other points are corners in the triangle
	public static bool IsPointInTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
	{
		bool isWithinTriangle = false;

		//Based on Barycentric coordinates
		float denominator = ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));

		float a = ((p2.y - p3.y) * (p.x - p3.x) + (p3.x - p2.x) * (p.y - p3.y)) / denominator;
		float b = ((p3.y - p1.y) * (p.x - p3.x) + (p1.x - p3.x) * (p.y - p3.y)) / denominator;
		float c = 1 - a - b;

		//The point is within the triangle or on the border if 0 <= a <= 1 and 0 <= b <= 1 and 0 <= c <= 1
		//if (a >= 0f && a <= 1f && b >= 0f && b <= 1f && c >= 0f && c <= 1f)
		//{
		//    isWithinTriangle = true;
		//}

		//The point is within the triangle
		if (a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f)
		{
			isWithinTriangle = true;
		}

		return isWithinTriangle;
	}

	//Is a point d inside, outside or on the same circle as a, b, c
	//https://gamedev.stackexchange.com/questions/71328/how-can-i-add-and-subtract-convex-polygons
	//Returns positive if inside, negative if outside, and 0 if on the circle
	public static float IsPointInsideOutsideOrOnCircle(Vector2 aVec, Vector2 bVec, Vector2 cVec, Vector2 dVec)
	{
		//This first part will simplify how we calculate the determinant
		float a = aVec.x - dVec.x;
		float d = bVec.x - dVec.x;
		float g = cVec.x - dVec.x;

		float b = aVec.y - dVec.y;
		float e = bVec.y - dVec.y;
		float h = cVec.y - dVec.y;

		float c = a * a + b * b;
		float f = d * d + e * e;
		float i = g * g + h * h;

		float determinant = (a * e * i) + (b * f * g) + (c * d * h) - (g * e * c) - (h * f * a) - (i * d * b);

		return determinant;
	}

	public static bool AreLinesIntersecting(Vector2 l1_p1, Vector2 l1_p2, Vector2 l2_p1, Vector2 l2_p2, bool shouldIncludeEndPoints)
	{
		//To avoid floating point precision issues we can add a small value
		float epsilon = 0.00001f;

		bool isIntersecting = false;

		float denominator = (l2_p2.y - l2_p1.y) * (l1_p2.x - l1_p1.x) - (l2_p2.x - l2_p1.x) * (l1_p2.y - l1_p1.y);

		//Make sure the denominator is > 0, if not the lines are parallel
		if (denominator != 0f)
		{
			float u_a = ((l2_p2.x - l2_p1.x) * (l1_p1.y - l2_p1.y) - (l2_p2.y - l2_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;
			float u_b = ((l1_p2.x - l1_p1.x) * (l1_p1.y - l2_p1.y) - (l1_p2.y - l1_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;

			//Are the line segments intersecting if the end points are the same
			if (shouldIncludeEndPoints)
			{
				//Is intersecting if u_a and u_b are between 0 and 1 or exactly 0 or 1
				if (u_a >= 0f + epsilon && u_a <= 1f - epsilon && u_b >= 0f + epsilon && u_b <= 1f - epsilon)
				{
					isIntersecting = true;
				}
			}
			else
			{
				//Is intersecting if u_a and u_b are between 0 and 1
				if (u_a > 0f + epsilon && u_a < 1f - epsilon && u_b > 0f + epsilon && u_b < 1f - epsilon)
				{
					isIntersecting = true;
				}
			}
		}

		return isIntersecting;
	}
	public static bool AreEdgesIntersecting(Edge edge1, Edge edge2)
	{
		Vector2 l1_p1 = new Vector2(edge1.v1.position.x, edge1.v1.position.z);
		Vector2 l1_p2 = new Vector2(edge1.v2.position.x, edge1.v2.position.z);

		Vector2 l2_p1 = new Vector2(edge2.v1.position.x, edge2.v1.position.z);
		Vector2 l2_p2 = new Vector2(edge2.v2.position.x, edge2.v2.position.z);

		bool isIntersecting = Intersections.AreLinesIntersecting(l1_p1, l1_p2, l2_p1, l2_p2, true);

		return isIntersecting;
	}
}
