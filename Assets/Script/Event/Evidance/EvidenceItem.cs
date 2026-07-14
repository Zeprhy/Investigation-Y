using UnityEngine;

/// <summary>
/// EvidenceItem — Dipasang di GameObject barang bukti fisik (objek 3D).
/// Berbeda dari item biasa — tidak masuk handPoint tapi ke inspectionPoint.
/// Setelah inspeksi selesai, barang disimpan ke EvidenceManager dan di-destroy.
/// </summary>
public class EvidenceItem : MonoBehaviour
{
    [Header("== Data Barang Bukti ==")]
    [Tooltip("Nama barang bukti yang muncul di UI")]
    public string evidenceName = "Barang Bukti";

    [Tooltip("Deskripsi singkat yang muncul saat inspeksi")]
    [TextArea(2, 4)] 
    [SerializeField] private string description = "Deskripsi barang bukti...";

    [Tooltip("Dialog yang muncul saat barang bukti diperiksa")]
    [TextArea(2, 4)] 
    [SerializeField] private string dialog = "DescriptionDialog...";

    [Tooltip("ID unik untuk tracking (opsional)")]
    [SerializeField] private string evidenceID = "";

    [Header("== Inspeksi ==")]
    [Tooltip("Apakah barang ini bisa dipegang/melayang ke kamera? (Matikan untuk bercak darah di dinding)")]
    public bool canBePickedUp = true;

    [Tooltip("Offset posisi saat dipegang di inspection point (local space)")]
    public Vector3 inspectPositionOffset = Vector3.zero;

    [Tooltip("Rotasi saat dipegang di inspection point")]
    public Vector3 inspectRotationOffset = Vector3.zero;

    // ---- Cache ----
    private Rigidbody _rb;
    private Collider _col;
    private bool _isBeingInspected = false;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;

    public Vector3 OriginalPosition => _originalPosition;
    public Quaternion OriginalRotation => _originalRotation; 

    public bool IsBeingInspected => _isBeingInspected;

    public string Description => description;
    public string Dialog => dialog;

    void Awake()
    {
        _rb  = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
    }

    public void StartInspect()
    {   
        _isBeingInspected = true;

        if (canBePickedUp)
        {
            _originalPosition = transform.position;
            _originalRotation = transform.rotation;

            if (_rb != null)
            {
                _rb.useGravity  = false;
                _rb.isKinematic = true;
            }

            if (_col != null)
                _col.enabled = false;
        }

    }

    public void StopInspect()
    {
        _isBeingInspected = false;

        if (canBePickedUp && _col != null)
            _col.enabled = false;
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

    public void CollectEvidence()
    {
        GameManager.Instance.evidenceManager.AddEvidence(evidenceName, description, evidenceID);

        if (canBePickedUp)
        {
            Destroy(gameObject);
        }
        else
        {
            if (_col != null)
            {
                _col.enabled = false;
            }

            this.enabled = false;
            
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.ShowDialogue(dialog);
            }

        }
    }
}