using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]

public class DragController : MonoBehaviour
{

    private Vector3 screenPoint;
    private Vector3 offset;
    private bool isShift;

    void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            isShift = true;
        }
    }

    private void OnMouseUp()
    {
        isShift = false;
    }

    void OnMouseDrag()
    {
        if (isShift && Input.GetKey(KeyCode.LeftShift))
        {
            Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
            Vector3 newPosition = transform.position;
            newPosition.z = Camera.main.ScreenToWorldPoint(curScreenPoint).z + offset.z;
            transform.position = newPosition;
        }
        else
        {
            Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
            Vector3 newPosition = transform.position;
            newPosition.x = Camera.main.ScreenToWorldPoint(curScreenPoint).x + offset.x;
            transform.position = newPosition;
        }
    }

}