using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{[Header("UI Panels")]
    public GameObject backgroundPanel; // Folder Background yang berisi tombol-tombol utama
    public GameObject settingsPanel;
    public GameObject confirmationPanel; // PanelPemilihanY/N
    public GameObject blurOverlay;

    private bool isPaused = false;

    void Start()
    {
        ResumeGame();
    }
    void Update()
    {
       
    }

    public void OnPauseMenu(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
        }
        
    }
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; 
        blurOverlay.SetActive(true);

        gameObject.SetActive(true); 
        ShowMainPauseMenu();     
 
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;      
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        blurOverlay.SetActive(false);

        gameObject.SetActive(false);
     
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;  
    }

   
    public void OpenSettings()
    {
        backgroundPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void ShowMainPauseMenu()
    {
        backgroundPanel.SetActive(true);
        settingsPanel.SetActive(false);
        confirmationPanel.SetActive(false);
    }

    public void ConfirmationMenu()
    {
        backgroundPanel.SetActive(false);
        confirmationPanel.SetActive(true); 
    }

    public void CancelConfirmationMenu()
    {
        backgroundPanel.SetActive(true);
        confirmationPanel.SetActive(false); 
    }

    public void BackToSetings()
    {
        settingsPanel.SetActive(false);
        backgroundPanel.SetActive(true);
    }
}
