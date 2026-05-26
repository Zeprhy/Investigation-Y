using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance;

    [Header("Shake Settings")]
    [SerializeField] private float lightDuration    = 0.3f;
    [SerializeField] private float lightMagnitude   = 0.5f;
    [SerializeField] private float heavyDuration    = 0.6f;
    [SerializeField] private float heavyMagnitude   = 1.5f;

    [Header("Impact Settings")]
    [SerializeField] private float impactDuration  = 0.8f;
    [SerializeField] private float impactMagnitude = 3f;

    [SerializeField] private Camera targetCamera;
    public Vector2 ShakeOffset { get; private set; }

    Coroutine currentShake;

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        targetCamera = Camera.main;
    }

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    public void ShakeLight() => TriggerShake(lightDuration, lightMagnitude);
    public void ShakeHeavy() => TriggerShake(heavyDuration, heavyMagnitude);
    public void ShakeImpact() => TriggerShake(impactDuration, impactMagnitude);

    public void ShakeCustom(float duration, float magnitude) 
        => TriggerShake(duration, magnitude);

    void TriggerShake(float duration, float magnitude)
    {
        if (currentShake != null) StopCoroutine(currentShake);
        currentShake = StartCoroutine(Shake(duration, magnitude));
    }

    IEnumerator Shake(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (PauseMenu.isPausedStatic)
            {
                ShakeOffset = Vector2.zero;
                yield return null;
                continue;
            }

            float strength = magnitude * (1f - elapsed / duration);

            // Hitung offset saja, tidak langsung ke kamera
            float x = Random.Range(-1f, 1f) * strength;
            float y = Random.Range(-1f, 1f) * strength;
            ShakeOffset = new Vector2(x, y);

            elapsed += Time.deltaTime;
            yield return null;
        }

        ShakeOffset = Vector2.zero;
        currentShake = null;
    }

}