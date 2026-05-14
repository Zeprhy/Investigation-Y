using UnityEngine;
using System.Collections;

public class ApartmentEventManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float delayBeforeBlackout = 10f;

    [Header("Referencess")]
    [SerializeField] private LightSwitch[] allLights;
    [SerializeField] private GameObject apartementDoor;

    [Header ("Audio")]
    [SerializeField] private AudioClip lightOut;

    private bool _playerHasEntered = false;

    public void StartApartementEvents()
    {
        if (_playerHasEntered) return;
        _playerHasEntered = true;

        LockDoor();

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.ShowDialogue("The door... it locked from the outside. I need to find another way out.");

        StartCoroutine(BlackoutRoutine());
    }

    public void RestoreApartementLights()
    {
        PowerSystem.RestorePower();
        {
            foreach (LightSwitch lamp in allLights)
            {
                if (lamp != null)
                {
                    lamp.isOn = true;
                    lamp.Toggle();
                }
            }
        }    
    }

    private void LockDoor()
    {
        if (apartementDoor != null)
        {
            NormalDoor doorScript = apartementDoor.GetComponent<NormalDoor>();
            if (doorScript != null)
            {
                doorScript.CloseDoor();
                doorScript.isLocked = true;
            }
        }

    }

    IEnumerator BlackoutRoutine()
    {
        yield return new WaitForSeconds(delayBeforeBlackout);

        PowerSystem.CutPower();

        foreach (LightSwitch lamp in allLights)
        {
            if (lamp != null) lamp.ForceTurnOff();
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(lightOut);

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.ShowDialogue("Perfect... the power is out. There must be a circuit breaker in the basement.");
    }
}
