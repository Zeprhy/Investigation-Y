using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    [SerializeField] private CanvasGroup dialogueCanvasGroup;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Animation Settings")]
    [SerializeField] private float typingSpeed = 0.04f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float defaultDuration = 3f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    public void Initialize()
    {
        if (dialogueCanvasGroup != null)
        {
            dialogueCanvasGroup.alpha = 0;
            dialogueCanvasGroup.blocksRaycasts = false;
            dialogueCanvasGroup.interactable = false;
        }
    
        if (dialogueText != null)
        {
            dialogueText.text = "";
        }
    }

    public void ShowDialogue(string message, float duration = 0)
    {
        float time = duration <= 0 ? defaultDuration : duration;
        StopAllCoroutines();
        StartCoroutine(DisplayRoutine(message, time));
    }

    private IEnumerator DisplayRoutine(string message, float time)
    {
        yield return StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 0, 1, fadeDuration));

        dialogueText.text = "";
        foreach (char letter in message.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(time);
        yield return StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 1, 0, fadeDuration));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        cg.alpha = end;
    }
}
