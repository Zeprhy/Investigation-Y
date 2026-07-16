using UnityEngine;

public class LockpickDoor : BaseDoor
{
    [Header("== Lockpick Settings ==")]
    [SerializeField] private LockpickMinigame lockpickMinigame;

    private bool _minigameInProgress = false;
    private Vector3 _lastInteractorPosition;
    private PlayerInteraction _playerInteraction;

    protected override void Awake()
    {
        base.Awake(); 

        // Hubungkan event minigame
        if (lockpickMinigame != null)
        {
            lockpickMinigame.onMinigameSuccess.AddListener(OnMinigameSuccess);
            lockpickMinigame.onMinigameFailed.AddListener(OnMinigameFailed);
        }
    }

    public override void Interact(GameObject player)
    {
        if (_minigameInProgress) return;    

        float distSqr = (transform.position - player.transform.position).sqrMagnitude;

        if (!isLocked)
        {
            ToggleDoor(player.transform.position);
            return;
        }

        if (_playerInteraction != null && _playerInteraction.IsHoldingLockPick())
        {
            if (lockpickMinigame != null)
            {
                _lastInteractorPosition = player.transform.position;
                _minigameInProgress = true;
                lockpickMinigame.StartMinigame();
            }
        }
    }

    private void OnMinigameSuccess()
    {
        _minigameInProgress = false;
        UnlockDoor();
        ToggleDoor(_lastInteractorPosition);

        _playerInteraction?.ConsumeLockPick();
    }

    private void OnMinigameFailed()
    {
        _minigameInProgress = false;
    }
}