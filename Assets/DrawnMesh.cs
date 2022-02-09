using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawnMesh : MonoBehaviour
{
    public List<GameObject> rigs;

    public void drag(Vector3 delta)
    {
        foreach (GameObject rig in rigs)
        {
            rig.transform.position += delta;
        }
        this.gameObject.transform.position += delta;
    }

}
