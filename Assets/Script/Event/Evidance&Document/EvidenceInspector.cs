using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Mono.Cecil.Cil;
using System.Xml.Serialization;

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
    public Camera playerCamera;
 
    [Tooltip("Transform titik inspeksi — buat GameObject kosong di depan kamera")]
    public Transform inspectionPoint;
 
    [Tooltip("Script movement player untuk freeze saat inspeksi")]
    public MovementPlayer movementPlayer;
 
    [Header("== Pengaturan ==")]
    [Tooltip("Jarak raycast untuk detect barang bukti")]
    public float detectRange = 3f;
 
    [Tooltip("Seberapa cepat item bergerak ke inspection point")]
    public float moveSpeed = 10f;
 
    [Header("== UI ==")]
    [Tooltip("UI hint saat melihat barang bukti")]
    public TextMeshProUGUI inspectHintText;
 
    [Tooltip("UI nama barang bukti saat sedang diinspeksi")]
    public TextMeshProUGUI evidenceNameText;
 
    [Tooltip("UI deskripsi saat sedang diinspeksi")]
    public TextMeshProUGUI evidenceDescText;
 
    [Tooltip("Panel UI yang muncul saat inspeksi aktif")]
    public GameObject inspectUIPanel;
 
    [Header("== Audio ==")]
    public AudioClip pickupSound;
    public AudioClip collectSound;
 
    // ---- State ----
    private EvidenceItem _currentEvidence;
    private DocumentItem _currentDocument;
    private bool _isInspecting = false;
 
    // ---- Cache string ----
    private const string HINT_EVIDENCE  = "Press [F] To Inspect";
    private const string HINT_COLLECT   = "Press [F] To Collect  |  Press [E] To Put Back";

    void Update()
    {
        if (PauseMenu.isPausedStatic) return;

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

    public void OnInspect(InputAction.CallbackContext context)
    {
        if (!context.performed || PauseMenu.isPausedStatic) return;

        if (_isInspecting)
        {
            CollectCurrent();
        }
    }

    public void OnPutBack(InputAction.CallbackContext context)
    {
        if (!context.performed || PauseMenu.isPausedStatic) return;
        if (!_isInspecting) return;

        PutBackCurrent();
    }

    private void TryStartInspect()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f , 0.5f , 0));
        if (!Physics.Raycast(ray, out RaycastHit hit, detectRange)) return;

        EvidenceItem evidence = hit.collider.GetComponentInParent<EvidenceItem>();
        if (evidence != null)
        {
            StartInspectEvidence(evidence);
            return;
        }

        DocumentItem document = hit.collider.GetComponentInParent<DocumentItem>();
        if (document != null)
        {
            StartInspectDocument(document);
        }
    }

    private void StartInspectEvidence(EvidenceItem evidence)
    {
        _currentEvidence = evidence;
        _isInspecting = true;

        evidence.StartInspect();
        FreezePlayer(true);
        PlaySFX(pickupSound);

        ShowInspectUI(evidence.evidenceName, evidence.description);
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
            _currentDocument = null;
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

        if (_currentEvidence != null)
        {
             _currentEvidence.StopInspect();
            _currentEvidence = null;
        }
        else if (_currentDocument != null)
        {
            _currentDocument.StopInspect();
            _currentDocument = null;
        }

        _isInspecting = false;
        HideHint();

        Debug.Log("[EvidenceInspector] Item dikembalikan.");
    }

    private void MoveItemToInspectionPoint()
    {
        Transform itemTransform = null;
        Vector3 targetRotation = Vector3.zero;

        if (_currentEvidence != null)
        {
            itemTransform = _currentEvidence.transform;
            targetRotation = _currentEvidence.inspectRotationOffset;
        }
        else if (_currentDocument != null)
        {
            itemTransform = _currentDocument.transform;
            targetRotation = new Vector3(0f, 180f, 0f);
        }

        if (itemTransform == null) return;
        itemTransform.position = Vector3.Lerp(
            itemTransform.position,
            inspectionPoint.position,
            moveSpeed * Time.deltaTime
        );

        itemTransform.rotation = Quaternion.Lerp(
            itemTransform.rotation,
            Quaternion.Euler(targetRotation),
            moveSpeed * Time.deltaTime
        );
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
