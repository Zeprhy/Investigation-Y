using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool isPausedStatic = false;

    [Header("UI Panels")]
    [SerializeField] private GameObject backgroundPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject blurOverlay;

    private bool isPaused = false;

    [Header("Manager")]
    [SerializeField] private SettingsManager settingsManager;

    void Start()
    {
        ResumeGame();
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
        isPausedStatic = true;
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
        isPausedStatic = false;
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
        settingsManager.OnSettingsPanelOpen();
    }

    public void ShowMainPauseMenu()
    {
        backgroundPanel.SetActive(true);
        settingsPanel.SetActive(false);
        if (settingsManager != null)
        {
          settingsManager.OnSettingsPanelClose();  
        }
    }

    public void BackToSetings()
    {
        settingsPanel.SetActive(false);
        backgroundPanel.SetActive(true);
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        PlayerPrefs.Save();
        isPausedStatic = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("MainMenu");
    }
}
