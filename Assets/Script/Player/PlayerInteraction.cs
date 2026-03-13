using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Transform handPoint;
    public float ForcePush;

    [Header("UI")]
    public TMPro.TextMeshProUGUI equipppedItemText;
    public TMPro.TextMeshProUGUI interactPromptText;
    [SerializeField] private CanvasGroup hideFadeGroup;
    
    [Header("Optimization")]
    [SerializeField] private float raycastFrequency = 0.1f;
    private float rayTimer;
    private Collider[] cachedColliders;

    private Item equippedItem;
    private Rigidbody equippedRb;
    private DragHandler dragHandler;
    private MovementPlayer player;
    private Locker currentLocker;
    private PlayerInventory iventory;
    private string lastItemName = "";

    private bool isHidden;
    private bool isInsideLocker = false;

    void Start()
    {
        dragHandler = GetComponent<DragHandler>();
        player = GetComponent<MovementPlayer>();
        iventory = GetComponent<PlayerInventory>();
    }

    void Update()
    { 
        if (PauseMenu.isPausedStatic) return;

        if (isHidden) HandleHidingLook();

        rayTimer += Time.deltaTime;
        if (rayTimer >= raycastFrequency)
        {
            UpdateInteractionLogic();
            rayTimer = 0;
        }

        if (equippedItem != null) FollowHand();
    }

    private void UpdateInteractionLogic()
    {
        UpdateItemText();
        UpdateInteractPrompt();
    }

    private void UpdateItemText()
    {
        if (equipppedItemText == null) return;

        string currentName = equippedItem != null ? equippedItem.itemName : "-";

        if (lastItemName != currentName)
        {
            equipppedItemText.text = $"Item: {currentName}";
            lastItemName = currentName;
        }
    }

    private void UpdateInteractPrompt()
    {
        if (interactPromptText == null) return;

        if (equippedItem == null)
        {
            if (interactPromptText.gameObject.activeSelf) 
                interactPromptText.gameObject.SetActive(false);
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, 2.5f))
        {
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                if (interactable.CanInteract(equippedItem.itemType, equippedItem.keyID))
                {
                    ShowPrompt(equippedItem.itemType);
                    return;
                }
            }

            else if (hit.collider.transform.parent != null && 
                     hit.collider.transform.parent.TryGetComponent(out interactable))
            {
                 if (interactable.CanInteract(equippedItem.itemType, equippedItem.keyID))
                {
                    ShowPrompt(equippedItem.itemType);
                    return;
                }
            }
        }
        
        if (interactPromptText.gameObject.activeSelf)
            interactPromptText.gameObject.SetActive(false);
    }

    private void ShowPrompt(ItemType type)
    {
        interactPromptText.gameObject.SetActive(true);
        // Menggunakan switch expression (C# 8+) yang lebih efisien
        interactPromptText.text = type switch
        {
            ItemType.Crowbar => "[E] Remove the board",
            ItemType.doorID  => "[E] Unlock The Door",
            ItemType.StunGun => "",
            _                => "[E] Interact"
        };
    }

    private void ToggleEquippedColliders(bool state)
    {
        if (equippedItem == null) return;

        // Jika belum di-cache, ambil dulu
        if (cachedColliders == null)
            cachedColliders = equippedItem.GetComponentsInChildren<Collider>();
        
        foreach (Collider col in cachedColliders)
        {
            col.enabled = state;
        }

        if (state == true) cachedColliders = null; // Reset cache saat dibuang
    }

    public void OnInteract(InputAction.CallbackContext context)
    {

    if (!context.performed || PauseMenu.isPausedStatic) return;
    
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (!Physics.Raycast(ray, out RaycastHit hit, 3f)) return;

        if (hit.collider.TryGetComponent(out Item item))
        {
            if (item.itemType == ItemType.Key) 
            {
                if (iventory != null)
                {
                    iventory.AddKey(item.keyID);
                    Debug.Log("Mengambil kunci: " + item.keyID);
                }
                else
                {
                    Debug.LogError("PlayerInventory tidak ditemukan pada Player!");
                }
                return;
            }
            
            TryEquip();
            return;
        }

        // DOOR
        if (hit.collider.TryGetComponent(out NormalDoor door))
        {
            door.Interact(gameObject);
            return;
        }

        // LOCKER
        Locker locker = hit.collider.GetComponentInParent<Locker>();
        if ( locker != null)
        {
            SetCurrentLocker(locker);
            locker.Interact(player);
            return;
        }
    }

    public bool IsHoldingKey(string requiredKeyID)
    {
        if (equippedItem == null) return false;

        bool isKeyType = equippedItem.itemType == ItemType.Key || equippedItem.itemType == ItemType.doorID;
        return isKeyType && equippedItem.keyID == requiredKeyID;
    }

    public void UpdateFadeAlpha(float alpha) 
    {
        if (hideFadeGroup != null)
        {
            hideFadeGroup.alpha = alpha;
            hideFadeGroup.blocksRaycasts = (alpha > 0.5f); 
        }
    }

    public void SetHiddenStatus(bool status)
    {
        isHidden = status;
        isInsideLocker = status;
        player.IsHidden = status;

        if (equippedItem != null)
        {
            ToggleEquippedColliders(false);
            equippedRb.isKinematic = true;
        }

        if (hideFadeGroup != null)
        {
            if (!status) 
            {
                hideFadeGroup.alpha = 0f;
                hideFadeGroup.blocksRaycasts = false;
            }
        }
    }

    void FollowHand()
    {
        equippedRb.MovePosition(handPoint.position);
        equippedRb.MoveRotation(handPoint.rotation);
    }

    public void OnExitHiding(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (!isHidden) return;

        if (currentLocker != null)
        {
            currentLocker.Interact(player);
            isHidden = false;
            ClearLocker();
        }
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

    public void OnTryShot(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (equippedItem == null) return;
            if (equippedItem.itemType != ItemType.StunGun) return;

            StunGun stunGun = equippedItem.GetComponent<StunGun>();
            if (stunGun != null) stunGun.TryShoot();
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
                if (equippedItem != null) DropEquipped();

                equippedItem = item;
                equippedRb = item.GetComponent<Rigidbody>();

                equippedItem.transform.SetParent(handPoint);
                equippedItem.transform.localPosition = Vector3.zero;
                equippedItem.transform.localRotation = Quaternion.identity;

                ToggleEquippedColliders(false);

                if (equippedRb != null)
                {
                    equippedRb.useGravity = false;
                    equippedRb.isKinematic = true;

                    StunGun stunGun = equippedItem.GetComponent<StunGun>();
                    if (stunGun != null) stunGun.OnPickedUp();
                }
            }
        }
    }

    void HandleHidingLook()
    {
        if (currentLocker == null) return;
    
        Vector2 lookInput = Mouse.current.delta.ReadValue();
    
        float mouseX = lookInput.x * 0.1f;
        float mouseY = lookInput.y * 0.1f;
    
        currentLocker.HandleCameraPeeking(
            playerCamera.transform,
            mouseX,
            mouseY
        );
    }

    public void SetCurrentLocker(Locker locker)
    {
        currentLocker = locker;
    }

    public void ClearLocker()
    {
        currentLocker = null;
    }

    public void OnDrop(InputAction.CallbackContext context)
    {
        if (context.started && equippedItem != null && !isHidden && !PauseMenu.isPausedStatic)
        {
            DropEquipped();
        }
    }

    void DropEquipped()
    {
        if (equippedItem == null) return;

        equippedItem.transform.SetParent(null);

        equippedRb.useGravity = true;
        equippedRb.isKinematic = false;
        equippedRb.constraints = RigidbodyConstraints.None;

        foreach (Collider col in equippedItem.GetComponentsInChildren<Collider>())
        {
            col.enabled = true;
        }

        equippedRb.AddForce(playerCamera.transform.forward * ForcePush, ForceMode.Impulse);

        StunGun stunGun = equippedItem.GetComponent<StunGun>();
        if (stunGun != null) stunGun.OnDropped();

        equippedItem = null;
        equippedRb = null;
    }

    void TryuseEquippedItem()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (!Physics.Raycast(ray, out RaycastHit hit, 2.5f)) return;

        IInteractable interactable = hit.collider.GetComponent<IInteractable>();
        if (interactable == null) interactable = hit.collider.GetComponentInParent<IInteractable>();
        if (interactable == null) return;
        if (interactable.CanInteract(equippedItem.itemType, equippedItem.keyID))
        {
            interactable.Interact(equippedItem.itemType);
        }
    }

    public StunGun GetHeldStunGun()
    {
        if (equippedItem == null) return null;
        if (equippedItem.itemType != ItemType.StunGun) return null;
        return equippedItem.GetComponent<StunGun>();
    }
}