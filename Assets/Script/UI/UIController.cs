
using UnityEngine;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    [Header("PauseMenu Ui Reference")]
    public GameObject PauseMenuCanvas;
    public Animator subPanelAnimator;
    private bool isPaused = false;

    [Header("Player References")]
   
    public MonoBehaviour playerMovementScript; 
    public PlayerInput playerInput;
   public void PauseMenu(InputAction.CallbackContext context)
{
    if (context.started)
    {
        Debug.Log("Tombol Esc Ditekan! Status Paused: " + isPaused);
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            OpenPauseMenu();
        }
    }
}
    public void OpenPauseMenu()
    {
        isPaused = true;
        Time.timeScale = 0f;
        PauseMenuCanvas.SetActive(true);

        if (playerMovementScript != null)
        {

            playerMovementScript.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("UI");
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        subPanelAnimator.SetTrigger("HideAll");
        PauseMenuCanvas.SetActive(false);

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("Player");
        }
    }
}
