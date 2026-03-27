using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathSceneManager : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
    
        // Cari semua CanvasGroup yang mungkin terbawa dari scene sebelumnya dan matikan blokirnya
        CanvasGroup[] allGroups = FindObjectsByType<CanvasGroup>(FindObjectsSortMode.None);
        foreach (var group in allGroups)
        {
            // Jika grup ini bukan bagian dari UI DeathScene, matikan raycast-nya
            if (group.gameObject.scene.name != "DeathScene")
            {
                group.blocksRaycasts = false;
                group.interactable = false;
            }
        }
    }

    public void TryAgain()
    {
        // Ganti "SampleScene" dengan nama scene gameplay utamamu
        Debug.Log("Tombol Try Again dipencet!");
        Time.timeScale = 1f;
        SceneManager.LoadScene("Gameplay");
        
        // Kenapa ini bekerja? 
        // Karena CheckpointManager punya fungsi OnSceneLoaded 
        // yang otomatis memanggil LoadCheckpoint() dari JSON setiap kali scene ganti!
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}