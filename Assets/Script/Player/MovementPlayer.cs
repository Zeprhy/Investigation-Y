using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class MovementPlayer : MonoBehaviour
{
    [Header("Kamera & Look")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float lookSpeed = 0.1f;
    [SerializeField] private float lookXLimit = 45f;

    [Header("Kecepatan Jalan")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float runSpeed = 12f;
    [SerializeField] private float crouchSpeed = 3f;

    [Header("Fisika")]
    [SerializeField] private float jumpPower = 7f;
    [SerializeField] private float gravity = 20f;
    [SerializeField] private float defaultHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;

    [Header("Stealth & Hide")]
    [SerializeField] private LayerMask obstacleMask;
    private Locker currentLocker;
    
    [Header("Flashlight Settings")]
    [SerializeField] private GameObject flashlightObject;

    [Header("Noise System")]
    [SerializeField] private float baseNoiseRadius = 5f;
    [SerializeField] private float sprintNoiseMultiplier = 2f;
    [SerializeField] private float crouchNoiseMultiplier = 0.5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Stamina System")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float currentStamina;
    [SerializeField] private float staminaDrain = 20f;
    [SerializeField] private float staminaRegen = 15f;
    [SerializeField] private float staminaRegenDelay = 2f;
    [SerializeField] private Image staminaBarFill;

    [Header("UI Effects")]
    [SerializeField] private CanvasGroup hideFadeGroup;

    // Komponen & Data Input internal
    private float regenDelayTimer;
    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private Vector2 inputMove;
    private Vector2 inputLook;
    private float rotationX = 0;
    private bool isCursorLocked;
    
    // Status Karakter
    private bool isRunning;
    private bool isCrouching;
    private bool isBlockedAbove;
    private bool isFlashlightOn = false;
    private bool isExhausted = false;
    public bool IsHidden { get; private set; }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        currentStamina = maxStamina;
        
        // Kunci kursor agar tidak keluar layar
        SetCursorState(false);

        if (flashlightObject != null)
        {
            flashlightObject.SetActive(false); // Senter mulai dalam keadaan mati
        }
    }

    // BAGIAN INPUT (Dihubungkan ke Player Input Component)
    public void OnMove(InputAction.CallbackContext context) => inputMove = context.ReadValue<Vector2>();
    public void OnLook(InputAction.CallbackContext context) => inputLook = context.ReadValue<Vector2>();
    public void OnJump(InputAction.CallbackContext context) { if (context.performed) ApplyJump(); }
    public void OnSprint(InputAction.CallbackContext context) => isRunning = context.performed;
    public void OnCrouch(InputAction.CallbackContext context) => isCrouching = context.performed;

    void Update()
    {
        if (IsHidden) return;
        
        ApplyRotation();
        HandleStamina();
        UpdateStaminaUI();
        ApplyMovement();
        ApplyGravity();
        ApplyCrouch();
        HandleNoiseEmission();

        // Eksekusi pergerakan akhir
        characterController.Move(moveDirection * Time.deltaTime);
    }

    public void SetHiddenStatus(bool status)
    {
        IsHidden = status;

        if (IsHidden && isFlashlightOn)
        {
            ToggleFlashlight();
        }

        if (hideFadeGroup != null)
        {
            hideFadeGroup.alpha = status ? 0.9f : 0f;
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Tombol E ditekan!"); // Pastikan ini muncul di Console
    
            if (IsHidden && currentLocker != null)
            {
                currentLocker.Interact(this);
                currentLocker = null;
                return;
            }
    
            RaycastHit hit;
            // Kita gunakan SphereCast dengan radius 0.2 agar lebih mudah mengenai loker
            Vector3 rayOrigin = playerCamera.transform.position;
            Vector3 rayDirection = playerCamera.transform.forward;
    
            if (Physics.SphereCast(rayOrigin, 0.2f, rayDirection, out hit, 3.0f))
            {
                Debug.Log("Menyentuh objek: " + hit.collider.name);
    
                if (hit.collider.TryGetComponent(out Locker locker))
                {
                    Debug.Log("Loker terdeteksi! Masuk...");
                    currentLocker = locker;
                    locker.Interact(this);
                }
            }
        }
    }

    public void OnToggleCursor(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            SetCursorState(!isCursorLocked);
        }
    }

    private void SetCursorState(bool locked)
    {
        isCursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = locked;
    }
    private void HandleStamina()
    {
        bool isMoving = inputMove.magnitude > 0.1f;

        // Jika sedang berlari, bergerak, dan tidak lelah
        if (isRunning && isMoving && !isExhausted && !isCrouching)
        {
            currentStamina -= staminaDrain * Time.deltaTime;
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isExhausted = true; // Masuk status sangat lelah

                regenDelayTimer = staminaRegenDelay;
            }
        }
        else
        {
            // Jika masih ada waktu jeda, kurangi timernya
            if (regenDelayTimer > 0)
            {
                regenDelayTimer -= Time.deltaTime;
            }
            else
            {
                // Jika timer sudah habis (<= 0), barulah stamina boleh terisi
                currentStamina += staminaRegen * Time.deltaTime;
            }

            // Pemain bisa lari lagi jika stamina sudah cukup pulih (misal di atas 20%)
            if (isExhausted && currentStamina >= (maxStamina * 0.2f))
            {
                isExhausted = false;
            }
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }

    private void UpdateStaminaUI()
    {
        if (staminaBarFill != null)
        {
            // Mengubah nilai fill amount (0 sampai 1) berdasarkan persentase stamina
            staminaBarFill.fillAmount = currentStamina / maxStamina;
        }
    }

    private void ApplyRotation()
    {
        if (!isCursorLocked)
        {

        float sensitivityMultiplier = 0.1f;
        
        // Putar Kamera (Atas - Bawah)
        rotationX -= inputLook.y * lookSpeed * sensitivityMultiplier;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        // Putar Badan (Kiri - Kanan)
        transform.Rotate(Vector3.up * inputLook.x * lookSpeed * sensitivityMultiplier);
        }
    }

    private void ApplyMovement()
    {
        // Jika sembunyi, jangan biarkan ada pergerakan fisik
        if (IsHidden) 
        {
            moveDirection = Vector3.zero;
            return;
        }

        // Tentukan kecepatan berdasarkan status
        bool canRun = isRunning && currentStamina > 0 && !isExhausted;
        float currentSpeed = isCrouching ? crouchSpeed : (isRunning ? runSpeed : walkSpeed);

        // Ubah input 2D menjadi arah 3D (Forward & Right)
        Vector3 move = (transform.forward * inputMove.y) + (transform.right * inputMove.x);
        
        float verticalTemp = moveDirection.y; // Simpan gravitasi saat ini
        moveDirection = move * currentSpeed;
        moveDirection.y = verticalTemp; // Kembalikan gravitasi agar tidak hilang
    }

    private void ApplyJump()
    {
        if (characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && moveDirection.y < 0)
        {
            moveDirection.y = -2f; // Pastikan karakter tetap menempel tanah
        }
        else
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
    }

    private void ApplyCrouch()
    {
        // Mengubah tinggi karakter secara instan
        if (isCrouching || isBlockedAbove)
        {
            characterController.height = crouchHeight;
        }
        else
        {
            characterController.height = defaultHeight;
        }
    }

    public void OnFlashlight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ToggleFlashlight();
        }
    }

    private void ToggleFlashlight()
    {
        if (flashlightObject != null)
        {
            isFlashlightOn = !isFlashlightOn;
            flashlightObject.SetActive(isFlashlightOn);
            Debug.Log("Senter: " + (isFlashlightOn ? "Menyala" : "Mati"));
        }
    }   
    private void HandleNoiseEmission()
    {
        Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0, characterController.velocity.z);
        float currentMoveSpeed = horizontalVelocity.magnitude;

        if (currentMoveSpeed > 0.1f)
        {
            // Tentukan radius berdasarkan status gerak
            float multiplier = 1f;
            bool actuallyRunning = isRunning && currentStamina > 0 && !isExhausted;
            if (isCrouching) multiplier = crouchNoiseMultiplier;
            else if (isRunning) multiplier = sprintNoiseMultiplier;

            float finalRadius = baseNoiseRadius * multiplier;

            // Panggil fungsi kirim sinyal suara
            EmitNoise(finalRadius);
        }
    }

    private void EmitNoise(float radius)
    {
        // Mencari Enemy di sekitar menggunakan OverlapSphere
        Collider[] enemies = Physics.OverlapSphere(transform.position, radius, enemyLayer);
        
        foreach (var enemyCollider in enemies)
        {
            // Mencoba mengambil script EnemyAI dari objek yang terkena radius
            if (enemyCollider.TryGetComponent(out EnemyAI enemyScript))
            {
                enemyScript.OnHeardNoise(transform.position);
            }
        }
    }

    // Untuk membantu visualisasi radius suara di Editor (Garis Kuning)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        float previewRadius = isCrouching ? baseNoiseRadius * crouchNoiseMultiplier : 
                             (isRunning ? baseNoiseRadius * sprintNoiseMultiplier : baseNoiseRadius);
        Gizmos.DrawWireSphere(transform.position, previewRadius);
    }
}