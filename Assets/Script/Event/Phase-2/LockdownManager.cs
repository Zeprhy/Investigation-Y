using UnityEngine;

public class LockdownManager : MonoBehaviour
{
    public static LockdownManager Instance;

    [Header("== Status System ==")]
    private bool _isLockdownActive = false;
    public bool IsLockdownActive => _isLockdownActive;

    [Header("== Elemen Lingkungan (Opsional) ==")]
    [Tooltip("Lampu utama gedung yang akan mati saat lockdown")]
    [SerializeField] private GameObject mainLightsParent;

    [Tooltip("Lampu darurat/merah yang aktif saat lockdown")]
    [SerializeField] private GameObject emergencyLightsParent;

    [Header("== Audio ==")]
    [SerializeField] private AudioClip lockdownAlarmSFX;
    [SerializeField] private AudioSource ambientAudioSource;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Pastikan di awal game kondisi lampu normal
        if (emergencyLightsParent != null) emergencyLightsParent.SetActive(false);
        if (mainLightsParent != null) mainLightsParent.SetActive(true);
    }

    /// <summary>
    /// Mengaktifkan sistem lockdown (Dipanggil otomatis saat bukti lantai 2 diambil)
    /// </summary>
    public void ActivateLockdown()
    {
        if (_isLockdownActive) return; // Mencegah aktif dua kali

        _isLockdownActive = true;
        Debug.Log("<color=red>[LOCKDOWN ACTIVATED]</color> Gedung terkunci, listrik padam!");

        // 1. Matikan lampu utama, nyalakan lampu darurat merah
        if (mainLightsParent != null) mainLightsParent.SetActive(false);
        if (emergencyLightsParent != null) emergencyLightsParent.SetActive(true);

        // 2. Mainkan sound effect alarm atau suara listrik mati
        if (lockdownAlarmSFX != null && ambientAudioSource != null)
        {
            ambientAudioSource.clip = lockdownAlarmSFX;
            ambientAudioSource.loop = true;
            ambientAudioSource.Play();
        }

        // 3. TODO: Tambahkan logika penguncian pintu keluar jika ada script Pintu
        // Contoh: DoorManager.Instance?.LockAllExits();
    }

    /// <summary>
    /// Mematikan sistem lockdown (Dipanggil dari PC Security Room)
    /// </summary>
    public void DeactivateLockdown()
    {
        if (!_isLockdownActive) return;

        _isLockdownActive = false;
        Debug.Log("<color=green>[LOCKDOWN DEACTIVATED]</color> Listrik kembali normal melalui PC Security.");

        // 1. Kembalikan lampu ke kondisi normal
        if (mainLightsParent != null) mainLightsParent.SetActive(true);
        if (emergencyLightsParent != null) emergencyLightsParent.SetActive(false);

        // 2. Matikan alarm
        if (ambientAudioSource != null)
        {
            ambientAudioSource.Stop();
        }

        // 3. TODO: Buka kembali pintu yang terkunci
        // Contoh: DoorManager.Instance?.UnlockAllExits();
    }
}