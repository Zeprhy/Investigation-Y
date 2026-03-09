using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // Wajib untuk Brightness URP

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
    [SerializeField] private Volume globalVolume; // Tarik Global Volume ke sini
    private ColorAdjustments colorAdjustments;

    void Start()
    {
        // --- Setup Audio ---
        masterVolumeSlider.minValue = 0f;
        masterVolumeSlider.maxValue = 1f;
        masterVolumeSlider.value = 0.5f;
        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);

        // --- Setup FOV ---
        fovSlider.minValue = minFOV;
        fovSlider.maxValue = maxFOV;
        fovSlider.value = playerCamera.fieldOfView;
        fovSlider.onValueChanged.AddListener(SetFOV);

        // --- Setup Brightness (Post Processing) ---
        if (globalVolume.profile.TryGet(out colorAdjustments))
        {
            brightnessSlider.minValue = -2f; // Gelap
            brightnessSlider.maxValue = 2f;  // Terang
            brightnessSlider.value = colorAdjustments.postExposure.value;
            brightnessSlider.onValueChanged.AddListener(SetBrightness);
        }
    }

    public void SetMasterVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMasterVolume(value);
    }

    public void SetFOV(float value)
    {
        if (playerCamera != null)
            playerCamera.fieldOfView = value;
    }

    public void SetBrightness(float value)
    {
        if (colorAdjustments != null)
            colorAdjustments.postExposure.value = value;
    }
}