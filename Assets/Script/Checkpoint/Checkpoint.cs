using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private string checkpointID;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Gunakan fungsi dari Manager agar terpusat
            CheckpointManager.Instance.SetNewCheckpoint(other.transform.position);
            Debug.Log($"Checkpoint {checkpointID} Disimpan!");
            GetComponent<Collider>().enabled = false;
        }
    }
}