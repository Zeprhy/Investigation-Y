using UnityEngine;
using System.Collections;
using TMPro;

public class NormalDoor : MonoBehaviour
{
    [Header("Settings")]
    public bool isOpen = false;
    public bool isLocked = true;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float interactionRadius = 3f;

    [Header("Auto Close")]
    [SerializeField] private bool useAutoClose = true;
    [SerializeField] private float autoCloseDelay = 3f;
    private Coroutine autoCloseCoroutine;

    [Header("UI System (Direct TMP)")]
    [SerializeField] private TextMeshProUGUI globalInteractText;
    [SerializeField] private float uiDisplayDistance = 3.0f;

    [Header("Lock Settings")]
    [SerializeField] private string doorID = "";
    [SerializeField] private string keyNameForUI = "Kunci Laboratorium";

    [Header("Lockpick Settings")]
    [Tooltip("Aktifkan agar pintu ini bisa dibuka dengan lockpick (Selain Kunci Utama/Asli)")]
    [SerializeField] private bool canBeLockpicked = false;
    [Tooltip("Refrensi ke LockPickMinigame yang ada di scene")]
    [SerializeField] private LockpickMinigame lockpickMinigame;

    private Quaternion targetRotation;
    private Quaternion defaultRotation;
    private Transform _playerTransform;
    private PlayerInteraction _playerInteraction;
    private UnityEngine.AI.NavMeshObstacle doorObstacle;
    private bool _isPlayerNear = false;
    private bool _lockpickInProgress = false;
    private Vector3 _lastInteractorPosition;

     private string _uiTextLocked;
    private string _uiTextLockedWithLockpick;
    private string _uiTextOpen;
    private string _uiTextClose;
 
    
    private float _uiDistanceSqr;
    private float _interactRadiusSqr;
 
    
    private WaitForSeconds _autoCloseWait;
    private WaitForSeconds _lockpickEndWait;

     void Awake()
    {
        // Pre-bake semua string UI
        string keyUpper = keyNameForUI.ToUpper();
        _uiTextLocked               = $"[Locked] Need {keyUpper}";
        _uiTextLockedWithLockpick   = $"[Locked] Need {keyUpper}  |  [F] Lockpick";
        _uiTextOpen                 = "Press [F] To Open The Door";
        _uiTextClose                = "Press [F] To Close The Door";
 
        // Pre-bake threshold kuadrat
        _uiDistanceSqr      = uiDisplayDistance * uiDisplayDistance;
        _interactRadiusSqr  = interactionRadius * interactionRadius;
 
        // Pre-bake WaitForSeconds
        _autoCloseWait   = new WaitForSeconds(autoCloseDelay);
    }
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        doorObstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();

        if (doorObstacle != null) doorObstacle.enabled = !isOpen;

        if (playerObj != null)
        {
            _playerTransform = playerObj.transform;
            _playerInteraction = playerObj.GetComponent<PlayerInteraction>();
        }

        if (globalInteractText != null) globalInteractText.text = "";
        
        defaultRotation = transform.localRotation;
        targetRotation = defaultRotation;

        //subscribe event minigame
        if (lockpickMinigame != null)
        {
            lockpickMinigame.onMinigameSuccess.AddListener(OnLockpickSuccess);
            lockpickMinigame.onMinigameFailed.AddListener(OnLockpickFailed);
        }
    }

    void Update()
    {
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * smoothSpeed);
        
        HandleUIDisplay();
    }

    private void HandleUIDisplay()
    {
        if (_playerTransform == null || globalInteractText == null) return;

        float distSqr = (_playerTransform.position - transform.position).sqrMagnitude;

        if (distSqr <= _uiDistanceSqr)
        {
            _isPlayerNear = true;
            UpdateUIText();
        }

        else if (_isPlayerNear)
        {
            _isPlayerNear = false;
            globalInteractText.text = "";
        }
    }

    private void UpdateUIText()
    {
        if (!_isPlayerNear || globalInteractText == null) return;
        if (_lockpickInProgress)
        {
            globalInteractText.text = "";
            return;
        }

        if (isLocked)
        {
        bool playerHasLockpick = canBeLockpicked
        && _playerInteraction != null 
        && _playerInteraction.IsHoldingLockPick();

        globalInteractText.text = playerHasLockpick
                ? _uiTextLockedWithLockpick   // "[Locked] Need X  |  [F] Lockpick"
                : _uiTextLocked;              // "[Locked] Need X" 
        }

        else
        {
            globalInteractText.text = isOpen ? _uiTextClose : _uiTextOpen;
        }

       
    }
    
    
    public void Interact(GameObject player)
    {
        if (_lockpickInProgress) return;

        float distSqr = (transform.position - player.transform.position).sqrMagnitude;
        if (distSqr > _interactRadiusSqr) return;

        if (!isLocked)
        {
            ToggleDoor(player.transform.position);
            return;
        }

        if (_playerInteraction == null) return;
        //Holding Keys
        if (_playerInteraction.IsHoldingKey(doorID))
        {
            UnlockDoor();
            ToggleDoor(player.transform.position);
            return;
        }
        //Holding LockPick
        if (canBeLockpicked && _playerInteraction.IsHoldingLockPick())
        {
            if (lockpickMinigame == null)
            {
                 Debug.LogWarning($"[NormalDoor] '{gameObject.name}' canBeLockpicked=true tapi LockpickMinigame belum di-assign!");
                 return;
            }
            _lastInteractorPosition = player.transform.position;
            _lockpickInProgress = true;
            lockpickMinigame.StartMinigame();
            return;
        }
    }

    private void OnLockpickSuccess()
    {
        _lockpickInProgress = false;
        UnlockDoor();
        ToggleDoor(_lastInteractorPosition);

        _playerInteraction?.ConsumeLockPick();
        UpdateUIText();
    }

    private void OnLockpickFailed()
    {
        _lockpickInProgress = false;
        _playerInteraction?.ConsumeLockPick();

        UpdateUIText();
        Debug.Log($"[NormalDoor] Lockpick gagal pada '{gameObject.name}'. Item lockpick rusak.");
    }


    public void Interact(Vector3 interactorPosition)
    {
        if (isLocked) return;

        ToggleDoor(interactorPosition);
    }

    private void UnlockDoor()
    {
        isLocked = false;
        UpdateUIText();
    }

    private void ToggleDoor(Vector3 interactorPosition)
    {
        if (autoCloseCoroutine != null) StopCoroutine(autoCloseCoroutine);
    
        isOpen = !isOpen;
    
        if (doorObstacle != null) 
        {
            doorObstacle.enabled = !isOpen; 
        }

        if (isOpen)
        {
            Vector3 directionToInteractor = transform.position - interactorPosition;
            float dot = Vector3.Dot(transform.forward, directionToInteractor);
            float angle = dot >= 0 ? openAngle : -openAngle;
            
            targetRotation = defaultRotation * Quaternion.Euler(0, angle, 0);
    
            if (useAutoClose) autoCloseCoroutine = StartCoroutine(AutoCloseTimer());
        }
        else
        {
            CloseDoor();
        }
    }

    private void CloseDoor()
    {
        isOpen = false;
        targetRotation = defaultRotation;
        if (doorObstacle != null) doorObstacle.enabled = true;
        UpdateUIText();
    }

    private IEnumerator AutoCloseTimer()
    {
        yield return _autoCloseWait;
        if (isOpen) CloseDoor();
        autoCloseCoroutine = null;
    }
}