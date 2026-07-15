using UnityEngine;
using TMPro;

public class Drawer : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip DrawerOpenSFX;
    [SerializeField] private AudioClip DrawerCloseSFX;

    [Header("Settings")]
    [SerializeField] private bool canInteract = true;
    [SerializeField] private float uiDisplayDistance = 2.5f;

    [Header("UI System")]
    [SerializeField] private TextMeshProUGUI globalInteractText;
    [SerializeField] private string interactMessage = "Press [F] To Open/close Drawer";

    [Header("Settings")]
    [SerializeField] private Animator drawerAnimator;
    public bool _isOpen = false;

    private Transform _playerTransform;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _playerTransform = playerObj.transform;
    }

    void Update()
    {
        HandleUI();
    }

    public void Interaction()
    {
        if(!canInteract) return;

        _isOpen = !_isOpen;

        if (drawerAnimator != null)
            drawerAnimator.SetBool("isOpen", _isOpen);

        if (GameManager.Instance.audioManager != null)
        {
            AudioClip clip = _isOpen ? DrawerOpenSFX : DrawerCloseSFX;
            GameManager.Instance.audioManager.PlaySFX(clip);
        }
    }

    public void UnlockAndOpen()
    {
        canInteract = true;

        if (!_isOpen)
        {
            Interaction();
        }
    }

    private void HandleUI()
    {
        if (_playerTransform == null || globalInteractText == null) return;

        if (!canInteract)
        {
            if (globalInteractText.text == interactMessage)
                globalInteractText.text = "";
            return;
        }

        float distance = Vector3.Distance(transform.position, _playerTransform.position);

        if (distance <= uiDisplayDistance)
        {
            globalInteractText.text = interactMessage;
        }
        else if (globalInteractText.text == interactMessage)
        {
            globalInteractText.text = "";
        }
    }
}
