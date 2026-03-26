using UnityEngine;
using System.Collections;
using TMPro;

public class Locker : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactionRadius = 2.5f; 
    [SerializeField] private float uiDisplayDistance = 3.0f;
    [SerializeField] private Color gizmoColor = Color.yellow;

    [Header("Hiding Timer")]
    [SerializeField] private float maxHidingTime = 10f; 
    private float _hidingTimer;

    [Header("Animation Settings")]
    [SerializeField] private Animator lockerAnimator;
    [SerializeField] private float transitionSpeed = 5f;

    [Header("References")]
    [SerializeField] private Transform hidingPoint;
    [SerializeField] private Transform exitPoint;

    [Header("UI System (Direct TMP)")]
    [SerializeField] private TextMeshProUGUI globalInteractText;

    private bool _isOccupied = false;
    private bool _isPlayerNear = false;
    private Transform _playerTransform;
    private MovementPlayer _currentPlayerScript;
    private PlayerInteraction _playerInteraction;

    public bool IsOccupied => _isOccupied;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;
        if (globalInteractText != null) globalInteractText.text = "";
    }

    private void Update()
    {
        if (_playerTransform == null) return;
        HandleUIDisplay();
        if (_isOccupied) HandleLockerTimer();
    }

    private void HandleUIDisplay()
    {
        if (globalInteractText == null) return;
        float distance = Vector3.Distance(transform.position, _playerTransform.position);
        if (distance <= uiDisplayDistance)
        {
            _isPlayerNear = true;
            UpdateLockerUIText();
        }
        else if (_isPlayerNear)
        {
            _isPlayerNear = false;
            globalInteractText.text = "";
        }
    }

    private void UpdateLockerUIText()
    {
        globalInteractText.text = _isOccupied ? "Press [Q] To Leave" : "Press [F] To Hide";
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
        if (_hidingTimer <= 0) ExitLocker(_currentPlayerScript);
    }

    public void Interact(MovementPlayer player)
    {
        if (!_isOccupied) EnterLocker(player);
        else ExitLocker(player);
    }

    private void EnterLocker(MovementPlayer player)
    {
        _isOccupied = true;
        _currentPlayerScript = player;
        _hidingTimer = maxHidingTime;
        _playerInteraction = player.GetComponent<PlayerInteraction>();

        if (_playerInteraction != null)
        {
            _playerInteraction.SetCurrentLocker(this);
            _playerInteraction.SetHiddenStatus(true);
            _playerInteraction.UpdateFadeAlpha(0.3f);
        }

        StartCoroutine(SmoothEnter(player));
    }

    public void ExitLocker(MovementPlayer player)
    {
        if (player == null || !_isOccupied) return;
        _isOccupied = false;
        
        if (_playerInteraction != null)
        {
            _playerInteraction.UpdateFadeAlpha(0f);
        }

        StartCoroutine(SmoothExit(player));
    }

    private IEnumerator SmoothEnter(MovementPlayer player)
    {
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

            if (player.PlayerCamera != null)
                player.PlayerCamera.localRotation = Quaternion.Slerp(player.PlayerCamera.localRotation, Quaternion.identity, t);
            
            yield return null;
        }

        player.transform.position = hidingPoint.position;
        player.transform.rotation = hidingPoint.rotation;
        if (lockerAnimator != null) lockerAnimator.SetBool("IsOpen", false);
    }

    private IEnumerator SmoothExit(MovementPlayer player)
    {
        if (lockerAnimator != null) lockerAnimator.SetBool("IsOpen", true);
        yield return new WaitForSeconds(0.2f);

        Transform cam = player.PlayerCamera; 
        Quaternion camStartRot = (cam != null) ? cam.localRotation : Quaternion.identity;

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

            if (cam != null) {
                cam.localRotation = Quaternion.Slerp(camStartRot, Quaternion.identity, t);
            }
            yield return null;
        }

        player.transform.position = exitPoint.position;
        player.ResetRotation(exitPoint.eulerAngles.y);

        if (_playerInteraction != null) {
            _playerInteraction.SetHiddenStatus(false);
            _playerInteraction.ClearLocker();
        }

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