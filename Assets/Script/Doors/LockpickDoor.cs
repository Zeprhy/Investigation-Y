using UnityEngine;

public class LockpickDoor : BaseDoor
{
    [Header("== Lockpick Settings ==")]
    [SerializeField] private LockpickMinigame lockpickMinigame;

    private bool _minigameInProgress = false;
    private Vector3 _lastInteractorPosition;

    protected override void Initialize()
    {
        base.Initialize(); 

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
        if (distSqr > _interactRadiusSqr) return;

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
                MinigameStateManager.Instance?.EnterMinigame(MinigameStateManager.MinigameType.Lockpick);
                UpdateUIText();
            }
        }
    }

    private void OnMinigameSuccess()
    {
        _minigameInProgress = false;
        UnlockDoor();
        ToggleDoor(_lastInteractorPosition);

        _playerInteraction?.ConsumeLockPick();
        MinigameStateManager.Instance?.ExitMinigame();
        UpdateUIText();
    }

    private void OnMinigameFailed()
    {
        _minigameInProgress = false;
        MinigameStateManager.Instance?.ExitMinigame();
        UpdateUIText();
    }

    protected override void UpdateUIText()
    {
        if (!_isPlayerNear || GameManager.Instance.interactionUIManager == null) return;

        if (_minigameInProgress)
        {
            GameManager.Instance.interactionUIManager.HideText();
            return;
        }

        string hintText = "";
        if (isLocked)
        {
            bool hasItem = _playerInteraction != null && _playerInteraction.IsHoldingLockPick();
            hintText = hasItem ? "[Locked] Press [F] To Lockpick" : "[Locked] Need Lockpick";
        }
        else
        {
            hintText = isOpen ? "Press [F] To Close" : "Press [F] To Open";
        }

        GameManager.Instance.interactionUIManager.ShowText(hintText);
    }

    protected override void HideUIText()
    {
        if (GameManager.Instance.interactionUIManager != null)
        {
            GameManager.Instance.interactionUIManager.HideText();
        }
    }
}