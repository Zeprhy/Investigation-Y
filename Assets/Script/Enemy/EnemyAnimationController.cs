using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimationController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private EnemyAI enemyAI;

    private EnemyAI.EnemyState previousState;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        enemyAI = GetComponent<EnemyAI>();
        previousState = enemyAI.currentState;
    }

    void Update()
    {
        if( enemyAI == null) return;
        float currentSpeed = agent.velocity.magnitude;
        EnemyAI.EnemyState state = enemyAI.currentState;

        anim.SetFloat("Speed", currentSpeed);

        if (state == EnemyAI.EnemyState.Attacking && previousState != EnemyAI.EnemyState.Attacking)
        {
           anim.SetTrigger("Attack");
        }
        
        anim.SetBool("Investigating", state == EnemyAI.EnemyState.Investigating);
        previousState = state;
    }
}
