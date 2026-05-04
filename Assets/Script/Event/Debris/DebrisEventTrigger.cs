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
    bool hasTriggered = false;

    public void LoadData(GameData data)
    {
        DebrisData dData = data.debrisListData.Find(x => x.id == id);
        
        if (dData != null) {
            this.hasTriggered = dData.hasTriggered;
        }

        if (hasTriggered) {
            Instantiate(debrisPrefab, spawnPoint.position, Quaternion.identity);
            this.gameObject.SetActive(false);
        }
    }

    public void SaveData(ref GameData data)
    {
        DebrisData dData = data.debrisListData.Find(x => x.id == id);

        if (dData != null) {
            // Update jika sudah ada
            dData.hasTriggered = hasTriggered;
        } else {
            // Tambah baru jika belum ada
            data.debrisListData.Add(new DebrisData(id, hasTriggered));
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