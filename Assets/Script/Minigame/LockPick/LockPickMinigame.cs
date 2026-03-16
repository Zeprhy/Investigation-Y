using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class LockpickMinigame : MonoBehaviour
{
    [Header(" Pengaturan Minigame ")]
    [SerializeField] private float needleSpeed = 180f;
 
    [Range(10f, 60f)]
    [SerializeField] private float successZoneSize = 30f;
 
    [SerializeField] private int requiredSuccesses = 3;
    [SerializeField] private int maxFailures = 2;
 
    [Header(" Visual ")]
    [SerializeField] private Transform needle;
    [SerializeField] private RectTransform successZoneRect;
    [SerializeField] private GameObject minigamePanel;
 
    [Header(" Audio (via AudioManager) ")]
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip failSound;
    [SerializeField] private AudioClip completeSound;
 
    [Header(" Events ")]
    public UnityEvent onMinigameSuccess;
    public UnityEvent onMinigameFailed;
    public UnityEvent<int, int> onProgress;
 
    // ---- State ----
    private float _currentAngle = 0f;
    private float _successZoneStartAngle = 0f;
    private float _baseNeedleSpeed;
    private bool _isActive = false;
    private int _currentSuccesses = 0;
    private int _currentFailures = 0;
    private bool _inputConsumed = false;
 
    // Cache null-check needle — cek sekali di Awake
    private bool _hasNeedle;
    private bool _hasZoneRect;
 
    // Cache WaitForSeconds — hindari GC alloc tiap coroutine dipanggil
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

        // Pakai cached bool — tidak null-check tiap frame
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
    }

    public void StopMinigame()
    {
        _isActive = false;
        if (minigamePanel != null)
        {
            minigamePanel.SetActive(false);
        }
    }


    // Logic
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
        PlaySFX(successSound);
 
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
        PlaySFX(failSound);
 
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
        PlaySFX(completeSound);

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

    private void PlaySFX(AudioClip clip)
    {
        if ( clip == null) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip);
        }
        else
        {
           AudioSource.PlayClipAtPoint(clip, Camera.main != null ? Camera.main.transform.position : Vector3.zero); 
        }
    }
}
