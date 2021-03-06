using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Geometry : ScriptableObject
{
	//Flip an edge
	public static void FlipEdge(HalfEdge one)
	{
		//The data we need
		//This edge's triangle
		HalfEdge two = one.nextEdge;
		HalfEdge three = one.prevEdge;
		//The opposite edge's triangle
		HalfEdge four = one.oppositeEdge;
		HalfEdge five = one.oppositeEdge.nextEdge;
		HalfEdge six = one.oppositeEdge.prevEdge;
		//The vertices
		Vertex a = one.v;
		Vertex b = one.nextEdge.v;
		Vertex c = one.prevEdge.v;
		Vertex d = one.oppositeEdge.nextEdge.v;

		//Flip

		//Change vertex
		a.halfEdge = one.nextEdge;
		c.halfEdge = one.oppositeEdge.nextEdge;

		//Change half-edge
		//Half-edge - half-edge connections
		one.nextEdge = three;
		one.prevEdge = five;

		two.nextEdge = four;
		two.prevEdge = six;

		three.nextEdge = five;
		three.prevEdge = one;

		four.nextEdge = six;
		four.prevEdge = two;

		five.nextEdge = one;
		five.prevEdge = three;

		six.nextEdge = two;
		six.prevEdge = four;

		//Half-edge - vertex connection
		one.v = b;
		two.v = b;
		three.v = c;
		four.v = d;
		five.v = d;
		six.v = a;

		//Half-edge - triangle connection
		Triangle t1 = one.t;
		Triangle t2 = four.t;

		one.t = t1;
		three.t = t1;
		five.t = t1;

		two.t = t2;
		four.t = t2;
		six.t = t2;

		//Opposite-edges are not changing!

		//Triangle connection
		t1.v1 = b;
		t1.v2 = c;
		t1.v3 = d;

		t2.v1 = b;
		t2.v2 = d;
		t2.v3 = a;

		t1.halfEdge = three;
		t2.halfEdge = four;
	}

	//Is a triangle in 2d space oriented clockwise or counter-clockwise
	//https://math.stackexchange.com/questions/1324179/how-to-tell-if-3-connected-points-are-connected-clockwise-or-counter-clockwise
	//https://en.wikipedia.org/wiki/Curve_orientation
	public static bool IsTriangleOrientedClockwise(Vector2 p1, Vector2 p2, Vector2 p3)
	{
		bool isClockWise = true;

		float determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;

		if (determinant > 0f)
		{
			isClockWise = false;
		}

		return isClockWise;
	}

	//Orient triangles so they have the correct orientation
	public static void OrientTrianglesClockwise(List<Triangle> triangles)
	{
		for (int i = 0; i < triangles.Count; i++)
		{
			Triangle tri = triangles[i];

			Vector2 v1 = new Vector2(tri.v1.position.x, tri.v1.position.z);
			Vector2 v2 = new Vector2(tri.v2.position.x, tri.v2.position.z);
			Vector2 v3 = new Vector2(tri.v3.position.x, tri.v3.position.z);

			if (!Geometry.IsTriangleOrientedClockwise(v1, v2, v3))
			{
				tri.ChangeOrientation();
			}
		}
	}

	//From triangle where each triangle has one vertex to half edge
	public static List<HalfEdge> TransformFromTriangleToHalfEdge(List<Triangle> triangles)
	{
		//Make sure the triangles have the same orientation
		Geometry.OrientTrianglesClockwise(triangles);

		//First create a list with all possible half-edges
		List<HalfEdge> halfEdges = new List<HalfEdge>(triangles.Count * 3);

		for (int i = 0; i < triangles.Count; i++)
		{
			Triangle t = triangles[i];

			HalfEdge he1 = new HalfEdge(t.v1);
			HalfEdge he2 = new HalfEdge(t.v2);
			HalfEdge he3 = new HalfEdge(t.v3);

			he1.nextEdge = he2;
			he2.nextEdge = he3;
			he3.nextEdge = he1;

			he1.prevEdge = he3;
			he2.prevEdge = he1;
			he3.prevEdge = he2;

			//The vertex needs to know of an edge going from it
			he1.v.halfEdge = he2;
			he2.v.halfEdge = he3;
			he3.v.halfEdge = he1;

			//The face the half-edge is connected to
			t.halfEdge = he1;

			he1.t = t;
			he2.t = t;
			he3.t = t;

			//Add the half-edges to the list
			halfEdges.Add(he1);
			halfEdges.Add(he2);
			halfEdges.Add(he3);
		}

		//Find the half-edges going in the opposite direction
		for (int i = 0; i < halfEdges.Count; i++)
		{
			HalfEdge he = halfEdges[i];

			Vertex goingToVertex = he.v;
			Vertex goingFromVertex = he.prevEdge.v;

			for (int j = 0; j < halfEdges.Count; j++)
			{
				//Dont compare with itself
				if (i == j)
				{
					continue;
				}

				HalfEdge heOpposite = halfEdges[j];

				//Is this edge going between the vertices in the opposite direction
				if (goingFromVertex.position == heOpposite.v.position && goingToVertex.position == heOpposite.prevEdge.v.position)
				{
					he.oppositeEdge = heOpposite;

					break;
				}
			}
		}

		return halfEdges;
	}

	//Is a quadrilateral convex? Assume no 3 points are colinear and the shape doesnt look like an hourglass
	public static bool IsQuadrilateralConvex(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
	{
		bool isConvex = false;

		bool abc = Geometry.IsTriangleOrientedClockwise(a, b, c);
		bool abd = Geometry.IsTriangleOrientedClockwise(a, b, d);
		bool bcd = Geometry.IsTriangleOrientedClockwise(b, c, d);
		bool cad = Geometry.IsTriangleOrientedClockwise(c, a, d);

		if (abc && abd && bcd & !cad)
		{
			isConvex = true;
		}
		else if (abc && abd && !bcd & cad)
		{
			isConvex = true;
		}
		else if (abc && !abd && bcd & cad)
		{
			isConvex = true;
		}
		//The opposite sign, which makes everything inverted
		else if (!abc && !abd && !bcd & cad)
		{
			isConvex = true;
		}
		else if (!abc && !abd && bcd & !cad)
		{
			isConvex = true;
		}
		else if (!abc && abd && !bcd & !cad)
		{
			isConvex = true;
		}

		return isConvex;
	}
}
