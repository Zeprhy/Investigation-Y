using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour, IDataPersistence
{
    public static AudioManager Instance;

    [Header("Mixer & Sources")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("3D Audio Pool")]
    [SerializeField] private int poolSize = 10;
    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    private Transform poolParent;

    [Header("Player Audio Clips")]
    public AudioClip StepSound;

    [Header("Item Audio Clips")]
    public AudioClip PryingSound;
    public AudioClip lockpickSuccess; 
    public AudioClip lockpickFail;
    public AudioClip lockpickComplete;
   

    private float currentMasterVol = 1f;
    private float currentMusicVol = 1f;
    private float currentSFXVol = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        poolParent = new GameObject("AudioSourcePool").transform;
        poolParent.SetParent(transform);

        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = new GameObject($"PooledAudio_{i}");
            go.transform.SetParent(poolParent);
            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            go.SetActive(false);
            audioSourcePool.Enqueue(source);
        }
    }

    public void LoadData(GameData data)
    {
        // Sekarang variabel ini sudah dikenali
        currentMasterVol = data.audioSettings.masterVolume;
        currentMusicVol = data.audioSettings.musicVolume;
        currentSFXVol = data.audioSettings.sfxVolume;

        ApplyAllVolumes();
    }

    public void SaveData(ref GameData data)
    {
        data.audioSettings.masterVolume = currentMasterVol;
        data.audioSettings.musicVolume = currentMusicVol;
        data.audioSettings.sfxVolume = currentSFXVol;
    }

    private void ApplyAllVolumes()
    {
        SetMixerVolume("MasterVol", currentMasterVol);
        SetMixerVolume("MusicVol", currentMusicVol);
        SetMixerVolume("SFXVol", currentSFXVol);
    }

    private void SetMixerVolume(string paramName, float linearValue)
    {
        // Rumus konversi Linear ke Decibel
        float dB = Mathf.Log10(Mathf.Clamp(linearValue, 0.0001f, 1f)) * 20;
        mainMixer.SetFloat(paramName, dB);
    }

    public void UpdateMasterVolume(float value)
    {
        currentMasterVol = value;
        SetMixerVolume("MasterVol", value);
    }

    public void UpdateMusicVolume(float value)
    {
        currentMusicVol = value;
        SetMixerVolume("MusicVol", value);
    }

    public void UpdateSFXVolume(float value)
    {
        currentSFXVol = value;
        SetMixerVolume("SFXVol", value);
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource != null && clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlaySFX3D(AudioClip clip, Vector3 position, float minDist = 5f, float maxDist = 50f)
    {
        if (clip == null) return;

    AudioSource source;
    
    if (audioSourcePool.Count > 0)
    {
        source = audioSourcePool.Dequeue();
        source.gameObject.SetActive(true);
    }
    else
    {
        GameObject tempGO = new GameObject("OverflowAudio");
        source = tempGO.AddComponent<AudioSource>();
        StartCoroutine(DestroyAfterPlay(tempGO, clip.length));
        Debug.LogWarning("Audio pool exhausted! Consider increasing poolSize.");
    }

        source.transform.position = position;
        source.clip = clip;
        source.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = minDist;
        source.maxDistance = maxDist;
        source.dopplerLevel = 0.5f;
        
        source.Play();
        
        StartCoroutine(ReturnToPool(source, clip.length));
    }

    private IEnumerator ReturnToPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay + 0.1f);

        if (source != null)
        {
            source.Stop();
            source.gameObject.SetActive(false);
            audioSourcePool.Enqueue(source);
        }
    }

    private IEnumerator DestroyAfterPlay(GameObject go, float delay)
    {
        yield return new WaitForSeconds(delay + 0.1f);
        Destroy(go);
    }
}