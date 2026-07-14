using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class MovementPlayer : MonoBehaviour
{
    [Header("Kamera & Look")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float lookSpeed = 0.1f;
    [SerializeField] private float lookXLimit = 45f;
    [SerializeField] private float savedFOV = 60f;

    [Header("Kecepatan Jalan")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float runSpeed = 12f;
    [SerializeField] private float crouchSpeed = 3f;

    [Header("Fisika")]
    [SerializeField] private float gravity = 20f;
    [SerializeField] private float defaultHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;


   /* [Header("Noise System")]
    [SerializeField] private float baseNoiseRadius = 5f;
    [SerializeField] private float sprintNoiseMultiplier = 2f;
    [SerializeField] private float crouchNoiseMultiplier = 0.5f;
    [SerializeField] private LayerMask enemyLayer;
    
    */

    [Header("Stamina System")]
    private PlayerStamina playerStamina;

    [Header("Health Integration")]
    [SerializeField] private HealthManager health;

    [Header("Optimization Settings")]
    [SerializeField] private float noiseUpdateFrequency = 0.2f;
    private float noiseTimer;
    private Collider[] enemyBuffer = new Collider[5]; 

    private Transform myTransform;
    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private Vector2 inputMove;
    private Vector2 inputLook;
    private float rotationX = 0;
    private bool isCursorLocked;

    private bool isRunning;
    private bool isCrouching;
    private bool isBlockedAbove;
    
    // public bool IsHidden { get; set; }
    private bool _isMinigameActive = false;
    public bool isDead = false;

    public Transform PlayerCamera => playerCamera;

    public void Initialize()
    {
        characterController = GetComponent<CharacterController>();
        savedFOV = PlayerPrefs.GetFloat("Settings_FOV", 60f);
        myTransform = transform;

        SetCursorState(false);
    }

    public void UpdateSavedFOV(float newFOV)
    {
        savedFOV = newFOV;
    }

    public void OnMove(InputAction.CallbackContext context) => inputMove = context.ReadValue<Vector2>();
    public void OnLook(InputAction.CallbackContext context) => inputLook = context.ReadValue<Vector2>();
    public void OnSprint(InputAction.CallbackContext context) => isRunning = context.performed;
    public void OnCrouch(InputAction.CallbackContext context) => isCrouching = context.performed;

    void Update()
    {
        if (PauseMenu.isPausedStatic) return;
        if (_isMinigameActive) return;

        ApplyRotation();

        bool isMovingState = characterController.velocity.magnitude > 0.1f;
        if (playerStamina != null)
        {
            playerStamina.HandleStamina(isRunning, isMovingState, isCrouching);
            playerStamina.UpdateStaminaUI();
        }
        

        ClimbingSystem climbing = GetComponent<ClimbingSystem>();
        if (climbing != null && climbing.IsClimbing) 
        {
            moveDirection = Vector3.zero;
            return;
        }

        if (!characterController.enabled) return;
        
        ApplyMovement();
        ApplyGravity();
        ApplyCrouch();

        float targetFOV = (isRunning && !playerStamina.isExhausted && inputMove.magnitude > 0.1f) ? savedFOV + 10f : savedFOV;
        Camera lens = playerCamera.GetComponentInChildren<Camera>();
        if (lens != null) {
            lens.fieldOfView = Mathf.Lerp(lens.fieldOfView, targetFOV, Time.deltaTime * 5f);
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void SetCursorState(bool locked)
    {
        isCursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = locked;
    }

    public void SetminigameState(bool active)
    {
        _isMinigameActive = active;
    }

    private void ApplyRotation()
    {
        if (_isMinigameActive || PauseMenu.isPausedStatic || isDead) return;

        // Modifikasi: Mouse hanya bisa digerakkan jika masih hidup
        float mouseInputX = isDead ? 0 : inputLook.x;
        float mouseInputY = isDead ? 0 : inputLook.y;

        if (!isCursorLocked)
        {
            float sensitivityMultiplier = 0.1f;

            // Gunakan mouseInputY (0 jika mati)
            rotationX -= mouseInputY * lookSpeed * sensitivityMultiplier;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

            Vector3 shakeOffset = Vector3.zero;
            if (CameraShakeManager.Instance != null)
                shakeOffset = CameraShakeManager.Instance.ShakeOffset; 

            // PENTING: Baris ini harus tetap jalan agar ShakeOffset bisa diterapkan!
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX + shakeOffset.x, shakeOffset.y, shakeOffset.z);

            // Gunakan mouseInputX (0 jika mati)
            transform.Rotate(Vector3.up * mouseInputX * lookSpeed * sensitivityMultiplier);
        }
    }

    public void ResetRotation(float targetYaw)
    {
        rotationX = 0f;
        transform.rotation = Quaternion.Euler(0, targetYaw, 0);

        if (playerCamera != null) {
            playerCamera.localRotation = Quaternion.identity;
        }
    }

    private void ApplyMovement()
    {

        Vector2 finalInput = characterController.isGrounded ? inputMove : Vector2.zero;

        bool canRun = isRunning && !playerStamina.isExhausted && !isCrouching;
        float currentSpeed = isCrouching ? crouchSpeed : (canRun ? runSpeed : walkSpeed);

        if (health != null)
        {
            if (health.currentHealth == 1)
            {
                currentSpeed *= 0.5f;
            }
            else if (health.currentHealth <= 0)
            {
                currentSpeed = 0;
            }
        }

        Vector3 move = (transform.forward * finalInput.y) + (transform.right * finalInput.x);
        
        float verticalTemp = moveDirection.y;

        if (characterController.isGrounded)
        {
            moveDirection = move * currentSpeed;
        }

        moveDirection.y = verticalTemp;
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && moveDirection.y < 0)
        {
            moveDirection.y = -2f;
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

    /*

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        float previewRadius = isCrouching ? baseNoiseRadius * crouchNoiseMultiplier : 
                             (isRunning ? baseNoiseRadius * sprintNoiseMultiplier : baseNoiseRadius);
        Gizmos.DrawWireSphere(transform.position, previewRadius);
    }
    
    */
}