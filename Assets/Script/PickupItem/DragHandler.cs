using UnityEngine;

public class DragHandler : MonoBehaviour
{
    public Camera playerCamera;
    public float followSpeed;

    private GameObject draggedObj;
    private Rigidbody draggedRb;
    private float grabDistance;

    public bool IsDragging => draggedObj != null;

    void Update()
    {
        HandleDrag();
    }

    void HandleDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryStartDrag();
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopDrag();
        }

        if (draggedObj != null)
        {
            DragObject();
        }
    }

    void TryStartDrag()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            if (hit.collider.CompareTag("Draggable"))
            {
                draggedObj = hit.collider.gameObject;
                draggedRb = draggedObj.GetComponent<Rigidbody>();

                if (draggedRb != null)
                {
                    draggedRb.useGravity = false;
                    draggedRb.linearDamping = 5f;
                    
                    //simpan jarak asli object dari kamera
                    grabDistance = Vector3.Distance(playerCamera.transform.position, draggedObj.transform.position);
                }
            }
        }
    }

    void DragObject()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPosition = ray.origin + ray.direction * grabDistance;

        Vector3 force = (targetPosition - draggedObj.transform.position) * followSpeed;
        draggedRb.linearVelocity = force;
    }

    void StopDrag()
    {
        if (draggedObj == null) return;

        draggedRb.useGravity = true;
        draggedRb.linearDamping = 0;

        draggedObj = null;
        draggedRb = null;
    }
}