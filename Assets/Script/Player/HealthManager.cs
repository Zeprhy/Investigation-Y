using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class HealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public int currentHealth;
    public bool isDead = false;

    [Header("Healing Settings")]
    public float healDelay = 180f; 
    private float currentHealTimer;

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
        ResetBloodUI();
    }

    void Start()
    {
        SetupReferences();
    }

    void Update()
    {
        // 1. Logika Visual Darah
        HandleBloodUI();

        // 2. Logika Healing Otomatis
        HandleAutoHealing();
    }

    private void SetupReferences()
    {
        // Gunakan Instance, jauh lebih cepat dan akurat
        stabCinematic = StabSequence.Instance;

        // Cari BloodScreenImage via Tag PlayerUI
        if (BloodScreenImage == null)
        {
            GameObject uiRoot = GameObject.FindWithTag("PlayerUI");
            if (uiRoot != null)
            {
                Image[] allImages = uiRoot.GetComponentsInChildren<Image>(true);
                foreach (Image img in allImages)
                {
                    if (img.gameObject.name == "BloodUI")
                    {
                        BloodScreenImage = img;
                        break;
                    }
                }
            }
        }

        ResetBloodUI();
    }

    private void HandleBloodUI()
    {
        if (BloodScreenImage != null)
        {
            Color curColor = BloodScreenImage.color;
            curColor.a = Mathf.MoveTowards(curColor.a, targetAlpha, fadeSpeed * Time.deltaTime);
            BloodScreenImage.color = curColor;
        }
    }

    private void HandleAutoHealing()
    {
        // Healing hanya berjalan jika HP tidak penuh dan player tidak sedang mati
        if (currentHealth < maxHealth && !isDead)
        {
            currentHealTimer += Time.deltaTime;

            if (currentHealTimer >= healDelay)
            {
                PerformHeal();
            }
        }
    }

    private void PerformHeal()
    {
        currentHealth = maxHealth;
        targetAlpha = 0f; // Hilangkan layar merah
        currentHealTimer = 0f; // Reset timer
        Debug.Log("<color=cyan>Player Healed!</color> Health kembali penuh.");
    }

    public void TakeDamage(int amount, EnemyAI attacker = null)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        // Reset timer healing setiap kali terkena hit (biar adil)
        currentHealTimer = 0f;

        targetAlpha = (1f - ((float)currentHealth / maxHealth)) * maxAlpha;

        if (currentHealth <= 0)
        {
            StartCoroutine(RespawnSequence(attacker));
        }
    }

    private void ResetBloodUI()
    {
        if (BloodScreenImage != null)
        {
            Color c = BloodScreenImage.color;
            c.a = 0;
            BloodScreenImage.color = c;
        }
    }

    IEnumerator RespawnSequence(EnemyAI attacker)
    {
        isDead = true;
        currentHealTimer = 0f; // Reset timer saat mati

        // Matikan kontrol & jalankan cinematic
        MovementPlayer moveScript = GetComponent<MovementPlayer>();
        if (moveScript != null) moveScript.isDead = true;

        PlayerInteraction interactScript = GetComponent<PlayerInteraction>();
        if (interactScript != null) interactScript.DropEquipped();
        
        if (stabCinematic == null) SetupReferences();
        if (stabCinematic != null) stabCinematic.TriggerStab();
    
        if (attacker != null)
        {
            transform.LookAt(new Vector3(attacker.transform.position.x, transform.position.y, attacker.transform.position.z));
            attacker.TriggerKillAnimation();
        }

        targetAlpha = maxAlpha;
        yield break;
    }
}