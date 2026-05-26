using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PuzzleSocket : MonoBehaviour
{
    [Header("== Syarat Item ==")]
    [Tooltip("Tipe Item (misal: Tool, Evidence, Key)")]
    public ItemType requiredItemType;
    [Tooltip("ID Unik barang (misal: 'Knife', 'Doll', 'Clock')")]
    public string requiredKeyID;

    [Header("== Visual Referensi (Seperti di Video) ==")]
    [Tooltip("Mesh barang transparan/warna biru atau merah (muncul saat player melihat tatakan ini)")]
    public GameObject hologramPreviewMesh;
    
    [Tooltip("Mesh wujud asli barang (muncul setelah berhasil diletakkan)")]
    public GameObject placedRealMesh;

    [Header("== Manager ==")]
    public ProjectorPuzzleManager puzzleManager;
    private Collider _Collider;
    private bool _isPreaviuwActive = false;

    public bool IsOccupied { get; private set; } = false;

    void Awake()
    {
        _Collider = GetComponent<Collider>();
    }

    void Start()
    {
        // Matikan kedua model visual di awal
        if (hologramPreviewMesh != null) hologramPreviewMesh.SetActive(false);
        if (placedRealMesh != null) placedRealMesh.SetActive(false);
    }

    // Fungsi untuk mengecek kecocokan barang di tangan Player dengan tatakan ini
    public bool IsCorrectItem(Item itemToTest)
    {
        if (itemToTest == null) return false;
        return itemToTest.itemType == requiredItemType && itemToTest.keyID == requiredKeyID;
    }

    // Fungsi memunculkan efek transparan (seperti di video)
    public void SetPreview(bool isVisible)
    {
        if (IsOccupied) return;

        if (_isPreaviuwActive == isVisible) return;

        _isPreaviuwActive = isVisible;
        if (hologramPreviewMesh != null) hologramPreviewMesh.SetActive(isVisible);
    }

    // Fungsi saat barang berhasil diletakkan (diklik Player)
    public void PutItemInSocket()
    {
        IsOccupied = true;
        
        // Matikan hologram, nyalakan objek asli
        if (hologramPreviewMesh != null) hologramPreviewMesh.SetActive(false);
        if (placedRealMesh != null) placedRealMesh.SetActive(true);

        if (_Collider != null)
        {
            _Collider.isTrigger = true;  
        }

        // Lapor ke Puzzle Manager bahwa item ini sudah terpasang
        if (puzzleManager != null)
        {
            puzzleManager.PlaceItem(requiredKeyID);
        }
    }
}