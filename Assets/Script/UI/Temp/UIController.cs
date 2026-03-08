
using UnityEngine;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    [Header("PauseMenu Ui Reference")]
    public GameObject PauseMenuCanvas;
    public Animator subPanelAnimator;
    public MenuController menuScript;
    private bool isPaused = false;

    [Header("Player References")]
   
    public MonoBehaviour playerMovementScript; 
    public PlayerInput playerInput;
  public void PauseMenu(InputAction.CallbackContext context)
{
    if (context.started)
    {
        Debug.Log("ESC DItekan");
        
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

        if (subPanelAnimator != null)
        {
            subPanelAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            subPanelAnimator.Play("SubHide", 0, 0f);
        }
    }

    public void ResumeGame()
    {
        
        isPaused = false;
        Time.timeScale = 1f;

         if(menuScript != null)
        {
            menuScript.ResetMenuState();
        }

        if (subPanelAnimator != null)
        {
            subPanelAnimator.SetTrigger("HideAll");
        }
        
        PauseMenuCanvas.SetActive(false);

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }

        if(menuScript != null)
        {
            menuScript.ClickResume();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
