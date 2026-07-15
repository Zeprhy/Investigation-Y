using UnityEngine;

[CreateAssetMenu(fileName = "New Evidence Data", menuName = "Game Data/Evidence Data")]
public class EvidenceDataSO : ScriptableObject
{
    [Header("== Data Identitas ==")]
    public string itemID = "";
    public string itemName = "Nama Item";

    [Header("== Konten ==")]
    [Tooltip("Isi teks untuk dokumen, atau deskripsi untuk barang bukti.")]
    [TextArea(4, 10)]
    public string description = "Isi atau deskripsi di sini...";

    [Tooltip("Dialog khusus untuk barang bukti fisik (kosongkan jika dokumen)")]
    [TextArea(2, 4)]
    public string dialog = "";
}