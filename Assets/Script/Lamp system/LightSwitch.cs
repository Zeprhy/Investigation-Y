using UnityEngine;
using System.Collections;

public class LightSwitch : MonoBehaviour
{
    [Header("References")]
    public Light[] lampLights;
    public Renderer bulbRenderer;
    [SerializeField] private Animator _animator;
    
    [Header("Settings")]
    public bool isOn = true;
    [ColorUsage(true, true)] 
    public Color emissionColor = Color.white;

    [Header("Flicker Settings")]
    public bool useFlicker = true;
    public int flickerCount = 4;
    public float flickerSpeed = 0.05f;

    private static readonly int IsOnHash = Animator.StringToHash("isOn");

    private void Awake()
    {
        if (_animator != null) _animator = GetComponent<Animator>();

        UpdateAnimator();
        SetLightState(isOn);
    }

    public void Toggle()
    {
        if (!PowerSystem.IsPowerOn) return;

        isOn = !isOn;
        UpdateAnimator();

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

    private void UpdateAnimator()
    {
        if (_animator != null)
        {
            _animator.SetBool(IsOnHash, isOn);
        }
    }

    // Fungsi pembantu untuk mengatur cahaya dan visual secara bersamaan
    private void SetLightState(bool state)
    {
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

    IEnumerator FlickerEffect()
    {
        for (int i = 0; i < flickerCount; i++)
        {
            SetLightState(false); 
            yield return new WaitForSeconds(flickerSpeed);
            SetLightState(true);
            yield return new WaitForSeconds(flickerSpeed);
        }
        SetLightState(isOn);
    }

    public void ForceTurnOff()
    {
        StartCoroutine(FlickerEffect());
        isOn = false;
    } 

    public void ForceTurnOn()
    {
        isOn = true;
        if (useFlicker)
        {
            StartCoroutine(FlickerEffect());
        }
        else
        {
            SetLightState(true);
        }
    }
}