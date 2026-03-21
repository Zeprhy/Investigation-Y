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
    public string description = "Deskripsi barang bukti...";

    [Tooltip("ID unik untuk tracking (opsional)")]
    public string evidenceID = "";

    [Header("== Inspeksi ==")]
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

    void Awake()
    {
        _rb  = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
    }

    public void StartInspect()
    {   
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
        _isBeingInspected = true;

        if (_rb != null)
        {
            _rb.useGravity  = false;
            _rb.isKinematic = true;
        }

        if (_col != null)
            _col.enabled = false;
    }

    public void StopInspect()
    {
        _isBeingInspected = false;
        if (_col != null)
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
        EvidenceManager.Instance?.AddEvidence(evidenceName, description, evidenceID);
        Destroy(gameObject);
    }
}