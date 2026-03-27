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
        isDead = true;

        MovementPlayer moveScript = GetComponent<MovementPlayer>();
        if (moveScript != null) moveScript.isDead = true;

        PlayerInteraction interactScript = GetComponent<PlayerInteraction>();
        if (interactScript != null) interactScript.DropEquipped();
        
        DragHandler dragScript = GetComponent<DragHandler>();
        if (dragScript != null) dragScript.DropItem();

        if (stabCinematic != null) stabCinematic.TriggerStab();
    
        if (attacker != null)
        {
            transform.LookAt(new Vector3(attacker.transform.position.x, transform.position.y, attacker.transform.position.z));
            attacker.TriggerKillAnimation();
        }

        targetAlpha = maxAlpha;

        yield return new WaitForSecondsRealtime(3.0f);

        CheckpointManager.Instance.LoadCheckpoint();

        currentHealth = maxHealth;

        if (attacker != null) attacker.ResetKillAnimationState();

        if (moveScript != null) moveScript.isDead = false;

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
