using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private Slider masterVolumeSlider;

    [Header("FOV Settings")]
    [SerializeField] private Slider fovSlider;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float minFOV = 60f;
    [SerializeField] private float maxFOV = 90f;

    [Header("Brightness Settings (URP)")]
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private Volume globalVolume;
    [SerializeField] private float minBrightness = -3f;
    [SerializeField] private float maxBrightness = 3f;
    private ColorAdjustments colorAdjustments;

    private const string KEY_FOV = "Settings_FOV";
    private const string KEY_VOLUME = "Settings_Volume";
    private const string KEY_BRIGHTNESS = "Settings_Brightness";

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void Awake()
    {
    // --- Volume ---
    float savedVolume = PlayerPrefs.GetFloat("Settings_Volume", 0.5f);
    SetMasterVolume(savedVolume);

    // --- FOV ---
    float savedFOV = PlayerPrefs.GetFloat("Settings_FOV", 60f);
    SetFOV(savedFOV);

    // // --- Brightness ---
    //     if (globalVolume.profile.TryGet(out colorAdjustments))
    //     {
    //         float savedBrightness = PlayerPrefs.GetFloat("Settings_Brightness", 0f);
    //         brightnessSlider.value = savedBrightness;
    //         SetBrightness(savedBrightness);
    //     }


    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        playerCamera = Camera.main;

        globalVolume = FindAnyObjectByType<Volume>();
        if (globalVolume != null)
        {
            globalVolume.profile.TryGet(out colorAdjustments);
        }

        GameObject uiRoot = GameObject.FindWithTag("PlayerUI");
        if(uiRoot != null)
        {
            Slider[] allSliders = uiRoot.GetComponentsInChildren<Slider>(true);
            foreach(Slider s in allSliders)
            {
                if(s.gameObject.name == "VolumeSlider") masterVolumeSlider = s;
                if(s.gameObject.name == "FOVSlider") fovSlider = s;
                if(s.gameObject.name == "BrightnessSlider") brightnessSlider = s;
            }
        }
    }

    public void SetMasterVolume(float value)
    {
        if (GameManager.Instance.audioManager != null)
        {
            GameManager.Instance.audioManager.UpdateMasterVolume(value);
        }
            

        PlayerPrefs.SetFloat(KEY_VOLUME, value);
        PlayerPrefs.Save();
    }

    public void SetFOV(float value)
    {
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = value;
        }
            
        MovementPlayer mp = FindAnyObjectByType<MovementPlayer>();
        if (mp != null) mp.UpdateSavedFOV(value);

        PlayerPrefs.SetFloat(KEY_FOV, value);
        PlayerPrefs.Save();
    }

    public void SetBrightness(float value)
    {
        if (colorAdjustments != null)
        {
           colorAdjustments.postExposure.value = value; 
        }
            

        PlayerPrefs.SetFloat(KEY_BRIGHTNESS, value);
        PlayerPrefs.Save();
    }
    public void OnSettingsPanelOpen()
    {
    // Setup slider baru dijalankan saat panel dibuka
    masterVolumeSlider.minValue = 0f;
    masterVolumeSlider.maxValue = 1f;
    masterVolumeSlider.value = PlayerPrefs.GetFloat("Settings_Volume", 0.5f);

    fovSlider.minValue = minFOV;
    fovSlider.maxValue = maxFOV;
    fovSlider.value = PlayerPrefs.GetFloat("Settings_FOV", 60f);

    brightnessSlider.minValue = minBrightness;
    brightnessSlider.maxValue = maxBrightness;
    brightnessSlider.value = PlayerPrefs.GetFloat("Settings_Brightness", 0f);

    masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
    fovSlider.onValueChanged.AddListener(SetFOV);
    brightnessSlider.onValueChanged.AddListener(SetBrightness);
    }

    public void OnSettingsPanelClose()
    {
    // Bersihkan listener saat panel ditutup agar tidak dobel
    masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
    fovSlider.onValueChanged.RemoveListener(SetFOV);
    brightnessSlider.onValueChanged.RemoveListener(SetBrightness);
    }
}