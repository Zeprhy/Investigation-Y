using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StabSequence : MonoBehaviour
{
    [Header("References")]
    public Image bloodUI;     // Gambar darah di Canvas
    public Camera mainCam;    // Drag Main Camera ke sini (bukan holder)

    [Header("Settings")]
    public float shakeDuration = 2.0f; // Durasi total tikaman
    public float shakeMagnitude = 0.2f;

    [Header("Fall Settings")]
    public float fallSpeed = 5.0f;
    public float fallRotateSpeed = 10.0f;

    private Vector3 originalCamPos;
    private Quaternion originalCamRot;

    void Awake()
    {
        // Simpan posisi asli kamera relatif terhadap Holder
        originalCamPos = mainCam.transform.localPosition;
        originalCamRot = mainCam.transform.localRotation;
    }

    public void TriggerStab()
    {
        StartCoroutine(PlayStabSequence());
    }

    private IEnumerator PlayStabSequence()
    {
        // 1. Panggil guncangan berat dari Manager temanmu
        if (CameraShakeManager.Instance != null)
        {
            CameraShakeManager.Instance.ShakeHeavy();
        }

        // Tunggu sebentar saat guncangan terjadi sebelum jatuh
        yield return new WaitForSeconds(0.5f);

        // 2. Fase Kamera Terjatuh (Tikaman selesai, player tumbang)
        yield return StartCoroutine(CameraFall());
    }

    private IEnumerator CameraFall()
    {
        // Target posisi di tanah (relatif terhadap holder)
        Vector3 targetPos = new Vector3(0.5f, -1.2f, 0); 
        // Target rotasi miring (seperti kepala yang rebah di lantai)
        Quaternion targetRot = Quaternion.Euler(10, 0, 70); 
    
        float elapsed = 0f;
        float duration = 1.0f; 
    
        while (elapsed < duration)
        {
            // PENTING: Gerakkan localPosition kamera, bukan holder
            mainCam.transform.localPosition = Vector3.Lerp(mainCam.transform.localPosition, targetPos, fallSpeed * Time.deltaTime);
            mainCam.transform.localRotation = Quaternion.Slerp(mainCam.transform.localRotation, targetRot, fallRotateSpeed * Time.deltaTime);
    
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    // Fungsi untuk mereset kamera ke posisi asli setelah respawn
    public void ResetCamera()
    {
        mainCam.transform.localPosition = originalCamPos;
        mainCam.transform.localRotation = originalCamRot;
        if (bloodUI != null) bloodUI.gameObject.SetActive(false);
    }
}