using UnityEngine;
using UnityEngine.Events;
using System.Collections;
 
/// <summary>
/// CrankMinigame — Sistem engkol putar.
/// Player hold klik kiri + gerak mouse searah jarum jam untuk isi progress.
/// Kalau dilepas terlalu lama, progress mundur perlahan.
/// </summary>
public class CrankMinigame : MonoBehaviour
{
    [Header("== Pengaturan ==")]
    [Tooltip("Seberapa cepat progress naik saat mouse diputar searah jarum jam")]
    [SerializeField] private float fillSpeed = 0.4f;
 
    [Tooltip("Seberapa cepat progress mundur saat tidak diputar")]
    [SerializeField] private float drainSpeed = 0.15f;
 
    [Tooltip("Berapa lama grace period sebelum progress mulai mundur (detik)")]
    [SerializeField] private float graceTime = 1.5f;
 
    [Tooltip("Sensitivitas deteksi gerakan mouse searah jarum jam")]
    [SerializeField] private float rotationSensitivity = 0.5f;
 
    [Header("== UI ==")]
    [Tooltip("Panel UI minigame")]
    [SerializeField] private GameObject crankPanel;
 
    [Header("== Audio ==")]
    [SerializeField] private AudioClip crankingSound;
    [SerializeField] private AudioClip completeSound;
 
    [Header("== Events ==")]
    public UnityEvent onCrankComplete;   // Dipanggil saat progress penuh
    public UnityEvent onCrankCancelled;  // Dipanggil saat player berhenti
 
    // ---- State ----
    private float _progress = 0f;        // 0 sampai 1
    private bool _isActive = false;
    private bool _isHolding = false;
    private float _graceTimer = 0f;
    private bool _isComplete = false;
 
    // ---- Cache ----
    private WaitForSeconds _completeWait;
    private AudioSource _audioSource;
 
    // ---- Property untuk UI baca progress ----
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

            Vector2 mouseDelta = new Vector2(
                Input.GetAxis("Mouse X"),
                Input.GetAxis("Mouse Y")
            );

            //Rumus: clockWise = mouseX - MouseY
            // (gerak kanan = CW, gerak atas = CCW, gerak bawah = CW, gerak kiri = CCW)
            float clockwiseInput = (mouseDelta.x - mouseDelta.y) * rotationSensitivity;

            if (clockwiseInput > 0)
            {
                //progress naik
                _progress += clockwiseInput * fillSpeed * Time.deltaTime;
                _progress = Mathf.Clamp01(_progress);

                PlayCrankSound();

                if (_progress >= 1f)
                {
                    StartCoroutine(CompleteMinigame());
                    return;
                }
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

        if (crankPanel != null)
        crankPanel.SetActive(true);

        Debug.Log("[CrankGame] Dimulai!");
    }

    public void StopMinigame()
    {
         _isActive = false;
        _progress = 0f;
        StopCrankSound();

        if (crankPanel != null)
        crankPanel.SetActive(false);
    }

    private IEnumerator CompleteMinigame()
    {
        _isComplete = true;
        _isActive = false;
        _progress = 1f;
        

        StopCrankSound();
        PlaySFX(completeSound);

        Debug.Log("[CrankMinigame] selesai pintu terbuka!");

        yield return _completeWait;
        StopMinigame();
        onCrankComplete?.Invoke();
    }

    private bool _isCrankPlaying = false;

    private void PlayCrankSound()
    {
        if (crankingSound == null || _isCrankPlaying) return;
 
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(crankingSound);
 
        _isCrankPlaying = true;
    }
 
    private void StopCrankSound()
    {
        _isCrankPlaying = false;
    }
 
    private void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
 
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(clip);
        else
            AudioSource.PlayClipAtPoint(clip, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
    }
}
