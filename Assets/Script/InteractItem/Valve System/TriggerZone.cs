using UnityEngine;
using UnityEngine.Events;

public class TriggerZone : MonoBehaviour
{
    public UnityEvent onPlayerEnter;

    private bool _hasTrigger = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !_hasTrigger)
        {
            onPlayerEnter.Invoke();
            _hasTrigger = true;
        }
    }
}
