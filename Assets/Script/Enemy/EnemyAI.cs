using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Patrolling, Chasing, Attacking }
    public EnemyState currentState = EnemyState.Patrolling;

    [Header("Navigation Settings")]
    private NavMeshAgent agent;
    private Transform playerTransform;

    [Header("Patrol Settings")]
    public List<Transform> waypoints;
    private int currentWaypointIndex = 0;

    [Header("Detection & Attack")]
    public float detectionRange = 10f;
    public float attackRange = 2.5f;
    public float attackCooldown = 2f;
    private float lastAttackTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("player");
        if (playerObj != null) playerTransform = playerObj.transform;

        if (waypoints.Count > 0) agent.SetDestination(waypoints[0].position);
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Logika Perpindahan Status
        if (distanceToPlayer <= attackRange)
        {
            currentState = EnemyState.Attacking;
        }
        else if (distanceToPlayer <= detectionRange)
        {
            currentState = EnemyState.Chasing;
        }
        else
        {
            currentState = EnemyState.Patrolling;
        }

        switch (currentState)
        {
            case EnemyState.Patrolling: Patrol(); break;
            case EnemyState.Chasing: Chase(); break;
            case EnemyState.Attacking: Attack(); break;
        }
    }

    void Patrol()
    {
        if (waypoints.Count == 0) return;

        // Gunakan stoppingDistance sebagai acuan agar tidak stuck
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    void Chase()
    {
        agent.isStopped = false; // Pastikan agent bisa jalan lagi setelah attacking
        agent.SetDestination(playerTransform.position);
    }

    void Attack()
    {
        agent.isStopped = true; // Hentikan agent agar fokus menyerang
        
        // Selalu menghadap ke Player saat menyerang
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Debug.Log("<color=red>Enemy Menyerang Player!</color>");
            lastAttackTime = Time.time;
        }
    }
}