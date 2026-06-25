using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PengendaliBanyakLampu : MonoBehaviour
{
    [System.Serializable]
    public class DataLampu
    {
        public string namaLampu = "Lampu"; 
        public Renderer rendererLampu;   // Objek lamp.014 / lamp.015
        public GameObject objekSpot;     // Kolom Baru: Objek Spot.013 / Spot.014
        
        public Color warnaNyala = Color.white;
        public Color warnaMati = Color.black;
        
        [HideInInspector]
        public Material materialLampu;
    }

    [Header("Daftar Semua Lampu")]
    public List<DataLampu> daftarLampu = new List<DataLampu>();

    [Header("Pengaturan Jeda Kedip (Detik)")]
    public float jedaNyala = 0.3f;
    public float jedaMati = 0.3f;

    void Start()
    {
        foreach (DataLampu lampu in daftarLampu)
        {
            if (lampu.rendererLampu != null)
            {
                lampu.materialLampu = lampu.rendererLampu.material;
            }
        }

        StartCoroutine(LoopingKedapKedip());
    }

    private IEnumerator LoopingKedapKedip()
    {
        while (true) 
        {
            // 1. NYALAKAN SEMUA LAMPU & SPOT
            UbahStatusSemuaLampu(true);
            yield return new WaitForSeconds(jedaNyala);

            // 2. MATIKAN SEMUA LAMPU & SPOT
            UbahStatusSemuaLampu(false);
            yield return new WaitForSeconds(jedaMati);
        }
    }

    private void UbahStatusSemuaLampu(bool apakahNyala)
    {
        foreach (DataLampu lampu in daftarLampu)
        {
            // 1. Atur Warna & Emission Material Lampu
            if (lampu.materialLampu != null)
            {
                Color warnaTarget = apakahNyala ? lampu.warnaNyala : lampu.warnaMati;
                lampu.materialLampu.SetColor("_BaseColor", warnaTarget);
                lampu.materialLampu.SetColor("_EmissionColor", warnaTarget);
                
                DynamicGI.SetEmissive(lampu.rendererLampu, warnaTarget);
            }

            // 2. Atur Aktif/Matinya Game Object Spot (Ide Bagus Anda)
            if (lampu.objekSpot != null)
            {
                lampu.objekSpot.SetActive(apakahNyala);
            }
        }
    }
}