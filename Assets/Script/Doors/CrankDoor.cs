using UnityEngine;

public class CrankDoor : BaseDoor
{
    [Header("== Crank Settings ==")]
    [SerializeField] private CrankMinigame crankMinigame;

    private bool _minigameInProgress = false;
    private Vector3 _lastInteractorPosition;

    protected override void Initialize()
    {
        base.Initialize();

        if (crankMinigame != null)
        {
            crankMinigame.onCrankComplete.AddListener(OnMinigameSuccess);
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

        if (_playerInteraction != null && _playerInteraction.IsHoldingCrankHandle())
        {
            if (crankMinigame != null)
            {
                _lastInteractorPosition = player.transform.position;
                _minigameInProgress = true;
                crankMinigame.StartMinigame();
                MinigameStateManager.Instance?.EnterMinigame(MinigameStateManager.MinigameType.Crank);
                UpdateUIText();
            }
        }
    }

    private void OnMinigameSuccess()
    {
        _minigameInProgress = false;
        UnlockDoor();
        ToggleDoor(_lastInteractorPosition);

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
            bool hasItem = _playerInteraction != null && _playerInteraction.IsHoldingCrankHandle();
            hintText = hasItem ? "[Locked] Press [F] To Crank" : "[Locked] Need CrankHandle";
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