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

    // MENGGUNAKAN ItemBase
    private ItemBase equippedItem;
    private Rigidbody equippedRb;
    private DragHandler dragHandler;
    private MovementPlayer player;
    private Locker currentLocker;
    private Outline lastHighlightedItem;
    private PuzzleSocket currentViewedSocket;
    private HideManager hideManager;

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
        hideManager = GetComponent<HideManager>();
    }

    void Update()
    { 
        if (PauseMenu.isPausedStatic) return;

        // HandleOutlineRaycast();

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

            // Pastikan PuzzleSocket juga sudah di-update untuk menerima ItemBase
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

    // void HandleOutlineRaycast()
    // {
    //     Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
    //     RaycastHit hit;

    //     if (Physics.Raycast(ray, out hit, 3f))
    //     {
    //         Outline outline = hit.collider.GetComponent<Outline>();

    //         if (outline != null)
    //         {                
    //             if (lastHighlightedItem != outline)
    //             {
    //                 if (lastHighlightedItem != null) lastHighlightedItem.enabled = false;

    //                 outline.enabled = true;
    //                 lastHighlightedItem = outline;
    //             }
    //         }
    //         else
    //         {
    //             DisableLastOutline();
    //         }
    //     }
    //     else
    //     {
    //         DisableLastOutline();
    //     }
    // }

    // void DisableLastOutline()
    // {
    //     if (lastHighlightedItem != null)
    //     {
    //         lastHighlightedItem.enabled = false;
    //         lastHighlightedItem = null;
    //     }    
    // }

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
                wheel.Interact();
                return;
            }

            // MENGGUNAKAN ItemBase
            if (hit.collider.TryGetComponent(out ItemBase item))
            {
                TryEquip();
                return;
            }

            BaseDoor door = hit.collider.GetComponentInParent<BaseDoor>();
            if (door != null)
            {
                door.Interact(gameObject);
                return;
            }

            Locker locker = hit.collider.GetComponentInParent<Locker>();
            if (locker != null)
            {
                SetCurrentLocker(locker);
                locker.Interact(player);
                return;
            }

            LightSwitch salkar = hit.collider.GetComponent<LightSwitch>();
            if (salkar != null)
            {
                salkar.Toggle();
                return;
            }

            if (hit.collider.TryGetComponent(out SmartMeter meter))
            {
                meter.UseMeter();
                return;    
            }

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

        // Langsung cek tipe item dan cocokkan ID-nya dari ItemBase
        return (equippedItem.itemType == ItemType.Key || equippedItem.itemType == ItemType.door) 
            && equippedItem.KeyID == requiredKeyID;
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
        hideManager.SetHidden(status);

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

            if (dragHandler != null && dragHandler.IsDragging) return;
            TryEquip();
        }
    }

    public void OnTryShot(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (equippedItem == null) return;
            
            // Cukup panggil UseItem(). Jika itu StunGun, dia akan otomatis menembak!
            // Jika itu item lain (misal obat), dia bisa memakai efek obatnya.
            equippedItem.UseItem(); 
        }
    }

    void TryEquip()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            // MENGGUNAKAN ItemBase
            ItemBase item = hit.collider.GetComponent<ItemBase>();
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

                // LANGSUNG PANGGIL OnEquip() TANPA PERLU CEK STUNGUN!
                equippedItem.OnEquip();
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

        // LANGSUNG PANGGIL OnDrop() TANPA PERLU CEK STUNGUN!
        equippedItem.OnDrop();

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
        
        // MENGIRIM SELURUH OBJEK ItemBase, BUKAN CUMA TIPE ATAU ID-NYA
        if (interactable.CanInteract(equippedItem.itemType, equippedItem.KeyID))
        {
            interactable.Interact(equippedItem.itemType);
        }
    }

    // Jika Anda masih membutuhkan fungsi ini di script lain, kita gunakan "as StunGun"
    public StunGun GetHeldStunGun()
    {
        return equippedItem as StunGun;
    }
}