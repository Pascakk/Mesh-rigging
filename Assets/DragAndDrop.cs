using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragAndDrop : MonoBehaviour
{
    GameObject rigDot;
    bool isDraggingMesh;
    bool isMovingRig;
    Vector3 offset;
    Vector3 screenPosition;

    GameObject ReturnClickedObject(out RaycastHit hit)
    {
        rigDot = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray.origin, ray.direction * 10, out hit))
        {
            rigDot = hit.collider.gameObject;
        }
        return rigDot;
    }

    void Update()
    {

        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hitInfo;
            rigDot = ReturnClickedObject(out hitInfo);
            if (rigDot != null)
            {
                isMovingRig = false;
                isDraggingMesh = true;
                Debug.Log("target position :" + rigDot.transform.position);
                //Convert world position to screen position.
                screenPosition = Camera.main.WorldToScreenPoint(rigDot.transform.position);
                offset = rigDot.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z));
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo;
            rigDot = ReturnClickedObject(out hitInfo);
            if (rigDot != null)
            {
                isDraggingMesh = false;
                isMovingRig = true;
                Debug.Log("target position :" + rigDot.transform.position);
                //Convert world position to screen position.
                screenPosition = Camera.main.WorldToScreenPoint(rigDot.transform.position);
                offset = rigDot.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z));
            }
        }

        if (isDraggingMesh && Input.GetMouseButtonUp(1))
        {
            isDraggingMesh = false;
        }

        if (isMovingRig && Input.GetMouseButtonUp(0))
        {
            isMovingRig = false;
        }

        if (isDraggingMesh)
        {
            //track mouse position.
            Vector3 currentScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z);

            //convert screen position to world position with offset changes.
            Vector3 currentPosition = Camera.main.ScreenToWorldPoint(currentScreenSpace) + offset;

            //It will update target gameobject's current postion.
            
            Vector3 delta = currentPosition - rigDot.transform.position;
            rigDot.GetComponent<Draggable>().riggedObject.GetComponent<DrawnMesh>().drag(delta);

            rigDot.transform.position = currentPosition;
        }

        if (isMovingRig)
        {
            //track mouse position.
            Vector3 currentScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z);

            //convert screen position to world position with offset changes.
            Vector3 currentPosition = Camera.main.ScreenToWorldPoint(currentScreenSpace) + offset;
            
            rigDot.GetComponent<Draggable>().drag(currentPosition);
        }
    }

}