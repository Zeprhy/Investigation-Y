using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class ValveInteraction : MonoBehaviour
{
    [Header("Valve Settings")]
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float fillRate = 0.3f;
    [SerializeField] private float drainRate = 0.1f;
    [SerializeField] private float gracePeriod = 1.5f;
    [SerializeField] private float interactionDistance = 3f;
    
    [Header("References")]
    [Tooltip("The physical valve transform to rotate")]
    [SerializeField] private Transform valveTransform;
    

    [Header("Events")]
    public UnityEvent onValveComplete;

    [Header (" Audio ")]
    [SerializeField] private AudioClip valveCompleteSound;
    [SerializeField] private AudioClip valveTurningSound;
    
    private float _progress = 0f;
    private bool _isInteracting = false;
    private bool _isComplete = false;
    private float _graceTimer = 0f;
    
    private Camera _mainCam;
    private Collider _valveCollider;
    private AudioSource _audioSource;
    private bool _isAudioPlaying = false;
    
    public float Progress => _progress;
    public bool IsComplete => _isComplete;

    void Awake() {
        _mainCam = Camera.main;
        _valveCollider = GetComponent<Collider>();

        if (valveTransform == null)
            valveTransform = transform;

        _audioSource = GetComponent<AudioSource>();
        if(_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (_isComplete) return;

        // 1. Deteksi Klik pada Valve menggunakan Titik Tengah Layar (Crosshair)
        if (Input.GetMouseButtonDown(0))
        {
            // Ray menembak tepat dari tengah layar (Viewport 0.5, 0.5)
            Ray ray = _mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
            {
                if (hit.collider == _valveCollider)
                {
                    StartInteracting();
                }
            }
        }
        
        // 2. Berhenti interaksi
        if (Input.GetMouseButtonUp(0) && _isInteracting)
        {
            StopInteracting();
        }
        
        if (_isInteracting)
        {
            HandleValveRotation();
        }
        else if (_progress > 0f)
        {
            HandleProggresDrain();
        }
    }

    private void StartInteracting()
    {
        _isInteracting = true;
        _graceTimer = gracePeriod;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void StopInteracting()
    {
        _isInteracting = false;
        StopValveAudio();

        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false; 
    }

    private void HandleValveRotation()
    {
        float mouseDeltaY = Input.GetAxis("Mouse Y") * 10f;
        float rotationInput = -mouseDeltaY * rotationSpeed;

        if (rotationInput > 0.01f)
        {
            valveTransform.Rotate(0, 0, rotationInput * Time.deltaTime * 60f, Space.Self);

            _progress += rotationInput * fillRate * Time.deltaTime;
            _progress = Mathf.Clamp01(_progress);

            _graceTimer = gracePeriod;
            PlayValveAudio();

            if (_progress >= 1 && !_isComplete)
            {
                StartCoroutine(CompleteValve());
            }
        }
        else
        {
            StopValveAudio();
        }
    }

    private void HandleProggresDrain()
    {
        StopValveAudio();
        if (_graceTimer > 0f) _graceTimer -= Time.deltaTime;
        else
        {
            _progress -= drainRate * Time.deltaTime;
            _progress = Mathf.Clamp01(_progress);
        }
    }

    private IEnumerator CompleteValve()
    {
        _isComplete = true;
        _isInteracting = false;
        _progress = 1f;
        StopValveAudio();
        GameManager.Instance.audioManager.PlaySFX(valveCompleteSound);
        yield return new WaitForSeconds(0.3f);
        onValveComplete?.Invoke();
    }

    private void PlayValveAudio()
    {
        if (valveTurningSound == null || _isAudioPlaying) return;
        if (GameManager.Instance.audioManager != null) GameManager.Instance.audioManager.PlaySFX(valveTurningSound);
        _isAudioPlaying = true;
    }

    private void StopValveAudio() => _isAudioPlaying = false; 
}