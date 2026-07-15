using UnityEngine;
using System.Collections;

/// <summary>
/// Parent (BaseDoor)
/// </summary>
public abstract class BaseDoor : MonoBehaviour
{
    [Header("Base Settings")]
    public bool isOpen = false;
    public bool isLocked = true;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float interactionRadius = 3f;

    [Header("Auto Close")]
    [SerializeField] protected bool useAutoClose = true;
    [SerializeField] private float autoCloseDelay = 3f;
    protected Coroutine autoCloseCoroutine;

    [Header("UI System (Direct TMP)")]
    //[SerializeField] private TextMeshProUGUI globalInteractText;
    [SerializeField] private float uiDisplayDistance = 3.0f;
    // ---- Cache komponen ----
    protected Quaternion _targetRotation;
    protected Quaternion _defaultRotation;
    protected Transform _playerTransform;
    protected PlayerInteraction _playerInteraction;
    protected UnityEngine.AI.NavMeshObstacle _doorObstacle;
 
  
    // ---- Cache threshold ----
    protected bool _isPlayerNear = false;
    private float _uiDistanceSqr;
    protected float _interactRadiusSqr;
 
    // ---- Cache WaitForSeconds ----
    protected WaitForSeconds _autoCloseWait;     
    
    protected virtual void Awake()
    {
        _uiDistanceSqr     = uiDisplayDistance * uiDisplayDistance;
        _interactRadiusSqr = interactionRadius * interactionRadius;
        _autoCloseWait     = new WaitForSeconds(autoCloseDelay);
    }
    protected virtual void Initialize()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        _doorObstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();

        if (_doorObstacle != null) _doorObstacle.enabled = !isOpen;

        if (playerObj != null)
        {
            _playerTransform = playerObj.transform;
            _playerInteraction = playerObj.GetComponent<PlayerInteraction>();
        }
        
        _defaultRotation = transform.localRotation;
        _targetRotation = _defaultRotation;
    }

    protected virtual void Update()
    {
        transform.localRotation = Quaternion.Slerp(transform.localRotation, _targetRotation, Time.deltaTime * smoothSpeed);
        HandleUIDistance();
    }

    protected  void HandleUIDistance()
    {
        if (_playerTransform == null) return;
        float distSqr = (_playerTransform.position - transform.position).sqrMagnitude;

        if (distSqr <= _uiDistanceSqr)
        {
            _isPlayerNear = true;
            UpdateUIText();
        }

        else if (_isPlayerNear)
        {
            _isPlayerNear = false;
            HideUIText();
        }
    }

    
    public abstract void Interact(GameObject player);
    protected abstract void UpdateUIText();
    protected abstract void HideUIText();


    public void UnlockDoor()
    {
        isLocked = false;
        UpdateUIText();
    }

    public virtual void ToggleDoor(Vector3 interactorPosition)
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

    public virtual void CloseDoor()
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