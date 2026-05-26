using System.Collections;
using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RotateWheel : MonoBehaviour
{
    public static event Action<string, int> Rotated = delegate { };

    // Membuat pilihan dropdown di Inspector
    public enum RotationAxis { X, Y, Z }

    [Header("Rotation Settings")]
    [Tooltip("Pilih sumbu rotasi yang menjadi poros putaran roda")]
    [SerializeField] private RotationAxis chosenAxis = RotationAxis.Z;
    
    [Tooltip("Centang jika putaran roda terbalik")]
    [SerializeField] private bool reverseDirection = false;
    
    [Tooltip("Total sudut putaran untuk ganti 1 angka (360 derajat / 10 angka = 36)")]
    [SerializeField] private float totalRotationAngle = 36f;
    
    [Tooltip("Berapa frame animasi rotasi ini berjalan (makin besar makin mulus)")]
    [SerializeField] private int smoothnessSteps = 12; 

    [Header("Status")]
    public int numberShown = 0; 

    private bool coroutineAllowed = true;

    public void Interact()
    {
        if (coroutineAllowed)
        {
            StartCoroutine(RotateRoutine());
        }
    }

    private IEnumerator RotateRoutine()
    {
        coroutineAllowed = false;

        // Hitung sudut rotasi per frame kecil
        float stepAngle = totalRotationAngle / smoothnessSteps;
        if (reverseDirection) stepAngle = -stepAngle;

        // Tentukan Vector3 berdasarkan sumbu yang kamu pilih di Inspector
        Vector3 rotationVector = Vector3.zero;
        switch (chosenAxis)
        {
            case RotationAxis.X: rotationVector = new Vector3(stepAngle, 0, 0); break;
            case RotationAxis.Y: rotationVector = new Vector3(0, stepAngle, 0); break;
            case RotationAxis.Z: rotationVector = new Vector3(0, 0, stepAngle); break;
        }

        // Jalankan animasi rotasi
        for (int i = 0; i < smoothnessSteps; i++)
        {
            // Menggunakan Space.Self agar rotasi selalu mengikuti arah lokal roda itu sendiri
            transform.Rotate(rotationVector, Space.Self); 
            yield return new WaitForSeconds(0.01f);
        }

        coroutineAllowed = true;
        numberShown += 1;

        if (numberShown > 9)
        {
            numberShown = 0; // Reset ke angka 0 setelah melewati 9
        }

        // Beritahu SafeBox3D bahwa roda ini telah berputar
        Rotated(gameObject.name, numberShown);
    }
}