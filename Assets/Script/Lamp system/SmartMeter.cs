using UnityEngine;

public class SmartMeter : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private ApartmentEventManager eventManager;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip switchSound;

    [SerializeField] private Animator _animator;
    [SerializeField] private bool _powerIsRestored = false;
    [SerializeField] private bool _IsBoxOpen = false;

    private static readonly int IsOpenHash = Animator.StringToHash("isOpen");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    public void UseMeter()
    {
        if (!_IsBoxOpen)
        {
            ToggleBox(true);
        }

        else if (!_powerIsRestored)
        {
            PerformPowerRestoration();
        }

        else
        {
            ToggleBox(false);
        }
    }

    private void ToggleBox(bool open)
    {
        _IsBoxOpen = open;
        _animator.SetBool(IsOpenHash, _IsBoxOpen);
        
        AudioClip clipToPlay = open ? openSound : closeSound;

        if (GameManager.Instance.audioManager != null)
            GameManager.Instance.audioManager.PlaySFX(clipToPlay);
    }

    private void PerformPowerRestoration()
    {   
        if (PowerSystem.IsPowerOn) return;

        _powerIsRestored = true;

        if (GameManager.Instance.audioManager != null)
            GameManager.Instance.audioManager.PlaySFX(switchSound);

        PowerSystem.RestorePower();

        if (eventManager != null)
            eventManager.RestoreApartementLights();
    }
}
