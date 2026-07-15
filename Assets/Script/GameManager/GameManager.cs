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
    public EvidenceManager evidenceManager;
    public HideManager hideManager;
    public GameplayStateManager gameplayStateManager;

    [Header("Sevice")]
    public AudioManager audioManager;
    public CameraShakeManager cameraShakeManager;
    public DialogueManager dialogueManager;
    public CheckpointManager checkpointManager;
    public DataPersistenceManager dataPersistenceManager;
    public InteractionUIManager interactionUIManager;


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
        StartCoroutine(DeferredLoadCheckpoint());
    }

    private void UpdateStageData()
    {
        if (hasInitialized) return;
        hasInitialized = true;

        if (movementPlayer == null) movementPlayer = Object.FindAnyObjectByType<MovementPlayer>();
        if (healthManager == null) healthManager = Object.FindAnyObjectByType<HealthManager>();
        if (playerInteraction == null) playerInteraction = Object.FindAnyObjectByType<PlayerInteraction>();
        if (evidenceManager == null) evidenceManager = Object.FindAnyObjectByType<EvidenceManager>();
        if (audioManager == null) audioManager = Object.FindAnyObjectByType<AudioManager>();
        if (cameraShakeManager == null) cameraShakeManager = Object.FindAnyObjectByType<CameraShakeManager>();
        if (dialogueManager == null) dialogueManager = Object.FindAnyObjectByType<DialogueManager>();
        if (checkpointManager == null) checkpointManager = Object.FindAnyObjectByType<CheckpointManager>();
        if (dataPersistenceManager == null) dataPersistenceManager = Object.FindAnyObjectByType<DataPersistenceManager>();
        if (hideManager == null) hideManager = Object.FindAnyObjectByType<HideManager>();
        if (gameplayStateManager == null) gameplayStateManager = Object.FindAnyObjectByType<GameplayStateManager>();
        if (interactionUIManager == null) interactionUIManager = Object.FindAnyObjectByType<InteractionUIManager>();
        

        if (movementPlayer != null) movementPlayer.Initialize();
        if (healthManager != null) healthManager.Initialize();
        if (playerInteraction != null) playerInteraction.Initialize();
        if (evidenceManager != null) evidenceManager.Initialize();
        if (audioManager != null) audioManager.Initialize();
        if (cameraShakeManager != null) cameraShakeManager.Initialize();
        if (dialogueManager != null) dialogueManager.Initialize();
        if (checkpointManager != null) checkpointManager.Initialize();
        if (dataPersistenceManager != null) dataPersistenceManager.Initialize();
        if (hideManager != null) hideManager.Initialize();
        if (gameplayStateManager != null) gameplayStateManager .Initialize();
        if (interactionUIManager != null) interactionUIManager.Initialize();
        
    }

    private IEnumerator DeferredLoadCheckpoint()
    {
        yield return null;
        if (checkpointManager != null) checkpointManager.LoadCheckpoint();
    }

    public void RunCoroutine(IEnumerator coroutine) => StartCoroutine(coroutine);
}