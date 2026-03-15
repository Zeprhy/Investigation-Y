using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class HealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public int currentHealth;
    private bool isDead = false;

    [Header("UI Element")]
    public Image BloodScreenImage;

    [Header("Blood Screen Settings")]
    [Range(0f, 1f)] public float minAlpha = 0f;
    [Range(0f, 1f)] public float maxAlpha = 0.8f;
    public float fadeSpeed = 2f;

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
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        Debug.Log("Healthmanager: player terkena hit! Damage:" + amount);
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        targetAlpha = (1f - ((float) currentHealth / maxHealth)) * maxAlpha;

        if (currentHealth <= 0)
        {
            StartCoroutine(RespawnSequence());
        }
    }
    IEnumerator RespawnSequence()
    {
        isDead = true;

        PlayerInteraction interactScript = GetComponent<PlayerInteraction>();
        if (interactScript != null) interactScript.DropEquipped();

        DragHandler dragScript = GetComponent<DragHandler>();
        if (dragScript != null) dragScript.DropItem();

        targetAlpha = maxAlpha;
        yield return new WaitForSeconds(1.0f);

        CheckpointManager.Instance.LoadCheckpoint();

        currentHealth = maxHealth;
        targetAlpha = 0;

        EnemyAI[] allEnemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (EnemyAI enemy in allEnemies)
        {
            enemy.ApplyStun(1.0f);
        }

        yield return new WaitForSeconds(0.5f);
        isDead = false;
    }
}
