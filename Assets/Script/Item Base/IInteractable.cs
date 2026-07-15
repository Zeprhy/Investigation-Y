public interface IInteractable
{
    bool CanInteract(ItemType itemType, string keyID = "");
    void Interact(ItemType itemType);
}