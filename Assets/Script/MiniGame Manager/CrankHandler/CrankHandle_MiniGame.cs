using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class CrankHandle_MiniGame : MonoBehaviour
{
    [Header(" Pengaturan ")]
    [Tooltip("Seberapa cepat progress naik saat mouse diputar searah jarum jam")]
    [SerializeField] private float fillSpeed = 0.4f;

    [Header (" Audio ")]
    [SerializeField] private AudioClip completeSound;
    [SerializeField] private AudioClip crankingSound;
 
    [Tooltip("Seberapa cepat progress mundur saat tidak diputar")]
    [SerializeField] private float drainSpeed = 0.15f;
 
    [Tooltip("Berapa lama grace period sebelum progress mulai mundur (detik)")]
    [SerializeField] private float graceTime = 1.5f;
 
    [Tooltip("Sensitivitas deteksi gerakan mouse searah jarum jam")]
    [SerializeField] private float rotationSensitivity = 0.5f;
 
    [Header("UI Referencess")]
    [Tooltip("Panel UI minigame")]
    [SerializeField] private GameObject crankPanel;
 
    [Header("Events")]
    public UnityEvent onCrankComplete;   // Dipanggil saat progress penuh
    public UnityEvent onCrankCancelled;  // Dipanggil saat player berhenti

    // [Header("Settings Camera")]
    // [SerializeField] private bool useCameraLock = true;
    // [SerializeField] private MonoBehaviour[] scriptsToDisable;
    // [SerializeField] private bool unlockCursorForUI = false;
 
    // ---- State ----
    private float _progress = 0f;
    private bool _isActive = false;
    private bool _isHolding = false;
    private float _graceTimer = 0f;
    private bool _isComplete = false;
    private float _smoothedInput = 0f;

    private WaitForSeconds _completeWait;
    private AudioSource _audioSource;

    public float Progress => _progress;
    public bool IsActive => _isActive;

    void Awake()
    {
        _completeWait = new WaitForSeconds(0.5f);
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (!_isActive || _isComplete) return;

        _isHolding = Input.GetMouseButton(0);

        if (_isHolding)
        {
            _graceTimer = graceTime;

           float mouseX = Input.GetAxis("Mouse X");
           float mouseY = Input.GetAxis("Mouse Y");
           float rawInput = (mouseX - mouseY) * rotationSensitivity;

           _smoothedInput = Mathf.Lerp(_smoothedInput, rawInput, 0.3f);
           if (_smoothedInput > 0.01f)
            {
                _progress += _smoothedInput * fillSpeed * Time.deltaTime;
                _progress = Mathf.Clamp01(_progress);

                PlayCrankSound();

                if (_progress >= 1f)
                {
                    StartCoroutine(CompleteMinigame());
                    return;
                }
            }
            else
            {
               StopCrankSound(); 
            }
        }
        else
        {
            StopCrankSound();
            if (_graceTimer > 0f)
            {
                _graceTimer -= Time.deltaTime;
            }
            else
            {
                _progress -= drainSpeed * Time.deltaTime;
                _progress = Mathf.Clamp01(_progress);
            }
        }
    }

    public void StartMinigame()
    {
        _progress = 0f;
        _isActive = true;
        _isComplete = false;
        _graceTimer = graceTime;
        _smoothedInput = 0f;

        if (crankPanel != null)
            crankPanel.SetActive(true);

        // if (useCameraLock && scriptsToDisable != null)
        // {
        //     foreach (MonoBehaviour script in scriptsToDisable)
        //     {
        //         if (script != null) script.enabled = false;
        //     }

        //     if (unlockCursorForUI)
        //     {
        //         Cursor.lockState = CursorLockMode.None;
        //         Cursor.visible = true;
        //     }
        // }
    }

    public void StopMinigame()
    {
        _isActive = false;
        _progress = 0f;
        StopCrankSound();

        if (crankPanel != null)
            crankPanel.SetActive(false);

        // if (useCameraLock && scriptsToDisable != null)
        // {
        //     foreach (MonoBehaviour script in scriptsToDisable)
        //     {
        //         if (script != null) script.enabled = true;
        //     }

        //     if (unlockCursorForUI)
        //     {
        //         Cursor.lockState = CursorLockMode.Locked;
        //         Cursor.visible = false;
        //     }
        // }
    }

    private IEnumerator CompleteMinigame()
    {
        _isComplete = true;
        _isActive = false;
        _progress = 1f;
        

        StopCrankSound();
        GameManager.Instance.audioManager.PlaySFX(completeSound);

        yield return _completeWait;
        StopMinigame();
        onCrankComplete?.Invoke();
    }

    private bool _isCrankPlaying = false;

    private void PlayCrankSound()
    {
        if (crankingSound == null || _isCrankPlaying) return;
 
        if (GameManager.Instance.audioManager != null)
            GameManager.Instance.audioManager.PlaySFX(crankingSound);
 
        _isCrankPlaying = true;
    }
 
    private void StopCrankSound()
    {
        _isCrankPlaying = false;
    }
 
    
}