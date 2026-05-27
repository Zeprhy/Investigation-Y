using UnityEngine;
using System.Collections;

public class TeleportLift : MonoBehaviour
{
    [Header("== Lift Settings ==")]
    [SerializeField] private bool isEnabled = true;
    public bool IsEnabeled
    {
        get => isEnabled;
        set => isEnabled = value;
    }
    
    [SerializeField] private Transform destination;
    [SerializeField] private string targetTag = "Player";
    
    [Tooltip("Waktu tunggu untuk teleport lift normal")]
    [SerializeField] private float travelDelay = 3.0f;

    [Header("== Normal Fade Settings ==")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeSpeed = 1.0f;

    [Header("== Audio Settings ==")]
    [SerializeField] private AudioClip elevatorSFX;
    [SerializeField] private AudioClip arrivalDing;

    [Header("== Crash Event Settings (Story) ==")]
    [Tooltip("Centang ini HANYA untuk lift yang akan jatuh")]
    [SerializeField] private bool isCrashEvent = false;
    [Tooltip("Waktu lift berjalan normal sebelum mulai bergetar (detik)")]
    [SerializeField] private float waitBeforeCrash = 7.0f;
    [Tooltip("Durasi getaran sebelum lift benar-benar jatuh (detik)")]
    [SerializeField] private float shakingDuration = 3.0f;
    
    [SerializeField] private AudioClip rumbleSFX; // Suara mesin rusak/bergetar
    [SerializeField] private AudioClip crashSFX;  // Suara lift putus/jatuh

    private bool _isTeleporting = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag) && isEnabled && !_isTeleporting)
        {
            if (isCrashEvent)
            {
                // Jalankan event kecelakaan
                StartCoroutine(CrashSequence(other.gameObject));
            }
            else
            {
                // Jalankan teleport normal
                StartCoroutine(TeleportSequence(other.gameObject));
            }
        }
        else if (other.CompareTag(targetTag) && !isEnabled)
        {
            Debug.Log("The lift is currently disabled or locked.");
        }
    }

    public void SetLiftState(bool state)
    {
        isEnabled = state;
    }

    // ==========================================
    // 1. SEQUENCE NORMAL
    // ==========================================
    private IEnumerator TeleportSequence(GameObject player)
    {
        _isTeleporting = true;
        CharacterController controller = player.GetComponent<CharacterController>();

        if (AudioManager.Instance != null && elevatorSFX != null)
            AudioManager.Instance.PlaySFX(elevatorSFX);

        yield return StartCoroutine(Fade(1f));
        yield return new WaitForSeconds(travelDelay);

        if (controller != null) controller.enabled = false;
        player.transform.position = destination.position;
        player.transform.rotation = destination.rotation;
        if (controller != null) controller.enabled = true;

        if (AudioManager.Instance != null && arrivalDing != null)
            AudioManager.Instance.PlaySFX(arrivalDing);

        yield return StartCoroutine(Fade(0f));
        _isTeleporting = false;
    }

    // ==========================================
    // 2. SEQUENCE CRASH (JATUH PINGSAN)
    // ==========================================
    private IEnumerator CrashSequence(GameObject player)
    {
        _isTeleporting = true;
        CharacterController controller = player.GetComponent<CharacterController>();

        // (Opsional) Kamu bisa mematikan script movement player di sini agar mereka
        // tidak bisa keluar dari lift saat pintu lift menutup / lift berjalan.
        
        // Fase 1: Lift berjalan normal selama beberapa detik
        if (AudioManager.Instance != null && elevatorSFX != null)
            AudioManager.Instance.PlaySFX(elevatorSFX);

        yield return new WaitForSeconds(waitBeforeCrash);

        // Fase 2: Mesin bermasalah, lift mulai bergetar!
        if (AudioManager.Instance != null && rumbleSFX != null)
            AudioManager.Instance.PlaySFX(rumbleSFX);

        float elapsed = 0;
        while(elapsed < shakingDuration)
        {
            // Getaran kecil yang konstan menggunakan CameraShakeManager yang sudah kamu punya
            CameraShakeManager.Instance.ShakeCustom(0.2f, 0.15f);
            
            elapsed += 0.2f;
            yield return new WaitForSeconds(0.2f);
        }

        // Fase 3: CRASH! Tali putus dan jatuh
        if (AudioManager.Instance != null && crashSFX != null)
            AudioManager.Instance.PlaySFX(crashSFX);
        
        CameraShakeManager.Instance.ShakeImpact(); // Getaran super keras!

        // Fase 4: Pingsan seketika (Blackout)
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f; // Tiba-tiba gelap 100% tanpa animasi memudar
        }

        yield return new WaitForSeconds(0.5f); // Jeda sedikit saat layar hitam

        // Fase 5: Pindahkan player ke lokasi jatuhnya (misal lantai bawah)
        if (controller != null) controller.enabled = false;
        player.transform.position = destination.position;
        player.transform.rotation = destination.rotation;
        if (controller != null) controller.enabled = true;

        // CATATAN PENTING:
        // Kita TIDAK mengembalikan fadeCanvasGroup.alpha ke 0 di sini.
        // Layar akan tetap hitam pekat karena player sedang pingsan.
        // Kamu perlu script lain (misal: WakeUpSequence) di lantai bawah untuk membangunkan player.

        // Matikan trigger ini agar tidak terpicu lagi
        gameObject.SetActive(false); 
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeCanvasGroup == null) yield break;

        while (!Mathf.Approximately(fadeCanvasGroup.alpha, targetAlpha))
        {
            fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            yield return null;
        }
    }
}