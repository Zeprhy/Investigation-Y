using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject creditPanel;
    
    [Header("Buttons")]
    public Button continueButton;

    [Header("Scene Settings")]
    public string levelToLoad = "Gameplay"; // Ganti dengan nama scene game kamu

    void Start()
    {
        // Cek apakah ada data save-an
        // Jika tidak ada, tombol Continue tidak bisa diklik (abu-abu)
        if (PlayerPrefs.GetInt("HasSave", 0) == 0)
        {
            continueButton.interactable = false;
        }
        
        if (creditPanel != null) creditPanel.SetActive(false);
    }

    public void StartGame()
    {
        // Start akan menghapus progress lama dan mulai dari awal
        PlayerPrefs.SetInt("HasSave", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene(levelToLoad);
    }

    public void ContinueGame()
    {
        // Langsung load level. CheckpointManager di scene gameplay 
        // akan otomatis memindahkan posisi player nantinya.
        SceneManager.LoadScene(levelToLoad);
    }

    public void ToggleCredit(bool status)
    {
        if (creditPanel != null) creditPanel.SetActive(status);
    }

    public void ExitGame()
    {
        Debug.Log("Keluar dari Game...");
        Application.Quit();
    }
}