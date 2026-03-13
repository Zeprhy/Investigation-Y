using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

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
    [SerializeField] private float jumpCooldown = 0.1f; 
    [SerializeField] private float jumpForwardForce = 2f;

    [Header("Stealth & Hide")]
    [SerializeField] private LayerMask obstacleMask;
    
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

    [Header("Health Integration")]
    [SerializeField] private HealthManager health;

    [Header("Optimization Settings")]
    [SerializeField] private float noiseUpdateFrequency = 0.2f;
    private float noiseTimer;
    private Collider[] enemyBuffer = new Collider[5]; 

    private Transform myTransform;
    private float lastStaminaPercent = -1f;
    private float regenDelayTimer;
    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private Vector2 inputMove;
    private Vector2 inputLook;
    private float rotationX = 0;
    private bool isCursorLocked;
    private float lastJumpTime;

    private bool isRunning;
    private bool isCrouching;
    private bool isBlockedAbove;
    private bool isFlashlightOn = false;
    private bool isExhausted = false;
    public bool IsHidden { get; set; }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        myTransform = transform;
        currentStamina = maxStamina;

        SetCursorState(false);

        if (flashlightObject != null)
        {
            flashlightObject.SetActive(false);
        }
    }

    public void OnMove(InputAction.CallbackContext context) => inputMove = context.ReadValue<Vector2>();
    public void OnLook(InputAction.CallbackContext context) => inputLook = context.ReadValue<Vector2>();
    public void OnJump(InputAction.CallbackContext context) { if (context.performed) ApplyJump(); }
    public void OnSprint(InputAction.CallbackContext context) => isRunning = context.performed;
    public void OnCrouch(InputAction.CallbackContext context) => isCrouching = context.performed;

    void Update()
    {
        if (PauseMenu.isPausedStatic) return;
        if (!characterController.enabled) return;
        
        ApplyRotation();
        HandleStamina();
        UpdateStaminaUI();
        ApplyMovement();
        ApplyGravity();
        ApplyCrouch();

        noiseTimer += Time.deltaTime;
        if (noiseTimer >= noiseUpdateFrequency)
        {
            HandleNoiseEmission();
            noiseTimer = 0;    
        }

        float targetFOV = (isRunning && !isExhausted && inputMove.magnitude > 0.1f) ? 70f : 60f;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * 5f);

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void SetCursorState(bool locked)
    {
        isCursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = locked;
    }

    public void SetHiddenStatus(bool status)
    {
        IsHidden = status;
        if (characterController != null) 
            characterController.enabled = !status;
    }

    private void HandleStamina()
    {
        bool isMoving = inputMove.magnitude > 0.1f;

        if (isRunning && isMoving && !isExhausted && !isCrouching)
        {
            currentStamina -= staminaDrain * Time.deltaTime;
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isExhausted = true;

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
            float currentPercent = currentStamina / maxStamina;

            if (Mathf.Abs(lastStaminaPercent - currentPercent) > 0.001f)
            {
                staminaBarFill.fillAmount = currentPercent;
                lastStaminaPercent = currentPercent;
            }
        }
    }

    private void ApplyRotation()
    {
        if (health != null && health.currentHealth <= 0 || PauseMenu.isPausedStatic) return;
         
        if (!isCursorLocked)
        {

        float sensitivityMultiplier = 0.1f;
        
        rotationX -= inputLook.y * lookSpeed * sensitivityMultiplier;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        transform.Rotate(Vector3.up * inputLook.x * lookSpeed * sensitivityMultiplier);
        }
    }

    private void ApplyMovement()
    {
        if (IsHidden) 
        {
            moveDirection = Vector3.zero;
            return;
        }

        Vector2 finalInput = characterController.isGrounded ? inputMove : Vector2.zero;

        bool canRun = isRunning && !isExhausted && !isCrouching;
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

    private void ApplyJump()
    {
        // Cek apakah di tanah, sedang tidak cooldown, dan tidak lelah
        if (characterController.isGrounded && Time.time >= lastJumpTime + jumpCooldown && !isExhausted)
        {
            moveDirection.y = jumpPower;
            currentStamina -= 10f; // Kurangi 10 stamina setiap lompat
            regenDelayTimer = staminaRegenDelay; // Reset delay regenerasi

            // Berikan dorongan ke depan saat melompat
            Vector3 forwardJump = transform.forward * jumpForwardForce;
            moveDirection.x = forwardJump.x;
            moveDirection.z = forwardJump.z;

            lastJumpTime = Time.time; // Catat waktu lompat
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
        }
    }   
    private void HandleNoiseEmission()
    {
        if (characterController.velocity.sqrMagnitude > 0.01f)
        {
            float multiplier = isCrouching ? crouchNoiseMultiplier : (isRunning ? sprintNoiseMultiplier : 1f);
            EmitNoise(baseNoiseRadius * multiplier);
        }
    }

    private void EmitNoise(float radius)
    {
        int numEnemies = Physics.OverlapSphereNonAlloc(myTransform.position, radius, enemyBuffer, enemyLayer);
        
        for (int i = 0; i < numEnemies; i++)
        {
            if (enemyBuffer[i].TryGetComponent(out EnemyAI enemyScript))
            {
                enemyScript.OnHeardNoise(myTransform.position);
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