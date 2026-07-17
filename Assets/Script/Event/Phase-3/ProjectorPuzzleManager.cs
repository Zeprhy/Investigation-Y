using UnityEngine;
using System.Collections;
using System;

public class ProjectorPuzzleManager : MonoBehaviour
{
    [Header("== Puzzle Status ==")]
    public bool isKnifePlaced = false;
    public bool isDollPlaced = false;
    public bool isClockPlaced = false;
    private bool _isPuzzleSolved = false;

    [Header("== Projector Settings ==")]
    [Tooltip("Quad/Layar tempat proyektor menembakkan gambar")]
    [SerializeField] private MeshRenderer projectorScreenRenderer;
    [Tooltip("Material putih polos (Awal)")]
    [SerializeField] private Material blankLightMaterial;
    [Tooltip("Material berisi angka 1985 (Setelah Solve)")]
    [SerializeField] private Material code1985Material;

    [Header("== Drawer & Evidence Settings ==")]
    [SerializeField] private Drawer ProjectorDrawer;
    
    [Tooltip("GameObject Dokumen Psychopath di dalam laci")]
    [SerializeField] private GameObject psychopathEvidence;

    [Header("== Debris Event Settings ==")]
    [Tooltip("Masukkan objek Trigger Debris yang ada di lorong lift")]
    [SerializeField] private DebrisEventTrigger elevatorDebrisTrigger; 

    // KODE BARU: Referensi ke pintu maintenance
    [Header("== Phase 4 Transition ==")]
    [Tooltip("Masukkan script CrankMinigame dari Pintu Maintenance")]
    [SerializeField] private CrankHandle_MiniGame maintenanceDoorCrank;

    [Header(" Objective Manager ")]
    [SerializeField] ObjectiveManager objectiveManager;

    [Header (" Audio ")]
    [SerializeField] private AudioClip projectorChangeSFX;

    void Start()
    {
        if (projectorScreenRenderer != null)
        {
            projectorScreenRenderer.material = blankLightMaterial;
        }

        if (psychopathEvidence != null)
        {
            psychopathEvidence.SetActive(false);
        }
    }

    public void PlaceItem(string itemName)
    {
        if (_isPuzzleSolved) return;

        switch (itemName)
        {
            case "Knife": isKnifePlaced = true; break;
            case "Doll": isDollPlaced = true; break;
            case "Clock": isClockPlaced = true; break;
        }

        CheckPuzzleCompletion();
    }

    private void CheckPuzzleCompletion()
    {
        if (isKnifePlaced && isDollPlaced && isClockPlaced && !_isPuzzleSolved)
        {
            _isPuzzleSolved = true;
            StartCoroutine(SolvePuzzleRoutine());
        }
    }

    private IEnumerator SolvePuzzleRoutine()
    {
        // 1. Ubah visual Proyektor menjadi angka "1985"
        if (GameManager.Instance.audioManager != null && projectorChangeSFX != null)
            GameManager.Instance.audioManager.PlaySFX(projectorChangeSFX);
            
        if (projectorScreenRenderer != null)
            projectorScreenRenderer.material = code1985Material;

        // Beri jeda dramatis sedikit
        yield return new WaitForSeconds(1.5f);

        // 2. Mainkan suara laci
        if (ProjectorDrawer != null)
            ProjectorDrawer.UnlockAndOpen();

        // 3. Munculkan Dokumen Psychopath agar bisa diambil player
        if (psychopathEvidence != null)
            psychopathEvidence.SetActive(true);

        // 4. Buka gembok trigger reruntuhan di lorong lift
        if (elevatorDebrisTrigger != null)
            elevatorDebrisTrigger.ActivateEventReady();

        CompletePuzzleObjective();
    }

    private void CompletePuzzleObjective()
    {
        if (objectiveManager != null)
        {
            objectiveManager.ForceCompleteCurrentObjective();
        }
    }
}