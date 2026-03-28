using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StabSequence : MonoBehaviour
{
    [Header("References")]
    public Image bloodUI;
    public Camera mainCam;
    public CanvasGroup deathPanelGroup;
    public GameObject gameplayHUD;

    [Header("Settings")]
    [SerializeField] private float bloodFadeSpeed = 0.5f;
    [Range(0, 1)] [SerializeField] private float maxBloodAlpha = 1.0f;
    public float fallSpeed = 5.0f;
    public float fallRotateSpeed = 10.0f;
    public float panelFadeSpeed = 1.0f;
    public static StabSequence Instance { get; private set; }

    private Vector3 originalCamPos;
    private Quaternion originalCamRot;
    private bool _isDead = false;

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (mainCam != null)
        {
            originalCamPos = mainCam.transform.localPosition;
            originalCamRot = mainCam.transform.localRotation;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        _isDead = false;
        StartCoroutine(DeferredSceneSetup());
    }

    private IEnumerator DeferredSceneSetup()
    {
        yield return null; 

        mainCam = Camera.main;
        GameObject uiRoot = GameObject.FindWithTag("PlayerUI");

        if (uiRoot != null)
        {
            Image[] allImages = uiRoot.GetComponentsInChildren<Image>(true);
            foreach (Image img in allImages)
            {
                if (img.gameObject.name == "BloodUI")
                {
                    bloodUI = img;
                    break; 
                }
            }

            CanvasGroup[] allGroups = uiRoot.GetComponentsInChildren<CanvasGroup>(true);
            foreach (CanvasGroup group in allGroups)
            {
                if (group.gameObject.name == "DeathPanel")
                {
                    deathPanelGroup = group;
                    break; 
                }
            }
        }

        if (mainCam != null)
        {
            originalCamPos = mainCam.transform.localPosition;
            originalCamRot = mainCam.transform.localRotation;
        }

        ResetUI();
    }

    public void ResetUI()
    {
        if (gameplayHUD != null) gameplayHUD.SetActive(true);
        
        if (deathPanelGroup != null) 
        {
            deathPanelGroup.alpha = 0f;
            deathPanelGroup.interactable = false;
            deathPanelGroup.blocksRaycasts = false;
        }

        if (bloodUI != null)
        {
            Color c = bloodUI.color;
            c.a = 0f;
            bloodUI.color = c;
            bloodUI.gameObject.SetActive(false);
        }
    }

    public void TriggerStab()
    {
        if (_isDead) return;
        _isDead = true;
        StartCoroutine(PlayStabSequence());
    }

    private IEnumerator PlayStabSequence()
    {
        // 1. Shake & Darah
        if (CameraShakeManager.Instance != null)
            CameraShakeManager.Instance.ShakeHeavy();

        if (bloodUI != null)
        {
            bloodUI.gameObject.SetActive(true);
            StartCoroutine(FadeInBlood());
        }

        yield return new WaitForSeconds(0.5f);

        // 2. Kamera jatuh
        yield return StartCoroutine(CameraFall());

        // 3. Panel muncul perlahan
        yield return StartCoroutine(FadeInDeathPanel());
    }

    private IEnumerator FadeInDeathPanel()
    {
        if (deathPanelGroup == null)
        {
            Debug.LogError("FadeInDeathPanel: deathPanelGroup NULL!");
            yield break;
        }

        if (gameplayHUD != null) 
        {
            gameplayHUD.SetActive(false);
        }
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.unscaledDeltaTime * panelFadeSpeed;
            deathPanelGroup.alpha = Mathf.Clamp01(alpha);
            yield return null;
        }

        deathPanelGroup.interactable = true;
        deathPanelGroup.blocksRaycasts = true;

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private IEnumerator FadeInBlood()
    {
        float currentAlpha = 0f;
        Color c = bloodUI.color;
        while (currentAlpha < maxBloodAlpha)
        {
            currentAlpha += Time.deltaTime * bloodFadeSpeed;
            c.a = Mathf.Clamp01(currentAlpha);
            bloodUI.color = c;
            yield return null;
        }
    }

    private IEnumerator CameraFall()
    {
        if (mainCam == null) yield break;

        Vector3 targetPos = new Vector3(0.5f, -1.2f, 0);
        Quaternion targetRot = Quaternion.Euler(10, 0, 70);

        float elapsed = 0f;
        float duration = 2.0f;

        while (elapsed < duration)
        {
            mainCam.transform.localPosition = Vector3.Lerp(
                mainCam.transform.localPosition, targetPos, fallSpeed * Time.deltaTime);
            mainCam.transform.localRotation = Quaternion.Slerp(
                mainCam.transform.localRotation, targetRot, fallRotateSpeed * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    public void TryAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}