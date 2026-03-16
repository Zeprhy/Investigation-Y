using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// LockpickUI - Mengontrol tampilan visual minigame lockpick.
/// Pasang script ini di Canvas/Panel minigame.
///
/// Hierarchy yang disarankan:
/// [Canvas]
///   └── LockpickPanel
///         ├── RingBackground      (Image: lingkaran putih/abu)
///         ├── SuccessZone         (Image: arc hijau, pakai Image Type = Filled)
///         ├── Needle              (Image: garis/arrow merah tipis)
///         ├── ProgressBar
///         │     └── Fill          (Image filled horizontal)
///         ├── SuccessCountText    (TextMeshPro: "1 / 3")
///         └── FeedbackText        (TextMeshPro: "GREAT!" / "MISS!")
/// </summary>
public class LockpickUI : MonoBehaviour
{
    [Header("== Referensi Minigame ==")]
    public LockpickMinigame minigame;

    [Header("== UI Elements ==")]
    [Tooltip("Panel utama minigame")]
    public GameObject lockpickPanel;

    [Tooltip("Image zona hijau (gunakan Image Type: Filled, Fill Method: Radial 360)")]
    public Image successZoneImage;

    [Tooltip("Transform jarum penunjuk")]
    public RectTransform needleTransform;

    [Tooltip("Image progress bar")]
    public Image progressBarFill;

    [Tooltip("Text jumlah sukses (contoh: '1 / 3')")]
    public TextMeshProUGUI successCountText;

    [Tooltip("Text feedback singkat (GREAT! / MISS!)")]
    public TextMeshProUGUI feedbackText;

    [Header("== Warna ==")]
    public Color successZoneColor = new Color(0.2f, 0.9f, 0.3f, 0.8f);
    public Color successFlashColor = new Color(1f, 1f, 0.2f, 1f);
    public Color failFlashColor = new Color(1f, 0.2f, 0.2f, 1f);
    public Color normalNeedleColor = Color.red;

    [Header("== Pengaturan Animasi ==")]
    public float feedbackDuration = 0.8f;

    // ---- Cache ----
    private Image needleImage;
    private Coroutine feedbackCoroutine;
    private int totalRequired;

    void Start()
    {
        if (needleTransform != null)
            needleImage = needleTransform.GetComponent<Image>();

        if (successZoneImage != null)
            successZoneImage.color = successZoneColor;

        // Subscribe events
        if (minigame != null)
        {
            minigame.onProgress.AddListener(OnProgress);
            minigame.onMinigameSuccess.AddListener(OnMinigameSuccess);
            minigame.onMinigameFailed.AddListener(OnMinigameFailed);
        }

        // Sembunyikan panel dan feedback awal
        if (lockpickPanel != null) lockpickPanel.SetActive(false);
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!lockpickPanel.activeSelf) return;
        if (minigame == null) return;

        // Sync rotasi jarum dari minigame ke UI
        // LockpickMinigame sudah menghandle rotasi needle-nya sendiri,
        // tapi kalau kamu mau UI needle terpisah, sync di sini:
        // needleTransform.localRotation = minigame.needle.localRotation;
    }

   
    public void ShowSuccessFeedback()
    {
        ShowFeedback("GREAT!", successFlashColor);
    }
    public void ShowFailFeedback()
    {
        ShowFeedback("MISS!", failFlashColor);
    }

    // ---- Event Handlers ----

    private void OnProgress(int current, int required)
    {
        totalRequired = required;

        // Update text
        if (successCountText != null)
            successCountText.text = $"{current} / {required}";

        // Update progress bar
        if (progressBarFill != null)
            progressBarFill.fillAmount = (float)current / required;

        // Feedback "GREAT!"
        ShowFeedback("GREAT!", successFlashColor);
    }

    private void OnMinigameSuccess()
    {
        ShowFeedback("UNLOCKED!", successFlashColor);
        StartCoroutine(HidePanelDelayed(1.0f));
    }

    private void OnMinigameFailed()
    {
        ShowFeedback("FAILED!", failFlashColor);
        StartCoroutine(HidePanelDelayed(1.0f));
    }

    // ---- Helpers ----

    private void ShowFeedback(string message, Color color)
    {
        if (feedbackText == null) return;

        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);

        feedbackCoroutine = StartCoroutine(FeedbackAnimation(message, color));
    }

    private IEnumerator FeedbackAnimation(string message, Color color)
    {
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = message;
        feedbackText.color = color;

        // Scale punch effect
        feedbackText.transform.localScale = Vector3.one * 1.4f;

        float elapsed = 0f;
        while (elapsed < feedbackDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / feedbackDuration;

            // Scale kembali ke normal
            float scale = Mathf.Lerp(1.4f, 1f, t);
            feedbackText.transform.localScale = Vector3.one * scale;

            // Fade out di akhir
            if (t > 0.6f)
            {
                float alpha = Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f);
                Color c = color;
                c.a = alpha;
                feedbackText.color = c;
            }

            yield return null;
        }

        feedbackText.gameObject.SetActive(false);
    }

    private IEnumerator HidePanelDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (lockpickPanel != null)
            lockpickPanel.SetActive(false);

        // Reset progress bar
        if (progressBarFill != null) progressBarFill.fillAmount = 0f;
        if (successCountText != null) successCountText.text = "0 / ?";
    }
}