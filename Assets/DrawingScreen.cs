using System.Collections.Generic;
using UnityEngine;

public class DrawingScreen : MonoBehaviour
{
    public GameObject drawPrefab;
    public GameObject RigPrefab;
    public GameObject drawnMeshPrefab;
    GameObject theTrail;
    Plane planeObj;
    List<Vector3> drawnVertices = new List<Vector3>();
    Vector3 startPos;
    int nbRig = 1;

    List<Vertex> earVertices = new List<Vertex>();

    // Start is called before the first frame update
    void Start()
    {
        planeObj = new Plane(Camera.main.transform.forward * -1, this.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began || Input.GetMouseButtonDown(0))
        {

            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            float _dist;
            if (planeObj.Raycast(mouseRay, out _dist))
            {
                PenFollowCamera(_dist);
                startPos = mouseRay.GetPoint(_dist);

                drawnVertices.Clear();
                drawnVertices.Add(mouseRay.GetPoint(_dist));
            }

            theTrail = (GameObject)Instantiate(drawPrefab, this.transform.position, Quaternion.identity);
        }
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetMouseButton(0))
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            float _dist;
            if (planeObj.Raycast(mouseRay, out _dist))
            {
                PenFollowCamera(_dist);
                Vector3 positionOnCanvas = mouseRay.GetPoint(_dist);
                if (drawnVertices.Count > 0)
                {
                    if (Vector3.Distance(positionOnCanvas, drawnVertices[drawnVertices.Count - 1]) > 0.5)
                    {
                        drawnVertices.Add(positionOnCanvas);
                    }
                }

                theTrail.transform.position = positionOnCanvas;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Destroy(theTrail);
            if (drawnVertices.Count > 3 && Vector3.Distance(drawnVertices[0], drawnVertices[drawnVertices.Count - 1]) < 1)
            {
                GameObject newGameObject = new GameObject();
                // createMesh
                Vector3 center = CenterOfVectors(drawnVertices);
                for (int i = 0; i < drawnVertices.Count; i++)
                {
                    drawnVertices[i] -= center;
                }
                newGameObject = Instantiate(drawnMeshPrefab, center, Quaternion.identity);

                Mesh newMesh = new Mesh();

                List<Triangle> complexTriangles = Triangulation.TriangulateByFlippingEdges(drawnVertices);

                Debug.Log("Number of triangles : " + complexTriangles.Count);

                drawnVertices.Clear();
                List<int> trianglesId = new List<int>();
                List<Vertex> referencedVertex = new List<Vertex>();
                foreach (Triangle tri in complexTriangles)
                {
                    if (!referencedVertex.Contains(tri.v1))
                    {
                        referencedVertex.Add(tri.v1);
                        drawnVertices.Add(tri.v1.position);
                        tri.v1.verticeID = referencedVertex.IndexOf(tri.v1);
                    }
                    if (!referencedVertex.Contains(tri.v2))
                    {
                        referencedVertex.Add(tri.v2);
                        drawnVertices.Add(tri.v2.position);
                        tri.v2.verticeID = referencedVertex.IndexOf(tri.v2);
                    }
                    if (!referencedVertex.Contains(tri.v3))
                    {
                        referencedVertex.Add(tri.v3);
                        drawnVertices.Add(tri.v3.position);
                        tri.v3.verticeID = referencedVertex.IndexOf(tri.v3);
                    }
                    trianglesId.Add(tri.v1.verticeID);
                    trianglesId.Add(tri.v2.verticeID);
                    trianglesId.Add(tri.v3.verticeID);

                }
                newMesh.vertices = drawnVertices.ToArray();
                newMesh.triangles = trianglesId.ToArray();
                newMesh.RecalculateNormals();
                newGameObject.GetComponent<MeshFilter>().mesh = newMesh;

                //linking vertex with the triangles they belong to
                foreach (Triangle tri in complexTriangles)
                {
                    tri.v1.triangles.Add(tri);
                    tri.v2.triangles.Add(tri);
                    tri.v3.triangles.Add(tri);

                    // listing neighbors
                    foreach (Triangle tri2 in complexTriangles)
                    {
                        // don't count self as neighbor
                        if (tri != tri2)
                        {
                            if ((tri.v1 == tri2.v1) || (tri.v1 == tri2.v2) || (tri.v1 == tri2.v3))
                            {
                                if ((tri.v2 == tri2.v1) || (tri.v2 == tri2.v2) || (tri.v2 == tri2.v3))
                                    tri.neighbours.Add(tri2);
                                else if ((tri.v3 == tri2.v1) || (tri.v3 == tri2.v2) || (tri.v3 == tri2.v3))
                                    tri.neighbours.Add(tri2);
                            }
                            else if ((tri.v2 == tri2.v1) || (tri.v2 == tri2.v2) || (tri.v2 == tri2.v3))
                            {
                                if ((tri.v1 == tri2.v1) || (tri.v1 == tri2.v2) || (tri.v1 == tri2.v3))
                                    tri.neighbours.Add(tri2);
                                else if ((tri.v3 == tri2.v1) || (tri.v3 == tri2.v2) || (tri.v3 == tri2.v3))
                                    tri.neighbours.Add(tri2);
                            }
                            else if ((tri.v3 == tri2.v1) || (tri.v3 == tri2.v2) || (tri.v3 == tri2.v3))
                            {
                                if ((tri.v2 == tri2.v1) || (tri.v2 == tri2.v2) || (tri.v2 == tri2.v3))
                                    tri.neighbours.Add(tri2);
                                else if ((tri.v1 == tri2.v1) || (tri.v1 == tri2.v2) || (tri.v1 == tri2.v3))
                                    tri.neighbours.Add(tri2);
                            }
                        }

                    }
                    // extremity rig
                    if (tri.neighbours.Count == 1)
                    {
                        GameObject newRig = Instantiate(RigPrefab, tri.getCenter() + center, Quaternion.identity);
                        newRig.GetComponent<Draggable>().linkedTriangle = tri;
                        newRig.GetComponent<Draggable>().riggedObject = newGameObject;
                        newRig.GetComponent<Draggable>().ID = nbRig++;
                        newGameObject.GetComponent<DrawnMesh>().rigs.Add(newRig);
                        tri.isRigged = true;
                        tri.rig = newRig;
                    }
                    // Joint rig
                    if (tri.neighbours.Count == 3)
                    {
                        GameObject newRig = Instantiate(RigPrefab, tri.getCenter() + center, Quaternion.identity);
                        newRig.GetComponent<Draggable>().linkedTriangle = tri;
                        newRig.GetComponent<Draggable>().riggedObject = newGameObject;
                        newRig.GetComponent<Draggable>().ID = nbRig++;
                        newRig.GetComponent<Draggable>().isJoint = true;
                        newGameObject.GetComponent<DrawnMesh>().rigs.Add(newRig);
                        tri.isRigged = true;
                        tri.rig = newRig;
                    }
                } // end foreEach triangle

                List<Triangle> visitedTriangles = new List<Triangle>(); // each triangle already visited
                List<Triangle> currentPathTriangles = new List<Triangle>(); // each triangle on the current path branch (used to link vertex to rigs
                Triangle currentTriangle;
                Triangle startTriangle;

                // search rig neigbors of all new rigs 
                // And link rigs the their vertex
                foreach (GameObject currentRig in newGameObject.GetComponent<DrawnMesh>().rigs)
                {
                    Debug.Log("start seeking for rig " + currentRig.GetComponent<Draggable>().ID);
                    //get the triagle linked to the current rig
                    startTriangle = currentRig.GetComponent<Draggable>().linkedTriangle;
                    // add this triangle to both path and visited list
                    visitedTriangles.Add(startTriangle);
                    currentPathTriangles.Add(startTriangle);
                    // the rig has as many rrig neigbors as the triangle has neigbors
                    foreach (Triangle tri in startTriangle.neighbours)
                    {
                        Debug.Log("checking new neigbor");
                        currentTriangle = tri;
                        // add this triangle to both path and visited list
                        visitedTriangles.Add(currentTriangle);
                        currentPathTriangles.Add(currentTriangle);
                        while (!currentTriangle.isRigged)
                        {
                            Debug.Log("neigbor has " + currentTriangle.neighbours.Count + " neigbors"); 
                            for (int oui =0; oui < currentTriangle.neighbours.Count; oui++)
                            {
                                Triangle tri2 = currentTriangle.neighbours[oui];
                                Debug.Log(oui);
                                if (!visitedTriangles.Contains(tri2)) 
                                {
                                    Debug.Log("moving to next triangle");
                                    visitedTriangles.Add(tri2);
                                    currentPathTriangles.Add(tri2);
                                    currentTriangle = tri2;
                                    break;
                                }
                            }
                        }
                        // current rig adds vertex to its area of effect when reaching a joint
                        if(currentTriangle.rig.GetComponent<Draggable>().isJoint) {
                            // adding vertex to rig bones
                            foreach (Triangle visitedTri in currentPathTriangles)
                            {
                                if (visitedTri != currentTriangle) { //don't add unreachable vertex
                                    if (!currentRig.GetComponent<Draggable>().affectedVertex.Contains(visitedTri.v1))
                                        currentRig.GetComponent<Draggable>().affectedVertex.Add(visitedTri.v1);
                                    if (!currentRig.GetComponent<Draggable>().affectedVertex.Contains(visitedTri.v2))
                                        currentRig.GetComponent<Draggable>().affectedVertex.Add(visitedTri.v2);
                                    if (!currentRig.GetComponent<Draggable>().affectedVertex.Contains(visitedTri.v3))
                                        currentRig.GetComponent<Draggable>().affectedVertex.Add(visitedTri.v3);
                                }
                                
                            }

                            // joint rigs have priority on their triangle's vertex
                            if (currentRig.GetComponent<Draggable>().isJoint) {
                                if (!currentRig.GetComponent<Draggable>().affectedVertex.Contains(startTriangle.v1))
                                    currentRig.GetComponent<Draggable>().affectedVertex.Add(startTriangle.v1);
                                if (!currentRig.GetComponent<Draggable>().affectedVertex.Contains(startTriangle.v2))
                                    currentRig.GetComponent<Draggable>().affectedVertex.Add(startTriangle.v2);
                                if (!currentRig.GetComponent<Draggable>().affectedVertex.Contains(startTriangle.v3))
                                    currentRig.GetComponent<Draggable>().affectedVertex.Add(startTriangle.v3);
                            }
                        }
                        currentPathTriangles.Clear();

                        Debug.Log(currentTriangle.isRigged);
                        Debug.Log(currentRig.GetComponent<Draggable>().linkedTriangle.isRigged);
                        if (!currentRig.GetComponent<Draggable>().linkedRigs.Contains(currentTriangle.rig))
                        {
                            currentRig.GetComponent<Draggable>().linkedRigs.Add(currentTriangle.rig);
                            Debug.Log("added rig " + currentTriangle.rig.GetComponent<Draggable>().ID + " to " + currentRig.GetComponent<Draggable>().ID);
                        }
                            
                    }
                    visitedTriangles.Clear();
                    Debug.Log("Rig " + currentRig.GetComponent<Draggable>().ID + " has " + currentRig.GetComponent<Draggable>().linkedRigs.Count);
                }

            }
        }
    }

    void PenFollowCamera(float dist)
    {
        Vector3 temp = Input.mousePosition;
        temp.z = dist;
        this.transform.position = Camera.main.ScreenToWorldPoint(temp);
    }

    Vector3 CenterOfVectors(List<Vector3> vectors)
    {
        Vector3 sum = new Vector3(0, 0, 0);
        if (vectors == null || vectors.Count == 0)
            return sum;
        foreach (Vector3 vec in vectors)
            sum += vec;
        return sum / vectors.Count;
    }
}