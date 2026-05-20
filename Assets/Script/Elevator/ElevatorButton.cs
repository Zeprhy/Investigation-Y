using UnityEngine;
using TMPro;

public class ElevatorButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ElevatorSlidingDoor elevatorDoor;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip buttonSFX;

    [Header("Settings")]
    [SerializeField] private bool canPressed = true;
    [SerializeField] private float uiDisplayDistance = 2.5f;

    [Header("UI System")]
    [SerializeField] private TextMeshProUGUI globalInteractText;
    [SerializeField] private string interactMessage = "Press [F] To Call Elevator";

    private Transform _playerTransform;
    private bool _isPlayerNear = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _playerTransform = playerObj.transform;
    }

    void Update()
    {
        HandleUI();
    }

    // This method handles the [F] interaction
    public void Interaction()
    {
        if (!canPressed) return;

        if (!PowerSystem.IsPowerOn) return;

        if (elevatorDoor != null)
        {
            elevatorDoor.Interact(); // Calls the door
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(buttonSFX);
        }
    }

    private void HandleUI()
    {
        if (_playerTransform == null || globalInteractText == null) return;

        float distance = Vector3.Distance(transform.position, _playerTransform.position);

        if (distance <= uiDisplayDistance)
        {
            _isPlayerNear = true;
            globalInteractText.text = interactMessage;
        }
        else if (_isPlayerNear)
        {
            _isPlayerNear = false;
            globalInteractText.text = "";
        }
    }
}