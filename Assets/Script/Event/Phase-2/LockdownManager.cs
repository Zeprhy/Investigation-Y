using UnityEngine;
using System.Collections;

public class LockdownManager : MonoBehaviour
{
    public static LockdownManager Instance;

    [Header("== Status System ==")]
    private bool _isLockdownActive = false;
    public bool IsLockdownActive => _isLockdownActive;

    [Header("== Pengaturan Lampu ==")]
    [SerializeField] private Light[] mainLights;

    [Header("== Pengaturan Efek Flicker (Kedipan) ==")]
    [SerializeField] private bool useFlicker = true;
    [SerializeField] private int flickerCount = 5;
    [SerializeField] private float flickerSpeed = 0.06f;

    [Header("== Audio ==")]
    [SerializeField] private AudioClip lockdownAlarmSFX;
    [SerializeField] private AudioSource ambientAudioSource;

    private Coroutine _flickerCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Kondisi awal game: Lampu utama hidup, lampu darurat mati tanpa flicker
        SetLightsState(mainLights, true);
    }

    /// <summary>
    /// Mengaktifkan sistem lockdown (Dipanggil otomatis saat bukti lantai 2 diambil)
    /// </summary>
    public void ActivateLockdown()
    {
        if (_isLockdownActive) return; // Mencegah aktif dua kali

        _isLockdownActive = true;
        Debug.Log("<color=red>[LOCKDOWN ACTIVATED]</color> Gedung terkunci, listrik padam!");

        // Amankan coroutine jika ada yang sedang berjalan
        if (_flickerCoroutine != null) StopCoroutine(_flickerCoroutine);

        if (useFlicker)
        {
            // Menjalankan efek kedipan saat lampu utama padam dan lampu darurat merah mulai menyala
            _flickerCoroutine = StartCoroutine(LockdownFlickerEffect(true));
        }
        else
        {
            // Jika tidak pakai flicker, langsung ganti status lampu secara instan
            SetLightsState(mainLights, false);
        }

        // Mainkan sound effect alarm atau suara listrik mati
        if (lockdownAlarmSFX != null && ambientAudioSource != null)
        {
            ambientAudioSource.clip = lockdownAlarmSFX;
            ambientAudioSource.loop = true;
            ambientAudioSource.Play();
        }
    }

    /// <summary>
    /// Mematikan sistem lockdown (Dipanggil dari PC Security Room)
    /// </summary>
    public void DeactivateLockdown()
    {
        if (!_isLockdownActive) return;

        _isLockdownActive = false;
        Debug.Log("<color=green>[LOCKDOWN DEACTIVATED]</color> Listrik kembali normal melalui PC Security.");

        if (_flickerCoroutine != null) StopCoroutine(_flickerCoroutine);

        if (useFlicker)
        {
            // Menjalankan efek kedipan saat listrik kembali dinyalakan (lampu utama hidup, darurat mati)
            _flickerCoroutine = StartCoroutine(LockdownFlickerEffect(false));
        }
        else
        {
            SetLightsState(mainLights, true);
        }

        // Matikan alarm
        if (ambientAudioSource != null)
        {
            ambientAudioSource.Stop();
        }
    }

    /// <summary>
    /// Fungsi pembantu untuk mematikan atau menghidupkan sekelompok lampu (Array)
    /// </summary>
    private void SetLightsState(Light[] lightGroup, bool state)
    {
        if (lightGroup == null) return;

        foreach (Light light in lightGroup)
        {
            if (light != null)
            {
                light.enabled = state;
            }
        }
    }

    /// <summary>
    /// Coroutine untuk memberikan efek ketegangan kedipan lampu saat perubahan status lockdown
    /// </summary>
    IEnumerator LockdownFlickerEffect(bool activatingLockdown)
    {
        for (int i = 0; i < flickerCount; i++)
        {
            if (activatingLockdown)
            {
                // Saat lockdown aktif: Lampu utama berkedip mati, lampu darurat berkedip menyala
                SetLightsState(mainLights, false);
                yield return new WaitForSeconds(flickerSpeed);
                
                SetLightsState(mainLights, true);
                yield return new WaitForSeconds(flickerSpeed);
            }
            else
            {
                // Saat lockdown mati (Normal kembali): Lampu utama berkedip menyala, lampu darurat berkedip mati
                SetLightsState(mainLights, true);
                yield return new WaitForSeconds(flickerSpeed);

                SetLightsState(mainLights, false);
                yield return new WaitForSeconds(flickerSpeed);
            }
        }

        // State Akhir setelah selesai berkedip
        if (activatingLockdown)
        {
            SetLightsState(mainLights, false);
        }
        else
        {
            SetLightsState(mainLights, true);
        }
    }
}