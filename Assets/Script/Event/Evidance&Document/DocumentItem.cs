using UnityEngine;
using TMPro;
 
/// <summary>
/// DocumentItem — Dipasang di GameObject kertas/dokumen.
/// Punya World Space Canvas yang "nempel" di kertas untuk tampilkan teks.
/// Canvas aktif saat inspeksi, nonaktif saat di scene.
/// </summary>
public class DocumentItem : MonoBehaviour
{
    [Header("== Data Dokumen ==")]
    [Tooltip("Judul dokumen")]
    public string documentTitle = "Surat";
 
    [Tooltip("Isi teks dokumen — ini yang muncul di kertas")]
    [TextArea(4, 10)]
    public string documentText = "Isi surat di sini...";
 
    [Tooltip("ID unik untuk tracking (opsional)")]
    public string documentID = "";
 
    [Header("== Referensi ==")]
    [Tooltip("Canvas World Space yang nempel di kertas")]
    public Canvas documentCanvas;
 
    [Tooltip("TextMeshPro untuk tampilkan isi teks di kertas")]
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
            documentTextTMP.text = documentText;

        if (documentTitleTMP != null)
            documentTitleTMP.text = documentTitle;
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
        EvidenceManager.Instance?.AddEvidence(documentTitle,documentText,documentID);
        Destroy(gameObject);
    }
}
