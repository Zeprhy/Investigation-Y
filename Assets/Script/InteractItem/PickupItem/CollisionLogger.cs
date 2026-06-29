using UnityEngine;

public class CollisionLogger : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[CollisionLogger] {gameObject.name} BERTABRAKAN dengan: {collision.gameObject.name}", collision.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[CollisionLogger] {gameObject.name} OVERLAP TRIGGER dengan: {other.gameObject.name}", other.gameObject);
    }
}