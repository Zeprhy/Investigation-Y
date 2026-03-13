using UnityEngine;
using System.Collections;

public class Locker : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactionRadius = 2.5f; 
    [SerializeField] private Color gizmoColor = Color.yellow;

    [Header("Hiding Timer")]
    [SerializeField] private float maxHidingTime = 10f; 
    private float _hidingTimer;

    [Header("Peeking Constraints")]
    [SerializeField] private float minYaw = -60f;
    [SerializeField] private float maxYaw = 60f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 30f;

    [Header("Animation Settings")]
    [SerializeField] private Animator lockerAnimator;
    [SerializeField] private float transitionSpeed = 5f;

    [Header("References")]
    [SerializeField] private Transform hidingPoint;
    [SerializeField] private Transform exitPoint;

    [Header("UI Panels")]
    [SerializeField] private GameObject interactUI;
    [SerializeField] private GameObject exitUI;

    private bool _isOccupied = false;
    private Transform _playerTransform;
    private MovementPlayer _currentPlayerScript;
    private PlayerInteraction _playerInteraction;
    private float _currentYaw = 0f;
    private float _currentPitch = 0f;

    public bool IsOccupied => _isOccupied;

    private void Start()
    {
        // Mencari player satu kali saat start
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;

        if (interactUI != null) interactUI.SetActive(false);
        if (exitUI != null) exitUI.SetActive(false);
    }

    private void Update()
    {
        if (_playerTransform == null) return;

        if (_isOccupied)
        {
            HandleLockerTimer();

            if (interactUI != null) interactUI.SetActive(false);
            if (exitUI != null) exitUI.SetActive(true);
            return;
        }

        float distance = Vector3.Distance(transform.position, _playerTransform.position);
        bool isInRange = distance <= interactionRadius;

        if (interactUI != null) interactUI.SetActive(isInRange);
        if (exitUI != null) exitUI.SetActive(false);
    }

    private void HandleLockerTimer()
    {
        _hidingTimer -= Time.deltaTime;

        if (_currentPlayerScript != null && _playerInteraction != null)
        {
            float progress = 1f - (_hidingTimer / maxHidingTime);
            float targetAlpha = Mathf.Lerp(0.3f, 1.0f, progress);
            _playerInteraction.UpdateFadeAlpha(targetAlpha);
        }

        if (_hidingTimer <= 0)
        {
            ExitLocker(_currentPlayerScript);
        }
    }

    public void HandleCameraPeeking(Transform camTransform, float mouseX, float mouseY)
    {
        _currentYaw += mouseX;
        _currentPitch -= mouseY;

        _currentYaw = Mathf.Clamp(_currentYaw, minYaw, maxYaw);
        _currentPitch = Mathf.Clamp(_currentPitch, minPitch, maxPitch);

        camTransform.localRotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
    }

    public void Interact(MovementPlayer player)
    {
        if (!_isOccupied)
        {
            EnterLocker(player);
        }
        else
        {
            ExitLocker(player);
        }
    }

    private void EnterLocker(MovementPlayer player)
    {
        _isOccupied = true;
        _currentPlayerScript = player;
        _hidingTimer = maxHidingTime;

        _playerInteraction = player.GetComponent<PlayerInteraction>();

        if (player != null)
        {
            player.GetComponent<PlayerInteraction>().SetHiddenStatus(true);
        }

        if (_playerInteraction != null)
        {
            _playerInteraction.SetCurrentLocker(this);
            _playerInteraction.SetHiddenStatus(true);
            _playerInteraction.UpdateFadeAlpha(0.3f);
        }

        _currentYaw = 0f;
        _currentPitch = 0f;
        if (interactUI != null) interactUI.SetActive(false);

        StartCoroutine(SmoothEnter(player));
    }

    public void ExitLocker(MovementPlayer player)
    {
        if (player == null || !_isOccupied) return;

        _isOccupied = false;
        
        if (_playerInteraction != null)
        {
            _playerInteraction.UpdateFadeAlpha(0f);
            _playerInteraction.SetHiddenStatus(false);
            _playerInteraction.ClearLocker();
        }

        StartCoroutine(SmoothExit(player));
    }

    private IEnumerator SmoothEnter(MovementPlayer player)
    {
        player.GetComponent<PlayerInteraction>().SetHiddenStatus(true);

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        if (lockerAnimator != null) lockerAnimator.SetBool("IsOpen", true);

        float elapsed = 0f;
        float duration = 1f / transitionSpeed;
        Vector3 startPos = player.transform.position;
        Quaternion startRot = player.transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            player.transform.position = Vector3.Lerp(startPos, hidingPoint.position, t);
            player.transform.rotation = Quaternion.Slerp(startRot, hidingPoint.rotation, t);
            yield return null;
        }

        player.transform.position = hidingPoint.position;
        player.transform.rotation = hidingPoint.rotation;

        if (lockerAnimator != null) lockerAnimator.SetBool("IsOpen", false);

        player.GetComponent<PlayerInteraction>().SetHiddenStatus(true);
    }

    private IEnumerator SmoothExit(MovementPlayer player)
    {
        if (lockerAnimator != null) lockerAnimator.SetBool("IsOpen", true);
        yield return new WaitForSeconds(0.2f);

        player.GetComponent<PlayerInteraction>().SetHiddenStatus(false);
        
        float elapsed = 0f;
        float duration = 1f / transitionSpeed;
        Vector3 startPos = player.transform.position;
        Quaternion startRot = player.transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            player.transform.position = Vector3.Lerp(startPos, exitPoint.position, t);
            player.transform.rotation = Quaternion.Slerp(startRot, exitPoint.rotation, t);
            yield return null;
        }

        player.transform.position = exitPoint.position;

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = true;

        yield return new WaitForSeconds(0.3f);
        if (lockerAnimator != null) lockerAnimator.SetBool("IsOpen", false);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}