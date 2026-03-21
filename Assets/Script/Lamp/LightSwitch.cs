using UnityEngine;
using System.Collections; // Wajib ditambahkan untuk Coroutine

public class LightSwitch : MonoBehaviour
{
    [Header("References")]
    public Light[] lampLights;
    public Renderer bulbRenderer;
    
    [Header("Settings")]
    public bool isOn = true;
    [ColorUsage(true, true)] 
    public Color emissionColor = Color.white;

    [Header("Flicker Settings")]
    public bool useFlicker = true;      // Centang jika ingin ada efek kedipan
    public int flickerCount = 4;        // Berapa kali kedipan terjadi
    public float flickerSpeed = 0.05f;  // Kecepatan antar kedipan

    public void Toggle()
    {
        isOn = !isOn;

        if (isOn && useFlicker)
        {
            // Jika lampu dinyalakan, jalankan efek kedipan
            StartCoroutine(FlickerEffect());
        }
        else
        {
            // Jika lampu dimatikan, langsung matikan saja
            SetLightState(isOn);
        }
    }

    // Fungsi pembantu untuk mengatur cahaya dan visual secara bersamaan
    private void SetLightState(bool state)
    {
        // Melakukan loop untuk setiap lampu yang ada di dalam daftar
        foreach (Light light in lampLights)
        {
            if (light != null) light.enabled = state;
        }
        
        if (bulbRenderer != null)
        {
            Color finalColor = state ? emissionColor : Color.black;
            bulbRenderer.material.SetColor("_EmissionColor", finalColor);
            DynamicGI.SetEmissive(bulbRenderer, finalColor);
        }
    }

    // Coroutine untuk efek lampu konslet
    IEnumerator FlickerEffect()
    {
        for (int i = 0; i < flickerCount; i++)
        {
            SetLightState(false); // Matikan sebentar
            yield return new WaitForSeconds(flickerSpeed);
            SetLightState(true);  // Hidupkan sebentar
            yield return new WaitForSeconds(flickerSpeed);
        }
        
        // Pastikan di akhir tetap menyala sesuai status isOn
        SetLightState(isOn);
    }
}