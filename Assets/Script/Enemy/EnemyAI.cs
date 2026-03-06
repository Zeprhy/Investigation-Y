using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Patrolling, Chasing, Attacking, Investigating }
    public EnemyState currentState = EnemyState.Patrolling;

    [Header("Components")]
    private NavMeshAgent agent;
    private Transform player;

    [Header("Patrol Settings")]
    public List<Transform> waypoints;
    private int currentWaypointIndex = 0;
    public float waypointWaitTime = 1f;
    private float waypointTimer;

    [Header("Detection Settings")]
    public float viewDistance = 15f;
    public float viewAngle = 60f;
    public LayerMask obstacleMask;
    
    [Header("Awareness System")]
    public float awarenessThreshold = 2f;
    public float awarenessDecreaseSpeed = 0.5f;
    private float awarenessMeter = 0f;

    [Header("Attack Settings")]
    public float attackRange = 2.5f;
    public float attackCooldown = 2f;
    private float lastAttackTime;

    [Header("Speed Settings")]
    public float patrolSpeed = 3.5f;
    public float investigateSpeed = 2f;
    public float chaseSpeed = 6f;

    private float investigateTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // Memastikan Tag "Player" sesuai standar Unity
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) playerObj = GameObject.FindGameObjectWithTag("player");
        
        if (playerObj != null) player = playerObj.transform;

        // Inisialisasi awal
        if (waypoints.Count > 0) MoveToNextWaypoint();
    }

    void Update()
    {
        if (player == null) return;

        HandleAwareness();
        UpdateBrain();
    }

    private void HandleAwareness()
    {
        if (player == null) return;

        // Ambil script MovementPlayer untuk cek status sembunyi
        MovementPlayer playerMovement = player.GetComponent<MovementPlayer>();
        bool playerIsHidden = playerMovement != null && playerMovement.IsHidden;

        Vector3 dir = (player.position - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, player.position);

        // AI tidak bisa melihat player jika player sedang bersembunyi (IsHidden)
        bool canSee = !playerIsHidden && 
                      dist <= viewDistance && 
                      Vector3.Angle(transform.forward, dir) < viewAngle / 2f && 
                      !Physics.Raycast(transform.position + Vector3.up, dir, dist, obstacleMask);

        float target = canSee ? awarenessThreshold + 0.1f : 0f;

        // Jika player tiba-tiba bersembunyi saat dikejar, turunkan awareness lebih cepat
        float speed = (canSee) ? 1.5f : (playerIsHidden ? awarenessDecreaseSpeed * 2f : awarenessDecreaseSpeed);
        awarenessMeter = Mathf.MoveTowards(awarenessMeter, target, speed * Time.deltaTime);
    }

    private void UpdateBrain()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        bool aware = awarenessMeter >= awarenessThreshold;

        // Logika Hierarki State
        if (aware && dist <= attackRange)
        {
            ChangeState(EnemyState.Attacking);
            ExecuteAttack();
        }
        else if (aware)
        {
            ChangeState(EnemyState.Chasing);
            ExecuteChase();
        }
        else if (currentState == EnemyState.Investigating)
        {
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

        // Logika Pemulihan: Jika AI terjebak di luar NavMesh, tarik kembali ke area biru
        NavMeshHit hit;
        if (!agent.isOnNavMesh && NavMesh.SamplePosition(transform.position, out hit, 3.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position); // Menarik AI kembali ke area NavMesh terdekat
        }

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
            ChangeState(EnemyState.Investigating);
            agent.SetDestination(noisePosition);
            awarenessMeter = Mathf.Max(awarenessMeter, awarenessThreshold * 0.6f);
        }
    }

    // --- LOGIKA EKSEKUSI ---

    void ExecutePatrol()
    {
        if (waypoints.Count == 0) return;

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
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    void ExecuteInvestigate()
    {
    // Jika AI sudah sampai di titik terakhir player terlihat/suara terdengar
    if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
    {
        agent.isStopped = true; // Berhenti untuk bingung
        
        investigateTimer += Time.deltaTime;
            if (investigateTimer >= 3f) 
            {
                Debug.Log("AI Bingung selesai, kembali patroli");
                // Pindah state hanya satu kali
                investigateTimer = 0;
                ChangeState(EnemyState.Patrolling);
            }
        }
    }

    void ExecuteChase()
    {
        // Update terus posisi player
        agent.SetDestination(player.position);
    }

    void ExecuteAttack()
    {
        // Menghadap Player dengan halus
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 10f);
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Debug.Log("<color=red>Enemy Menyerang!</color>");
            // DISINI: Panggil animasi menyerang kamu
            // GetComponent<Animator>().SetTrigger("Attack"); 
            lastAttackTime = Time.time;
        }
    }
}