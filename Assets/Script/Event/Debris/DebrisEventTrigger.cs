using System.Collections;
using UnityEngine;

public class DebrisEventTrigger : MonoBehaviour, IDataPersistence
{
    [Header("Debris")]
    [SerializeField] private GameObject debrisPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Warning FX")]
    [SerializeField] private AudioClip crackSFX;
    [SerializeField] private AudioClip rumbleSFX;       // suara gemuruh memanjang
    [SerializeField] private AudioClip impactSFX;       // suara BOOM saat jatuh
    [SerializeField] private ParticleSystem dustParticle;
    [SerializeField] private ParticleSystem impactDustParticle; // particle lebih besar saat impact
    [SerializeField] private Light warningLight;         // optional: lampu yang kedip-kedip

    [Header("Timing")]
    [SerializeField] private float warningDuration = 3f;

    [Header("Save")]
    [SerializeField] private string id;

    [Header("Event State")]
    public bool isEventReady = false;

    private bool hasTriggered = false;

    public void LoadData(GameData data)
    {
        DebrisData dData = data.debrisListData.Find(x => x.id == id);
        if (dData != null) hasTriggered = dData.hasTriggered;

        if (hasTriggered)
        {
            Instantiate(debrisPrefab, spawnPoint.position, Quaternion.identity);
            gameObject.SetActive(false);
        }
    }

    public void SaveData(ref GameData data)
    {
        DebrisData dData = data.debrisListData.Find(x => x.id == id);
        if (dData != null)
            dData.hasTriggered = hasTriggered;
        else
            data.debrisListData.Add(new DebrisData(id, hasTriggered));
    }

    public void ActivateEventReady()
    {
        isEventReady = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isEventReady || hasTriggered || !other.CompareTag("Player")) return;

        hasTriggered = true;
        StartCoroutine(DebrisSequence());
    }

    IEnumerator DebrisSequence()
    {
        yield return new WaitUntil(() => !PauseMenu.isPausedStatic);

        // === FASE 1: WARNING (crack kecil + shake ringan berulang) ===
        if (crackSFX != null) GameManager.Instance.audioManager.PlaySFX(crackSFX);
        if (dustParticle != null) dustParticle.Play();
        if (warningLight != null) StartCoroutine(FlickerLight(warningDuration));

        // Shake ringan berulang selama warning phase
        StartCoroutine(RepeatingShake(warningDuration, 0.4f, 0.15f));

        yield return new WaitForSeconds(warningDuration * 0.6f);

        // === FASE 2: CLIMAX (intensitas naik, gemuruh makin keras) ===
        if (rumbleSFX != null) GameManager.Instance.audioManager.PlaySFX(rumbleSFX);
        CameraShakeManager.Instance.ShakeHeavy();

        yield return new WaitForSeconds(warningDuration * 0.4f);

        // === FASE 3: IMPACT (debris jatuh) ===
        GameObject debris = Instantiate(debrisPrefab, spawnPoint.position, Quaternion.identity);

        if (impactSFX != null) GameManager.Instance.audioManager.PlaySFX(impactSFX);
        if (impactDustParticle != null) impactDustParticle.Play();

        // Shake paling keras saat impact
        CameraShakeManager.Instance.ShakeImpact();

        // Spawn debris dengan sedikit random rotation biar natural
        debris.transform.rotation = Quaternion.Euler(
            Random.Range(-5f, 5f),
            Random.Range(0f, 360f),
            Random.Range(-5f, 5f)
        );

        gameObject.SetActive(false);
    }

    // Shake kecil berulang untuk efek "bangunan mau runtuh"
    IEnumerator RepeatingShake(float duration, float interval, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            yield return new WaitUntil(() => !PauseMenu.isPausedStatic);
            CameraShakeManager.Instance.ShakeCustom(0.2f, magnitude);
            elapsed += interval;
            yield return new WaitForSeconds(interval);
        }
    }

    // Lampu kedip-kedip saat warning
    IEnumerator FlickerLight(float duration)
    {
        if (warningLight == null) yield break;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            warningLight.enabled = !warningLight.enabled;
            float wait = Random.Range(0.05f, 0.2f);
            elapsed += wait;
            yield return new WaitForSeconds(wait);
        }
        warningLight.enabled = false;
    }
}