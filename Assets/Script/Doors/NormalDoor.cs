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

    [Header("Lock Settings")]
    public string doorID = "";

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
    
    // --- VERSION 1: KHUSUS PLAYER ---
    // Fungsi ini dipanggil oleh script MovementPlayer
    public void Interact(MovementPlayer player)
    {
        // Cek kunci hanya berlaku untuk Player
        if (isLocked)
        {
            if (player.HasKey(doorID))
            {
                isLocked = false;
                Debug.Log("Pintu Terbuka dengan Kunci");
            }
            else
            {
                Debug.Log("Pintu Terkunci!");
                return;
            }
        }
    
        // Panggil logika inti interaksi menggunakan posisi player
        ToggleDoor(player.transform.position);
    }
    
    // --- VERSION 2: KHUSUS MUSUH / UMUM ---
    // Fungsi ini dipanggil oleh EnemyAI (mengirimkan Vector3)
    public void Interact(Vector3 interactionPosition)
    {
        // Musuh biasanya tidak bisa buka pintu terkunci (atau bisa, tergantung maumu)
        if (isLocked) return; 
    
        // Panggil logika inti interaksi menggunakan posisi yang dikirim
        ToggleDoor(interactionPosition);
    }
    
    // --- LOGIKA INTI (PRIVATE) ---
    // Agar kita tidak menulis kode yang sama berulang-ulang
    private void ToggleDoor(Vector3 interactorPosition)
    {
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
        }
    
        isOpen = !isOpen;
    
        if (isOpen)
        {
            // LOGIKA DOT PRODUCT: Menentukan arah ayunan pintu
            Vector3 directionToInteractor = transform.position - interactorPosition;
            float dot = Vector3.Dot(transform.forward, directionToInteractor);
            
            // Jika pembuka ada di depan pintu (Dot > 0), pintu ayun ke belakang (90)
            // Jika pembuka ada di belakang pintu (Dot < 0), pintu ayun ke depan (-90)
            float angle = dot >= 0 ? openAngle : -openAngle;
            
            targetRotation = defaultRotation * Quaternion.Euler(0, angle, 0);
    
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