using UnityEngine;

public class SafeBox3D : MonoBehaviour
{
    [Header("== Lock Status ==")]
    private int[] currentResult;
    
    [Tooltip("Kode 4 digit (1, 9, 8, 5)")]
    public int[] correctCombination = { 1, 9, 8, 5 };
    public bool isUnlocked = false;

    [Header("== Drawer System Integration ==")]
    [Tooltip("Masukkan komponen Drawer dari pintu brankas ini")]
    public Drawer safeDoorDrawer;
    
    [Header("== Rewards ==")]
    public GameObject stunGunReward;
    public GameObject photoEvidence;

    private void Start()
    {
        // Angka default saat game dimulai
        currentResult = new int[] { 0, 0, 0, 0 };
        
        // Berlangganan event putaran roda
        RotateWheel.Rotated += CheckResults;
        
        if (stunGunReward != null) stunGunReward.SetActive(false);
        if (photoEvidence != null) photoEvidence.SetActive(false);
    }

    private void CheckResults(string wheelName, int number)
    {
        if (isUnlocked) return;

        // Simpan angka berdasarkan roda mana yang diputar
        switch (wheelName)
        {
            case "wheel1": currentResult[0] = number; break;
            case "wheel2": currentResult[1] = number; break;
            case "wheel3": currentResult[2] = number; break;
            case "wheel4": currentResult[3] = number; break; 
        }

        // Cek jika kombinasi sudah benar
        if (currentResult[0] == correctCombination[0] &&
            currentResult[1] == correctCombination[1] &&
            currentResult[2] == correctCombination[2] &&
            currentResult[3] == correctCombination[3])
        {
            UnlockSafe();
        }
    }

    private void UnlockSafe()
    {
        isUnlocked = true;
        Debug.Log("KODE BENAR! BRANKAS TERBUKA!");

        // Panggil sistem laci/drawer untuk membuka pintu brankas!
        if (safeDoorDrawer != null)
        {
            safeDoorDrawer.UnlockAndOpen();
        }

        // Munculkan item hadiah
        if (stunGunReward != null) stunGunReward.SetActive(true);
        if (photoEvidence != null) photoEvidence.SetActive(true);
    }

    private void OnDestroy()
    {
        // Cegah memory leak saat pindah level
        RotateWheel.Rotated -= CheckResults;
    }
}