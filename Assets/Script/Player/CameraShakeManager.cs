using System.Collections;
using UnityEngine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance;

    [Header("Shake Settings")]
    [SerializeField] float lightDuration    = 0.3f;
    [SerializeField] float lightMagnitude   = 0.5f;
    [SerializeField] float heavyDuration    = 0.6f;
    [SerializeField] float heavyMagnitude   = 1.5f;

    [SerializeField] Camera targetCamera;

    // Offset yang akan ditambahkan ke rotasi MovementPlayer
    public Vector2 ShakeOffset { get; private set; }

    Coroutine currentShake;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    public void ShakeLight() => TriggerShake(lightDuration, lightMagnitude);
    public void ShakeHeavy() => TriggerShake(heavyDuration, heavyMagnitude);

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