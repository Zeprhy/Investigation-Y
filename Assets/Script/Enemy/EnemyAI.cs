using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Patrolling, Chasing, Attacking, Investigating }
    public EnemyState currentState = EnemyState.Patrolling;

    [Header("Components")]
    private NavMeshAgent agent;
    private Transform player;
    private MovementPlayer playerScript;

    [Header("Patrol Settings")]
    public List<Transform> waypoints;
    private int currentWaypointIndex = 0;
    [SerializeField] private float waypointWaitTime = 1f;
    private float waypointTimer;

    [Header("Detection Settings")]
    [SerializeField] private float viewDistance = 15f;
    [SerializeField] private float viewAngle = 60f;
    [SerializeField] private LayerMask obstacleMask;
    
    [Header("Awareness System")]
    [SerializeField] private float awarenessThreshold = 2f;
    [SerializeField] private float awarenessDecreaseSpeed = 0.5f;
    [SerializeField] private float awarenessIncreaseSpeed = 2.0f; 
    [SerializeField] private float awarenessMeter = 0f;
    private Vector3 lastKnownPosition;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float preAttackDelay = 1.0f;
    [SerializeField] private float postAttackDelay = 1.5f;
    [SerializeField] private float attackCooldown = 2f;
    private float lastAttackTime;
    private bool isAttackingSequence = false;

    [Header("Speed Settings")]
    [SerializeField] private float patrolSpeed = 3.5f;
    [SerializeField] private float investigateSpeed = 2f;
    [SerializeField] private float chaseSpeed = 6f;

    [Header("Door Interaction")]
    [SerializeField] private float doorCheckDistance = 1.5f;
    [SerializeField] private LayerMask doorLayer;
    
    [Header("Stun Settings")]
    private bool isStunned = false;
    private float stunTimer = 0f;

    private float investigateTimer;
    private bool isForward = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {            
            player = playerObj.transform;
            playerScript = playerObj.GetComponent<MovementPlayer>();
        }

        agent.updateRotation = true; 
        
        if (waypoints.Count > 0) MoveToNextWaypoint();
    }

    void Update()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f) RecoverFromStun();
            return;
        }
        
        if (player == null || playerScript == null) return;

        HandleAwareness();
        UpdateBrain();
        CheckForDoors();
    }

    public void ApplyStun(float duration)
    {
        isStunned = true;
        isAttackingSequence = false;
        StopAllCoroutines();

        stunTimer = duration;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        currentState = EnemyState.Investigating;
    }

    private void RecoverFromStun()
    {
        isStunned = false;
        agent.isStopped = false;

        SetToClosestWaypoint();
        ChangeState(EnemyState.Patrolling);
    }
    private void CheckForDoors()
    {
        RaycastHit hit;
        // Menembakkan laser ke depan musuh untuk mencari pintu
        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, doorCheckDistance, doorLayer))
        {
            if (hit.collider.TryGetComponent(out NormalDoor door))
            {
                // Jika pintu tertutup dan tidak terkunci, musuh membukanya
                if (!door.isOpen && !door.isLocked)
                {
                    // Musuh memanggil fungsi Interact (mengirim posisi musuh agar pintu terbuka menjauh)
                    agent.isStopped = true;
                    door.Interact(transform.position);
                    StartCoroutine(ResumeMovementAfterDoor());
                }
            }
        }
    }

    private IEnumerator ResumeMovementAfterDoor()
    {
        yield return new WaitForSeconds(0.5f); // Jeda agar animasi pintu mulai jalan
        agent.isStopped = false;
    }

    private void HandleAwareness()
    {
        bool playerIsHidden = playerScript.IsHidden;
    
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, player.position);
    
        // 1. CEK LINE OF SIGHT (Apakah ada tembok?)
        bool hasLineOfSight = !Physics.Raycast(transform.position + Vector3.up, dirToPlayer, dist, obstacleMask);
        
        // 2. CEK FIELD OF VIEW (Apakah dalam sudut pandang?)
        bool inViewAngle = Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2f;
    
        // 3. LOGIKA DETEKSI SENSITIF
        bool canSee = !playerIsHidden && dist <= viewDistance && inViewAngle && hasLineOfSight;
        
        // A. INSTINCT RADIUS (Jika sangat dekat, langsung sadar tanpa peduli sudut pandang)
        if (!playerIsHidden && dist < 9.0f && hasLineOfSight) 
        {
            canSee = true;
            // Langsung penuhkan awareness jika jaraknya sangat intim (misal < 3m)
            if (dist < 5f) awarenessMeter = awarenessThreshold; 
        }
    
        // B. MODIFIKASI SPEED (Semakin dekat player, semakin cepat AI sadar)
        float proximityMultiplier = Mathf.Clamp(viewDistance / dist, 1f, 3f); 
        float currentIncreaseSpeed = awarenessIncreaseSpeed * proximityMultiplier;
    
        if (canSee)
        {
            lastKnownPosition = player.position;
        }
    
        float target = canSee ? awarenessThreshold + 0.1f : 0f;
        
        // Gunakan currentIncreaseSpeed agar awareness naik lebih cepat saat player dekat
        float speed = (canSee) ? currentIncreaseSpeed : (playerIsHidden ? awarenessDecreaseSpeed * 2f : awarenessDecreaseSpeed);
        awarenessMeter = Mathf.MoveTowards(awarenessMeter, target, speed * Time.deltaTime);
    }

    private void UpdateBrain()
    {
        if (isAttackingSequence || isStunned) return;

        float dist = Vector3.Distance(transform.position, player.position);
        bool aware = awarenessMeter >= awarenessThreshold;

        // Jika player sembunyi, paksa AI ke state Investigating di lokasi terakhir
        if (aware && !playerScript.IsHidden)
        {
            // Logika Hierarki State
            if (dist <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                ChangeState(EnemyState.Attacking);
                StartCoroutine(AttackRoutine());
            }
            else
            {
                ChangeState(EnemyState.Chasing);
                ExecuteChase();
            }
        }
        
        else if (currentState == EnemyState.Chasing || currentState == EnemyState.Attacking || currentState == EnemyState.Investigating)
        {
            ChangeState(EnemyState.Investigating);
            ExecuteInvestigate();
        }
        else
        {
            ChangeState(EnemyState.Patrolling);
            ExecutePatrol();
        }
    }

    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        agent.updateRotation = true;

        switch (currentState)
        {
            case EnemyState.Patrolling:
                agent.speed = patrolSpeed;
                agent.isStopped = false;
                // Gunakan index yang ada sekarang daripada memanggil MoveToNextWaypoint langsung
                if (waypoints.Count > 0) agent.SetDestination(waypoints[currentWaypointIndex].position);
                break;
            case EnemyState.Investigating:
                agent.speed = investigateSpeed;
                agent.isStopped = false; // Biarkan AI berjalan ke lokasi suara terakhir
                investigateTimer = 0;
                break;
            case EnemyState.Chasing:
                agent.speed = chaseSpeed;
                agent.isStopped = false;
                break;
            case EnemyState.Attacking:
                agent.isStopped = true;
                break;
        }
    }

    public void OnHeardNoise(Vector3 noisePosition)
    {
        if (currentState == EnemyState.Patrolling || currentState == EnemyState.Investigating)
        {
            lastKnownPosition = noisePosition;
            ChangeState(EnemyState.Investigating);
            awarenessMeter = Mathf.Max(awarenessMeter, awarenessThreshold * 0.6f);
        }
    }

    void ExecutePatrol()
    {
        if (waypoints.Count == 0) return;
        if (!agent.isOnNavMesh) return;

        // Cek apakah sudah sampai
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.3f)
        {
            waypointTimer += Time.deltaTime;
            if (waypointTimer >= waypointWaitTime)
            {
                MoveToNextWaypoint();
            }
        }
    }

    void MoveToNextWaypoint()
    {
        if (waypoints.Count == 0) return;

        waypointTimer = 0;

        if (isForward)
        {
            currentWaypointIndex++;
            // Jika sampai di titik terakhir, balik arah
            if (currentWaypointIndex >= waypoints.Count)
            {
                currentWaypointIndex = waypoints.Count - 2; // Kembali ke titik sebelum terakhir
                isForward = false;
            }
        }
        else
        {
            currentWaypointIndex--;
            // Jika sampai di titik awal, balik arah lagi
            if (currentWaypointIndex < 0)
            {
                currentWaypointIndex = 1; // Mulai maju ke titik kedua
                isForward = true;
            }
        }

        // Pastikan index tidak minus jika waypoint hanya sedikit
        currentWaypointIndex = Mathf.Clamp(currentWaypointIndex, 0, waypoints.Count - 1);

        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    void ExecuteInvestigate()
    {
    if (!agent.isOnNavMesh) return;
    
    // Musuh pergi ke titik terakhir dia melihat kamu, bukan posisi kamu sekarang (karena kamu sembunyi)
    agent.SetDestination(lastKnownPosition);
    agent.speed = investigateSpeed;
    
    // Jika AI sudah sampai di titik terakhir player terlihat/suara terdengar
    if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
    {
        agent.isStopped = true; // Berhenti untuk bingung
        
        investigateTimer += Time.deltaTime;
            if (investigateTimer >= 3f) 
            {
                // Pindah state hanya satu kali
                investigateTimer = 0;
                SetToClosestWaypoint();
                ChangeState(EnemyState.Patrolling);
            }
        }
        else
        {
            // Jika belum sampai, pastikan agent tidak stopped
            agent.isStopped = false;
        }
    }

    void SetToClosestWaypoint()
    {
        if (waypoints.Count == 0) return;

        float closestDistance = Mathf.Infinity;
        int closestIndex = 0;

        for (int i = 0; i < waypoints.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, waypoints[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        currentWaypointIndex = closestIndex;
        // Set tujuan ke waypoint tersebut
        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    void ExecuteChase()
    {
        // Update terus posisi player
        agent.SetDestination(player.position);
    }

    private IEnumerator AttackRoutine()
    {
        isAttackingSequence = true;
        agent.isStopped = true; // Musuh berhenti total
        agent.velocity = Vector3.zero;

        // --- 1. JEDA SEBELUM MENYERANG (Ancang-ancang) ---
        // Di sini kamu bisa memicu animasi "Prepare Attack" jika ada
        yield return new WaitForSeconds(preAttackDelay);

        // Cek kembali apakah player masih dalam jangkauan setelah jeda
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= attackRange + 0.5f) 
        {
            HealthManager hp = player.GetComponent<HealthManager>();
            if (hp != null) hp.TakeDamage(1);
        }

        // --- 2. JEDA SETELAH MENYERANG (Recovery) ---
        // Musuh diam sejenak setelah memukul (lelah/reset posisi)
        yield return new WaitForSeconds(postAttackDelay);

        lastAttackTime = Time.time;
        
        // --- 3. SELESAI ---
        isAttackingSequence = false;
        agent.isStopped = false; // Musuh boleh bergerak lagi

        // Paksa kembali ke Chasing agar dia mengejar lagi
        ChangeState(EnemyState.Chasing);
    }

    private void OnDrawGizmos()
    {
        // 1. GIZMO JARAK PANDANG (View Distance) - Warna Biru/Cyan
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        // 2. GIZMO JARAK SERANG (Attack Range) - Warna Merah
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 3. VISUALISASI SUDUT PANDANG (View Angle)
        Gizmos.color = Color.yellow;
        Vector3 leftRayDirection = Quaternion.AngleAxis(-viewAngle / 2, Vector3.up) * transform.forward;
        Vector3 rightRayDirection = Quaternion.AngleAxis(viewAngle / 2, Vector3.up) * transform.forward;
        
        Gizmos.DrawRay(transform.position + Vector3.up, leftRayDirection * viewDistance);
        Gizmos.DrawRay(transform.position + Vector3.up, rightRayDirection * viewDistance);

        // 4. GARIS DETEKSI KE PLAYER (Hanya muncul jika Player terdeteksi)
        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= viewDistance)
            {
                // Warna berubah sesuai Awareness Meter
                Gizmos.color = Color.Lerp(Color.white, Color.red, awarenessMeter / awarenessThreshold);
                Gizmos.DrawLine(transform.position + Vector3.up, player.position + Vector3.up);
            }
        }
    }
}