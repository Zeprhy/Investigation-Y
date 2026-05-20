using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class EvidenceManager : MonoBehaviour
{
    public static EvidenceManager Instance;
 
    [Header("== UI Counter ==")]
    [Tooltip("Icon barang bukti di HUD")]
    [SerializeField] private GameObject evidenceIconUI;
 
    [Tooltip("Text counter jumlah barang bukti")]
    [SerializeField] private TextMeshProUGUI evidenceCountText;
 
    [Tooltip("Animasi saat barang bukti baru masuk (opsional)")]
    [SerializeField] private Animator counterAnimator;
 
    [Tooltip("Nama trigger animator saat evidence baru masuk")]
    [SerializeField] private string newEvidenceTrigger = "NewEvidence";
 
    // ---- Data ----
    private List<EvidenceData> _collectedEvidence = new List<EvidenceData>();
    public int EvidenceCount => _collectedEvidence.Count;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddEvidence(string name, string description, string id = "")
    {
        var data = new EvidenceData
        {
            evidenceName = name,
            description = description,
            evidenceID = id
        };

        _collectedEvidence.Add(data);
        UpdateUI();
        if (counterAnimator != null)
            counterAnimator.SetTrigger(newEvidenceTrigger);

        if (id == "BloodSlash")
        {
            LockdownManager.Instance.ActivateLockdown();
        }
    }
    public void UpdateUI()
    {
        if (evidenceCountText != null)
            evidenceCountText.text = _collectedEvidence.Count.ToString();

        if (evidenceIconUI != null)
            evidenceIconUI.SetActive(_collectedEvidence.Count > 0);
    }

    [System.Serializable]
    public class EvidenceData
    {
        public string evidenceName;
        public string description;
        public string evidenceID;
    }
}
