using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using Unity.VisualScripting;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    [Header("UI Refencess")]
    [SerializeField] private CanvasGroup objectiveCanvasGroup;
    [SerializeField] private TextMeshProUGUI objectiveText;

    [Header("Animation Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Current Progress")]
    [SerializeField] private ObjectiveSO currentObjective;

    private void Awake()
    {
        if (Instance == null ) Instance = this;
        else Destroy(gameObject);

        if (objectiveCanvasGroup != null) objectiveCanvasGroup.alpha = 0;
    }

    private Coroutine _activeRoutine;

    public void SetNewObjective(ObjectiveSO newObj)
    {
        if (newObj == null) return;

        ObjectiveSO clone = Instantiate(newObj);
        clone.Reset();

        if (currentObjective != null && currentObjective.objectiveID == clone.objectiveID 
            && objectiveCanvasGroup.alpha > 0) return;

        currentObjective = clone;

        if (_activeRoutine != null) StopCoroutine(_activeRoutine);
        _activeRoutine = StartCoroutine(AppearRoutine());
    }

    public void ForceCompleteCurrentObjective()
    {
        if (currentObjective == null) return;
        currentObjective.iscompleted = true;

        if (_activeRoutine != null) StopCoroutine(_activeRoutine);
        _activeRoutine = StartCoroutine(DissapearRoutine());
    }

    public void CompleteObjetive(string id)
    {
        if (currentObjective != null && currentObjective.objectiveID == id)
        {
            currentObjective.iscompleted = true;
            if (_activeRoutine != null) StopCoroutine(_activeRoutine);
            _activeRoutine = StartCoroutine(DissapearRoutine());
        }
    }

    private IEnumerator AppearRoutine()
    {
        objectiveText.text = "";
        yield return StartCoroutine(FadecanvasGroup(0, 1, fadeDuration)); 
        yield return StartCoroutine(TypeText(currentObjective.Description));  
    }

    private IEnumerator DissapearRoutine()
    {
        yield return StartCoroutine(FadecanvasGroup(objectiveCanvasGroup.alpha, 0, fadeDuration)); 
        objectiveText.text = "";
        currentObjective = null;
    }

    private IEnumerator TypeText(string text)
    {
        objectiveText.text = "";
        foreach (char letter in text.ToCharArray())
        {
            objectiveText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private IEnumerator FadecanvasGroup(float start, float end, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            objectiveCanvasGroup.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        objectiveCanvasGroup.alpha = end;
    }
}
