using UnityEngine;
using UnityEngine.InputSystem;

public class DragHandler : MonoBehaviour
{
    public Camera playerCamera;
    public float followSpeed;

    private GameObject draggedObj;
    private Rigidbody draggedRb;
    private Vector3 grabOffset;

    public bool IsDragging => draggedObj != null;

    void Update()
    {
        if (draggedObj != null)
        {
            DragObject();
        }
    }

    public void OnDrag(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            TryStartDrag();
        }

        if (context.canceled)
        {
            StopDrag();
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
                    draggedRb.linearDamping = 0.5f;

                    grabOffset = draggedObj.transform.position - ray.origin;
                }
            }
        }
    }

    void DragObject()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPosition = ray.origin + (ray.direction * grabOffset.magnitude);

        Vector3 desireVelocity = (targetPosition - draggedObj.transform.position) * followSpeed;

        draggedRb.linearVelocity = Vector3.ClampMagnitude(desireVelocity, 50f);
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