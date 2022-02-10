using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRendererScript : MonoBehaviour
{
    public Vector3[] positions;
    GameObject start;
    GameObject end;

    public void SetupLine(GameObject start, GameObject end)
    {
        positions = new Vector3[] { start.transform.position, end.transform.position };
        this.start = start;
        this.end = end;
    }


    // Update is called once per frame
    void Update()
    {
        if(positions.Length > 0) positions = new Vector3[] { start.transform.position, end.transform.position };
        GetComponent<LineRenderer>().SetPositions(positions);
    }
}
