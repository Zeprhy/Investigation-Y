using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// EvidenceInspector — Handle sistem inspeksi barang bukti.
/// Attach di Player.
/// 
/// Alur:
/// 1. Player raycast ke EvidenceItem/DocumentItem → tekan F → StartInspect()
/// 2. Item melayang di inspectionPoint di depan kamera
/// 3. Player klik kiri / tekan tombol collect → CollectEvidence()
/// 4. Item destroy + EvidenceManager counter naik
/// </summary>
public class EvidenceInspector : MonoBehaviour
{
    [Header("== Referensi ==")]
    [SerializeField] private Camera playerCamera;
 
    [Tooltip("Transform titik inspeksi — buat GameObject kosong di depan kamera")]
    [SerializeField] private Transform inspectionPoint;
 
    [Tooltip("Script movement player untuk freeze saat inspeksi")]
    [SerializeField] private MovementPlayer movementPlayer;
 
    [Header("== Pengaturan ==")]
    [Tooltip("Jarak raycast untuk detect barang bukti")]
    [SerializeField] private float detectRange = 1f;
 
    [Tooltip("Seberapa cepat item bergerak ke inspection point")]
    [SerializeField] private float moveSpeed = 10f;
 
    [Header("== UI ==")]
    [Tooltip("UI hint saat melihat barang bukti")]
    [SerializeField] private TextMeshProUGUI inspectHintText;
 
    [Tooltip("UI nama barang bukti saat sedang diinspeksi")]
    [SerializeField] private TextMeshProUGUI evidenceNameText;
 
    [Tooltip("UI deskripsi saat sedang diinspeksi")]
    [SerializeField] private TextMeshProUGUI evidenceDescText;
 
    [Tooltip("Panel UI yang muncul saat inspeksi aktif")]
    [SerializeField] private GameObject inspectUIPanel;
 
    [Header("== Audio ==")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip collectSound;
 
    // ---- State ----
    private EvidenceItem _currentEvidence;
    private DocumentItem _currentDocument;
    private bool _isInspecting = false;
 
    // ---- Cache string ----
    private const string HINT_EVIDENCE  = "Press [F] To Inspect";
    private const string HINT_COLLECT   = "Press [F] To Collect  |  Press [E] To Put Back";
    private bool _isReturning = false;
    private Vector3 _returnTargetPos;
    private Quaternion _returnTargetRot;

    void Update()
    {
        if (PauseMenu.isPausedStatic) return;

        if (_isReturning)
        {
            HandleReturnMovement();
        }

        if (_isInspecting)
        {
            MoveItemToInspectionPoint();
        }
        else
        {
            HandleRaycast();
        }
    }

    private void HandleRaycast()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        
        if (Physics.Raycast(ray, out RaycastHit hit, detectRange))
        {
            SecurityComputer pcSecurity = hit.collider.GetComponentInParent<SecurityComputer>();
            if (pcSecurity != null)
            {
                if (LockdownManager.Instance != null && LockdownManager.Instance.IsLockdownActive)
                {
                    ShowHint("Press [F] To Acces Security PC");
                }
                else
                {
                    ShowHint("Security PC offline");
                }
            }

            EvidenceItem evidence = hit.collider.GetComponentInParent<EvidenceItem>();
            if (evidence != null)
            {
                ShowHint(HINT_EVIDENCE);
                return;
            }
            DocumentItem document = hit.collider.GetComponentInParent<DocumentItem>();
            if (document != null)
            {
                ShowHint(HINT_EVIDENCE);
                return;
            }
        }
        HideHint();
    }

    public bool TryHandleInteract()
    {
    //Debug.Log($"[TryHandleInteract] _isInspecting: {_isInspecting}");

    if (_isReturning) return false;

    if (_isInspecting)
    {
        //Debug.Log("[TryHandleInteract] → CollectCurrent");
        CollectCurrent();
        return true;
    }

    Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
    if (!Physics.Raycast(ray, out RaycastHit hit, detectRange)) return false;

    SecurityComputer pcSecurity = hit.collider.GetComponentInParent<SecurityComputer>();
    if (pcSecurity != null)
    {
        pcSecurity.InteractWithComputer();
        return true;
    }

    EvidenceItem evidence = hit.collider.GetComponentInParent<EvidenceItem>();
    if (evidence != null)
    {
        Debug.Log("[TryHandleInteract] → StartInspectEvidence");
        StartInspectEvidence(evidence);
        return true;
    }

    DocumentItem document = hit.collider.GetComponentInParent<DocumentItem>();
    if (document != null)
    {
        Debug.Log("[TryHandleInteract] → StartInspectDocument");
        StartInspectDocument(document);
        return true;
    }

    return false;
}

    public void OnPutBack(InputAction.CallbackContext context)
    {
        if (!context.performed || PauseMenu.isPausedStatic) return;

        if (!_isInspecting || _isReturning) return;

        PutBackCurrent();
    }

    private void StartInspectEvidence(EvidenceItem evidence)
    {
        _currentEvidence = evidence;
        _isInspecting = true;

        evidence.StartInspect();
        FreezePlayer(true);
        PlaySFX(pickupSound);

        ShowInspectUI(evidence.evidenceName, evidence.Description);
        ShowHint(HINT_COLLECT);

         Debug.Log($"[EvidenceInspector] Inspeksi: {evidence.evidenceName}");
    }

    private void StartInspectDocument(DocumentItem document)
    {
        _currentDocument = document;
        _isInspecting = true;

        document.StartInspect();
        FreezePlayer(true);
        PlaySFX(pickupSound);

        ShowInspectUI(document.documentTitle, "");
        ShowHint(HINT_COLLECT);

         Debug.Log($"[EvidenceInspector] Membaca: {document.documentTitle}");
    }

    private void CollectCurrent()
    {
        PlaySFX(collectSound);
        HideInspectUI();
        FreezePlayer(false);

        if(_currentEvidence != null)
        {
            _currentEvidence.CollectEvidence();
            _currentEvidence = null;
        }
        else if (_currentDocument != null)
        {
            _currentDocument.CollectDocument();
            _currentDocument = null;
        }

        _isInspecting = false;
        HideHint();
        
    }

    private void PutBackCurrent()
    {
        HideInspectUI();
        FreezePlayer(false);
        _isInspecting = false;
        HideHint();

        if (_currentEvidence != null)
        {
            if (!_currentEvidence.canBePickedUp)
            {
                _currentEvidence.StopInspect();
                _currentEvidence = null;
                _isReturning = false;
                return;    
            }

            _returnTargetPos = _currentEvidence.OriginalPosition;
            _returnTargetRot = _currentEvidence.OriginalRotation;
            _currentEvidence.StopInspect();
            _isReturning = true;
            
        }
        else if (_currentDocument != null)
        {
            _returnTargetPos = _currentDocument.OriginalPosition;
            _returnTargetRot = _currentDocument.OriginalRotation;
            _currentDocument.StopInspect();
            _isReturning = true;
        }
    }

    private void MoveItemToInspectionPoint()
    {
        Transform itemTransform = null;
        Quaternion targetRotation = Quaternion.identity;

        if (_currentEvidence != null)
        {
            if (!_currentEvidence.canBePickedUp) return;
            
            itemTransform = _currentEvidence.transform;
            targetRotation = inspectionPoint.rotation * Quaternion.Euler(_currentEvidence.inspectRotationOffset);
        }
        else if (_currentDocument != null)
        {
            itemTransform = _currentDocument.transform;
            targetRotation = inspectionPoint.rotation;
        }

        if (itemTransform == null) return;
        itemTransform.position = Vector3.Lerp(
            itemTransform.position,
            inspectionPoint.position,
            moveSpeed * Time.deltaTime
        );

        itemTransform.rotation = Quaternion.Slerp(
            itemTransform.rotation,
            targetRotation,
            moveSpeed * Time.deltaTime
        );
    }

    private void HandleReturnMovement()
    {
        Transform itemTransform = null;

        if (_currentEvidence != null)
            itemTransform = _currentEvidence.transform;

        if (_currentDocument != null)
            itemTransform = _currentDocument.transform;

        if (itemTransform == null)
        {
            _isReturning = false;
            return;
        }

        itemTransform.position = Vector3.Lerp(
            itemTransform.position,
            _returnTargetPos,
            moveSpeed * Time.deltaTime
        );

        itemTransform.rotation = Quaternion.Slerp(
            itemTransform.rotation,
            _returnTargetRot,
            moveSpeed * Time.deltaTime
        );

        float dist = Vector3.Distance(itemTransform.position, _returnTargetPos);
        if (dist < 0.05f)
        {
            itemTransform.position = _returnTargetPos;
            itemTransform.rotation = _returnTargetRot;

            if (_currentEvidence != null) 
                _currentEvidence.RestorePhysics();
            if (_currentDocument != null)
                _currentDocument.RestorePhysics();

            _isReturning = false;
            _currentEvidence = null;
            _currentDocument = null;


        Debug.Log("[EvidenceInspector] Item kembali ke posisi asal.");
        }
    }

    private void FreezePlayer(bool freeze)
    {
        if (movementPlayer != null)
            movementPlayer.SetminigameState(freeze);

        Cursor.lockState = freeze ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = freeze;
    }

    private void ShowHint(string text)
    {
        if (inspectHintText != null)
            inspectHintText.text = text;
    }

    private void HideHint()
    {
        if (inspectHintText != null)
            inspectHintText.text = "";
    }

    private void ShowInspectUI(string name, string desc)
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

    private void HideInspectUI()
    {
        if (inspectUIPanel != null)
            inspectUIPanel.SetActive(false);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(clip);
    }
}
