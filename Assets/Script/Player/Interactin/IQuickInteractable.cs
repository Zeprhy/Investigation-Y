public interface IQuickInteractable
{
    string InteractPrompt { get; }
    void OnPlayerInteract(PlayerInteraction interactor);
}