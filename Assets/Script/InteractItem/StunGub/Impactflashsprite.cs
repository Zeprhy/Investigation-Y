using UnityEngine;

/// <summary>
/// Efek flash saat peluru stun gun kena enemy.
/// Sama seperti MuzzleFlashSprite tapi di-spawn di titik impact.
/// Attach ke Quad child, posisinya diset dari StunGun.cs saat hit.
/// </summary>
public class ImpactFlashSprite : MonoBehaviour
{
    [Header("Sprite Sheet Settings")]
    [SerializeField] private int columns = 4;
    [SerializeField] private int rows    = 2;
    [SerializeField] private int fps     = 30;              // sedikit lebih cepat dari muzzle

    [Header("Scale")]
    [SerializeField] private float flashScale = 0.5f;       // lebih besar dari muzzleflash

    private Renderer quad;
    private Material  mat;
    private Camera    mainCam;

    private int   totalFrames;
    private float frameTimer;
    private int   currentFrame;
    private bool  isPlaying;

    private void Awake()
    {
        quad        = GetComponent<Renderer>();
        mat         = quad.material;
        mainCam     = Camera.main;
        totalFrames = columns * rows;

        mat.mainTextureScale = new Vector2(1f / columns, 1f / rows);
        transform.localScale = Vector3.one * flashScale;

        quad.enabled = false;
    }

    private void Update()
    {
        // Billboard — selalu hadap kamera
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
                quad.enabled = false;
                isPlaying    = false;
                return;
            }

            ApplyFrame(currentFrame);
        }
    }

    /// <summary>
    /// Panggil dari StunGun.cs saat raycast kena enemy.
    /// Posisi di-set dari luar sebelum Play() dipanggil.
    /// </summary>
    public void PlayAt(Vector3 worldPosition)
    {
        transform.position = worldPosition;
        currentFrame = 0;
        frameTimer   = 0f;
        isPlaying    = true;
        quad.enabled = true;
        ApplyFrame(0);
    }

    private void ApplyFrame(int index)
    {
        int col        = index % columns;
        int row        = index / columns;
        int flippedRow = (rows - 1) - row;

        float offsetX = col        * (1f / columns);
        float offsetY = flippedRow * (1f / rows);

        mat.mainTextureOffset = new Vector2(offsetX, offsetY);
    }
}