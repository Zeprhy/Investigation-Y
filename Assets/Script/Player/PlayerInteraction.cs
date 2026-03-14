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

    private bool isHidden;
    private bool isInsideLocker = false;

    void Start()
    {
        dragHandler = GetComponent<DragHandler>();
        player = GetComponent<MovementPlayer>();
    }

    void Update()
    { 
        if (PauseMenu.isPausedStatic) return;

        if (isHidden) HandleHidingLook();

        rayTimer += Time.deltaTime;
        if (rayTimer >= raycastFrequency)
        {
            rayTimer = 0;
        }

        if (equippedItem != null) FollowHand();
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

        return (equippedItem.itemType == ItemType.Key || equippedItem.itemType == ItemType.doorID) 
            && equippedItem.keyID == requiredKeyID;
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

                if (equippedRb != null)
                {
                    equippedRb.useGravity = false;
                    equippedRb.isKinematic = false;
                    equippedRb.interpolation = RigidbodyInterpolation.Interpolate;
                }

                equippedItem.transform.SetParent(handPoint);
                equippedItem.transform.localPosition = Vector3.zero;
                equippedItem.transform.localRotation = Quaternion.identity;

                ToggleEquippedColliders(false);

                if (equippedItem.TryGetComponent(out StunGun stunGun))
                {
                    stunGun.OnPickedUp();
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

        ToggleEquippedColliders(true);

        if (equippedRb != null)
        {
            equippedRb.isKinematic = false;
            equippedRb.useGravity = true;
            equippedRb.constraints = RigidbodyConstraints.None;

            Vector3 pushDirection = playerCamera.transform.forward;
            equippedRb.AddForce(pushDirection * ForcePush, ForceMode.Impulse);
        }

        if (equippedItem.TryGetComponent(out StunGun stunGun))
        {
            stunGun.OnDropped();
        }

        cachedColliders = null; 
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