using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

public class DragHandler : MonoBehaviour
{
    public Camera playerCamera;
    [SerializeField] private float maxDistance = 3f;
    [SerializeField] private float lerpSpeed = 20f;

    private GameObject draggedObj;
    private Rigidbody draggedRb;
    private float targetDistance; 
    private int originalLayer;
    private NavMeshObstacle navObstacle;

    public bool IsDragging => draggedObj != null;

    void FixedUpdate()
    {
        if (draggedObj != null && draggedRb != null)
        {
            DragObjectMovePosition();
        }
    }

    public void OnDrag(InputAction.CallbackContext context)
    {
        if (context.started) TryStartDrag();
        if (context.canceled) StopDrag();
    }

    void TryStartDrag()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            if (hit.collider.CompareTag("Draggable"))
            {
                draggedObj = hit.collider.gameObject;
                draggedRb = draggedObj.GetComponent<Rigidbody>();

                if (draggedRb != null)
                {
                    originalLayer = draggedObj.layer;
                    SetLayerRecursive(draggedObj, LayerMask.NameToLayer("Dragging"));

                    draggedRb.isKinematic = false;
                    draggedRb.useGravity = false;
                    draggedRb.interpolation = RigidbodyInterpolation.Interpolate;

                    draggedRb.linearDamping = 10f; 
                    draggedRb.angularDamping = 10f;

                    targetDistance = Vector3.Distance(ray.origin, hit.point);
                }
            }
        }
    }

    void DragObjectMovePosition()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPosition = ray.origin + (ray.direction * targetDistance);

        Vector3 nextPosition = Vector3.Lerp(draggedRb.position, targetPosition, Time.fixedDeltaTime * lerpSpeed);

        draggedRb.MovePosition(nextPosition);
    }

    void StopDrag()
    {
        if (draggedObj == null) return;

        SetLayerRecursive(draggedObj, originalLayer);

        draggedRb.isKinematic = false;
        draggedRb.useGravity = true;
        
        draggedRb.linearDamping = 0.5f; 
        draggedRb.angularDamping = 0.5f;
        draggedRb.interpolation = RigidbodyInterpolation.None;

        draggedObj = null;
        draggedRb = null;
    }

    void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }
}