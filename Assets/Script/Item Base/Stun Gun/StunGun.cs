using UnityEngine;
using System.Collections;

// 1. UBAH INDUK CLASS DARI MonoBehaviour MENJADI ItemBase
public class StunGun : ItemBase
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private LineRenderer bulletTrail;
    [SerializeField] private ElectricArc electricArc;       
    [SerializeField] private ImpactFlashSprite impactFlash;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip emptySound;
    [SerializeField] private AudioClip chargingSound;

    [Header("Shoot Settings")]
    [SerializeField] private float shootRange = 20f;
    [SerializeField] private float stunDuration = 4f;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private float trailDuration = 0.05f;

    [Header("Battery Settings")]
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float batteryPerShot = 100f;       
    [SerializeField] private float chargeRateHeld = 15f;        
    [SerializeField] private float chargeRateDropped = 25f;     
    [SerializeField] private float holdChargeDelay = 1.5f;      

    private float currentBattery = 100f;
    private bool isHeld = false;
    private bool isDropped = false;
    private bool isCharging = false;
    private float holdStillTimer = 0f;
    private bool chargingSoundPlayed = false;
    private Vector3 lastPosition;

    [Header("UI Property")]
    public float BatteryPercent => currentBattery / maxBattery * 100;
    public bool IsCharging => isCharging;

    void Start()
    {
        currentBattery = maxBattery;
        lastPosition = transform.position;

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    void Update()
    {
        HandleCharging();
    }

    // 3. OVERRIDE FUNGSI USE DARI ITEMBASE UNTUK MENEMBAK
    public override void UseItem()
    {
        TryShoot();
    }

    public void TryShoot()
    {
        if (currentBattery < batteryPerShot)
        {
            if (emptySound != null)
            {
                GameManager.Instance.audioManager.PlaySFX(emptySound);
            }
            return;
        }
        Shoot();
    }

    private void Shoot()
    {
        currentBattery = 0f;
        isCharging = false;
        holdStillTimer = 0f;
        chargingSoundPlayed = false;

        if (muzzleFlash) muzzleFlash.Play();
        if (fireSound != null) GameManager.Instance.audioManager.PlaySFX(fireSound);

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f ,0.5f ,0f ));
        RaycastHit hit;
        Vector3 endPoint = ray.origin + ray.direction * shootRange;

        Vector3 arcStart = muzzleFlash != null ? muzzleFlash.transform.position : transform.position;

        if (Physics.Raycast(ray, out hit, shootRange, hitMask))
        {
            endPoint = hit.point;

            // Pastikan referensi NewEnemyAI Anda benar
            NewEnemyAI enemy = hit.collider.GetComponentInParent<NewEnemyAI>();

            if (enemy != null) enemy.ApplyStun(stunDuration);
            
            if (impactFlash) impactFlash.PlayAt(hit.point);
        }
        
        if (electricArc)
            electricArc.Play(arcStart, endPoint);

        if (bulletTrail)
            StartCoroutine(ShowTrail(bulletTrail, transform.position, endPoint));
    }

    private void HandleCharging()
    {
       if (currentBattery >= maxBattery)
        {
            currentBattery = maxBattery;
            isCharging = false;
            chargingSoundPlayed = false;
            return;
        } 

        if (isDropped)
        {
            isCharging = true;
            PlayChargingSound();
            currentBattery += chargeRateDropped * Time.deltaTime;
        }
        else if (isHeld)
        {
            float moved = Vector3.Distance(transform.position, lastPosition);
            bool isStill = moved < 0.01f;

            if (isStill)
            {
                holdStillTimer += Time.deltaTime;
            }
            else
            {
                holdStillTimer = 0f;
                isCharging = false;
                chargingSoundPlayed = false;
            }

            if (holdStillTimer >= holdChargeDelay)
            {
                isCharging = true;
                PlayChargingSound();
                currentBattery += chargeRateHeld * Time.deltaTime;
            }
        }
        currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery);
        lastPosition = transform.position;
    }

    private void PlayChargingSound()
    {
        if (!chargingSoundPlayed && chargingSound != null)
        {
            GameManager.Instance.audioManager.PlaySFX(chargingSound);
            chargingSoundPlayed = true;
        }
    }

    // 4. MENGGANTI OnPickedUp MENJADI override OnEquip
    public override void OnEquip()
    {
        base.OnEquip(); // Memanggil logika dasar (jika ada) di ItemBase
        isHeld = true;
        isDropped = false;
        holdStillTimer = 0f;
        chargingSoundPlayed = false;
        lastPosition = transform.position;
    }

    // 5. MENGGANTI OnDropped MENJADI override OnDrop
    public override void OnDrop()
    {
        base.OnDrop();
        isHeld = false;
        isDropped = true;
        chargingSoundPlayed = false;
    }

    private IEnumerator ShowTrail(LineRenderer lr, Vector3 from, Vector3 to)
    {
        lr.enabled = true;
        lr.SetPosition(0, from);
        lr.SetPosition(1, to);
        yield return new WaitForSeconds(trailDuration);
        lr.enabled = false;
    }
}