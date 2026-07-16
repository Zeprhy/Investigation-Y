using UnityEngine;

public class KeyDoor : BaseDoor
{
    [Header("== Pengaturan Kunci ==")]
    [SerializeField] private KeyDataSO requiredKey;
    private PlayerInteraction _playerInteraction;

    public override void Interact(GameObject player)
    {
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
}