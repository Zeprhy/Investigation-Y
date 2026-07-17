using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    [Header("== Logic References ==")]
    public CrankHandle_MiniGame crankMinigame;
    public LockPick_MiniGame lockpickMinigame;

    [Header("== UI References ==")]
    public CrankHandle_MiniGame_UI crankUI;
    public LockPick_MiniGame_UI lockpickUI;

    public void Initialize()
    {
        if (crankUI != null && crankMinigame != null)
        {
            crankUI.Setup(crankMinigame); 
        }

        if (lockpickUI != null && lockpickMinigame != null)
        {
            lockpickUI.Setup(lockpickMinigame);
        }

        Debug.Log("[MinigameManager] Berhasil diinisialisasi. Logic dan UI telah terhubung.");
    }

    public bool IsAnyMinigameActive()
    {
        bool isCrankActive = (crankMinigame != null && crankMinigame.IsActive);
        bool isLockpickActive = (lockpickMinigame != null && lockpickMinigame.IsActive);
        
        return isCrankActive || isLockpickActive;
    }

    public void StartCrankMinigame()
    {
        if (crankMinigame == null) return;
        if (!IsAnyMinigameActive()) crankMinigame.StartMinigame();
    }

    public void StartLockpickMinigame()
    {
        if (lockpickMinigame == null) return;
        if (!IsAnyMinigameActive()) lockpickMinigame.StartMinigame();
    }

    public void ForceStopAllMinigames()
    {
        if (crankMinigame != null && crankMinigame.IsActive) crankMinigame.StopMinigame();
        if (lockpickMinigame != null && lockpickMinigame.IsActive) lockpickMinigame.StopMinigame();
    }
}