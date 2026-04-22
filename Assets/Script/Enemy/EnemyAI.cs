// using UnityEngine;
// using UnityEngine.AI;
// using System.Collections;
// using System.Collections.Generic;

// public class EnemyAI : MonoBehaviour
// {
//     public enum EnemyState { Patrolling, Chasing, Attacking, Investigating, Idle }
//     public EnemyState currentState = EnemyState.Patrolling;
//     public bool isPerformingKill = false;

//     [Header("Components")]
//     private NavMeshAgent agent;
//     private Transform player;
//     private MovementPlayer playerScript;

//     [Header("Patrol Settings")]
//     public List<Transform> waypoints;
//     private int currentWaypointIndex = 0;
//     [SerializeField] private float waypointWaitTime = 1f;
//     private float waypointTimer;
//     private bool isForward = true;

//     [Header("Detection Settings")]
//     [SerializeField] private float viewDistance = 15f;
//     [SerializeField] private float viewAngle = 60f;
//     [SerializeField] private LayerMask obstacleMask;

//     [Header("Awareness System")]
//     [SerializeField] private float awarenessThreshold = 2f;
//     [SerializeField] private float awarenessDecreaseSpeed = 0.5f;
//     [SerializeField] private float awarenessIncreaseSpeed = 2.0f;
//     [SerializeField] private float awarenessMeter = 0f;
//     private Vector3 lastKnownPosition;

//     [Header("Attack Settings")]
//     [SerializeField] private float attackRange = 2.5f;
//     [SerializeField] private float preAttackDelay = 1.0f;
//     [SerializeField] private float postAttackDelay = 1.5f;
//     [SerializeField] private float attackCooldown = 2f;
//     private float lastAttackTime;
//     private bool isAttackingSequence = false;

//     [Header("Speed Settings")]
//     [SerializeField] private float patrolSpeed = 3.5f;
//     [SerializeField] private float investigateSpeed = 2f;
//     [SerializeField] private float chaseSpeed = 6f;

//     [Header("Door Interaction")]
//     [SerializeField] private float doorCheckDistance = 1.5f;
//     [SerializeField] private LayerMask doorLayer;

//     [Header("Stun Settings")]
//     private bool isStunned = false;
//     private float stunTimer = 0f;

//     private float investigateTimer;

//     // ─── Satu-satunya flag init yang dibutuhkan ───
//     private bool hasReachedFirstWaypoint = false;

//     void Start()
//     {
//         agent = GetComponent<NavMeshAgent>();

//         GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
//         if (playerObj != null)
//         {
//             player = playerObj.transform;
//             playerScript = playerObj.GetComponent<MovementPlayer>();
//         }

//         agent.updateRotation = true;
//         StartCoroutine(InitPatrol());
//     }

//     private IEnumerator InitPatrol()
//     {
//         // Tunggu 2 frame agar NavMesh agent fully ready
//         yield return null;
//         yield return null;

//         if (waypoints.Count > 0 && agent.isOnNavMesh)
//         {
//             currentWaypointIndex = 0;
//             agent.speed = patrolSpeed;
//             agent.isStopped = false;
//             agent.SetDestination(waypoints[0].position);
//         }
//     }

//     void Update()
//     {
//         if (isStunned)
//         {
//             stunTimer -= Time.deltaTime;
//             if (stunTimer <= 0f) RecoverFromStun();
//             return;
//         }

//         if (player == null || playerScript == null) return;

//         HandleAwareness();
//         UpdateBrain();
//         CheckForDoors();
//     }

//     private void HandleAwareness()
//     {
//         bool playerIsHidden = playerScript.IsHidden;
//         Vector3 dirToPlayer = (player.position - transform.position).normalized;
//         float dist = Vector3.Distance(transform.position, player.position);

//         bool hasLineOfSight = !Physics.Raycast(transform.position + Vector3.up, dirToPlayer, dist, obstacleMask);
//         bool inViewAngle = Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2f;
//         bool canSee = !playerIsHidden && dist <= viewDistance && inViewAngle && hasLineOfSight;

//         if (!playerIsHidden && dist < 9.0f && hasLineOfSight)
//         {
//             canSee = true;
//             if (dist < 5f) awarenessMeter = awarenessThreshold;
//         }

//         float proximityMultiplier = Mathf.Clamp(viewDistance / dist, 1f, 3f);
//         float currentIncreaseSpeed = awarenessIncreaseSpeed * proximityMultiplier;

//         if (canSee) lastKnownPosition = player.position;

//         float target = canSee ? awarenessThreshold + 0.1f : 0f;
//         float speed = canSee ? currentIncreaseSpeed : (playerIsHidden ? awarenessDecreaseSpeed * 2f : awarenessDecreaseSpeed);
//         awarenessMeter = Mathf.MoveTowards(awarenessMeter, target, speed * Time.deltaTime);
//     }

//     private void UpdateBrain()
//     {
//         if (isAttackingSequence || isStunned) return;

//         float dist = Vector3.Distance(transform.position, player.position);
//         bool aware = awarenessMeter >= awarenessThreshold;
//         bool playerVisible = aware && (!playerScript.IsHidden || dist < 2f);

//         if (playerVisible)
//         {
//             if (dist <= attackRange && Time.time >= lastAttackTime + attackCooldown)
//             {
//                 if (currentState != EnemyState.Attacking)
//                 {
//                     ChangeState(EnemyState.Attacking);
//                     StartCoroutine(AttackRoutine());
//                 }
//             }
//             else
//             {
//                 ChangeState(EnemyState.Chasing);
//                 ExecuteChase();
//             }
//         }
//         else if (aware && playerScript.IsHidden)
//         {
//             if (currentState != EnemyState.Investigating)
//                 ChangeState(EnemyState.Investigating);
//             ExecuteInvestigate();
//         }
//         else
//         {
//             if (currentState == EnemyState.Chasing ||
//                 currentState == EnemyState.Attacking ||
//                 currentState == EnemyState.Investigating)
//             {
//                 SetToClosestWaypoint();
//                 ChangeState(EnemyState.Patrolling);
//             }

//             ExecutePatrol();
//         }
//     }

//     void ExecutePatrol()
//     {
//         if (waypoints.Count == 0 || !agent.isOnNavMesh || agent.pathPending || !agent.hasPath) return;

//         if (agent.remainingDistance <= agent.stoppingDistance + 0.5f)
//         {
//             if (currentState != EnemyState.Idle)
//             {
//                 ChangeState(EnemyState.Idle);
//                 waypointTimer = 0f;
//             }

//             waypointTimer += Time.deltaTime;

//             if (waypointTimer >= waypointWaitTime)
//             {
//                 MoveToNextWaypoint();
//             }
//         }
//     }

//     void MoveToNextWaypoint()
//     {
//         if (waypoints.Count == 0) return;

//         if (isForward)
//         {
//             currentWaypointIndex++;
//             if (currentWaypointIndex >= waypoints.Count)
//             {
//                 currentWaypointIndex = waypoints.Count - 2;
//                 isForward = false;
//             }
//         }
//         else
//         {
//             currentWaypointIndex--;
//             if (currentWaypointIndex < 0)
//             {
//                 currentWaypointIndex = 1;
//                 isForward = true;
//             }
//         }

//         currentWaypointIndex = Mathf.Clamp(currentWaypointIndex, 0, waypoints.Count - 1);

//         ChangeState(EnemyState.Patrolling);
//     }

//     private void ChangeState(EnemyState newState)
//     {
//         if (currentState == newState) return;
//         currentState = newState;
//         agent.updateRotation = true;

//         switch (currentState)
//         {
//             case EnemyState.Patrolling:
//                 agent.speed = patrolSpeed;
//                 agent.isStopped = false;
//                 if (waypoints.Count > 0)
//                     agent.SetDestination(waypoints[currentWaypointIndex].position);
//                 break;
//             case EnemyState.Investigating:
//                 agent.speed = investigateSpeed;
//                 agent.isStopped = false;
//                 investigateTimer = 0;
//                 break;
//             case EnemyState.Chasing:
//                 agent.speed = chaseSpeed;
//                 agent.isStopped = false;
//                 break;
//             case EnemyState.Attacking:
//                 agent.isStopped = true;
//                 break;
//             case EnemyState.Idle:
//                 agent.isStopped = true;
//                 agent.speed = 0;
//                 break;
//         }
//     }

//     void ExecuteChase()
//     {
//         agent.SetDestination(player.position);
//     }

//     void ExecuteInvestigate()
//     {
//         if (!agent.isOnNavMesh) return;

//         agent.SetDestination(lastKnownPosition);
//         agent.speed = investigateSpeed;

//         if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
//         {
//             agent.isStopped = true;
//             investigateTimer += Time.deltaTime;

//             if (investigateTimer >= 3f)
//             {
//                 investigateTimer = 0;
//                 SetToClosestWaypoint();
//                 ChangeState(EnemyState.Patrolling);
//             }
//         }
//         else
//         {
//             agent.isStopped = false;
//         }
//     }

//     void SetToClosestWaypoint()
//     {
//         if (waypoints.Count == 0) return;

//         float closestDistance = Mathf.Infinity;
//         int closestIndex = 0;

//         for (int i = 0; i < waypoints.Count; i++)
//         {
//             float distance = Vector3.Distance(transform.position, waypoints[i].position);
//             if (distance < closestDistance)
//             {
//                 closestDistance = distance;
//                 closestIndex = i;
//             }
//         }

//         currentWaypointIndex = closestIndex;
//         agent.SetDestination(waypoints[currentWaypointIndex].position);
//     }

//     public void OnHeardNoise(Vector3 noisePosition)
//     {
//         if (currentState == EnemyState.Patrolling || currentState == EnemyState.Investigating)
//         {
//             lastKnownPosition = noisePosition;
//             ChangeState(EnemyState.Investigating);
//             awarenessMeter = Mathf.Max(awarenessMeter, awarenessThreshold * 0.6f);
//         }
//     }

//     public void ApplyStun(float duration)
//     {
//         isStunned = true;
//         isAttackingSequence = false;
//         StopAllCoroutines();
//         stunTimer = duration;
//         agent.isStopped = true;
//         agent.velocity = Vector3.zero;
//         currentState = EnemyState.Investigating;
//     }

//     public void TriggerKillAnimation()
//     {
//         isPerformingKill = true;
//         agent.isStopped = true;
//         agent.velocity = Vector3.zero;
//         Animator anim = GetComponentInChildren<Animator>();
//         if (anim != null) anim.SetTrigger("Stab");
//     }

//     public void ResetKillAnimationState()
//     {
//         isPerformingKill = false;
//         isAttackingSequence = false;
//         agent.isStopped = false;
//     }

//     private void RecoverFromStun()
//     {
//         isStunned = false;
//         agent.isStopped = false;
//         SetToClosestWaypoint();
//         ChangeState(EnemyState.Patrolling);
//     }

//     private void CheckForDoors()
//     {
//         RaycastHit hit;
//         if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, doorCheckDistance, doorLayer))
//         {
//             if (hit.collider.TryGetComponent(out NormalDoor door))
//             {
//                 if (!door.isOpen)
//                 {
//                     if (door.isLocked)
//                     {
//                         agent.isStopped = true;
//                         if (currentState == EnemyState.Chasing)
//                             ChangeState(EnemyState.Investigating);
//                         else
//                             MoveToNextWaypoint();
//                     }
//                     else
//                     {
//                         agent.isStopped = true;
//                         door.Interact(transform.position);
//                         StartCoroutine(ResumeMovementAfterDoor());
//                     }
//                 }
//             }
//         }
//     }

//     private IEnumerator ResumeMovementAfterDoor()
//     {
//         yield return new WaitForSeconds(0.5f);
//         agent.isStopped = false;
//     }

//     private IEnumerator AttackRoutine()
//     {
//         isAttackingSequence = true;
//         agent.isStopped = true;
//         agent.velocity = Vector3.zero;

//         yield return new WaitForSeconds(preAttackDelay);

//         float dist = Vector3.Distance(transform.position, player.position);
//         if (dist <= attackRange + 0.5f)
//         {
//             HealthManager hp = player.GetComponent<HealthManager>();
//             if (hp != null) hp.TakeDamage(1, this);
//         }

//         yield return new WaitForSeconds(postAttackDelay);

//         lastAttackTime = Time.time;
//         isAttackingSequence = false;

//         agent.isStopped = false; 
//         ChangeState(EnemyState.Chasing);
//     }

//     private void OnDrawGizmos()
//     {
//         Gizmos.color = Color.cyan;
//         Gizmos.DrawWireSphere(transform.position, viewDistance);
//         Gizmos.color = Color.red;
//         Gizmos.DrawWireSphere(transform.position, attackRange);
//         Gizmos.color = Color.yellow;
//         Vector3 left = Quaternion.AngleAxis(-viewAngle / 2, Vector3.up) * transform.forward;
//         Vector3 right = Quaternion.AngleAxis(viewAngle / 2, Vector3.up) * transform.forward;
//         Gizmos.DrawRay(transform.position + Vector3.up, left * viewDistance);
//         Gizmos.DrawRay(transform.position + Vector3.up, right * viewDistance);

//         if (player != null)
//         {
//             float dist = Vector3.Distance(transform.position, player.position);
//             if (dist <= viewDistance)
//             {
//                 Gizmos.color = Color.Lerp(Color.white, Color.red, awarenessMeter / awarenessThreshold);
//                 Gizmos.DrawLine(transform.position + Vector3.up, player.position + Vector3.up);
//             }
//         }
//     }
// }