
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

    [Header("Inventory System")]
    public float interactDistance = 3f;
    public Transform dropPoint;
    public float throwForce = 5f;
    private GameObject heldItemPrefab;
    private bool isHoldingItem = false;

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
        checkObstacleAbove();
        ApplyRotation();
        ApplyMovement();
        ApplyGravity();
        ApplyCrouch();

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

    public void OnPickup(InputAction.CallbackContext context)
    {
        Debug.Log("Pickup Ditekan");
        if (context.performed && !isHoldingItem)
        {
            PickupItem();
        }
    }

    public void OnDrop(InputAction.CallbackContext context)
    {
        if (context.performed && isHoldingItem)
        {
            DropItem();
        }
    }

    private void PickupItem()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.SphereCast(ray, 0.2f, out hit, interactDistance))
    {
        // Cek apakah objek yang terkena laser punya script "Item"
        Item item = hit.collider.GetComponentInParent<Item>();
        
        if (item != null)
        {
            Debug.Log("Mengambil: " + item.itemName);
            
            // Simpan prefabnya dan tandai sedang membawa barang
            heldItemPrefab = item.itemPrefab;
            isHoldingItem = true;

            // Hapus barang dari dunia
            Destroy(hit.collider.gameObject);
        }
        }

    }

    private void DropItem()
    {
    // Munculkan barang kembali di depan Player
    GameObject droppedObj = Instantiate(heldItemPrefab, dropPoint.position, dropPoint.rotation);
    
    // Beri efek lemparan ke depan
    Rigidbody rb = droppedObj.GetComponent<Rigidbody>();
    if (rb != null)
    {
        rb.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);
    }

    // Kosongkan inventory
    heldItemPrefab = null;
    isHoldingItem = false;
    Debug.Log("Barang dibuang");
    }   
}