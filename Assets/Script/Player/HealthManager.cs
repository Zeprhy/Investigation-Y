using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class HealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public int currentHealth;
    public bool isDead = false;

    [Header("UI Element")]
    public Image BloodScreenImage;

    [Header("Blood Screen Settings")]
    [Range(0f, 1f)] public float minAlpha = 0f;
    [Range(0f, 1f)] public float maxAlpha = 0.8f;
    public float fadeSpeed = 2f;
    public StabSequence stabCinematic;

    private float targetAlpha;

    void Awake()
    {
        currentHealth = maxHealth;
        if (BloodScreenImage != null)
        {
            Color c = BloodScreenImage.color;
            c.a = 0;
            BloodScreenImage.color = c;
        }
    }

    void Update()
    {
        if (BloodScreenImage != null)
        {
            Color curColor = BloodScreenImage.color;
            curColor.a = Mathf.MoveTowards(curColor.a, targetAlpha, fadeSpeed * Time.deltaTime);
            BloodScreenImage.color = curColor;
        }
    }
    public void TakeDamage(int amount, EnemyAI attacker = null)
    {
        if (isDead) return;

        Debug.Log("Healthmanager: player terkena hit! Damage:" + amount);
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        targetAlpha = (1f - ((float) currentHealth / maxHealth)) * maxAlpha;

        if (currentHealth <= 0)
        {
            StartCoroutine(RespawnSequence(attacker));
        }
    }
    IEnumerator RespawnSequence(EnemyAI attacker)
    {
        // --- TAHAP 1: INISIASI KEMATIAN ---
        isDead = true;
    
        // 1. Matikan kontrol player segera agar tidak bisa kabur saat ditikam
        MovementPlayer moveScript = GetComponent<MovementPlayer>();
        if (moveScript != null) moveScript.isDead = true;
    
        // 2. Jatuhkan barang yang dibawa (Opsional)
        PlayerInteraction interactScript = GetComponent<PlayerInteraction>();
        if (interactScript != null) interactScript.DropEquipped();
        
        DragHandler dragScript = GetComponent<DragHandler>();
        if (dragScript != null) dragScript.DropItem();
    
        // 3. Jalankan Sinematik (Guncangan & Darah)
        if (stabCinematic != null) stabCinematic.TriggerStab();
    
        if (attacker != null)
        {
            // Player dipaksa melihat musuh yang menikam
            transform.LookAt(new Vector3(attacker.transform.position.x, transform.position.y, attacker.transform.position.z));
            
            // Musuh memutar animasi tikaman (Stab)
            attacker.TriggerKillAnimation();
        }
    
        // 4. Mulai gelapkan layar (Layar merah/hitam perlahan muncul)
        targetAlpha = maxAlpha;
    
        // --- TAHAP 2: MENUNGGU ANIMASI SELESAI ---
        // Kita beri waktu 3 detik agar player melihat musuh menikam dan kamera jatuh ke tanah
        yield return new WaitForSeconds(3.0f);

        // --- TAHAP 3: RESPAWN (PINDAH LOKASI) ---
        // Sekarang layar sudah hitam pekat, aman untuk pindah lokasi
        CheckpointManager.Instance.LoadCheckpoint();
    
        // Reset posisi kamera ke atas (kepala) agar tidak "ndlosor" di tanah saat hidup lagi
        if (stabCinematic != null) stabCinematic.ResetCamera();
    
        // Reset nyawa dan hilangkan layar merah
        currentHealth = maxHealth;
    
        // Buka kembali "gerbang" animasi musuh agar musuh bisa jalan lagi
        if (attacker != null) attacker.ResetKillAnimationState();
    
        // --- TAHAP 4: NORMALISASI ---
        // Aktifkan kembali kontrol player
        if (moveScript != null) moveScript.isDead = false;
        targetAlpha = 0;
    
        // Stun semua musuh sejenak agar player punya waktu bernapas setelah hidup lagi
        EnemyAI[] allEnemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (EnemyAI enemy in allEnemies)
        {
            enemy.ApplyStun(1.0f);
        }
    
        yield return new WaitForSeconds(0.5f);
        isDead = false;
    }
}
