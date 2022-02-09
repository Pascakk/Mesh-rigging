using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Vertex
{
    public Vector3 position;

    //The outgoing halfedge (a halfedge that starts at this vertex)
    //Doesnt matter which edge we connect to it
    public HalfEdge halfEdge;

    //Which triangle is this vertex a part of?
    public Triangle triangle;
    public List<Triangle> triangles = new List<Triangle>();

    //The previous and next vertex this vertex is attached to
    public Vertex prevVertex;
    public Vertex nextVertex;

    //Properties this vertex may have
    //Reflex is concave
    public bool isReflex;
    public bool isConvex;
    public bool isEar;
    public int verticeID;

    public Vertex(Vector3 position)
    {
        this.position = position;
    }

    //Get 2d pos of this vertex
    public Vector2 GetPos2D_XZ()
    {
        Vector2 pos_2d_xz = new Vector2(position.x, position.z);

        return pos_2d_xz;
    }

    public Vector2 GetPos2D_XY()
    {
        Vector2 pos_2d_xy = new Vector2(position.x, position.y);

        return pos_2d_xy;
    }
}

public class Triangle
{
    //Corners
    public Vertex v1;
    public Vertex v2;
    public Vertex v3;

    public bool isRigged = false;
    public GameObject rig;

    //How many neighbours does the triangle have ?
    public List<Triangle> neighbours = new List<Triangle>();

    //If we are using the half edge mesh structure, we just need one half edge
    public HalfEdge halfEdge;

    public Triangle(Vertex v1, Vertex v2, Vertex v3)
    {
        this.v1 = v1;
        this.v2 = v2;
        this.v3 = v3;
    }

    public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        this.v1 = new Vertex(v1);
        this.v2 = new Vertex(v2);
        this.v3 = new Vertex(v3);
    }

    public Triangle(HalfEdge halfEdge)
    {
        this.halfEdge = halfEdge;
    }

    //Change orientation of triangle from cw -> ccw or ccw -> cw
    public void ChangeOrientation()
    {
        Vertex temp = this.v1;

        this.v1 = this.v2;

        this.v2 = temp;
    }

    public Vector3 getCenter()
    {
        return (v1.position + v2.position + v3.position) / 3;
    }

    public Vector3 getActualCenter(Vector3[] vertices)
    {
        return (vertices[v1.verticeID] + vertices[v2.verticeID] + vertices[v3.verticeID]) / 3;
    }
}

public class HalfEdge
{
    //The vertex the edge points to
    public Vertex v;

    //The face this edge is a part of
    public Triangle t;

    //The next edge
    public HalfEdge nextEdge;
    //The previous
    public HalfEdge prevEdge;
    //The edge going in the opposite direction
    public HalfEdge oppositeEdge;

    //This structure assumes we have a vertex class with a reference to a half edge going from that vertex
    //and a face (triangle) class with a reference to a half edge which is a part of this face 
    public HalfEdge(Vertex v)
    {
        this.v = v;
    }
}

//And edge between two vertices
public class Edge
{
    public Vertex v1;
    public Vertex v2;

    //Is this edge intersecting with another edge?
    public bool isIntersecting = false;

    public Edge(Vertex v1, Vertex v2)
    {
        this.v1 = v1;
        this.v2 = v2;
    }

    public Edge(Vector3 v1, Vector3 v2)
    {
        this.v1 = new Vertex(v1);
        this.v2 = new Vertex(v2);
    }

    //Get vertex in 2d space (assuming x, z)
    public Vector2 GetVertex2D(Vertex v)
    {
        return new Vector2(v.position.x, v.position.z);
    }

    //Flip edge
    public void FlipEdge()
    {
        Vertex temp = v1;

        v1 = v2;

        v2 = temp;
    }
}