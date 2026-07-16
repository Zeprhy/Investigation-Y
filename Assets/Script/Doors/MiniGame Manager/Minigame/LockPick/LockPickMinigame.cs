using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class LockpickMinigame : MonoBehaviour
{
    [Header(" Pengaturan Minigame ")]
    [SerializeField] private float needleSpeed = 180f;

    [Header("Pengaturan Kamera")]
    [SerializeField] private bool useCameraLock = true;
    [SerializeField] private MonoBehaviour[] scriptsToDisable;
    [SerializeField] private bool unlockCursorForUI = true;
 
    [Range(10f, 60f)]
    [SerializeField] private float successZoneSize = 30f;
 
    [SerializeField] private int requiredSuccesses = 3;
    [SerializeField] private int maxFailures = 2;
 
    [Header(" Visual ")]
    [SerializeField] private Transform needle;
    [SerializeField] private RectTransform successZoneRect;
    [SerializeField] private GameObject minigamePanel;

    // --- BAGIAN AUDIO LOKAL DIHAPUS ---

    [Header(" Events ")]
    public UnityEvent onMinigameSuccess;
    public UnityEvent onMinigameFailed;
    public UnityEvent<int, int> onProgress;
 
    private float _currentAngle = 0f;
    private float _successZoneStartAngle = 0f;
    private float _baseNeedleSpeed; 
    private bool _isActive = false;
    public bool IsActive => _isActive;
    private int _currentSuccesses = 0;
    private int _currentFailures = 0;
    private bool _inputConsumed = false;
 
    private bool _hasNeedle;
    private bool _hasZoneRect;
 
    private WaitForSeconds _endWait;
    private WaitForSeconds _failPauseWait;

    void Awake()
    {
        _baseNeedleSpeed = needleSpeed;
        _hasNeedle = needle != null;
        _hasZoneRect = successZoneRect != null;
        _endWait = new WaitForSeconds(0.5f);
        _failPauseWait = new WaitForSeconds(0.25f);
    }

    void Update()
    {   
        if (!_isActive) return;
        _currentAngle += needleSpeed * Time.deltaTime;
        
        if (_currentAngle >= 360f) _currentAngle -= 360f;

        if (_hasNeedle)
        {
            needle.localRotation = Quaternion.AngleAxis(- _currentAngle,Vector3.forward);
        }

        if (Input.GetKeyUp(KeyCode.F))
            _inputConsumed = false;
 
        if (Input.GetKeyDown(KeyCode.F) && !_inputConsumed)
        {
            _inputConsumed = true;
            CheckInput();
        }
    }

    public void StartMinigame()
    {
        _currentSuccesses = 0;
        _currentFailures  = 0;
        _isActive         = true;
        _inputConsumed    = false;
        _currentAngle     = 0f;
        needleSpeed       = _baseNeedleSpeed;
 
        PlaceSuccessZone();
 
        if (minigamePanel != null)
            minigamePanel.SetActive(true);


        if (useCameraLock)
        {
            if (useCameraLock)
            {
                foreach (MonoBehaviour script in scriptsToDisable)
                {
                    if (script != null) script.enabled = false;
                }
            }

            if (unlockCursorForUI)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = false;
            }
        }
    }

    public void StopMinigame()
    {
        _isActive = false;
        if (minigamePanel != null)
        {
            minigamePanel.SetActive(false);
        }

        if (useCameraLock)
        {
            foreach (MonoBehaviour script in scriptsToDisable)
            {
                if (script != null) script.enabled = true;
            }

            if (unlockCursorForUI)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    private void CheckInput()
    {
        float end = _successZoneStartAngle + successZoneSize;
        bool inZone;

        if (end > 360f)
        {
            inZone = _currentAngle >= _successZoneStartAngle || _currentAngle <= (end - 360f);
        }
        else
        {
            inZone = _currentAngle >= _successZoneStartAngle && _currentAngle <= end;
        }

        if (inZone)
        {
            HandleSuccess();
        }
        else
        {
            HandleFail();
        }
    }

    private void HandleSuccess()
    {
        _currentSuccesses++;
        
        // if (AudioManager.Instance != null)
            // AudioManager.Instance.PlaySFX(AudioManager.Instance.lockpickSuccess);
 
        onProgress?.Invoke(_currentSuccesses, requiredSuccesses);
 
        if (_currentSuccesses >= requiredSuccesses)
        {
            StartCoroutine(CompleteMinigame());
        }
        else
        {
            PlaceSuccessZone();
            needleSpeed += 15f;
        }
    }
 
    private void HandleFail()
    {
        _currentFailures++;
        
        // if (AudioManager.Instance != null)
            // AudioManager.Instance.PlaySFX(AudioManager.Instance.lockpickFail);
 
        if (_currentFailures >= maxFailures)
        {
            StartCoroutine(FailMinigame());
        }
        else
        {
            StartCoroutine(FailPause());
            PlaceSuccessZone();
        }
    }

    private void PlaceSuccessZone()
    {
        _successZoneStartAngle = Random.Range(0f, 360f - successZoneSize);

        if (_hasZoneRect)
        {
            successZoneRect.localRotation = Quaternion.AngleAxis(- _successZoneStartAngle, Vector3.forward);
        }
    }

    private IEnumerator CompleteMinigame()
    {
        _isActive = false;

        // if (AudioManager.Instance != null)
            // AudioManager.Instance.PlaySFX(AudioManager.Instance.lockpickComplete);

        yield return _endWait;

        StopMinigame();
        onMinigameSuccess?.Invoke();
    }

    private IEnumerator FailMinigame()
    {
        _isActive = false;
        yield return _endWait;
        StopMinigame();
        onMinigameFailed?.Invoke();
    }

    private IEnumerator FailPause()
    {
        float saved = needleSpeed;
        needleSpeed = 0f;
        yield return _failPauseWait;
        needleSpeed = saved;
    }
}