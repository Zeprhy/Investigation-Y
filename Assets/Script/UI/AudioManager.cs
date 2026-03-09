using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("BGM Clip")]
    public AudioClip backgroundMusic;
    public AudioMixer mainMixer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (backgroundMusic != null)
        {
            bgmSource.clip = backgroundMusic;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    // Satu fungsi untuk kendalikan semua suara di game
    public void SetMasterVolume(float value)
    {
        float db = Mathf.Log10(Mathf.Max(0.0001f, value)) * 20;
        mainMixer.SetFloat("MasterVolume", db);
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
}