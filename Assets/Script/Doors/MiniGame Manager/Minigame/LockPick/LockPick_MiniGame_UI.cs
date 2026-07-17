using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class LockPick_MiniGame_UI : MonoBehaviour
{
    [Header("== Referensi Minigame ==")]
    public LockPick_MiniGame minigame;

    [Header("== UI Elements ==")]
    public GameObject lockpickPanel;
    public Image successZoneImage;
    public RectTransform needleTransform;
    public Image progressBarFill;
    public TextMeshProUGUI successCountText;
    public TextMeshProUGUI feedbackText;

    [Header("== Warna ==")]
    public Color successZoneColor = new Color(0.2f, 0.9f, 0.3f, 0.8f);
    public Color successFlashColor = new Color(1f, 1f, 0.2f, 1f);
    public Color failFlashColor = new Color(1f, 0.2f, 0.2f, 1f);
    public Color normalNeedleColor = Color.red;

    [Header("== Pengaturan Animasi ==")]
    public float feedbackDuration = 0.8f;

    private Image needleImage;
    private Coroutine feedbackCoroutine;
    private int totalRequired;

    private void Awake() // Ganti Start() yang di atas menjadi Awake() hanya untuk komponen visual
    {
        if (needleTransform != null)
            needleImage = needleTransform.GetComponent<Image>();

        if (successZoneImage != null)
            successZoneImage.color = successZoneColor;

        // Sembunyikan panel dan feedback awal
        if (lockpickPanel != null) lockpickPanel.SetActive(false);
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
    }

    // --- KODE BARU: Menggantikan fungsi Start() yang lama ---
    public void Setup(LockPick_MiniGame logicInstance)
    {
        minigame = logicInstance;

        // Subscribe events setelah menerima referensi yang valid
        if (minigame != null)
        {
            minigame.onProgress.AddListener(OnProgress);
            minigame.onMinigameSuccess.AddListener(OnMinigameSuccess);
            minigame.onMinigameFailed.AddListener(OnMinigameFailed);
        }
    }

    void Update()
    {
        if (!lockpickPanel.activeSelf) return;
        if (minigame == null) return;
    }

    public void ShowSuccessFeedback()
    {
        ShowFeedback("GREAT!", successFlashColor);
    }
    
    public void ShowFailFeedback()
    {
        ShowFeedback("MISS!", failFlashColor);
    }

    private void OnProgress(int current, int required)
    {
        totalRequired = required;
        if (successCountText != null) successCountText.text = $"{current} / {required}";
        if (progressBarFill != null) progressBarFill.fillAmount = (float)current / required;
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

    private void ShowFeedback(string message, Color color)
    {
        if (feedbackText == null) return;
        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(FeedbackAnimation(message, color));
    }

    private IEnumerator FeedbackAnimation(string message, Color color)
    {
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = message;
        feedbackText.color = color;
        feedbackText.transform.localScale = Vector3.one * 1.4f;

        float elapsed = 0f;
        while (elapsed < feedbackDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / feedbackDuration;

            float scale = Mathf.Lerp(1.4f, 1f, t);
            feedbackText.transform.localScale = Vector3.one * scale;

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
        if (lockpickPanel != null) lockpickPanel.SetActive(false);
        if (progressBarFill != null) progressBarFill.fillAmount = 0f;
        if (successCountText != null) successCountText.text = "0 / ?";
    }
}