using UnityEngine;

public class HideManager : MonoBehaviour
{
    private CharacterController characterController;

    public bool IsHidden { get; private set; }

    public void Initialize()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void SetHidden(bool hidden)
    {
        IsHidden = hidden;

        if (characterController != null)
            characterController.enabled = !hidden;
    }
}
