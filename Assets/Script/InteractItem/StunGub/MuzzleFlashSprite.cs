using UnityEngine;
public class MuzzleFlashSprite : MonoBehaviour
{
    [Header("Sprite Sheet Settings")]
    [SerializeField] private int columns = 4;
    [SerializeField] private int rows    = 2;
    [SerializeField] private int fps     = 24;

    private Renderer quad;
    private Material mat;
    private Camera   mainCam;

    private int   totalFrames;
    private float frameTimer;
    private int   currentFrame;
    private bool  isPlaying;

    private void Awake()
    {
        quad      = GetComponent<Renderer>();
        mat       = quad.material;          // instance material (tidak pengaruh asset asli)
        mainCam   = Camera.main;
        totalFrames = columns * rows;

        
        mat.mainTextureScale = new Vector2(1f / columns, 1f / rows);

        // Sembunyikan di awal
        quad.enabled = false;
    }

    private void Update()
    {
        // ── Billboard: selalu hadap kamera ──
        transform.LookAt(transform.position + mainCam.transform.forward);

        if (!isPlaying) return;

        frameTimer += Time.deltaTime;
        float frameDuration = 1f / fps;

        if (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            currentFrame++;

            if (currentFrame >= totalFrames)
            {
                // Animasi selesai
                quad.enabled = false;
                isPlaying = false;
                return;
            }

            ApplyFrame(currentFrame);
        }
    }

    public void Play()
    {
        currentFrame = 0;
        frameTimer   = 0f;
        isPlaying    = true;
        quad.enabled = true;
        ApplyFrame(0);
    }

    private void ApplyFrame(int index)
    {
        int col = index % columns;
        int row = index / columns;

        // UV: flip row karena Unity UV dimulai dari bawah
        int flippedRow = (rows - 1) - row;

        float offsetX = col       * (1f / columns);
        float offsetY = flippedRow * (1f / rows);

        mat.mainTextureOffset = new Vector2(offsetX, offsetY);
    }
}