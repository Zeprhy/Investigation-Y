using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : MonoBehaviour
{
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float currentStamina;
    [SerializeField] private float staminaDrain = 20f;
    [SerializeField] private float staminaRegen = 15f;
    [SerializeField] private float staminaRegenDelay = 2f;
    [SerializeField] private Image staminaBarFill;

    private float lastStaminaPercent = -1f;
    private float regenDelayTimer;
    public bool isExhausted { get; private set; }

    void Start()
    {
         currentStamina = maxStamina;
    }

    public void HandleStamina(bool isRunning, bool isMovingState, bool isCrouching)
    {
       

        if (isRunning && isMovingState && !isExhausted && !isCrouching)
        {
            currentStamina -= staminaDrain * Time.deltaTime;
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isExhausted = true;
                regenDelayTimer = staminaRegenDelay;
            }
        }
        else
        {
            if (regenDelayTimer > 0)
            {
                regenDelayTimer -= Time.deltaTime;
            }
            else
            {
                currentStamina += staminaRegen * Time.deltaTime;
            }

            if (isExhausted && currentStamina >= (maxStamina * 0.2f))
            {
                isExhausted = false;
            }
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }

    public void UpdateStaminaUI()
    {
        if (staminaBarFill != null)
        {
            float currentPercent = currentStamina / maxStamina;

            if (Mathf.Abs(lastStaminaPercent - currentPercent) > 0.001f)
            {
                staminaBarFill.fillAmount = currentPercent;
                lastStaminaPercent = currentPercent;
            }
        }
    }
}