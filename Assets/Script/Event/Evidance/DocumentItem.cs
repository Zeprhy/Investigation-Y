using UnityEngine;
using TMPro;
 
/// <summary>
/// DocumentItem — Dipasang di GameObject kertas/dokumen.
/// Punya World Space Canvas yang "nempel" di kertas untuk tampilkan teks.
/// Canvas aktif saat inspeksi, nonaktif saat di scene.
/// </summary>
public class DocumentItem : ItemBase
{
    [Header(" Data Dokumen ")]
    public EvidenceDataSO evidenceDataSO;
    public string DocumentName => evidenceDataSO != null ? evidenceDataSO.itemName : "Barang Bukti";
    public string Description => evidenceDataSO != null ? evidenceDataSO.description : ""; 
    public string DocumentID => evidenceDataSO != null ? evidenceDataSO.itemID : "";

 
    [Header(" Referensi ")]
    public Canvas documentCanvas;
    public TextMeshProUGUI documentTextTMP;
    [SerializeField] [Tooltip("TextMeshPro untuk tampilkan judul dokumen (opsional)")]
    public TextMeshProUGUI documentTitleTMP;
 
    // ---- Cache ----
    private Rigidbody _rb;
    private Collider _col;
    private bool _isBeingInspected = false;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;

    public Vector3 OriginalPosition => _originalPosition;
    public Quaternion OriginalRotation => _originalRotation; 
 
    public bool IsBeingInspected => _isBeingInspected;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();

        if (documentCanvas != null)
            documentCanvas.gameObject.SetActive(false);
    }

    void Start()
    {
        if (documentTextTMP != null)
            documentTextTMP.text = Description;

        if (documentTitleTMP != null)
            documentTitleTMP.text = DocumentName;
    }

    public void StartInspect()
    {
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
        _isBeingInspected = true;

        if (_rb != null)
        {
            _rb.useGravity = false;
            _rb.isKinematic = true;
        }

        if (_col != null)
        {
            _col.enabled = false;
        }

        if (documentCanvas != null)
            documentCanvas.gameObject.SetActive(true);
    }

    public void StopInspect()
    {
        _isBeingInspected = false;

        if (_col != null)
        {
            _col.enabled = false;
        }

        if (documentCanvas != null)
            documentCanvas.gameObject.SetActive(false);
    }

    public void RestorePhysics()
    {
         if (_rb != null)
        {
            _rb.useGravity  = false;
            _rb.isKinematic = true;
        }

        if (_col != null)
            _col.enabled = true;
    }
    public void CollectDocument()
    {
        GameManager.Instance.evidenceManager.AddEvidence(DocumentName, Description, DocumentID);
        Destroy(gameObject);
    }
}
