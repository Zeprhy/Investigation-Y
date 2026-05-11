using UnityEngine;
using System.Collections;

public class SteamTrapManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject blockObject;
    [SerializeField] private ParticleSystem steamParticle;
    [SerializeField] private AudioSource steamAudio;

    [Header ("visual settings")]
    [SerializeField] private bool triggerOnlyOnce = true;
    [SerializeField] private float fadeOutTime = 1.0f;

    private bool _hasTriggered = false;
    private bool _hasShake = false;

    void Start()
    {
        if (steamParticle != null) steamParticle.Stop();
        if (steamAudio != null) steamAudio.Stop();
        if (blockObject != null) blockObject.SetActive(false);
    }

    public void ActiveTrap()
    {
        if (_hasTriggered && triggerOnlyOnce) return;

        if (steamParticle != null) steamParticle.Play();
        if (blockObject != null) blockObject.SetActive(true);
        if (steamAudio != null)
        {
            steamAudio.volume = 1f;
            steamAudio.Play();
        }

        if (CameraShakeManager.Instance && !_hasShake)
        {
            CameraShakeManager.Instance.ShakeHeavy();
            _hasShake = true;
        }
    }

    public void DeactvateTrap()
    {
        if (steamParticle != null) steamParticle.Stop();
        if (steamAudio != null) StartCoroutine(FadeOutAudio());
    }

    private IEnumerator FadeOutAudio()
    {
        float startVolume = steamAudio.volume;
        float elapsed = 0;

        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            steamAudio.volume = Mathf.Lerp(startVolume, 0, elapsed / fadeOutTime);
            yield return null;
        }

        steamAudio.Stop();
    }
}
