using UnityEngine;
using System.Collections;
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class ClimbingSystem : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float climbSpeed = 3.5f;
    [SerializeField] private float detectionRange = 0.6f;
    [SerializeField] private float cameraAlignSpeed = 12f;
    [SerializeField] public LayerMask climbableLayer;

    [Header("UI System (Single Object)")]
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private float interactionRange = 2.5f;
    [SerializeField] private string climbMessage = "Press [F] To Climb";

    [Header("Safety Offset")]
    [SerializeField] private float startOffset = 0.6f;
    [SerializeField] private float DownOffset = 1f;
    [SerializeField] private Vector3 rayOffset = new Vector3(0, 0.2f, 0);
    [SerializeField] private float topPushForce = 1.8f;

    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform playerCamera;

    [Header("Detection Settings")]
    [SerializeField] private float sphereRadius = 0.25f;
    
    private bool _isClimbing;
    public bool IsClimbing => _isClimbing; 

    private Vector3 _ladderNormal;
    private Coroutine _alignCoroutine;
    private float _verticalInput;

    private void Awake()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (playerCamera == null) playerCamera = Camera.main.transform;

        if (interactionText != null) interactionText.text = "";
    }

    public void ToggleClimb(Vector3 hitNormal, Vector3 hitPoint)
    {
        if (!_isClimbing) StartClimbing(hitNormal, hitPoint);
        else StopClimbing();
    }

    private void StartClimbing(Vector3 hitNormal, Vector3 hitPoint)
    {
        _isClimbing = true;

        if (interactionText != null) interactionText.text = "";

        // 1. Cek apakah kita mulai dari atas (Turun)
        bool startingFromTop = transform.position.y > hitPoint.y + 0.1f;

        Vector3 horizontalNormal = new Vector3(hitNormal.x, 0, hitNormal.z).normalized;

        // 2. LOGIKA FLIP UNTUK TURUN:
        if (startingFromTop)
        {
            // Jika turun, kita ingin player "menyeberang" ke sisi lain hitPoint.
            // Kita hitung arah dari player ke tangga, lalu gunakan itu sebagai normal.
            Vector3 dirToLadder = (hitPoint - transform.position);
            dirToLadder.y = 0;
            _ladderNormal = dirToLadder.normalized; 
        }
        else
        {
            // Jika manjat dari bawah, gunakan normal permukaan seperti biasa.
            if (horizontalNormal.sqrMagnitude < 0.1f)
            {
                horizontalNormal = (transform.position - hitPoint);
                horizontalNormal.y = 0;
                horizontalNormal.Normalize();
            }
            _ladderNormal = horizontalNormal;
        }

        // 3. POSISI SNAPPING
        // targetPos sekarang akan berada di SISI SEBERANG tangga jika startingFromTop = true
        Vector3 targetPos = hitPoint + (_ladderNormal * startOffset);

        if (startingFromTop)
        {
            // Turunkan player agar menggantung di bawah bibir lantai
            targetPos.y = hitPoint.y - DownOffset; 
        }
        else
        {
            targetPos.y = transform.position.y;
        }

        // Penting: Matikan controller sebentar agar bisa teleport tanpa hambatan collision lantai
        controller.enabled = false;
        transform.position = targetPos;
        controller.enabled = true;

        if (_alignCoroutine != null) StopCoroutine(_alignCoroutine);
        _alignCoroutine = StartCoroutine(AlignCameraRoutine());
    }

    public void StopClimbing()
    {
        if (!_isClimbing) return;
        _isClimbing = false;
        _verticalInput = 0;
        if (_alignCoroutine != null) StopCoroutine(_alignCoroutine);
    }

    private IEnumerator AlignCameraRoutine()
    {
        Quaternion targetRot = Quaternion.LookRotation(-_ladderNormal, Vector3.up);
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * cameraAlignSpeed;

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, t);

            playerCamera.localRotation = Quaternion.Slerp(playerCamera.localRotation, Quaternion.identity, t);
            
            yield return null;
        }

        transform.rotation = targetRot;
        playerCamera.localRotation = Quaternion.identity;
    }

    private void Update()
    {
        if (_isClimbing)
        {            
            _verticalInput = Input.GetAxisRaw("Vertical");
            HandleMovement();
        }
        else
        {
            UpdateInteractionUI();
        }
    }

    private void UpdateInteractionUI()
    {
        if (interactionText == null) return;

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, climbableLayer))
        {
            interactionText.text = climbMessage;
        }
        else
        {
            interactionText.text = "";
        }
    }

    private void HandleMovement()
    {
        Vector3 directionToLadder = -_ladderNormal;
        
        bool hasWall = Physics.SphereCast(transform.position + rayOffset, sphereRadius, directionToLadder, out RaycastHit hit, detectionRange, climbableLayer);

        if (hasWall)
        {
            Vector3 move = transform.up * _verticalInput;
            controller.Move(move * climbSpeed * Time.deltaTime);

            if (_verticalInput < -0.1f && (controller.isGrounded || CheckGroundBelow()))
            {
                StopClimbing();
            }
        }
        else
        {
            if (_verticalInput > 0.1f) StartCoroutine(FinishClimbRoutine());
            else StopClimbing();
        }
    }

    private bool CheckGroundBelow() => Physics.Raycast(transform.position, Vector3.down, 0.2f);

    private IEnumerator FinishClimbRoutine()
    {
        float t = 0;
        Vector3 pushDir = (-_ladderNormal * topPushForce) + (transform.up * 0.5f);
        
        while (t < 0.2f)
        {
            controller.Move(pushDir * Time.deltaTime * climbSpeed);
            t += Time.deltaTime;
            yield return null;
        }
        StopClimbing();
    }
}