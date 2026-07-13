using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Core System")]
    public MovementPlayer movementPlayer;
    public HealthManager healthManager;
    public PlayerInteraction playerInteraction;
    // public EvidenceInspector
    // public MinigameStateManager

    // [Header("Sevice")]
    // public AudioManager
    // public CameraShakeManager
    // public DialogueManager
    // public CheckpointManager
    // public DataPersistenceManager


    private bool hasInitialized = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void Start()
    {
        if (hasInitialized) return;
        UpdateStageData();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        hasInitialized = false;
        UpdateStageData();
    }

    private void UpdateStageData()
    {
        if (hasInitialized) return;
        hasInitialized = true;

        if (movementPlayer == null) movementPlayer = Object.FindAnyObjectByType<MovementPlayer>();
        if (healthManager == null) healthManager = Object.FindAnyObjectByType<HealthManager>();
        if (playerInteraction == null) playerInteraction = Object.FindAnyObjectByType<PlayerInteraction>();

        if (movementPlayer != null) movementPlayer.Initialize();
        if (healthManager != null) healthManager.Initialize();
        if (playerInteraction != null) playerInteraction.Initialize();
    }

    public void RunCoroutine(IEnumerator coroutine) => StartCoroutine(coroutine);
}