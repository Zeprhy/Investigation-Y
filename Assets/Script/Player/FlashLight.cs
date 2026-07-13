using UnityEngine;
using UnityEngine.InputSystem;

public class FlashLight : MonoBehaviour
{
    [Header("Flashlight Settings")]
    [SerializeField] private GameObject flashlightObject;

    private bool isFlashlightOn = false;

    void Start()
    {
        if (flashlightObject != null)
        {
            flashlightObject.SetActive(false);
        }
    }

    public void OnFlashlight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ToggleFlashlight();
        }
    }

    private void ToggleFlashlight()
    {
        if (flashlightObject != null)
        {
            isFlashlightOn = !isFlashlightOn;
            flashlightObject.SetActive(isFlashlightOn);
        }
    }  
}
