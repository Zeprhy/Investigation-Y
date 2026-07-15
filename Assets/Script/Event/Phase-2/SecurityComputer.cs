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

    [Header("== elevator active ==")]
    [SerializeField] private ElevatorButton elevatorButton;
    [SerializeField] private TeleportLift teleportLift;

    [Header("== Enemy Chasing Event ==")]
    [SerializeField] private GameObject chaseTriggerZone;

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
            if (typingAndClickSFX != null && GameManager.Instance.audioManager != null)
            {
                GameManager.Instance.audioManager.PlaySFX(typingAndClickSFX);
            }

            // 2. Matikan lockdown melalui manager
            LockdownManager.Instance.DeactivateLockdown();
            _hasBeenCleared = true;

            if (chaseTriggerZone != null)
            {
                chaseTriggerZone.SetActive(true);
            }

            if (elevatorButton != null && teleportLift != null)
            {
                elevatorButton.CanPressed = true;
                teleportLift.IsEnabeled = true;
            }
        }

    }
}