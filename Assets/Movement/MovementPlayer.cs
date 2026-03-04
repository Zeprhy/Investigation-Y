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

    // Komponen & Data Input internal
    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private Vector2 inputMove;
    private Vector2 inputLook;
    private float rotationX = 0;
    
    // Status Karakter
    private bool isRunning;
    private bool isCrouching;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        // Kunci kursor agar tidak keluar layar
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // BAGIAN INPUT (Dihubungkan ke Player Input Component)
    public void OnMove(InputAction.CallbackContext context) => inputMove = context.ReadValue<Vector2>();
    public void OnLook(InputAction.CallbackContext context) => inputLook = context.ReadValue<Vector2>();
    public void OnJump(InputAction.CallbackContext context) { if (context.performed) ApplyJump(); }
    public void OnSprint(InputAction.CallbackContext context) => isRunning = context.performed;
    public void OnCrouch(InputAction.CallbackContext context) => isCrouching = context.performed;

    void Update()
    {
        ApplyRotation();
        ApplyMovement();
        ApplyGravity();
        ApplyCrouch();

        // Eksekusi pergerakan akhir
        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void ApplyRotation()
    {
        // Putar Kamera (Atas - Bawah)
        rotationX -= inputLook.y * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        // Putar Badan (Kiri - Kanan)
        transform.Rotate(Vector3.up * inputLook.x * lookSpeed);
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
        if (isCrouching)
        {
            characterController.height = crouchHeight;
        }
        else
        {
            characterController.height = defaultHeight;
        }
    }
}