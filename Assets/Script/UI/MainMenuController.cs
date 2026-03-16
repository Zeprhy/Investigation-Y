using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel; // Objek yang berisi tombol New Game, Load, dll
    public GameObject creditPanel;
    
    [Header("Buttons")]
    public Button continueButton;

    [Header("Scene Settings")]
    public string levelToLoad = "Gameplay";

    void Start()
    {
        if (PlayerPrefs.GetInt("HasSave", 0) == 0)
        {
            continueButton.interactable = false;
        }
        
        // Pastikan saat mulai, menu utama aktif dan credit mati
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (creditPanel != null) creditPanel.SetActive(false);
    }

    void Update()
    {
        // Jika panel credit sedang aktif dan pemain menekan klik kiri mouse (0)
        if (creditPanel != null && creditPanel.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
                CloseCredit();
            }
        }
    }

    // Fungsi baru untuk mempermudah perpindahan panel
    public void OpenCredit()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (creditPanel != null) creditPanel.SetActive(true);
    }

    public void CloseCredit()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (creditPanel != null) creditPanel.SetActive(false);
    }

    // Fungsi StartGame, ContinueGame, dan Exit tetap sama...
    public void StartGame()
    {
        PlayerPrefs.SetInt("HasSave", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene(levelToLoad);
    }

    public void ContinueGame()
    {
        SceneManager.LoadScene(levelToLoad);
    }

    public void ExitGame()
    {
        Debug.Log("Keluar dari Game...");
        Application.Quit();
    }
}