using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Triangulation : ScriptableObject
{
    //This assumes that we have a polygon and now we want to triangulate it
    //The points on the polygon should be ordered counter-clockwise
    //This alorithm is called ear clipping and it's O(n*n) Another common algorithm is dividing it into trapezoids and it's O(n log n)
    //One can maybe do it in O(n) time but no such version is known
    //Assumes we have at least 3 points
    public static List<Triangle> TriangulateConcavePolygon(List<Vector3> points)
    {
        //The list with triangles the method returns
        List<Triangle> triangles = new List<Triangle>();

        //If we just have three points, then we dont have to do all calculations
        if (points.Count == 3)
        {
            triangles.Add(new Triangle(points[0], points[1], points[2]));

            return triangles;
        }

        //Step 1. Store the vertices in a list and we also need to know the next and prev vertex
        List<Vertex> vertices = new List<Vertex>();

        for (int i = 0; i < points.Count; i++)
        {
            vertices.Add(new Vertex(points[i]));
        }

        //Find the next and previous vertex
        for (int i = 0; i < vertices.Count; i++)
        {
            int nextPos = MathUtility.ClampListIndex(i + 1, vertices.Count);

            int prevPos = MathUtility.ClampListIndex(i - 1, vertices.Count);

            vertices[i].prevVertex = vertices[prevPos];

            vertices[i].nextVertex = vertices[nextPos];
        }

        //Step 2. Find the reflex (concave) and convex vertices, and ear vertices
        for (int i = 0; i < vertices.Count; i++)
        {
            CheckIfReflexOrConvex(vertices[i]);
        }

        //Have to find the ears after we have found if the vertex is reflex or convex
        List<Vertex> earVertices = new List<Vertex>();

        for (int i = 0; i < vertices.Count; i++)
        {
            IsVertexEar(vertices[i], vertices, earVertices);
        }

        //Step 3. Triangulate!
        while (true)
        {
            //This means we have just one triangle left
            if (vertices.Count == 3)
            {
                //The final triangle
                triangles.Add(new Triangle(vertices[0], vertices[0].prevVertex, vertices[0].nextVertex));

                break;
            }

            //Make a triangle of the first ear
            Vertex earVertex = earVertices[0];

            Vertex earVertexPrev = earVertex.prevVertex;
            Vertex earVertexNext = earVertex.nextVertex;

            Triangle newTriangle = new Triangle(earVertex, earVertexPrev, earVertexNext);

            triangles.Add(newTriangle);

            //Remove the vertex from the lists
            earVertices.Remove(earVertex);

            vertices.Remove(earVertex);

            //Update the previous vertex and next vertex
            earVertexPrev.nextVertex = earVertexNext;
            earVertexNext.prevVertex = earVertexPrev;

            //...see if we have found a new ear by investigating the two vertices that was part of the ear
            CheckIfReflexOrConvex(earVertexPrev);
            CheckIfReflexOrConvex(earVertexNext);

            earVertices.Remove(earVertexPrev);
            earVertices.Remove(earVertexNext);

            IsVertexEar(earVertexPrev, vertices, earVertices);
            IsVertexEar(earVertexNext, vertices, earVertices);
        }

        Debug.Log(triangles.Count);

        return triangles;
    }



    //Check if a vertex if reflex or convex, and add to appropriate list
    private static void CheckIfReflexOrConvex(Vertex v)
    {
        v.isReflex = false;
        v.isConvex = false;

        //This is a reflex vertex if its triangle is oriented clockwise
        Vector2 a = v.prevVertex.GetPos2D_XY();
        Vector2 b = v.GetPos2D_XY();
        Vector2 c = v.nextVertex.GetPos2D_XY();

        if (Geometry.IsTriangleOrientedClockwise(a, b, c))
        {
            v.isReflex = true;
        }
        else
        {
            v.isConvex = true;
        }
    }

    //Check if a vertex is an ear
    private static void IsVertexEar(Vertex v, List<Vertex> vertices, List<Vertex> earVertices)
    {
        //A reflex vertex cant be an ear!
        if (v.isReflex)
        {
            return;
        }

        //This triangle to check point in triangle
        Vector2 a = v.prevVertex.GetPos2D_XY();
        Vector2 b = v.GetPos2D_XY();
        Vector2 c = v.nextVertex.GetPos2D_XY();

        bool hasPointInside = false;

        for (int i = 0; i < vertices.Count; i++)
        {
            //We only need to check if a reflex vertex is inside of the triangle
            if (vertices[i].isReflex)
            {
                Vector2 p = vertices[i].GetPos2D_XY();

                //This means inside and not on the hull
                if (Intersections.IsPointInTriangle(a, b, c, p))
                {
                    hasPointInside = true;

                    break;
                }
            }
        }

        if (!hasPointInside)
        {
            earVertices.Add(v);
        }
    }

    //Sort the points along one axis. The first 3 points form a triangle. Consider the next point and connect it with all
    //previously connected points which are visible to the point. An edge is visible if the center of the edge is visible to the point.

    public static List<Triangle> IncrementalTriangulation(List<Vertex> points)
    {
        List<Triangle> triangles = new List<Triangle>();

        //Sort the points along x-axis
        //OrderBy is always soring in ascending order - use OrderByDescending to get in the other order
        points.Sort((x,y) => x.position.x.CompareTo(y.position.x));
        
        //The first 3 vertices are always forming a triangle
        Triangle newTriangle = new Triangle(points[0].position, points[1].position, points[2].position);

        triangles.Add(newTriangle);

        //All edges that form the triangles, so we have something to test against
        List<Edge> edges = new List<Edge>();

        edges.Add(new Edge(newTriangle.v1, newTriangle.v2));
        edges.Add(new Edge(newTriangle.v2, newTriangle.v3));
        edges.Add(new Edge(newTriangle.v3, newTriangle.v1));

        //Add the other triangles one by one
        //Starts at 3 because we have already added 0,1,2
        for (int i = 3; i < points.Count; i++)
        {
            Vector3 currentPoint = points[i].position;

            //The edges we add this loop or we will get stuck in an endless loop
            List<Edge> newEdges = new List<Edge>();

            //Is this edge visible? We only need to check if the midpoint of the edge is visible 
            for (int j = 0; j < edges.Count; j++)
            {
                Edge currentEdge = edges[j];

                Vector3 midPoint = (currentEdge.v1.position + currentEdge.v2.position) / 2f;

                Edge edgeToMidpoint = new Edge(currentPoint, midPoint);

                //Check if this line is intersecting
                bool canSeeEdge = true;

                for (int k = 0; k < edges.Count; k++)
                {
                    //Dont compare the edge with itself
                    if (k == j)
                    {
                        continue;
                    }

                    if (Intersections.AreEdgesIntersecting(edgeToMidpoint, edges[k]))
                    {
                        canSeeEdge = false;

                        break;
                    }
                }

                //This is a valid triangle
                if (canSeeEdge)
                {
                    Edge edgeToPoint1 = new Edge(currentEdge.v1, new Vertex(currentPoint));
                    Edge edgeToPoint2 = new Edge(currentEdge.v2, new Vertex(currentPoint));

                    newEdges.Add(edgeToPoint1);
                    newEdges.Add(edgeToPoint2);

                    Triangle newTri = new Triangle(edgeToPoint1.v1, edgeToPoint1.v2, edgeToPoint2.v1);

                    triangles.Add(newTri);
                }
            }

            for (int j = 0; j < newEdges.Count; j++)
            {
                edges.Add(newEdges[j]);
            }
        }

        return triangles;
    }

    //Alternative 1. Triangulate with some algorithm - then flip edges until we have a delaunay triangulation
    public static List<Triangle> TriangulateByFlippingEdges(List<Vector3> sites)
	{
		//Step 1. Triangulate the points with some algorithm
		//Vector3 to vertex
		List<Vertex> vertices = new List<Vertex>();

		for (int i = 0; i < sites.Count; i++)
		{
			vertices.Add(new Vertex(sites[i]));
		}

		//Triangulate the convex hull of the sites
		List<Triangle> triangles = TriangulateConcavePolygon(sites);
		//List triangles = TriangulatePoints.TriangleSplitting(vertices);

		//Step 2. Change the structure from triangle to half-edge to make it faster to flip edges
		List<HalfEdge> halfEdges = Geometry.TransformFromTriangleToHalfEdge(triangles);

		//Step 3. Flip edges until we have a delaunay triangulation
		int safety = 0;

		int flippedEdges = 0;

		while (true)
		{
			safety += 1;

			if (safety > 100000)
			{
				Debug.Log("Stuck in endless loop");

				break;
			}

			bool hasFlippedEdge = false;

			//Search through all edges to see if we can flip an edge
			for (int i = 0; i < halfEdges.Count; i++)
			{
				HalfEdge thisEdge = halfEdges[i];

				//Is this edge sharing an edge, otherwise its a border, and then we cant flip the edge
				if (thisEdge.oppositeEdge == null)
				{
					continue;
				}

				//The vertices belonging to the two triangles, c-a are the edge vertices, b belongs to this triangle
				Vertex a = thisEdge.v;
				Vertex b = thisEdge.nextEdge.v;
				Vertex c = thisEdge.prevEdge.v;
				Vertex d = thisEdge.oppositeEdge.nextEdge.v;

				Vector2 aPos = a.GetPos2D_XY();
				Vector2 bPos = b.GetPos2D_XY();
				Vector2 cPos = c.GetPos2D_XY();
				Vector2 dPos = d.GetPos2D_XY();

				//Use the circle test to test if we need to flip this edge
				if (Intersections.IsPointInsideOutsideOrOnCircle(aPos, bPos, cPos, dPos) < 0f)
				{
					//Are these the two triangles that share this edge forming a convex quadrilateral?
					//Otherwise the edge cant be flipped
					if (Geometry.IsQuadrilateralConvex(aPos, bPos, cPos, dPos))
					{
						//If the new triangle after a flip is not better, then dont flip
						//This will also stop the algoritm from ending up in an endless loop
						if (Intersections.IsPointInsideOutsideOrOnCircle(bPos, cPos, dPos, aPos) < 0f)
						{
							continue;
						}

						//Flip the edge
						flippedEdges += 1;

						hasFlippedEdge = true;

						Geometry.FlipEdge(thisEdge);
					}
				}
			}

			//We have searched through all edges and havent found an edge to flip, so we have a Delaunay triangulation!
			if (!hasFlippedEdge)
			{
				Debug.Log("Found a delaunay triangulation");

				break;
			}
		}

		Debug.Log("Flipped edges: " + flippedEdges);

		//Dont have to convert from half edge to triangle because the algorithm will modify the objects, which belongs to the 
		//original triangles, so the triangles have the data we need

		return triangles;
	}
}