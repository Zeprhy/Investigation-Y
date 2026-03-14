using System.Collections;
using UnityEngine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance;

    [Header("Shake Settings")]
    [SerializeField] float lightDuration   = 0.3f;
    [SerializeField] float lightMagnitude  = 0.05f;
    [SerializeField] float heavyDuration   = 0.6f;
    [SerializeField] float heavyMagnitude  = 0.2f;

    [SerializeField] Camera targetCamera;
    Vector3 originalPos;
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

        originalPos = transform.localPosition;
    }

    public void ShakeHeavy()
{
    TriggerShake(heavyDuration, heavyMagnitude);
}

public void ShakeLight()
{
    TriggerShake(lightDuration, lightMagnitude);
}

    void TriggerShake(float duration, float magnitude)
    {
        // Kalau ada shake yang sedang jalan, stop dulu
        if (currentShake != null) StopCoroutine(currentShake);
        currentShake = StartCoroutine(Shake(duration, magnitude));
    }

    IEnumerator Shake(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Respect pause — sama seperti MovementPlayer kamu
            if (PauseMenu.isPausedStatic)
            {
                yield return null;
                continue;
            }

            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            targetCamera.transform.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        targetCamera.transform.localPosition = originalPos;
        currentShake = null;
    }
}