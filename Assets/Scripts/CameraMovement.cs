using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float dragSpeed = .1f;
    public float zoomSpeed = 10f;
    public float minZoom = 5f;
    public float maxZoom = 50f;
    private Vector3 lastMousePosition;
    
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            Vector3 move = new Vector3(delta.y, 0, -delta.x) * dragSpeed;

            transform.Translate(move, Space.World);

            lastMousePosition = Input.mousePosition;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            Vector3 zoomDirection = transform.forward;
            transform.position += zoomDirection * scroll * zoomSpeed;

            float distance = Vector3.Distance(transform.position, Vector3.zero);
            distance = Mathf.Clamp(distance, minZoom, maxZoom);
            transform.position = Vector3.zero + (transform.position - Vector3.zero).normalized * distance;
        }
    }
}