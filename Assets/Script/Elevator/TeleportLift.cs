using UnityEngine;
using System.Collections;
using UnityEngine.UIElements.Experimental;

public class TeleportLift : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool isEnabled = true;
    public bool IsEnabeled
    {
        get => isEnabled;
        set => isEnabled = value;
    }
    
    [SerializeField] private Transform destination;
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private float travelDelay = 3.0f;

    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeSpeed = 1.0f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip elevatorSFX;
    [SerializeField] private AudioClip arrivalDing;

    private bool _isTeleporting = false;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the trigger is the player, if we aren't already teleporting, 
        // and if the condition (isEnabled) is met.
        if (other.CompareTag(targetTag) && isEnabled && !_isTeleporting)
        {
            StartCoroutine(TeleportSequence(other.gameObject));
        }
        else if (other.CompareTag(targetTag) && !isEnabled)
        {
            Debug.Log("The lift is currently disabled or locked.");
        }
    }

    public void SetLiftState(bool state)
    {
        isEnabled = state;
    }

    private IEnumerator TeleportSequence(GameObject player)
    {
        _isTeleporting = true;
        
        // Disable player movement if you have a MovementPlayer component
        // var movement = player.GetComponent<MovementPlayer>();
        // if (movement != null) movement.enabled = false;

        CharacterController controller = player.GetComponent<CharacterController>();

        if (AudioManager.Instance != null && elevatorSFX != null)
            AudioManager.Instance.PlaySFX(elevatorSFX);

        yield return StartCoroutine(Fade(1f));
        yield return new WaitForSeconds(travelDelay);

        if (controller != null) controller.enabled = false;
        player.transform.position = destination.position;
        player.transform.rotation = destination.rotation;
        if (controller != null) controller.enabled = true;

        if (AudioManager.Instance != null && arrivalDing != null)
            AudioManager.Instance.PlaySFX(arrivalDing);

        yield return StartCoroutine(Fade(0f));

        // if (movement != null) movement.enabled = true;
        _isTeleporting = false;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeCanvasGroup == null) yield break;

        while (!Mathf.Approximately(fadeCanvasGroup.alpha, targetAlpha))
        {
            fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            yield return null;
        }
    }
}