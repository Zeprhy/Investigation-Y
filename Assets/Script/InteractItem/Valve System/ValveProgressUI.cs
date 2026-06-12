using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic; // WAJIB DITAMBAHKAN UNTUK LIST

public class ValveProgressUI : MonoBehaviour
{
    [Header("   References   ")]
    [Tooltip("Reference ke Valve Puzzle Manager untuk mengambil semua data valve secara otomatis")]
    [SerializeField] private ValvePuzzleManager puzzleManager;
    
    [Header("   UI Elements   ")]
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
    
    [Header("   Visual Settings   ")]
    [Tooltip("Color when progress is increasing")]
    [SerializeField] private Color fillColor = Color.green;
    
    [Tooltip("Color when progress is draining")]
    [SerializeField] private Color drainColor = Color.red;
    
    [Tooltip("Color when idle (no change)")]
    [SerializeField] private Color idleColor = Color.white;
    
    [Header("   Animation Settings   ")]
    [Tooltip("Fade in duration (seconds)")]
    [SerializeField] private float fadeInDuration = 0.3f;
    
    [Tooltip("Fade out duration (seconds)")]
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    [Tooltip("How long to wait before fading out after interaction stops")]
    [SerializeField] private float displayDuration = 2f;
    
    // --- State Internal ---
    private List<ValveInteraction> _valves = new List<ValveInteraction>();
    private float[] _lastProgressValues; // Menyimpan progress terakhir tiap valve untuk deteksi delta
    private bool _isVisible = false;
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

        // === MODIFIKASI: Ambil list valve dari Puzzle Manager ===
        if (puzzleManager != null)
        {
            _valves = puzzleManager.Valves;
            if (_valves != null && _valves.Count > 0)
            {
                // Inisialisasi array tracking sesuai jumlah valve
                _lastProgressValues = new float[_valves.Count];
                for (int i = 0; i < _valves.Count; i++)
                {
                    if (_valves[i] != null)
                        _lastProgressValues[i] = _valves[i].Progress;
                }
            }
        }
        else
        {
            Debug.LogError("ValvePuzzleManager belum dimasukkan ke referensi ValveProgressUI!");
        }
    }

    void Update()
    {
        if (_valves == null || _valves.Count == 0) return;
    
        ValveInteraction activeValve = null;
        float targetProgress = 0f;
        float currentDelta = 0f;

        // Loop untuk mengecek apakah ada salah satu valve yang sedang berputar
        for (int i = 0; i < _valves.Count; i++)
        {
            if (_valves[i] == null) continue;

            float currentProgress = _valves[i].Progress;
            float delta = currentProgress - _lastProgressValues[i];

            // Jika ada perubahan nilai progress pada frame ini
            if (Mathf.Abs(delta) > 0.001f)
            {
                activeValve = _valves[i];
                targetProgress = currentProgress;
                currentDelta = delta;
            }

            // Update catatan progress terakhir untuk frame berikutnya
            _lastProgressValues[i] = currentProgress;
        }

        // Jika terdeteksi ada valve yang aktif berputar
        if (activeValve != null)
        {
            if (!_isVisible)
            {
                ShowPanel();
            }

            UpdateProgressVisuals(targetProgress, currentDelta);

            if (_hideCoroutine != null)
            {
                StopCoroutine(_hideCoroutine);
            }
            _hideCoroutine = StartCoroutine(AutoHidePanel());
        }
    }

    // Menggunakan parameter delta langsung dari kalkulasi Update loop
    private void UpdateProgressVisuals(float progress, float delta)
    {
        if (progressFill != null)
        {
            progressFill.fillAmount = progress;
        }

        if (progressText != null)
        {
            progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
        }

        if (delta > 0.001f) // Sedang Mengisi (Filling)
        {
            if (progressFill != null) progressFill.color = fillColor;
            if (fillIcon != null) fillIcon.enabled = true;
            if (drainIcon != null) drainIcon.enabled = false;
        }
        else if (delta < -0.001f) // Sedang Berkurang (Draining)
        {
            if (progressFill != null) progressFill.color = drainColor;
            if (drainIcon != null) drainIcon.enabled = true;
            if (fillIcon != null) fillIcon.enabled = false;
        }
        else // Diam (Idle)
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