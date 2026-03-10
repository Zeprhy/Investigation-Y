using UnityEngine;

public class Locker : MonoBehaviour
{
    [Header("Settings")]
    // [SerializeField] memungkinkan variabel private muncul di Inspector
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

    [Header("References")]
    [SerializeField] private Transform hidingPoint;
    [SerializeField] private Transform exitPoint;

    [Header("UI Panels")]
    [SerializeField] private GameObject interactUI;
    [SerializeField] private GameObject exitUI;

    private bool _isOccupied = false;
    private Transform _playerTransform;
    private MovementPlayer _currentPlayerScript;
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

        // JIKA PLAYER DI DALAM LOKER
        if (_isOccupied)
        {
            HandleLockerTimer();

            if (interactUI != null) interactUI.SetActive(false); // Sembunyikan UI Masuk
            if (exitUI != null) exitUI.SetActive(true);        // Tampilkan UI Keluar
            return;
        }

        // JIKA PLAYER DI LUAR LOKER
        float distance = Vector3.Distance(transform.position, _playerTransform.position);
        bool isInRange = distance <= interactionRadius;

        if (interactUI != null) interactUI.SetActive(isInRange); // Muncul jika dekat
        if (exitUI != null) exitUI.SetActive(false);             // Selalu mati jika di luar
    }

    private void HandleLockerTimer()
    {
        _hidingTimer -= Time.deltaTime;

        // Jika waktu habis, keluarkan paksa
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
            EnterLocker(player);
        else
            ExitLocker(player);
    }

    private void EnterLocker(MovementPlayer player)
    {
        _isOccupied = true;

        _currentPlayerScript = player;
        _hidingTimer = maxHidingTime;

        _currentYaw = 0f;
        _currentPitch = 0f;

        if (interactUI != null) interactUI.SetActive(false);

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.position = hidingPoint.position;
        player.transform.rotation = hidingPoint.rotation;

        player.SetHiddenStatus(true);
    }

    private void ExitLocker(MovementPlayer player)
    {
        _isOccupied = false;

        if (exitUI != null) exitUI.SetActive(false);
        player.transform.position = exitPoint.position;

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = true;

        player.SetHiddenStatus(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}