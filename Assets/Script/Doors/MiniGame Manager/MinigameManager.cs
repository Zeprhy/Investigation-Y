using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    [Header("Game List")]
    [SerializeField] private CrankMinigame crankMinigame;
    [SerializeField] private LockpickMinigame lockpickMinigame;

    public void Initialize()
    {
        Destroy(gameObject);
    }

    public bool IsAnyMinigameActive()
    {
        bool isCrankActive = (crankMinigame != null && crankMinigame.IsActive);
        bool isLockpickActive = (lockpickMinigame != null && lockpickMinigame.IsActive);
        
        return isCrankActive || isLockpickActive;
    }

    public void StartCrankMinigame()
    {
        if (crankMinigame == null)
        {
            Debug.LogWarning("[MinigameManager] Crank Minigame belum dimasukkan ke referensi!");
            return;
        }

        if (!IsAnyMinigameActive())
        {
            crankMinigame.StartMinigame();
        }
    }

    public void StartLockpickMinigame()
    {
        if (lockpickMinigame == null)
        {
            Debug.LogWarning("[MinigameManager] Lockpick Minigame belum dimasukkan ke referensi!");
            return;
        }

        if (!IsAnyMinigameActive())
        {
            lockpickMinigame.StartMinigame();
        }
    }

    public void ForceStopAllMinigames()
    {
        if (crankMinigame != null && crankMinigame.IsActive)
        {
            crankMinigame.StopMinigame();
        }

        if (lockpickMinigame != null && lockpickMinigame.IsActive)
        {
            lockpickMinigame.StopMinigame();
        }
    }
}