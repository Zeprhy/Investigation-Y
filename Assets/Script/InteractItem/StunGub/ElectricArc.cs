using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Efek petir bercabang dari laras senjata ke titik kena enemy.
/// Attach ke GameObject kosong di StunGun, assign di StunGun.cs.
/// Butuh beberapa LineRenderer — script ini spawn otomatis.
/// </summary>
public class ElectricArc : MonoBehaviour
{
    [Header("Arc Settings")]
    [SerializeField] private int segmentsPerBolt = 12;      // makin banyak = makin berkelok
    [SerializeField] private float displacementAmount = 0.4f; // seberapa jauh kelok-keloknya
    [SerializeField] private float arcDuration = 0.12f;     // berapa lama arc tampil
    [SerializeField] private int maxBranches = 3;           // jumlah cabang petir
    [SerializeField] private float branchChance = 0.35f;    // probabilitas cabang muncul per segmen
    [SerializeField] private float branchLength = 0.4f;     // panjang cabang relatif ke bolt utama

    [Header("Visuals")]
    [SerializeField] private Material arcMaterial;             [SerializeField] private float mainWidth = 0.04f;
    [SerializeField] private float branchWidth = 0.015f;
    [SerializeField] private Color mainColor   = new Color(0.5f, 0.8f, 1f, 1f);
    [SerializeField] private Color branchColor = new Color(0.3f, 0.6f, 1f, 0.7f);

    
    private List<LineRenderer> activeRenderers = new List<LineRenderer>();

    public void Play(Vector3 from, Vector3 to)
    {
        StopAllCoroutines();
        ClearRenderers();
        StartCoroutine(ArcRoutine(from, to));
    }

    private IEnumerator ArcRoutine(Vector3 from, Vector3 to)
    {
        float elapsed = 0f;
        int branchCount = 0;

        while (elapsed < arcDuration)
        {
            ClearRenderers();

            DrawBolt(from, to, mainWidth, mainColor, segmentsPerBolt, displacementAmount);

            branchCount = 0;
            Vector3 dir = (to - from);
            float totalLen = dir.magnitude;

            for (int i = 1; i < segmentsPerBolt && branchCount < maxBranches; i++)
            {
                if (Random.value < branchChance)
                {
                    float t = (float)i / segmentsPerBolt;
                    Vector3 branchOrigin = Vector3.Lerp(from, to, t) + RandomPerp(dir.normalized) * displacementAmount * Random.Range(0.5f, 1.5f);

                    // Arah cabang: agak menyamping dari arah utama
                    Vector3 branchDir = (dir.normalized + RandomPerp(dir.normalized) * 1.2f).normalized;
                    Vector3 branchEnd = branchOrigin + branchDir * totalLen * branchLength * Random.Range(0.5f, 1f);

                    DrawBolt(branchOrigin, branchEnd, branchWidth, branchColor, segmentsPerBolt / 2, displacementAmount * 0.6f);
                    branchCount++;
                }
            }

            elapsed += Time.deltaTime;
            yield return null; // update tiap frame → kedip-kedip seperti listrik
        }

        ClearRenderers();
    }

    private void DrawBolt(Vector3 from, Vector3 to, float width, Color color, int segments, float displacement)
    {
        LineRenderer lr = GetOrCreateLineRenderer(width, color);
        lr.positionCount = segments + 1;

        lr.SetPosition(0, from);
        lr.SetPosition(segments, to);

        // Jalankan midpoint displacement untuk efek berkelok
        Vector3[] points = new Vector3[segments + 1];
        points[0] = from;
        points[segments] = to;

        for (int i = 1; i < segments; i++)
        {
            float t = (float)i / segments;
            Vector3 basePos = Vector3.Lerp(from, to, t);
            Vector3 perp = RandomPerp((to - from).normalized);
            points[i] = basePos + perp * displacement * Random.Range(-1f, 1f);
        }

        for (int i = 0; i <= segments; i++)
            lr.SetPosition(i, points[i]);
    }

    private LineRenderer GetOrCreateLineRenderer(float width, Color color)
    {
        GameObject go = new GameObject("ArcSegment");
        go.transform.SetParent(transform);

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.startWidth = width;
        lr.endWidth   = width * 0.5f;
        lr.material   = arcMaterial;
        lr.startColor = color;
        lr.endColor   = new Color(color.r, color.g, color.b, 0f);
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;

        activeRenderers.Add(lr);
        return lr;
    }

    private void ClearRenderers()
    {
        foreach (var lr in activeRenderers)
        {
            if (lr != null) Destroy(lr.gameObject);
        }
        activeRenderers.Clear();
    }

    // Vektor tegak lurus acak dari suatu arah
    private Vector3 RandomPerp(Vector3 dir)
    {
        Vector3 perp = Vector3.Cross(dir, Random.insideUnitSphere);
        return perp == Vector3.zero ? Vector3.up : perp.normalized;
    }

    private void OnDisable()
    {
        ClearRenderers();
    }
}