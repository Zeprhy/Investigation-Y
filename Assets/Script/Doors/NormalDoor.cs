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

    private Quaternion targetRotation;
    private Quaternion defaultRotation;
    private Transform _playerTransform;
    private bool _isPlayerNear = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _playerTransform = playerObj.transform;

        if (globalInteractText != null) globalInteractText.text = "";
        
        defaultRotation = transform.localRotation;
        targetRotation = defaultRotation;
    }

    void Update()
    {
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * smoothSpeed);
        
        HandleUIDisplay();
    }

    private void HandleUIDisplay()
    {
        if (_playerTransform == null || globalInteractText == null) return;

        float distance = Vector3.Distance(transform.position, _playerTransform.position);

        if (distance <= uiDisplayDistance)
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
        if (isLocked)
        {
            globalInteractText.text = $"[Locked] Need {keyNameForUI.ToUpper()}";
        }
        else
        {
            string action = isOpen ? "Closed" : "Open";
            globalInteractText.text = $"Press [F] To {action} The Door";
        }
    }
    
    public void Interact(GameObject player)
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance > interactionRadius) return;

        if (!isLocked)
        {
            ToggleDoor(player.transform.position);
            return;
        }

        PlayerInteraction interaction = player.GetComponent<PlayerInteraction>();

        if (interaction != null && interaction.IsHoldingKey(doorID))
        {
            UnlockDoor();
            ToggleDoor(player.transform.position);
        }
    }

    public void Interact(Vector3 interactorPosition)
    {
        if (isLocked) return;

        ToggleDoor(interactorPosition);
    }

    private void UnlockDoor()
    {
        isLocked = false;   
    }

    private void ToggleDoor(Vector3 interactorPosition)
    {
        if (autoCloseCoroutine != null) StopCoroutine(autoCloseCoroutine);
    
        isOpen = !isOpen;
    
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
    }

    private IEnumerator AutoCloseTimer()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        if (isOpen) CloseDoor();
        autoCloseCoroutine = null;
    }
}