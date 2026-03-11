using UnityEngine;
using System.Collections;

public class NormalDoor : MonoBehaviour
{
    [Header("Settings")]
    public bool isOpen = false;
    public bool isLocked = false;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float interactionRadius = 2.5f;

    [Header("Auto Close")]
    [SerializeField] private bool useAutoClose = true;
    [SerializeField] private float autoCloseDelay = 3f;
    private Coroutine autoCloseCoroutine;

    [Header("UI Panels")]
    [SerializeField] private GameObject interactUI;

    private Quaternion targetRotation;
    private Quaternion defaultRotation;
    private Transform _playerTransform;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _playerTransform = playerObj.transform;

        if (interactUI != null) interactUI.SetActive(false);
        defaultRotation = transform.localRotation;
        targetRotation = defaultRotation;
    }

    void Update()
    {
        // Pergerakan pintu yang halus
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * smoothSpeed);
        HandleUI();
    }

    private void HandleUI()
    {
        if (_playerTransform == null || interactUI == null) return;

        float distance = Vector3.Distance(transform.position, _playerTransform.position);
        
        // Munculkan UI jika player dekat dan pintu tidak sedang "sibuk" (opsional)
        if (distance <= interactionRadius)
        {
            interactUI.SetActive(true);
        }
        else
        {
            interactUI.SetActive(false);
        }
    }
    
    public void Interact(Vector3 playerPosition)
    {
        // Jika player berinteraksi saat coroutine sedang berjalan, batalkan dulu agar tidak bentrok
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
        }

        isOpen = !isOpen;

        if (isOpen)
        {
            // Tentukan arah buka
            Vector3 directionToPlayer = transform.position - playerPosition;
            float dot = Vector3.Dot(transform.forward, directionToPlayer);
            float angle = dot >= 0 ? openAngle : -openAngle;
            
            targetRotation = defaultRotation * Quaternion.Euler(0, angle, 0);

            // Mulai hitung mundur untuk menutup otomatis
            if (useAutoClose)
            {
                autoCloseCoroutine = StartCoroutine(AutoCloseTimer());
            }
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
        // Tunggu selama durasi yang ditentukan
        yield return new WaitForSeconds(autoCloseDelay);

        // Tutup pintu jika masih terbuka
        if (isOpen)
        {
            CloseDoor();
        }
        
        autoCloseCoroutine = null;
    }
}