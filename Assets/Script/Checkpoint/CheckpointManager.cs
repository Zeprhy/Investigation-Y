using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }
    [SerializeField] private GameObject player;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        if (PlayerPrefs.GetInt("HasSave", 0) == 1)
        {
            LoadCheckpoint();
            Debug.Log("Sistem: Melanjutkan dari Checkpoint terakhir.");
        }
    }

    public void SetNewCheckpoint(Vector3 pos)
    {
        PlayerPrefs.SetFloat("CP_X", pos.x);
        PlayerPrefs.SetFloat("CP_Y", pos.y);
        PlayerPrefs.SetFloat("CP_Z", pos.z);
        PlayerPrefs.SetInt("HasSave", 1);
        PlayerPrefs.Save();
    }

    public void LoadCheckpoint()
    {
        if (PlayerPrefs.GetInt("HasSave", 0) == 0) return;

        Vector3 savedPos = new Vector3(
            PlayerPrefs.GetFloat("CP_X"),
            PlayerPrefs.GetFloat("CP_Y"),
            PlayerPrefs.GetFloat("CP_Z")
        );

        if (player == null) player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // PENTING: Matikan CharacterController agar teleportasi tidak ditarik kembali ke posisi lama oleh physics
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = savedPos;

            if (cc != null) cc.enabled = true;
        }
    }
}