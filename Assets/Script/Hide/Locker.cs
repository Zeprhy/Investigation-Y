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

        if (_currentPlayerScript != null)
        {
            // 1. Hitung progress waktu (0 saat baru masuk, 1 saat waktu habis)
            float progress = 1f - (_hidingTimer / maxHidingTime);

            // 2. Map progress ke range 0.3 sampai 1.0
            float targetAlpha = Mathf.Lerp(0.3f, 1.0f, progress);

            // 3. Kirim ke script player
            _currentPlayerScript.UpdateFadeAlpha(targetAlpha);
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
        
        // Mulai animasi masuk
        StartCoroutine(SmoothEnter(player));
    }

    private IEnumerator SmoothEnter(MovementPlayer player)
    {
        player.SetHiddenStatus(true);
        player.UpdateFadeAlpha(0.3f);
        
        // Matikan CC agar bisa dipindahkan lewat script
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // Buka pintu
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

        // Tutup pintu setelah masuk
        if (lockerAnimator != null) lockerAnimator.SetBool("IsOpen", false);

        player.SetHiddenStatus(true);
    }

    private void ExitLocker(MovementPlayer player)
    {
        if (player == null) return;

        _isOccupied = false;
        _currentPlayerScript = null;

        if (exitUI != null) exitUI.SetActive(false);
        
        // JANGAN teleport player di sini, biarkan Coroutine yang melakukannya
        StartCoroutine(SmoothExit(player));
        player.SetHiddenStatus(false);
    }

    private IEnumerator SmoothExit(MovementPlayer player)
    {
        // 1. Buka pintu dulu
        if (lockerAnimator != null) lockerAnimator.SetBool("IsOpen", true);
        yield return new WaitForSeconds(0.2f); // Tunggu sebentar agar pintu mulai terbuka

        player.SetHiddenStatus(false);
        
        float elapsed = 0f;
        float duration = 1f / transitionSpeed;
        Vector3 startPos = player.transform.position;
        Quaternion startRot = player.transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            player.transform.position = Vector3.Lerp(startPos, exitPoint.position, t);
            // Tambahkan rotasi ke arah exit point agar tidak kaku
            player.transform.rotation = Quaternion.Slerp(startRot, exitPoint.rotation, t);
            yield return null;
        }

        player.transform.position = exitPoint.position;

        // 2. Aktifkan kembali kontrol player
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = true;

        // 3. Tutup pintu setelah player di luar
        yield return new WaitForSeconds(0.3f);
        if (lockerAnimator != null) lockerAnimator.SetBool("IsOpen", false);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}