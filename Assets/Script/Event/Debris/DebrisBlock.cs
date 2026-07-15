using UnityEngine;
using UnityEngine.AI;

public class DebrisBlock : MonoBehaviour
{
    [Header("Impact FX")]
    [SerializeField] AudioClip impactSFX;
    [SerializeField] ParticleSystem impactDust;

    Rigidbody rb;
    NavMeshObstacle obstacle;
    bool hasLanded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        obstacle = GetComponent<NavMeshObstacle>();

        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

        obstacle.carving = false;
        obstacle.carveOnlyStationary = true;
    }

    void FixedUpdate() {
        if (!hasLanded && rb != null)
        {
            rb.AddForce(Vector3.down * 20f, ForceMode.Acceleration);
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (hasLanded) return;
        if (!collision.gameObject.CompareTag("Ground") && !collision.gameObject.CompareTag("Floor")) return;

        hasLanded = true;

        if ( impactSFX != null) GameManager.Instance.audioManager.PlaySFX(impactSFX);
        if (impactDust != null) impactDust.Play(); 
        CameraShakeManager.Instance.ShakeHeavy();

        rb.isKinematic = true;
        obstacle.carving = true;   
    }
}
