using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PuzzleSocket : ItemBase // Catatan: Pastikan memang sengaja menjadikan Socket ini sebagai ItemBase (bisa di-interact/diambil)
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
        if (hologramPreviewMesh != null) hologramPreviewMesh.SetActive(false);
        if (placedRealMesh != null) placedRealMesh.SetActive(false);
    }

    // --- PERUBAHAN DI SINI ---
    // Ubah tipe parameter 'Item' menjadi 'ItemBase'
    public bool IsCorrectItem(ItemBase itemToTest)
    {
        if (itemToTest == null) return false;
        
        // Pastikan variabel 'keyID' sudah Anda tambahkan di script ItemBase utama Anda
        return itemToTest.itemType == requiredItemType && itemToTest.KeyID == requiredKeyID;
    }
    // -------------------------

    public void SetPreview(bool isVisible)
    {
        if (IsOccupied) return;

        if (_isPreaviuwActive == isVisible) return;

        _isPreaviuwActive = isVisible;
        if (hologramPreviewMesh != null) hologramPreviewMesh.SetActive(isVisible);
    }

    public void PutItemInSocket()
    {
        IsOccupied = true;
        
        if (hologramPreviewMesh != null) hologramPreviewMesh.SetActive(false);
        if (placedRealMesh != null) placedRealMesh.SetActive(true);

        if (_Collider != null)
        {
            _Collider.isTrigger = true;  
        }

        if (puzzleManager != null)
        {
            puzzleManager.PlaceItem(requiredKeyID);
        }
    }
}