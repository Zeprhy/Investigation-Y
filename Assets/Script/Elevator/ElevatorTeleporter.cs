using UnityEngine;
using System.Collections;

public class ElevatorTeleporter : MonoBehaviour
{
    [Header("Waypoints (Daftar Lantai)")]
    public Transform[] floorPoints; // Masukkan waypoint Lantai 1, 2, 3 di sini

    [Header("References")]
    [SerializeField] private GameObject player;              
    [SerializeField] private CharacterController characterController; 
    [SerializeField] private CanvasGroup floorPanelUI; // Panel dengan tombol 1, 2, 3
    [SerializeField] private ElevatorSlidingDoor doorScript;

    [Header("Transition UI")]
    [SerializeField] private CanvasGroup blurPanelGroup;   
    [SerializeField] private float fadeTime = 0.5f;        
    [SerializeField] private float holdTime = 0.2f;        

    private bool _isTransitioning = false;

    private void Start()
    {
        if (floorPanelUI != null) CloseFloorUI();
    }

    // Dipanggil saat Player menekan F pada Panel di dalam lift
    public void OpenFloorUI()
    {
        floorPanelUI.alpha = 1;
        floorPanelUI.interactable = true;
        floorPanelUI.blocksRaycasts = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseFloorUI()
    {
        floorPanelUI.alpha = 0;
        floorPanelUI.interactable = false;
        floorPanelUI.blocksRaycasts = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Dipanggil oleh BUTTON di UI (On Click)
    public void SelectFloor(int floorIndex)
    {
        if (!_isTransitioning && floorIndex < floorPoints.Length)
        {
            CloseFloorUI();
            StartCoroutine(TeleportSequence(floorPoints[floorIndex]));
        }
    }

    private IEnumerator TeleportSequence(Transform target)
    {
        _isTransitioning = true;
    
        // TUTUP PINTU OTOMATIS sebelum pindah lantai
        if (doorScript != null && doorScript.isOpen)
        {
            doorScript.Interact(player); // Memanggil toggle agar menutup
        }

        // 1. FADE IN
        float elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            blurPanelGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeTime);
            yield return null;
        }

        yield return new WaitForSeconds(holdTime);

        // 2. TELEPORT (Logika tetap sama)
        if (characterController != null) characterController.enabled = false;
        player.transform.position = target.position;
        player.transform.rotation = target.rotation;
        
        yield return null; 
        if (characterController != null) characterController.enabled = true;

        yield return new WaitForSeconds(holdTime);

        // 3. FADE OUT
        elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            blurPanelGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
            yield return null;
        }

        _isTransitioning = false;
    }
}