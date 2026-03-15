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
        EnemyAI.EnemyState state = enemyAI.currentState;
        float currentSpeed = agent.velocity.magnitude;
        
        //Walk/Run
        if (state == EnemyAI.EnemyState.Investigating || state == EnemyAI.EnemyState.Attacking)
        {
            anim.SetFloat("Speed", 0f);
        }
        else
        {
            anim.SetFloat("Speed", currentSpeed); 
        }
        
        //Attack
        if (state == EnemyAI.EnemyState.Attacking && previousState != EnemyAI.EnemyState.Attacking)
        {
           anim.SetTrigger("Attack");
        }

        //investigation
        if (state == EnemyAI.EnemyState.Investigating && previousState != EnemyAI.EnemyState.Investigating)
        {
            anim.SetTrigger("Investigating");
        }
        previousState = state;
    }
}
