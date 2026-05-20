using UnityEngine;

/// <summary>
/// Pasang di GameObject PC Security Room. Ensure memiliki Collider agar bisa dideteksi.
/// </summary>
public class SecurityComputer : MonoBehaviour
{
    [Header("== Settings ==")]
    [Tooltip("Ubah material layar PC saat lockdown aktif (opsional)")]
    [SerializeField] private MeshRenderer computerScreenRenderer;
    [SerializeField] private Material normalScreenMat;
    [SerializeField] private Material lockdownScreenMat;

    [Header("== Audio ==")]
    [SerializeField] private AudioClip typingAndClickSFX;

    private bool _hasBeenCleared = false;

    void Update()
    {
        // Logika visual layar PC (Opsional)
        if (LockdownManager.Instance != null && computerScreenRenderer != null)
        {
            if (LockdownManager.Instance.IsLockdownActive && !_hasBeenCleared)
            {
                computerScreenRenderer.material = lockdownScreenMat; // Layar merah/error
            }
            else
            {
                computerScreenRenderer.material = normalScreenMat; // Layar normal
            }
        }
    }

    /// <summary>
    /// Fungsi ini dipanggil saat Player berinteraksi/klik PC ini
    /// </summary>
    public void InteractWithComputer()
    {
        // 1. Cek apakah lockdown sedang aktif
        if (LockdownManager.Instance != null && LockdownManager.Instance.IsLockdownActive)
        {
            // Mainkan suara ketik/akses diijinkan
            if (typingAndClickSFX != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(typingAndClickSFX);
            }

            // 2. Matikan lockdown melalui manager
            LockdownManager.Instance.DeactivateLockdown();
            _hasBeenCleared = true;
            
            Debug.Log("[SecurityPC] Akses diterima. Sistem lockdown dimatikan.");
        }
        else
        {
            Debug.Log("[SecurityPC] Tidak ada ancaman aktif atau sistem offline.");
        }
    }
}