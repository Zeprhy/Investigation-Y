using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class NewEnemyAI : MonoBehaviour
{
    public enum EnemyState { Patrolling, Chasing, Attacking, Investigating, Idle }
    
    [Header("Status")]
    public EnemyState currentState = EnemyState.Patrolling;
    public bool isStunned = false;
    public bool isPerformingKill = false;

    [Header ("Audio Settings")]
    [SerializeField] private float suspensesDistance = 20f;

    [Header("Components")]
    private NavMeshAgent agent;
    [SerializeField] private Transform player;
    [SerializeField] private MovementPlayer playerScript;

    [Header ("Cutscene Camera")]
    [SerializeField] private Animator anim;
    [SerializeField] private Camera deathCamera;

    [Header("Detection Settings")]
    [SerializeField] private float viewDistance = 15f;
    [SerializeField] private float viewAngle = 75f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask playerLayer;

    [Header("Awareness System")]
    [SerializeField] private float awarenessThreshold = 2f;
    [SerializeField] private float awarenessMeter = 0f;
    [SerializeField] private float awarenessDecreaseSpeed = 0.5f;
    [SerializeField] private float awarenessIncreaseSpeed = 2.0f;
    private Vector3 lastKnownPosition;

    [Header("Patrol Settings")]
    public List<Transform> waypoints;
    [SerializeField] private float patrolSpeed = 3.5f;
    [SerializeField] private float waypointWaitTime = 2f;
    private int currentWaypointIndex = 0;
    private float waypointTimer;
    private bool isForward = true;

    [Header("Combat Settings")]
    [SerializeField] private float chaseSpeed = 6f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 2f;
    private float lastAttackTime;
    private bool isAttackingSequence = false;

    [Header("Door Interaction")]
    [SerializeField] private float doorCheckDistance = 1.5f;
    [SerializeField] private LayerMask doorLayer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        anim = GetComponentInChildren<Animator>();

        if (deathCamera != null)
            deathCamera.gameObject.SetActive(false);
    }

    void Start()
    {
        // Mencari Player secara otomatis jika lupa ditarik di Inspector
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerScript = playerObj.GetComponent<MovementPlayer>();
            }
        }

        if (waypoints.Count > 0)
        {
            agent.SetDestination(waypoints[0].position);
        }
    }

    void Update()
    {
        if (isStunned || isPerformingKill || player == null) return;

        HandleDetection();
        CheckForDoors();
        UpdateMusicStatus();
        UpdateAnimations();

        // State Machine sederhana seperti di video referensi
        switch (currentState)
        {
            case EnemyState.Patrolling:
                PatrolLogic();
                break;
            case EnemyState.Chasing:
                ChaseLogic();
                break;
            case EnemyState.Attacking:
                AttackLogic();
                break;
            case EnemyState.Investigating:
                InvestigateLogic();
                break;
            case EnemyState.Idle:
                IdleLogic();
                break;
        }
    }

    public void EnemyActivation()
    {
        gameObject.SetActive(true);
    }

    private void UpdateAnimations()
    {
        if (anim == null) return;

        anim.SetBool("isWalking", false);
        anim.SetBool("isChasing", false);
        anim.SetBool("isAttacking", false);
        anim.SetBool("isInvestigating", false);

        switch (currentState)
        {
            case EnemyState.Patrolling:
                anim.SetBool("isWalking", true);   // Berjalan antar waypoint
                break;
            case EnemyState.Chasing:
                anim.SetBool("isChasing", true);   // Melihat player
                break;
            case EnemyState.Attacking:
                anim.SetBool("isAttacking", true); // Masuk jarak serang
                break;
            case EnemyState.Investigating:
                anim.SetBool("isInvestigating", true); // Kehilangan jejak player
                break;
            case EnemyState.Idle:
                // Saat Idle, semua bool di atas bernilai false, 
                // sehingga Animator otomatis memutar animasi Idle bawaan.
                break;
        }
    }

    private void UpdateMusicStatus()
    {
        if (GameManager.Instance.audioManager == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (currentState == EnemyState.Chasing || currentState == EnemyState.Attacking)
        {
            GameManager.Instance.audioManager.SetMusicState(AudioManager.MusicState.Chase);
        }

        else if (currentState == EnemyState.Investigating || dist <= suspensesDistance)
        {
            GameManager.Instance.audioManager.SetMusicState(AudioManager.MusicState.Investigate);
        }

        else
        {
           GameManager.Instance.audioManager.SetMusicState(AudioManager.MusicState.Ambient);
        }
    }

    private void HandleDetection()
    {
        bool canSeePlayer = false;
    
        if (playerScript != null && GameManager.Instance.hideManager.IsHidden)
        {
            awarenessMeter = Mathf.MoveTowards(awarenessMeter, 0f, awarenessDecreaseSpeed * Time.deltaTime);
            if (currentState == EnemyState.Chasing || currentState == EnemyState.Attacking)
                ChangeState(EnemyState.Investigating);
            return;
        }
    
        float distToPlayer = Vector3.Distance(transform.position, player.position);
    
        if (distToPlayer <= viewDistance)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToPlayer);
    
            if (angle < viewAngle / 2f)
            {
                if (!Physics.Raycast(transform.position + Vector3.up, dirToPlayer, distToPlayer, obstacleMask))
                {
                    if (!GameManager.Instance.hideManager.IsHidden) canSeePlayer = true;
                }
            }
        }
    
        if (canSeePlayer)
        {
            awarenessMeter = Mathf.MoveTowards(awarenessMeter, awarenessThreshold + 0.1f, awarenessIncreaseSpeed * Time.deltaTime);
            lastKnownPosition = player.position;
    
            // Langsung chase begitu melihat player, tanpa tunggu awareness penuh
            if (currentState != EnemyState.Attacking && currentState != EnemyState.Chasing)
            {
                ChangeState(EnemyState.Chasing);
            }
        }
        else
        {
            awarenessMeter = Mathf.MoveTowards(awarenessMeter, 0f, awarenessDecreaseSpeed * Time.deltaTime);
    
            if (awarenessMeter <= 0 && currentState == EnemyState.Chasing)
                ChangeState(EnemyState.Investigating);
        }
    }

    private void PatrolLogic()
    {
        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
        {
            ChangeState(EnemyState.Idle);
        }
    }

    private void IdleLogic()
    {
        waypointTimer += Time.deltaTime;
        if (waypointTimer >= waypointWaitTime)
        {
            waypointTimer = 0;
            SetNextWaypoint();
            ChangeState(EnemyState.Patrolling);
        }
    }

    private void ChaseLogic()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);

        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            ChangeState(EnemyState.Attacking);
        }
    }

    private void AttackLogic()
    {
        if (isAttackingSequence) return;

        if (Vector3.Distance(transform.position, player.position) > attackRange + 0.5f)
        {
            ChangeState(EnemyState.Chasing);
            return;
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private void InvestigateLogic()
    {
        agent.isStopped = false;
        agent.speed = patrolSpeed * 0.8f;
        agent.SetDestination(lastKnownPosition);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
        {
            agent.isStopped = true;
            waypointTimer += Time.deltaTime;

            if (waypointTimer >= 3f) 
            {
                waypointTimer = 0;

                SetNearestWaypoint();

                ChangeState(EnemyState.Patrolling);
            }
        }
    }

    private void SetNearestWaypoint()
    {
        if (waypoints == null || waypoints.Count == 0) return;

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
    }

    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        waypointTimer = 0; 

        UpdateAnimations();

        if (newState == EnemyState.Patrolling)
        {
            agent.isStopped = false;
            agent.speed = patrolSpeed;
            if (waypoints.Count > 0) 
                agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
        else if (newState == EnemyState.Idle || newState == EnemyState.Attacking)
        {
            agent.isStopped = true;
        }
        else
        {
            agent.isStopped = false;
        }
    }

    private void SetNextWaypoint()
    {
        if (waypoints.Count == 0) return;

        if (isForward)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Count) { currentWaypointIndex = waypoints.Count - 2; isForward = false; }
        }
        else
        {
            currentWaypointIndex--;
            if (currentWaypointIndex < 0) { currentWaypointIndex = 1; isForward = true; }
        }

        currentWaypointIndex = Mathf.Clamp(currentWaypointIndex, 0, waypoints.Count - 1);
        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    private IEnumerator AttackRoutine()
    {
        isAttackingSequence = true;
        lastAttackTime = Time.time;

        yield return new WaitForSeconds(0.5f);

        if (Vector3.Distance(transform.position, player.position) <= attackRange + 0.8f)
        {
            HealthManager hp = player.GetComponent<HealthManager>();
            if (hp != null) 
            {
                hp.TakeDamage(1, this); 
            }
        }

        yield return new WaitForSeconds(0.5f);

        isAttackingSequence = false; 

        if (currentState == EnemyState.Attacking)
        {
            ChangeState(EnemyState.Chasing);
        }
    }

    public void OnHeardNoise(Vector3 pos)
    {
        if (currentState == EnemyState.Patrolling)
        {
            lastKnownPosition = pos;
            ChangeState(EnemyState.Investigating);
        }
    }

    private void CheckForDoors()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, doorCheckDistance, doorLayer))
        {
            if (hit.collider.TryGetComponent(out BaseDoor door) && !door.isOpen && !door.isLocked)
            {
                door.ToggleDoor(transform.position);
            }
        }
    }

    public void ApplyStun(float duration)
    {
        if (isStunned) return;

        isStunned = true;
        isAttackingSequence = false;
        
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (anim != null) anim.SetBool("isStunned", true);

        StopAllCoroutines();
        StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float d)
    {
        yield return new WaitForSeconds(d);
        isStunned = false;

        if (anim != null) anim.SetBool("isStunned", false);

        if (agent != null && agent.isOnNavMesh) agent.isStopped = false;
        ChangeState(EnemyState.Patrolling);
    }

    public void TriggerKillAnimation()
    {
        isPerformingKill = true;
        
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.gameplayStateManager.SetState(GameplayState.Gameplay);
            GameManager.Instance.gameplayStateManager.SetState(GameplayState.Dead);
        }

        GameObject playerObj = playerScript != null ? playerScript.gameObject : null;
        if (playerObj != null)
        {
            // Nonaktifkan semua Renderer (mesh tubuh player)
            foreach (Renderer rend in playerObj.GetComponentsInChildren<Renderer>())
                rend.enabled = false;
    
            // Nonaktifkan Collider player
            foreach (Collider col in playerObj.GetComponentsInChildren<Collider>())
                col.enabled = false;
    
            // Nonaktifkan CharacterController
            CharacterController cc = playerObj.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
        }

        if (anim != null)
        {
            anim.SetTrigger("catch");
        }

        if (Camera.main != null) 
        {
            Camera.main.gameObject.SetActive(false);
        }
        
        if (deathCamera != null) 
        {
            deathCamera.gameObject.SetActive(true);
        }

        StartCoroutine(ShowDeathPanelAfterCutscene());
    }

    private IEnumerator ShowDeathPanelAfterCutscene()
    {
        yield return new WaitForSeconds(3f);

        if (StabSequence.Instance != null)
            StabSequence.Instance.TriggerStab();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);
    }
}