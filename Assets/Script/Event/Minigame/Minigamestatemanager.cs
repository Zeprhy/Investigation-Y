using UnityEngine;
using UnityEngine.InputSystem;
public class MinigameStateManager : MonoBehaviour
{
    public static MinigameStateManager Instance;

    [Header("== Referensi ==")]
    [Tooltip("Script movement player")]
    [SerializeField] private MovementPlayer movementPlayer;

    [Tooltip("Script PlayerInteraction")]
    [SerializeField] private PlayerInteraction playerInteraction;

    [Header("== Referensi Minigame ==")]
    [Tooltip("Assign semua minigame yang ada di scene")]
    [SerializeField] private LockpickMinigame lockpickMinigame;
    [SerializeField] private CrankMinigame crankMinigame;

    // ---- State ----
    private bool _isInMinigame = false;
    public bool IsInMinigame => _isInMinigame;
    public enum MinigameType
    {
        Lockpick,
         Crank
    }
    public MinigameType _currentType;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void EnterMinigame(MinigameType type = MinigameType.Lockpick)
    {
        if (_isInMinigame) return;
        _isInMinigame = true;

        // Freeze movement dan camera
        if (movementPlayer != null)
            movementPlayer.SetminigameState(true);

        switch (type)
        {
            case MinigameType.Lockpick:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;

            case MinigameType.Crank:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;
        }
    }

    public void ExitMinigame()
    {
        if (!_isInMinigame) return;
        _isInMinigame = false;

        // Unfreeze movement dan camera
        if (movementPlayer != null)
            movementPlayer.SetminigameState(false);

        // Lock cursor kembali
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("[MinigameStateManager] Minigame selesai — player di-unfreeze");
    }

    public void ForceExitMinigame()
    {
        if (!_isInMinigame) return;

        // Hentikan lockpick jika sedang aktif
        if (lockpickMinigame != null && lockpickMinigame.IsActive)
        {
            lockpickMinigame.StopMinigame();
            lockpickMinigame.onMinigameFailed?.Invoke();
        }

        // Hentikan crank jika sedang aktif
        if (crankMinigame != null && crankMinigame.IsActive)
        {
            crankMinigame.StopMinigame();
            crankMinigame.onCrankCancelled?.Invoke();
        }

        ExitMinigame();
    }
}