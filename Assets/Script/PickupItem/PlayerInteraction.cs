using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Transform handPoint;
    public float ForcePush;

    [Header("UI")]
    public TMPro.TextMeshProUGUI equipppedItemText;
    public TMPro.TextMeshProUGUI interactPromptText;
    
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
        UpdateUI();
            
    }
    public void OnUse(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (equippedItem != null)
            {
                TryuseEquippedItem();
                return;
            }

            if (dragHandler != null && dragHandler.IsDragging)return;
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
                
                

                foreach(Collider col in equippedItem.GetComponentsInChildren<Collider>())
                {
                    col.enabled = false;
                }
                

                if (equippedRb != null)
                {
                    equippedRb.useGravity = false;
                    equippedRb.isKinematic = true;
                    equippedRb.linearDamping = 10f;
                    equippedRb.angularDamping = 10f;
                    

                    equippedRb.constraints = RigidbodyConstraints.FreezeRotation;
                    equippedRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                    equippedRb.interpolation = RigidbodyInterpolation.Interpolate;
                }
            }
        }
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
        equippedRb.constraints = RigidbodyConstraints.None;

        foreach (Collider col in equippedItem.GetComponentsInChildren<Collider>())
        {
            col.enabled = true;
        }

        equippedRb.AddForce(playerCamera.transform.forward * ForcePush, ForceMode.Impulse);

        equippedItem = null;
        equippedRb = null;
    }

    void TryuseEquippedItem()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (!Physics.Raycast(ray, out RaycastHit hit, 2.5f)) return;

        IInteractable interactable = hit.collider.GetComponent<IInteractable>();
        if (interactable == null) return;
        if (interactable.CanInteract(equippedItem.itemType, equippedItem.keyID))
        {
            interactable.Interact(equippedItem.itemType);
        }
    }
    void FollowHand()
    {
        if(equippedRb == null) return;
        equippedRb.MovePosition(handPoint.position);
        equippedRb.MoveRotation(handPoint.rotation);
    }

    void UpdateUI()
    {
        if (equipppedItemText != null)
        {
            equipppedItemText.text = equippedItem != null ? $"Item: {equippedItem.itemName}" : "Item: -";
        }

        if (interactPromptText == null) return;

        if (equippedItem != null)
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out RaycastHit hit, 2.5f))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null && interactable.CanInteract(equippedItem.itemType, equippedItem.keyID))
                {
                    interactPromptText.gameObject.SetActive(true);
                    interactPromptText.text = equippedItem.itemType switch
                    {
                        ItemType.Crowbar => "[E] Remove the board",
                        ItemType.Key     => "[E] Unlock The Door",
                        _                => "[E] Equip"
                    };
                    return;
                }
            }
        }
        interactPromptText.gameObject.SetActive(false);
    }
}