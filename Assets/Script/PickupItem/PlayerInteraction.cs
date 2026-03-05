using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public Transform handPoint;
    public float ForcePush;
    private Item equippedItem;
    private Rigidbody equippedRb;
    private DragHandler dragHandler;
    

    void Start()
    {
        dragHandler = GetComponent<DragHandler>();
    }

    void Update()
    { 
        if (equippedItem != null)
        {
            FollowHand();  
        }
            
    }
    public void OnUse(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (equippedItem != null) return;
            if (dragHandler != null && dragHandler.IsDragging)
                return;

            TryEquip();
        }
    }

    void TryEquip()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            Item item = hit.collider.GetComponent<Item>();

            if (item != null && item.isUsable)
            {
                equippedItem = item;
                equippedRb = item.GetComponent<Rigidbody>();
                Collider[] cols = equippedItem.GetComponentsInChildren<Collider>();

                foreach(Collider col in cols)
                {
                    col.enabled = false;
                }
                

                if (equippedRb != null)
                {
                    equippedRb.useGravity = false;
                    equippedRb.linearDamping = 10f;
                    equippedRb.angularDamping = 10f;
                    

                    equippedRb.constraints = RigidbodyConstraints.FreezeRotation;
                    equippedRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                    equippedRb.interpolation = RigidbodyInterpolation.Interpolate;
                }
            }
        }
    }

    void FollowHand()
    {
        if(equippedRb == null) return;
        equippedRb.MovePosition(handPoint.position);
        equippedRb.MoveRotation(handPoint.rotation);
    }

    public void OnDrop(InputAction.CallbackContext context)
    {
        if (context.started && equippedItem != null)
        {
            DropEquipped();
        }
    }

    void DropEquipped()
    {
        if (equippedItem == null) return;

        equippedRb.useGravity = true;
        equippedRb.isKinematic = false;
        equippedRb.linearDamping = 0;
        equippedRb.angularDamping = 0.05f;

        equippedRb.linearVelocity = Vector3.zero;
        Collider[] cols = equippedItem.GetComponentsInChildren<Collider>();

                foreach(Collider col in cols)
                {
                    col.enabled = true;
                }

        equippedRb.constraints = RigidbodyConstraints.None;
        equippedRb.AddForce(playerCamera.transform.forward * ForcePush, ForceMode.Impulse);

        equippedItem = null;
        equippedRb = null;
    }
}