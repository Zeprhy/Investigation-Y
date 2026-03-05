using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public Transform handPoint;

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
            FollowHand();
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

                if (equippedRb != null)
                {
                    equippedRb.useGravity = false;
                    equippedRb.isKinematic = false;
                    equippedRb.linearDamping = 10f;
                }
            }
        }
    }

    void FollowHand()
    {
        Vector3 targetPosition = handPoint.position;

        Vector3 force = (targetPosition - equippedItem.transform.position) * 20f;
        equippedRb.linearVelocity = force;
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
        equippedRb.linearDamping = 0;
        equippedRb.linearVelocity = Vector3.zero;

        equippedItem = null;
        equippedRb = null;
    }
}