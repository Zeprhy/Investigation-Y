using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform handPoint;
    [SerializeField] private float ForcePush;
    [SerializeField] private ClimbingSystem climbingSystem;

    [Header("UI")]
    [SerializeField] private TMPro.TextMeshProUGUI equipppedItemText;
    [SerializeField] private TMPro.TextMeshProUGUI interactPromptText;
    [SerializeField] private CanvasGroup hideFadeGroup;
    
    [Header("Optimization")]
    [SerializeField] private float raycastFrequency = 0.1f;
    [SerializeField] private LayerMask SocketLayerMask;
    private float rayTimer;
    private Collider[] cachedColliders;

    private Item equippedItem;
    private Rigidbody equippedRb;
    private DragHandler dragHandler;
    private MovementPlayer player;
    private Locker currentLocker;
    private Outline lastHighlightedItem;
    private PuzzleSocket currentViewedSocket;

    private bool isHidden;
    private bool isInsideLocker = false;
    private Vector3 originalItemScale;

    private EvidenceInspector evidenceInspector;
    private PuzzleSocket _lastViewedSocket;

    public void Initialize()
    {
        dragHandler = GetComponent<DragHandler>();
        player = GetComponent<MovementPlayer>();
        evidenceInspector = GetComponent<EvidenceInspector>();
    }

    void Update()
    { 
        if (PauseMenu.isPausedStatic) return;

        HandleOutlineRaycast();

        rayTimer += Time.deltaTime;
        if (rayTimer >= raycastFrequency)
        {
            rayTimer = 0;
        }

        HandleSocketPreviewRaycast();
    }

    void LateUpdate()
    {
        if (equippedItem != null) FollowHand();
    }

    private void HandleSocketPreviewRaycast()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        
        if (Physics.Raycast(ray, out RaycastHit hit, 3f, SocketLayerMask))
        {
            PuzzleSocket socket = hit.collider.GetComponent<PuzzleSocket>();

            if (socket != null && equippedItem != null && socket.IsCorrectItem(equippedItem) && !socket.IsOccupied)
            {
                if (_lastViewedSocket != socket)
                {
                    if(_lastViewedSocket != null) _lastViewedSocket.SetPreview(false);
                    _lastViewedSocket = socket;
                    _lastViewedSocket.SetPreview(true);
                }
                return;
            }
        }

        if (_lastViewedSocket != null)
        {
            _lastViewedSocket.SetPreview(false);
            _lastViewedSocket = null;
        }
    }

    private void ConsumeCurrentItem()
    {
        if (equippedItem == null) return;

        equippedItem.transform.SetParent(null);

        GameObject ObjItem = equippedItem.gameObject;

        equippedItem = null;
        equippedRb = null;
        cachedColliders = null;
        

        Destroy(ObjItem);
    }

    void HandleOutlineRaycast()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 3f))
        {
            Outline outline = hit.collider.GetComponent<Outline>();

            if (outline != null)
            {                
                if (lastHighlightedItem != outline)
                {
                    if (lastHighlightedItem != null) lastHighlightedItem.enabled = false;

                    outline.enabled = true;
                    lastHighlightedItem = outline;
                }
            }
            else
            {
                DisableLastOutline();
            }
        }
        else
        {
            DisableLastOutline();
        }
    }

    void DisableLastOutline()
    {
        if (lastHighlightedItem != null)
        {
            lastHighlightedItem.enabled = false;
            lastHighlightedItem = null;
        }    
    }

    private void ToggleEquippedColliders(bool state)
    {
        if (equippedItem == null) return;

        if (cachedColliders == null || (cachedColliders.Length > 0 && cachedColliders[0] == null))
            cachedColliders = equippedItem.GetComponentsInChildren<Collider>();
        
        foreach (Collider col in cachedColliders)
        {
            if (col != null)
            {
                col.enabled = state;   
            }
        }

        if (state == true) cachedColliders = null;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed || PauseMenu.isPausedStatic) return;

        if (climbingSystem != null && climbingSystem.IsClimbing)
        {
            climbingSystem.StopClimbing();
            return;
        }

        if (evidenceInspector != null && evidenceInspector.TryHandleInteract()) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 3f)) 
        {
            // 1. CEK TOMBOL LIFT
            ElevatorButton buttonLift = hit.collider.GetComponent<ElevatorButton>();
            if (buttonLift != null)
            {
                buttonLift.Interaction();
                return;
            }

            Drawer drawer = hit.collider.GetComponent<Drawer>();
            if (drawer != null)
            {
                drawer.Interaction();
                return;
            }

            PuzzleSocket socket = hit.collider.GetComponent<PuzzleSocket>();
            if (socket != null)
            {
                if (equippedItem != null && socket.IsCorrectItem(equippedItem) && !socket.IsOccupied)
                {
                    socket.PutItemInSocket();
                    ConsumeCurrentItem();
                }
                return;
            }

            RotateWheel wheel = hit.collider.GetComponent<RotateWheel>();
            if (wheel != null)
            {
                // Putar rodanya!
                wheel.Interact();
                return;
            }

            // 2. CEK ITEM (Pick up)
            if (hit.collider.TryGetComponent(out Item item))
            {
                TryEquip();
                return;
            }

            // 3. CEK PINTU BIASA (Normal Door)
            NormalDoor door = hit.collider.GetComponentInParent<NormalDoor>();
            if (door != null)
            {
                door.Interact(gameObject);
                return;
            }

            // 4. CEK LOCKER (Tempat Sembunyi)
            Locker locker = hit.collider.GetComponentInParent<Locker>();
            if (locker != null)
            {
                SetCurrentLocker(locker);
                locker.Interact(player);
                return;
            }

            // 5. CEK SAKLAR LAMPU
            LightSwitch salkar = hit.collider.GetComponent<LightSwitch>();
            if (salkar != null)
            {
                salkar.Toggle();
                return;
            }

            // 6. SMART METER
            if (hit.collider.TryGetComponent(out SmartMeter meter))
            {
                meter.UseMeter();
                return;    
            }

            // 7. CLIMB SYSTEM
            if (Physics.Raycast(ray, out hit, 3.5f))
            {                
                if (climbingSystem != null && ((1 << hit.collider.gameObject.layer) & climbingSystem.climbableLayer) != 0)
                {
                    climbingSystem.ToggleClimb(hit.normal, hit.point); 
                    return;
                }
            }
        }
    }

    public bool IsHoldingKey(string requiredKeyID)
    {
        if (equippedItem == null) return false;

        return (equippedItem.itemType == ItemType.Key || equippedItem.itemType == ItemType.doorID) 
            && equippedItem.keyID == requiredKeyID;
    }

    public bool IsHoldingLockPick()
    {
        if (equippedItem == null) return false;
        return equippedItem.itemType == ItemType.LockPick;
    }

    public bool IsHoldingCrankHandle()
    {
        if (equippedItem == null) return false;
        return equippedItem.itemType == ItemType.CrankHandle;
    }

    public void ConsumeLockPick()
    {
        if (equippedItem == null) return;
        if (equippedItem.itemType != ItemType.LockPick) return;

        equippedItem.transform.SetParent(null);
        equippedItem.transform.localScale = originalItemScale;
        ToggleEquippedColliders(true);
 
        Destroy(equippedItem.gameObject);
 
        cachedColliders = null;
        equippedItem = null;
        equippedRb = null;
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

                originalItemScale = item.transform.localScale;

                if (equippedRb != null)
                {
                    equippedRb.useGravity = false;
                    equippedRb.isKinematic = true;
                    equippedRb.interpolation = RigidbodyInterpolation.None;
                }

                equippedItem.transform.SetParent(handPoint);
                equippedItem.transform.localPosition = Vector3.zero;
                equippedItem.transform.localRotation = Quaternion.identity;

                Vector3 parentScale = handPoint.lossyScale;
                equippedItem.transform.localScale = new Vector3(
                    originalItemScale.x / parentScale.x,
                    originalItemScale.y / parentScale.y,
                    originalItemScale.z / parentScale.z
                    );

                ToggleEquippedColliders(false);

                if (equippedItem.TryGetComponent(out StunGun stunGun))
                {
                    stunGun.OnPickedUp();
                }
            }
        }
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

    public void DropEquipped()
    {
        if (equippedItem == null) return;

        equippedItem.transform.SetParent(null);
        equippedItem.transform.localScale = originalItemScale;
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