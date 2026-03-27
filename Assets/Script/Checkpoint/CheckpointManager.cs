using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO; // Penting untuk akses file
using System.Collections;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }
    
    private string filePath;
    private GameSaveData currentData = new GameSaveData();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Menentukan lokasi file (biasanya di AppData/LocalLow)
            filePath = Path.Combine(Application.persistentDataPath, "checkpoint.json");
        }
        else { Destroy(gameObject); }
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(DeferredLoad());
    }

    private IEnumerator DeferredLoad()
    {
        yield return null; // Tunggu 1 frame agar Player spawn
        LoadCheckpoint();
    }

    public void SetNewCheckpoint(Vector3 pos)
    {
        // 1. Masukkan data ke objek
        currentData.checkpointX = pos.x;
        currentData.checkpointY = pos.y;
        currentData.checkpointZ = pos.z;
        currentData.hasSaveData = true;

        // 2. Ubah objek menjadi teks JSON
        string json = JsonUtility.ToJson(currentData, true);

        // 3. Tulis ke file
        File.WriteAllText(filePath, json);
        
        Debug.Log("<color=green>Checkpoint Saved to JSON:</color> " + filePath);
    }

    public void LoadCheckpoint()
    {
        // Cek apakah filenya ada
        if (!File.Exists(filePath)) return;

        // 1. Baca teks dari file
        string json = File.ReadAllText(filePath);

        // 2. Ubah teks JSON kembali menjadi objek
        currentData = JsonUtility.FromJson<GameSaveData>(json);

        if (currentData.hasSaveData)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                CharacterController cc = player.GetComponent<CharacterController>();
                Vector3 targetPos = new Vector3(currentData.checkpointX, currentData.checkpointY, currentData.checkpointZ);
                
                if (cc != null) cc.enabled = false;
                player.transform.position = targetPos;
                if (cc != null) cc.enabled = true;
                
                Debug.Log("Player dipindahkan ke posisi JSON: " + targetPos);
            }
        }
    }

    // Fungsi tambahan untuk debugging (Bisa dipanggil via Button)
    public void DeleteSaveFile()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("File Save Dihapus.");
        }
    }
}