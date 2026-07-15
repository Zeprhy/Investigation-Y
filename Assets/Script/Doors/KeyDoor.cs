using UnityEngine;

public class KeyDoor : BaseDoor
{
    [Header("== Pengaturan Kunci ==")]
    [SerializeField] private KeyDataSO requiredKey;

    public override void Interact(GameObject player)
    {
        float distSqr = (transform.position - player.transform.position).sqrMagnitude;
        if (distSqr > _interactRadiusSqr) return;

        if (!isLocked)
        {
            ToggleDoor(player.transform.position);
            return;
        }

        if (_playerInteraction != null && requiredKey != null)
        {
            if (_playerInteraction.IsHoldingKey(requiredKey.keyID))
            {
                UnlockDoor();
                ToggleDoor(player.transform.position);
            }
        }
    }
    protected override void UpdateUIText()
    {
        if (!_isPlayerNear) return;

        string hintText = "";

        if (isLocked)
        {
            string kName = requiredKey != null ? requiredKey.keyName.ToUpper() : "KEY";
            
            bool hasKey = _playerInteraction != null && requiredKey != null && _playerInteraction.IsHoldingKey(requiredKey.keyID);
            hintText = hasKey ? $"[Locked] Press [F] To Use {kName}" : $"[Locked] Need {kName}";
        }
        else
        {
            hintText = isOpen ? "Press [F] To Close The Door" : "Press [F] To Open The Door";
        }

        if (GameManager.Instance.interactionUIManager != null)
        {
            GameManager.Instance.interactionUIManager.ShowText(hintText);
        }
        Debug.Log($"UI Pintu Update: {hintText}");
    }
    protected override void HideUIText()
    {
        if(GameManager.Instance.interactionUIManager != null)
        {
            GameManager.Instance.interactionUIManager.HideText();
            Debug.Log("UI Pintu Disembunyikan");
        }
    }
}