using UnityEngine;
using UnityEngine.XR;

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
        HandleEquipInput();
        HandleUseInput();
        HandleDropInput();

        if (equippedItem != null)
        {
            FollowHand();
            float distance = Vector3.Distance(playerCamera.transform.position, equippedItem.transform.position);
            
            if (distance > 3f)
            {
               DropEquipped(); 
            }
            
        }
            
    }

    void HandleEquipInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
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
                    

                    
                    equippedRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                    equippedRb.interpolation = RigidbodyInterpolation.Interpolate;
                }
            }
        }
    }

    void FollowHand()
    {
        if(equippedRb == null)
        return;
        equippedRb.MovePosition(handPoint.position);
        equippedRb.MoveRotation(handPoint.rotation);
    }

    void HandleUseInput()
    {
        if (equippedItem == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Using: " + equippedItem.itemType);
        }
    }

    void HandleDropInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
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

        equippedRb.linearVelocity = Vector3.zero;
        equippedItem.transform.position = handPoint.position;
        equippedItem.transform.rotation = Quaternion.identity;
        
        Collider[] cols = equippedItem.GetComponentsInChildren<Collider>();

                foreach(Collider col in cols)
                {
                    col.enabled = true;
                }

        
        equippedRb.AddForce(playerCamera.transform.forward * ForcePush, ForceMode.Impulse);

        equippedItem = null;
        equippedRb = null;
    }
}