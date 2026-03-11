using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach script ini ke GameObject UI panel baterai.
/// Otomatis muncul/sembunyi tergantung apakah player sedang pegang StunGun.
/// </summary>
public class StunGunUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInteraction playerInteraction;

    [Header("UI Elements")]
    [SerializeField] private GameObject batteryPanel;       // Panel utama (parent semua elemen)
    [SerializeField] private Slider batterySlider;          // Slider fill baterai
    [SerializeField] private TextMeshProUGUI batteryText;   // Teks persen "75%"
    [SerializeField] private TextMeshProUGUI statusText;    // Teks "READY" / "EMPTY" / "CHARGING..."
    [SerializeField] private Image sliderFill;              // Image fill di dalam Slider

    [Header("Colors")]
    [SerializeField] private Color readyColor    = new Color(0.2f, 0.9f, 0.2f);
    [SerializeField] private Color chargingColor = new Color(0.3f, 0.6f, 1f);
    [SerializeField] private Color emptyColor    = new Color(0.9f, 0.1f, 0.1f);

    private StunGun currentStunGun;

    private void Update()
    {
        StunGun held = playerInteraction.GetHeldStunGun();

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