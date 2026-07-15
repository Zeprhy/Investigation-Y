using UnityEngine;
using TMPro;

public class InteractionUIManager : MonoBehaviour
{
    [Header("== Komponen UI ==")]
    [Tooltip("Teks di tengah layar untuk interaksi (misal: Press F to Open)")]
    [SerializeField] private TextMeshProUGUI interactText;

    
    public void Initialize()
    {
        HideText(); 
    }

    public void ShowText(string text)
    {
        if (interactText != null)
            interactText.text = text;
    }

    public void HideText()
    {
        if (interactText != null)
            interactText.text = "";
    }
}