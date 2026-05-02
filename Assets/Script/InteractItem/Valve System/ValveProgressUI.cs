using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class ValveProgressUI : MonoBehaviour
{
    [Header("  References  ")]
    [Tooltip("Reference to the ValveInteraction script")]
    [SerializeField] private ValveInteraction valveInteraction;
    
    [Header("  UI Elements  ")]
    [Tooltip("The Canvas Group for fade in/out control")]
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Tooltip("Progress bar fill image")]
    [SerializeField] private Image progressFill;
    
    [Tooltip("Text to show percentage (optional)")]
    [SerializeField] private Text progressText;
    
    [Tooltip("Arrow/Icon showing drain direction")]
    [SerializeField] private Image drainIcon;
    
    [Tooltip("Arrow/Icon showing fill direction")]
    [SerializeField] private Image fillIcon;
    
    [Header("  Visual Settings  ")]
    [Tooltip("Color when progress is increasing")]
    [SerializeField] private Color fillColor = Color.green;
    
    [Tooltip("Color when progress is draining")]
    [SerializeField] private Color drainColor = Color.red;
    
    [Tooltip("Color when idle (no change)")]
    [SerializeField] private Color idleColor = Color.white;
    
    [Header("  Animation Settings  ")]
    [Tooltip("Fade in duration (seconds)")]
    [SerializeField] private float fadeInDuration = 0.3f;
    
    [Tooltip("Fade out duration (seconds)")]
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    [Tooltip("How long to wait before fading out after interaction stops")]
    [SerializeField] private float displayDuration = 2f;
    
    // --- State ---
    private bool _isVisible = false;
    private float _lastProgress = 0f;
    private Coroutine _fadeCoroutine;
    private Coroutine _hideCoroutine;

    void Start()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            _isVisible = false;
        }

        if (drainIcon != null)
            drainIcon.enabled = false;
        if (fillIcon != null)
            fillIcon.enabled = false;

        if (valveInteraction == null)
        {
            Debug.LogError("ValveInteraction.cs Hilang dari refrensi");
        }
    }

    void Update()
    {
        if (valveInteraction == null) return;
    
        float currentProgress = valveInteraction.Progress;

        bool isActive = Mathf.Abs(currentProgress - _lastProgress) > 0.001f;

        if (isActive)
        {
            if (!_isVisible)
            {
                ShowPanel();
            }

            UpdateProgressVisuals(currentProgress);

            if (_hideCoroutine != null)
            {
                StopCoroutine(_hideCoroutine);
            }
            _hideCoroutine = StartCoroutine(AutoHidePanel());
        }
        _lastProgress = currentProgress;
    }

    private void UpdateProgressVisuals(float progress)
    {
        if (progressFill != null)
        {
            progressFill.fillAmount = progress;
        }

        if (progressText != null)
        {
            progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
        }

        float delta = progress - _lastProgress;
        if (delta > 0.001f)
        {
            if (progressFill != null) progressFill.color = fillColor;
            if (fillIcon != null) fillIcon.enabled = true;
            if (drainIcon != null) drainIcon.enabled = false;
        }
        else if (delta < -0.001f) // Draining
        {
            if (progressFill != null) progressFill.color = drainColor;
            if (drainIcon != null) drainIcon.enabled = true;
            if (fillIcon != null) fillIcon.enabled = false;
        }
        else // Idle
        {
            if (progressFill != null) progressFill.color = idleColor;
            if (drainIcon != null) drainIcon.enabled = false;
            if (fillIcon != null) fillIcon.enabled = false;
        }

    }

    private void ShowPanel()
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeIn());
    }

    private void HidePanel()
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        _isVisible = true;
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        _isVisible = false;
    }

    private IEnumerator AutoHidePanel()
    {
        yield return new WaitForSeconds(displayDuration);
        HidePanel();
    }

}