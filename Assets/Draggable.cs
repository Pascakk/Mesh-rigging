using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : MonoBehaviour
{
    public GameObject riggedObject;
    public int ID;
    public bool isJoint = false;
    public Triangle linkedTriangle;
    public List<GameObject> linkedRigs = new List<GameObject>();
    public List<Vertex> affectedVertex = new List<Vertex>();
    public Quaternion rotation;
    public Vector3 axis;

    public void drag(Vector3 cursorPosition)
    {
        Vector3[] modifiedVertice = riggedObject.GetComponent<MeshFilter>().mesh.vertices;
        Vector3 delta = cursorPosition - transform.position;

        // check if the rig is structural 
        int nbJointNearby =0;
        foreach (GameObject rig in linkedRigs)
        {
            if (rig.GetComponent<Draggable>().isJoint) nbJointNearby++;
        }
        if (nbJointNearby > 1) return; // TODO: create specific behaviour

        foreach (GameObject rig in linkedRigs)
        {
            if (rig.GetComponent<Draggable>().isJoint) // found a joint, adjust self affected vertex
            {
                // compute rotation parameters
                Vector3 pivot = rig.GetComponent<Draggable>().linkedTriangle.getActualCenter(modifiedVertice);
                Vector3 actualPosition = linkedTriangle.getActualCenter(modifiedVertice);
                Vector3 vec1 = actualPosition - pivot;
                Vector3 vec2 = (actualPosition + delta) - pivot;
                float angle = Vector3.Angle(vec1, vec2);
                axis = Vector3.Cross(vec1, vec2);
                rotation = Quaternion.AngleAxis(angle, axis);
                /*
                foreach (Vertex v in affectedVertex)
                {
                    int id = v.verticeID;
                    modifiedVertice[id] = rotation * (modifiedVertice[id] - pivot) + pivot;
                }
                transform.position = linkedTriangle.getActualCenter(modifiedVertice) + riggedObject.transform.position;
                */

                List<GameObject> rigsToRotate = new List<GameObject>();
                rigsToRotate.Add(this.gameObject);

                List<Vertex> vertexTorotate = new List<Vertex>();

                foreach (GameObject n in linkedRigs)
                {
                    if (!rigsToRotate.Contains(n) && !n.GetComponent<Draggable>().isJoint)
                        rigsToRotate.Add(n);
                }
                int nbRigsToRotate = rigsToRotate.Count;

                for (int i = 0; i < nbRigsToRotate; i++)
                {
                    foreach (Vertex v in rigsToRotate[i].GetComponent<Draggable>().affectedVertex)
                    {
                        if (!vertexTorotate.Contains(v))
                            vertexTorotate.Add(v);
                    }

                    foreach (GameObject n in rigsToRotate[i].GetComponent<Draggable>().linkedRigs)
                    {
                        if (!rigsToRotate.Contains(n) && !n.GetComponent<Draggable>().isJoint)
                            rigsToRotate.Add(n);
                    }

                    nbRigsToRotate = rigsToRotate.Count;
                }

                foreach (Vertex v in vertexTorotate)
                {
                    int id = v.verticeID;
                    modifiedVertice[id] = rotation * (modifiedVertice[id] - pivot) + pivot;
                }

                foreach (GameObject g in rigsToRotate)
                {
                    g.GetComponent<Draggable>().transform.position = g.GetComponent<Draggable>().linkedTriangle.getActualCenter(modifiedVertice) + riggedObject.transform.position;
                }
            }
        }
        riggedObject.GetComponent<MeshFilter>().mesh.vertices = modifiedVertice;
    }
}
