using UnityEngine;
using UnityEngine.UI;

public class DeathPanelBridge : MonoBehaviour
{
    [Header("Assign Tombol Secara Manual Di Sini")]
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button mainMenuButton;

    void Start()
    {
        if (tryAgainButton != null)
        {
            tryAgainButton.onClick.RemoveAllListeners();
            tryAgainButton.onClick.AddListener(() => StabSequence.Instance.TryAgain());
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(() => StabSequence.Instance.BackToMainMenu());
        }
        
        Debug.Log("<color=cyan>DeathPanelBridge: Tombol berhasil di-link ke StabSequence!</color>");
    }
}