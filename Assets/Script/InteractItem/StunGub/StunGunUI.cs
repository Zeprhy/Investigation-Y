using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StunGunUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInteraction playerInteraction;

    [Header("UI Elements")]
    [SerializeField] private GameObject batteryPanel;       
    [SerializeField] private Slider batterySlider;          
    [SerializeField] private TextMeshProUGUI batteryText;   
    [SerializeField] private TextMeshProUGUI statusText;    
    [SerializeField] private Image sliderFill;              

    [Header("Colors")]
    [SerializeField] private Color readyColor    = new Color(0.2f, 0.9f, 0.2f);
    [SerializeField] private Color chargingColor = new Color(0.3f, 0.6f, 1f);
    [SerializeField] private Color emptyColor    = new Color(0.9f, 0.1f, 0.1f);

    private StunGun currentStunGun;

    private void Update()
    {
        StunGun held = playerInteraction.GetHeldStunGun();
         Debug.Log("GetHeldStunGun: " + (held != null));

        if (held != currentStunGun)
            currentStunGun = held;

        bool show = currentStunGun != null;
        if (batteryPanel.activeSelf != show)
            batteryPanel.SetActive(show);

        if (!show) return;

        float pct = currentStunGun.BatteryPercent;

        if (batterySlider) batterySlider.value = pct / 100f;
        if (batteryText)   batteryText.text = $"{Mathf.RoundToInt(pct)}%";

        Color col;
        string status;

        if (currentStunGun.IsCharging)
        {
            col    = chargingColor;
            status = "CHARGING...";
        }
        else if (pct >= 100f)
        {
            col    = readyColor;
            status = "READY";
        }
        else
        {
            col    = emptyColor;
            status = "EMPTY";
        }

        if (sliderFill)  sliderFill.color  = col;
        if (statusText) { statusText.text  = status; statusText.color = col; }
    }
}