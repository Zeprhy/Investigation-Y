using UnityEngine;
using System.Collections;
using TMPro;

public class ElevatorSlidingDoor : MonoBehaviour
{
    [Header("Door Parts")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;

    [Header("Settings")]
    public bool isOpen = false;
    public bool isLocked = false;
    [SerializeField] private float slideDistance = 1.2f; // Seberapa jauh pintu bergeser
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
    [SerializeField] private string keyNameForUI = "Access Card";

    // Posisi awal pintu
    private Vector3 leftDoorDefaultPos;
    private Vector3 rightDoorDefaultPos;
    // Target posisi pintu
    private Vector3 leftDoorTargetPos;
    private Vector3 rightDoorTargetPos;

    private Transform _playerTransform;
    private PlayerInteraction _playerInteraction;
    private UnityEngine.AI.NavMeshObstacle doorObstacle;
    private bool _isPlayerNear = false;

    private string _uiTextLocked;
    private string _uiTextOpen;
    private string _uiTextClose;
    private float _uiDistanceSqr;
    private float _interactRadiusSqr;
    private WaitForSeconds _autoCloseWait;

    void Awake()
    {
        string keyUpper = keyNameForUI.ToUpper();
        _uiTextLocked = $"[Locked] Need {keyUpper}";
        _uiTextOpen = "Press [F] To Open Elevator";
        _uiTextClose = "Press [F] To Close Elevator";

        _uiDistanceSqr = uiDisplayDistance * uiDisplayDistance;
        _interactRadiusSqr = interactionRadius * interactionRadius;
        _autoCloseWait = new WaitForSeconds(autoCloseDelay);
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        doorObstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();

        if (playerObj != null)
        {
            _playerTransform = playerObj.transform;
            _playerInteraction = playerObj.GetComponent<PlayerInteraction>();
        }

        // Simpan posisi default (tertutup)
        leftDoorDefaultPos = leftDoor.localPosition;
        rightDoorDefaultPos = rightDoor.localPosition;

        // Set target awal ke posisi tertutup
        leftDoorTargetPos = leftDoorDefaultPos;
        rightDoorTargetPos = rightDoorDefaultPos;

        if (doorObstacle != null) doorObstacle.enabled = !isOpen;
    }

    void Update()
    {
        // Pergerakan halus menggunakan Lerp (bukan Slerp karena ini posisi)
        leftDoor.localPosition = Vector3.Lerp(leftDoor.localPosition, leftDoorTargetPos, Time.deltaTime * smoothSpeed);
        rightDoor.localPosition = Vector3.Lerp(rightDoor.localPosition, rightDoorTargetPos, Time.deltaTime * smoothSpeed);
        
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

        if (isLocked)
        {
            globalInteractText.text = _uiTextLocked;
        }
        else
        {
            globalInteractText.text = isOpen ? _uiTextClose : _uiTextOpen;
        }
    }

    public void Interact(GameObject player)
    {
        float distSqr = (transform.position - player.transform.position).sqrMagnitude;
        if (distSqr > (_interactRadiusSqr + 0.5f)) return;

        if (!isLocked)
        {
            ToggleDoor();
            return;
        }

        if (_playerInteraction != null && _playerInteraction.IsHoldingKey(doorID))
        {
            isLocked = false;
            ToggleDoor();
        }
    }

    private void ToggleDoor()
    {
        if (autoCloseCoroutine != null) StopCoroutine(autoCloseCoroutine);
    
        isOpen = !isOpen;
    
        if (doorObstacle != null) doorObstacle.enabled = !isOpen;

        if (isOpen)
        {
            // Pintu Kiri geser ke kiri (-X), Pintu Kanan geser ke kanan (+X)
            leftDoorTargetPos = leftDoorDefaultPos + new Vector3(-slideDistance, 0, 0);
            rightDoorTargetPos = rightDoorDefaultPos + new Vector3(slideDistance, 0, 0);
    
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
        leftDoorTargetPos = leftDoorDefaultPos;
        rightDoorTargetPos = rightDoorDefaultPos;
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