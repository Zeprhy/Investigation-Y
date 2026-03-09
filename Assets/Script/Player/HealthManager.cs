using UnityEngine.UI;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public int currentHealth;

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
            Color curColor = BloodScreenImage.color;//Transisi ke targetalpha
            curColor.a = Mathf.MoveTowards(curColor.a, targetAlpha, fadeSpeed * Time.deltaTime);
            BloodScreenImage.color = curColor;
        }
    }
    public void TakeDamage(int amount)
    {
        Debug.Log("Healthmanager: player terkena hit! Damage:" + amount);
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        targetAlpha = (1f - ((float) currentHealth / maxHealth)) * maxAlpha;
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    void Die()
    {
        Debug.Log("Player Mati");
    }
}
