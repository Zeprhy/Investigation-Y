using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// NormalDoor — Handle semua tipe pintu dalam satu script.
/// Pilih DoorOpenMode di Inspector untuk menentukan cara membuka pintu.
/// 
/// Key      : Dibuka dengan kunci utama (keyID harus cocok)
/// Lockpick : Dibuka dengan item Lockpick + minigame timing (DBD style)
/// Crank    : Dibuka dengan item CrankHandle + minigame putar mouse
/// </summary>
public class NormalDoor : MonoBehaviour
{
    public enum DoorOpenMode
    {
        Key,
        Lockpick,
        Crank
    }
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

    [Header("Door Mode")]
    [Tooltip("Key = kunci Utama | Lockpick = minigame timing | Crank = minigame putar")]
    [SerializeField] private DoorOpenMode doorMode = DoorOpenMode.Key;

    [Header("Key Settings(Mode: Key)")]
    [SerializeField] private string doorID = "";
    [SerializeField] private string keyNameForUI = "Kunci Laboratorium";

    [Header("Lockpick Settings (Mode: LockPick)")]
    [SerializeField] private LockpickMinigame lockpickMinigame;

    [Header("Crank Settings (Mode: Crank)")]
    [SerializeField] private CrankMinigame crankminigame;

    // ---- Cache komponen ----
    private Quaternion _targetRotation;
    private Quaternion _defaultRotation;
    private Transform _playerTransform;
    private PlayerInteraction _playerInteraction;
    private UnityEngine.AI.NavMeshObstacle _doorObstacle;
 
    // ---- State ----
    private bool _isPlayerNear = false;
    private bool _minigameInProgress = false;
    private Vector3 _lastInteractorPosition;
 
    // ---- Cache string UI ----
    private string _uiTextLocked;
    private string _uiTextWithItem;
    private string _uiTextOpen;
    private string _uiTextClose;
    private string _uiTextCranking;
 
    // ---- Cache threshold ----
    private float _uiDistanceSqr;
    private float _interactRadiusSqr;
 
    // ---- Cache WaitForSeconds ----
    private WaitForSeconds _autoCloseWait;     
    
    void Awake()
    {
        _uiDistanceSqr     = uiDisplayDistance * uiDisplayDistance;
        _interactRadiusSqr = interactionRadius * interactionRadius;
        _autoCloseWait     = new WaitForSeconds(autoCloseDelay);
    }
    void Start()
    {
        BakeUIStrings();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        _doorObstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();

        if (_doorObstacle != null) _doorObstacle.enabled = !isOpen;

        if (playerObj != null)
        {
            _playerTransform = playerObj.transform;
            _playerInteraction = playerObj.GetComponent<PlayerInteraction>();
        }

        if (globalInteractText != null) globalInteractText.text = "";
        
        _defaultRotation = transform.localRotation;
        _targetRotation = _defaultRotation;
        SubscribeMinigameEvent();
    }

    private void SubscribeMinigameEvent()
    {
        if (doorMode == DoorOpenMode.Lockpick && lockpickMinigame != null)
        {
            lockpickMinigame.onMinigameSuccess.AddListener(OnMinigameSuccess);
            lockpickMinigame.onMinigameFailed.AddListener(OnMinigameFailed);
        }
        
        if (doorMode == DoorOpenMode.Crank && crankminigame != null)
        {
            crankminigame.onCrankComplete.AddListener(OnMinigameSuccess);
        }
    }

    void Update()
    {
        transform.localRotation = Quaternion.Slerp(transform.localRotation, _targetRotation, Time.deltaTime * smoothSpeed);
        
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

    private void BakeUIStrings()
    {
    _uiTextOpen     = "Press [F] To Open The Door";
    _uiTextClose    = "Press [F] To Close The Door";
    _uiTextCranking = "Hold [LMB] + Putar Mouse searah jarum jam";

        switch (doorMode)
        {
        case DoorOpenMode.Key:
            string keyUpper = string.IsNullOrEmpty(keyNameForUI) ? "KEY" : keyNameForUI.ToUpper();
            _uiTextLocked   = $"[Locked] Need {keyUpper}";
            _uiTextWithItem = $"[Locked] Need {keyUpper}";
            break;

        case DoorOpenMode.Lockpick:
            _uiTextLocked   = "[Locked] Need Lockpick";
            _uiTextWithItem = "[Locked] Press [F] To Lockpick";
            break;

        case DoorOpenMode.Crank:
            _uiTextLocked   = "[Locked] Need Crank Handle";
            _uiTextWithItem = "[Locked] Press [F] To Use Crank";
            break;
        }
    }
    private void UpdateUIText()
    {
        if (!_isPlayerNear || globalInteractText == null) return;

        //Saat Minigame Crank
        if (_minigameInProgress && doorMode == DoorOpenMode.Crank)
        {
            globalInteractText.text = _uiTextCranking;
            return;
        }

        //Saat Minigame LockPick
        if (_minigameInProgress)
        {
            globalInteractText.text = "";
            return;
        }

        if (isLocked)
        {
            bool hasCorrectItem = HasCorrectItem();
            globalInteractText.text = hasCorrectItem ? _uiTextWithItem : _uiTextLocked;
        }

        else
        {
            globalInteractText.text = isOpen ? _uiTextClose : _uiTextOpen;
        }

       
    }

    private bool HasCorrectItem()
    {
        if (_playerInteraction == null) return false;

        return doorMode switch
        {
            DoorOpenMode.Key      => _playerInteraction.IsHoldingKey(doorID),
            DoorOpenMode.Lockpick => _playerInteraction.IsHoldingLockPick(),
            DoorOpenMode.Crank    => _playerInteraction.IsHoldingCrankHandle(),
            _                     => false
        };
    }
    
    
    public void Interact(GameObject player)
    {
    if (_minigameInProgress) return;

        float distSqr = (transform.position - player.transform.position).sqrMagnitude;
    if (distSqr > _interactRadiusSqr) return;

        if (!isLocked)
        {
            ToggleDoor(player.transform.position);
            return;
        }

         if (_playerInteraction == null) return;

        //Holding Keys
       switch (doorMode)
        {
            case DoorOpenMode.Key:
                HandleKeyInteract(player);
                break;

            case DoorOpenMode.Lockpick:
                HandleLockpickInteract(player);
                break;

            case DoorOpenMode.Crank:
                HandleCrankInteract(player);
                break;
        }
    }

    private void HandleKeyInteract(GameObject player)
    {
        if (_playerInteraction.IsHoldingKey(doorID))
        {
            UnlockDoor();
            ToggleDoor(player.transform.position);
        }
    }
    private void HandleLockpickInteract(GameObject player)
    {
        if (!_playerInteraction.IsHoldingLockPick()) return;

        if (lockpickMinigame == null)
        {
            return;
        }

        _lastInteractorPosition = player.transform.position;
        _minigameInProgress = true;
        lockpickMinigame.StartMinigame();
        MinigameStateManager.Instance?.EnterMinigame();
    }
    private void HandleCrankInteract(GameObject player)
    {
        if (!_playerInteraction.IsHoldingCrankHandle()) return;

        if (crankminigame == null)
        {
            return;
        }

        _lastInteractorPosition = player.transform.position;
        _minigameInProgress = true;
        crankminigame.StartMinigame();
        MinigameStateManager.Instance?.EnterMinigame();
        UpdateUIText();
    }

    private void OnMinigameSuccess()
    {
        _minigameInProgress = false;
        UnlockDoor();
        ToggleDoor(_lastInteractorPosition);

        if (doorMode == DoorOpenMode.Lockpick)
        _playerInteraction?.ConsumeLockPick();
        MinigameStateManager.Instance?.ExitMinigame();
        UpdateUIText();
    }

    private void OnMinigameFailed()
    {
        _minigameInProgress = false;
        MinigameStateManager.Instance?.ExitMinigame();
        UpdateUIText();
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
    
        if (_doorObstacle != null) 
        {
            _doorObstacle.enabled = !isOpen; 
        }

        if (isOpen)
        {
            Vector3 directionToInteractor = transform.position - interactorPosition;
            float dot = Vector3.Dot(transform.forward, directionToInteractor);
            float angle = dot >= 0 ? openAngle : -openAngle;
            
            _targetRotation = _defaultRotation * Quaternion.Euler(0, angle, 0);
    
            if (useAutoClose) autoCloseCoroutine = StartCoroutine(AutoCloseTimer());
        }
        else
        {
            CloseDoor();
        }
        UpdateUIText();
    }

    private void CloseDoor()
    {
        isOpen = false;
        _targetRotation = _defaultRotation;
        if (_doorObstacle != null) _doorObstacle.enabled = true;
        UpdateUIText();
    }

    private IEnumerator AutoCloseTimer()
    {
        yield return _autoCloseWait;
        if (isOpen) CloseDoor();
        autoCloseCoroutine = null;
    }
}