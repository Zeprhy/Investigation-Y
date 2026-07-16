using UnityEngine;
using System.Collections;
using DG.Tweening; // Required for DOTween

public class ElevatorSlidingDoor : MonoBehaviour
{
    [Header("== Status Settings ==")]
    public bool isOpen = false;
    public bool isLocked = true;

    [Header("== Door Animation (DOTween) ==")]
    [SerializeField] private Transform leftDoor;
    [Tooltip("How far the left door slides when opened (based on local axis)")]
    [SerializeField] private Vector3 leftDoorSlideOffset = new Vector3(-1.5f, 0, 0);

    [Space(10)]
    [Tooltip("Optional: Assign if the elevator has 2 doors (left and right). Leave empty if it only has 1 door.")]
    [SerializeField] private Transform rightDoor;
    [Tooltip("How far the right door slides when opened (based on local axis)")]
    [SerializeField] private Vector3 rightDoorSlideOffset = new Vector3(1.5f, 0, 0);
    
    [Space(10)]
    [SerializeField] private float animationDuration = 1.5f;

    [Header("== Auto Close ==")]
    [SerializeField] private bool useAutoClose = true;
    [SerializeField] private float autoCloseDelay = 4f;

    private Coroutine autoCloseCoroutine;
    private WaitForSeconds _autoCloseWait;

    private Vector3 _leftDoorClosedPos;
    private Vector3 _rightDoorClosedPos;
    
    private Transform _playerTransform;
    private bool _isPlayerNear = false;
    private float _uiDistanceSqr;
    private float _interactRadiusSqr;

    void Awake()
    {
        _autoCloseWait = new WaitForSeconds(autoCloseDelay);
    }

    void Start()
    {
        // Save the default (closed) position at the start of the game
        if (leftDoor != null) _leftDoorClosedPos = leftDoor.localPosition;
        if (rightDoor != null) _rightDoorClosedPos = rightDoor.localPosition;
    }
   
    void Update()
    {
        HandleUIDistance(); 
    }

    private void HandleUIDistance()
    {
        if (_playerTransform == null) return;
        
        float distSqr = (_playerTransform.position - transform.position).sqrMagnitude;

        if (distSqr <= _uiDistanceSqr)
        {
            _isPlayerNear = true;
        }
        else if (_isPlayerNear)
        {
            _isPlayerNear = false;
        }
    }

    public void Interact(GameObject player)
    {
        float distSqr = (transform.position - player.transform.position).sqrMagnitude;
        if (distSqr > _interactRadiusSqr) return;

        if (!isLocked)
        {
            ToggleDoor();
        }
    }
   
    public void ToggleDoor()
    {
        if (autoCloseCoroutine != null) StopCoroutine(autoCloseCoroutine);
    
        isOpen = !isOpen;

        // --- DOTween Open/Close Elevator Animation Logic ---
        if (isOpen)
        {
            OpenElevatorAnimation();
            if (useAutoClose) autoCloseCoroutine = StartCoroutine(AutoCloseTimerLift());
        }
        else
        {
            CloseDoor();
        }
    }

    public void CloseDoor()
    {
        isOpen = false;

        CloseElevatorAnimation();
    }

    // --- DOTween Helper Functions ---

    private void OpenElevatorAnimation()
    {
        if (leftDoor != null)
        {
            leftDoor.DOKill(); // Stop previous animation if any to prevent glitches
            leftDoor.DOLocalMove(_leftDoorClosedPos + leftDoorSlideOffset, animationDuration).SetEase(Ease.InOutQuad);
        }

        if (rightDoor != null)
        {
            rightDoor.DOKill();
            rightDoor.DOLocalMove(_rightDoorClosedPos + rightDoorSlideOffset, animationDuration).SetEase(Ease.InOutQuad);
        }
    }

    private void CloseElevatorAnimation()
    {
        if (leftDoor != null)
        {
            leftDoor.DOKill();
            leftDoor.DOLocalMove(_leftDoorClosedPos, animationDuration).SetEase(Ease.InOutQuad);
        }

        if (rightDoor != null)
        {
            rightDoor.DOKill();
            rightDoor.DOLocalMove(_rightDoorClosedPos, animationDuration).SetEase(Ease.InOutQuad);
        }
    }

    private IEnumerator AutoCloseTimerLift()
    {
        yield return _autoCloseWait;
        if (isOpen) CloseDoor();
        autoCloseCoroutine = null;
    }
}