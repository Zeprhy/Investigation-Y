using UnityEngine;
using UnityEngine.Events;

public class EventSystemTrigger : MonoBehaviour
{
    public UnityEvent eventManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            eventManager.Invoke();
            GetComponent<Collider>().enabled = false;
        }
    }
}
