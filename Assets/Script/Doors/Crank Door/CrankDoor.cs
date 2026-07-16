using UnityEngine;

public class CrankDoor : BaseDoor
{
    [Header("== Crank Settings ==")]
    [SerializeField] private CrankMinigame crankMinigame;

    private bool _minigameInProgress = false;
    private Vector3 _lastInteractorPosition;
    private PlayerInteraction _playerInteraction;

    protected override void Awake()
    {
        base.Awake();

        if (crankMinigame != null)
        {
            crankMinigame.onCrankComplete.AddListener(OnMinigameSuccess);
        }
    }

    public override void Interact(GameObject player)
    {
        if (_minigameInProgress) return;

        if (!isLocked)
        {
            ToggleDoor(player.transform.position);
            return;
        }

        if (_playerInteraction != null && _playerInteraction.IsHoldingCrankHandle())
        {
            if (crankMinigame != null)
            {
                _lastInteractorPosition = player.transform.position;
                _minigameInProgress = true;
                crankMinigame.StartMinigame();
            }
        }
    }

    private void OnMinigameSuccess()
    {
        _minigameInProgress = false;
        UnlockDoor();
        ToggleDoor(_lastInteractorPosition);
    }
}