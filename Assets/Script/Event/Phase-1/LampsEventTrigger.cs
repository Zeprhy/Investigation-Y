using UnityEngine;
using UnityEngine.Events;

public class LampsEventTrigger : MonoBehaviour
{
    public UnityEvent eventManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            eventManager.Invoke();
            Destroy(gameObject);
        }
    }
}
