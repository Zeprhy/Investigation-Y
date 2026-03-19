using UnityEngine;

public class ClimbingSystem : MonoBehaviour
{
    [Header("Settings")]
    public float climbSpeed = 3f;
    public LayerMask climbableLayer;
    public float detectionRange = 0.7f;

    [Header("References")]
    public CharacterController controller;
    
    // Variabel private yang aman
    private bool _isClimbing;

    // Property untuk dibaca oleh MovementPlayer
    public bool IsClimbing => _isClimbing; 

    // FUNGSI START (Cukup satu saja)
    void StartClimbing()
    {
        _isClimbing = true;
        // Kamu bisa tambahkan efek suara atau reset gravity di sini jika perlu
    }

    // FUNGSI STOP (Cukup satu saja)
    void StopClimbing()
    {
        _isClimbing = false;
    }

    void Update()
    {
        CheckForWall();

        if (_isClimbing)
        {
            HandleClimbingMovement();
        }
    }

    void CheckForWall()
    {
        RaycastHit hit;
        // Deteksi dinding
        Vector3 rayOrigin = transform.position + (Vector3.down * 0.7f); // Menembak dari area pinggang
        bool hitWall = Physics.Raycast(rayOrigin, transform.forward, out hit, detectionRange, climbableLayer);

        if (hitWall)
        {
            if (Input.GetKey(KeyCode.W) && !_isClimbing)
            {
                StartClimbing();
            }
        }
        else if (_isClimbing)
        {
            // JIKA TIDAK KENA DINDING TAPI MASIH MANJAT (Artinya sudah sampai ujung atas)
            if (Input.GetKey(KeyCode.W))
            {
                // Berikan dorongan kecil ke depan agar kaki menapak di lantai atas
                Vector3 pushForward = transform.forward * 2f + transform.up * 1f;
                controller.Move(pushForward * Time.deltaTime * climbSpeed);
            }

            StopClimbing();
        }
    }

    void HandleClimbingMovement()
    {
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector3 climbDirection = transform.up * verticalInput;

        controller.Move(climbDirection * climbSpeed * Time.deltaTime);

        // Berhenti jika menyentuh tanah saat turun
        if (controller.isGrounded && verticalInput < 0)
        {
            StopClimbing();
        }
    }
}