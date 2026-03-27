
using UnityEngine;
using UnityEngine.UI;

public class CrankUI : MonoBehaviour
{
     [Header(" Referensi ")]
    [SerializeField] private CrankMinigame crankMinigame;

    [Header(" UI Elements ")]
    [SerializeField] private Image progressBarFill;
    [SerializeField] private RectTransform crankVisual;

    [Header(" Pengaturan ")]
    [Tooltip("Seberapa cepat visual engkol berputar")]
    [SerializeField] private float crankRotateSpeed = 200f;

    private float _currentRotation = 0f;

    void Update()
    {
        if (crankMinigame == null) return;
        if (!crankMinigame.IsActive) return;

        if (progressBarFill != null)
            progressBarFill.fillAmount = crankMinigame.Progress;

        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            float clockwise = (mouseX - mouseY);

            if (clockwise > 0)
            {
                _currentRotation -= crankRotateSpeed * Time.deltaTime;
                crankVisual.localRotation = Quaternion.Euler(0, 0, _currentRotation);
            }
        }
    }
        
    
}
