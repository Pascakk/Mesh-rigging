using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableCamera : MonoBehaviour
{
    Vector3 origin = new Vector3(0, 0, 0);

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("z"))
        {
            transform.position = new Vector3(0, 10, 0);
        }
        else if (Input.GetKey("d"))
        {
            transform.position = new Vector3(-10, 0, 0);
        }
        else if (Input.GetKey("q"))
        {
            transform.position = new Vector3(10, 10, 0);
        }
        else
        {
            transform.position = new Vector3(0, 1, -10);
        }
        transform.LookAt(origin);
    }
}
