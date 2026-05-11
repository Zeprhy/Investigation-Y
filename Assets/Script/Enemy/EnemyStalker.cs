using System.Collections;
using UnityEngine;

public class EnemyStalker : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private float disappearDistance = 8f;
    [SerializeField] private AudioClip dissapearSFX;
    [SerializeField] private float slideSpeed = 25f;
    [SerializeField] private float slideDistance = 5f;

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

        if (dissapearSFX != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(dissapearSFX);
        }

        Vector3 targetPosition = transform.position + (transform.right * slideDistance);

        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, slideSpeed * Time.unscaledDeltaTime);
            yield return null;    
        }

        gameObject.SetActive(false);
    }
}
