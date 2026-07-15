using UnityEngine;
using TMPro;
public class InspectionUIManager : MonoBehaviour
{
    [Header(" UI ")]
    [Tooltip("UI hint saat melihat barang bukti")]
    [SerializeField] private TextMeshProUGUI inspectHintText;
 
    [Tooltip("UI nama barang bukti saat sedang diinspeksi")]
    [SerializeField] private TextMeshProUGUI evidenceNameText;
 
    [Tooltip("UI deskripsi saat sedang diinspeksi")]
    [SerializeField] private TextMeshProUGUI evidenceDescText;
 
    [Tooltip("Panel UI yang muncul saat inspeksi aktif")]
    [SerializeField] private GameObject inspectUIPanel;
    

    public void ShowHint(string text)
    {
        if (inspectHintText != null)
            inspectHintText.text = text;
    }

    public void HideHint()
    {
        if (inspectHintText != null)
            inspectHintText.text = "";
    }

    public void ShowInspectUI(string name, string desc)
    {
        if (inspectUIPanel != null)
        {
            inspectUIPanel.SetActive(true);
        }

        if (evidenceNameText != null)
            evidenceNameText.text = name;

        if (evidenceDescText != null)
            evidenceDescText.text = desc;
    }

    public void HideInspectUI()
    {
        if (inspectUIPanel != null)
            inspectUIPanel.SetActive(false);
    }
}