using System.Collections;
using UnityEngine;

public class EnemyStalker : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private float disappearDistance = 8f;
    [SerializeField] private float slideSpeed = 25f;
    [SerializeField] private float slideDistance = 5f;

    [Header (" Audio Clip ")]
    [SerializeField] private AudioClip dissapearSFX;

    private bool _hasDisappeared = false;

    void Update()
    {
        if (_hasDisappeared || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        
        if (distance < disappearDistance)
        {
            StartCoroutine(Disapear());
        }
    }

    private IEnumerator Disapear()
    {
        _hasDisappeared = true;

        if (dissapearSFX != null && GameManager.Instance.audioManager != null)
        {
            GameManager.Instance.audioManager.PlaySFX(dissapearSFX);
        }

        Vector3 targetPosition = transform.position + (transform.right * slideDistance);

        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, slideSpeed * Time.unscaledDeltaTime);
            yield return null;    
        }

        DialogueManager.Instance.ShowDialogue("Hey who's there!");

        gameObject.SetActive(false);
    }
}
