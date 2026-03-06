
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class MovementPlayer : MonoBehaviour
{
    [Header("Kamera & Look")]
    public Camera playerCamera;
    public float lookSpeed = 0.1f;
    public float lookXLimit = 45f;

    [Header("Kecepatan Jalan")]
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float crouchSpeed = 3f;

    [Header("Fisika")]
    public float jumpPower = 7f;
    public float gravity = 20f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;

    [Header("Stealth & Hide")]
    public LayerMask obstacleMask;
    public float checkDistance = 1.0f;
    
    [Header("Flashlight Settings")]
    public GameObject flashlightObject;

    [Header("Noise System")]
    public float baseNoiseRadius = 5f;
    public float sprintNoiseMultiplier = 2f;
    public float crouchNoiseMultiplier = 0.5f;
    public LayerMask enemyLayer;

    // Komponen & Data Input internal
    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private Vector2 inputMove;
    private Vector2 inputLook;
    private float rotationX = 0;
    
    // Status Karakter
    private bool isRunning;
    private bool isCrouching;
    private bool isBlockedAbove;
    private bool isFlashlightOn = false;
    public bool IsHidden { get; private set; }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        // Kunci kursor agar tidak keluar layar
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

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
        checkObstacleAbove();
        ApplyRotation();
        ApplyMovement();
        ApplyGravity();
        ApplyCrouch();
        HandleNoiseEmission();

        // Eksekusi pergerakan akhir
        characterController.Move(moveDirection * Time.deltaTime);
    }

    
    private void ApplyRotation()
    {
        float sensitivityMultiplier = 0.1f;
        
        // Putar Kamera (Atas - Bawah)
        rotationX -= inputLook.y * lookSpeed * sensitivityMultiplier;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        // Putar Badan (Kiri - Kanan)
        transform.Rotate(Vector3.up * inputLook.x * lookSpeed * sensitivityMultiplier);
    }

    private void ApplyMovement()
    {
        // Tentukan kecepatan berdasarkan status
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
    private void checkObstacleAbove()
    {
        isBlockedAbove = Physics.Raycast(transform.position, Vector3.up, checkDistance, obstacleMask);
        Debug.DrawRay(transform.position, Vector3.up * checkDistance, isBlockedAbove ? Color.red : Color.green);
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