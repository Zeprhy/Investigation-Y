using System.Collections;
using UnityEngine;

public class DebrisEventTrigger : MonoBehaviour, IDataPersistence
{
    [Header("Debris")]
    [SerializeField] GameObject debrisPrefab;
    [SerializeField] Transform spawnPoint;

    [Header("Warning FX")]
    [SerializeField] AudioClip crackSFX;          // tetap ada, tapi diplay lewat AudioManager
    [SerializeField] ParticleSystem dustParticle;
    [SerializeField] float warningDuration = 2f;

    [SerializeField] private string id;
    [ContextMenu("Generate guid for id")]
    private void GenerateGuid() { id = System.Guid.NewGuid().ToString(); }

    bool hasTriggered = false;

    public void LoadData(GameData data)
    {
        // Cek apakah ID debris ini sudah pernah jatuh di data save
        data.debrisFallenStatus.TryGetValue(id, out hasTriggered);
        
        if (hasTriggered)
        {
            // Jika sudah pernah jatuh, langsung munculkan debris tanpa animasi lagi
            Instantiate(debrisPrefab, spawnPoint.position, Quaternion.identity);
            // Matikan trigger agar tidak bunyi SFX lagi
            this.gameObject.SetActive(false); 
        }
    }

    public void SaveData(ref GameData data)
    {
        // Simpan status apakah debris ini sudah dipicu
        if (data.debrisFallenStatus.ContainsKey(id))
        {
            data.debrisFallenStatus[id] = hasTriggered;
        }
        else
        {
            data.debrisFallenStatus.Add(id, hasTriggered);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        hasTriggered = true;
        StartCoroutine(DebrisSequence());
    }

    IEnumerator DebrisSequence()
    {
        yield return new WaitUntil(() => !PauseMenu.isPausedStatic);

        // Pakai AudioManager kamu
        if (crackSFX != null)
            AudioManager.Instance.PlaySFX(crackSFX);

        if (dustParticle != null) dustParticle.Play();
        CameraShakeManager.Instance.ShakeLight();

        yield return new WaitForSeconds(warningDuration);

        Instantiate(debrisPrefab, spawnPoint.position, Quaternion.identity);
    }
}