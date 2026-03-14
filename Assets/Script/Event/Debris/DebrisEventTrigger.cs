using System.Collections;
using UnityEngine;

public class DebrisEventTrigger : MonoBehaviour
{
    [Header("Debris")]
    [SerializeField] GameObject debrisPrefab;
    [SerializeField] Transform spawnPoint;

    [Header("Warning FX")]
    [SerializeField] AudioClip crackSFX;          // tetap ada, tapi diplay lewat AudioManager
    [SerializeField] ParticleSystem dustParticle;
    [SerializeField] float warningDuration = 2f;

    bool hasTriggered = false;

    // Tidak perlu AudioSource lagi di sini

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        hasTriggered = true;
        StartCoroutine(DebrisSequence());
    }

    IEnumerator DebrisSequence()
    {
        yield return new WaitUntil(() => !PauseMenu.isPausedStatic);

        // Pakai AudioManager kamu
        if (crackSFX != null)
            AudioManager.Instance.PlaySFX(crackSFX);

        if (dustParticle != null) dustParticle.Play();
        CameraShakeManager.Instance.ShakeLight();

        yield return new WaitForSeconds(warningDuration);

        Instantiate(debrisPrefab, spawnPoint.position, Quaternion.identity);
    }
}