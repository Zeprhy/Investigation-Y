using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour, IDataPersistence
{
    public static AudioManager Instance;

    public enum MusicState { Ambient, Investigate, Chase }
    private MusicState currentMusicState = MusicState.Ambient;

    [Header("Mixer & Base Sources")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioSource investigateSource;
    [SerializeField] private AudioSource chaseSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Settings")]
    [SerializeField] private float fadeSpeed = 1.5f;

    [Header("3D Audio Pool")]
    [SerializeField] private int poolSize = 10;
    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    private Transform poolParent;

    [Header("Item Audio Clips (Optional References)")]
    public AudioClip StepSound;
    public AudioClip PryingSound;

    private float currentVolume = 0.5f;

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
            return;
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

    public void Initialize()
    {
        SetupSource(ambientSource, 1f);
        SetupSource(investigateSource, 0f);
        SetupSource(chaseSource, 0f);

        SetMasterVolume(PlayerPrefs.GetFloat("Settings_Volume", 0.5f));
    }

    private void Update()
    {
        HandleMusicFading();
    }

    private void SetupSource(AudioSource source, float initialVolume)
    {
        if (source != null && source.clip != null)
        {
            source.loop = true;
            source.volume = initialVolume;

            if (!source.isPlaying)
            {
                source.Play();
            }
        }
    }

    public void SetMusicState(MusicState newState)
    {
        currentMusicState = newState;
    }

    private void HandleMusicFading()
    {
        if (ambientSource == null || investigateSource == null || chaseSource == null) return;

        float targetAmbient = (currentMusicState == MusicState.Ambient) ? 1f : 0f;
        float targetInvestigate = (currentMusicState == MusicState.Investigate) ? 1f : 0f;
        float targetChase = (currentMusicState == MusicState.Chase) ? 1f : 0f;

        ambientSource.volume = Mathf.MoveTowards(ambientSource.volume, targetAmbient, fadeSpeed * Time.unscaledDeltaTime);
        investigateSource.volume = Mathf.MoveTowards(investigateSource.volume, targetInvestigate, fadeSpeed * Time.unscaledDeltaTime);
        chaseSource.volume = Mathf.MoveTowards(chaseSource.volume, targetChase, fadeSpeed * Time.unscaledDeltaTime);
    }

    public void SetMasterVolume(float value)
    {
        currentVolume = value;
        float db = Mathf.Log10(Mathf.Max(0.0001f, value)) * 20;
        
        if (mainMixer != null)
        {
            mainMixer.SetFloat("MasterVolume", db);
            mainMixer.SetFloat("BGMVolume", db);
            mainMixer.SetFloat("SFXVolume", db);
        }

        PlayerPrefs.SetFloat("Settings_Volume", value);
    }

    public void UpdateMasterVolume(float value) => SetMasterVolume(value);

    public void LoadData(GameData data)
    {
        currentVolume = data.audioSettings.masterVolume;
        SetMasterVolume(currentVolume);
    }

    public void SaveData(ref GameData data)
    {
        data.audioSettings.masterVolume = currentVolume;
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
        if (clip == null || audioSourcePool.Count == 0) return;

        AudioSource source = audioSourcePool.Dequeue();
        source.gameObject.SetActive(true);
        source.transform.position = position;
        source.clip = clip;

        source.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
        
        source.spatialBlend = 1f;
        source.minDistance = minDist;
        source.maxDistance = maxDist;
        
        source.Play();
        StartCoroutine(ReturnToPool(source, clip.length));
    }

    private IEnumerator ReturnToPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay + 0.1f);
        source.Stop();
        source.gameObject.SetActive(false);
        audioSourcePool.Enqueue(source);
    }
}