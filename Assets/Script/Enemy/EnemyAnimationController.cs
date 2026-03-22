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
        // Menggunakan GetComponentInChildren karena model biasanya ada di bawah parent
        anim = GetComponentInChildren<Animator>();
        enemyAI = GetComponent<EnemyAI>();
        
        if (enemyAI != null) previousState = enemyAI.currentState;
    }

    void Update()
    {
        if (enemyAI == null || anim == null) return;

        // 1. PRIORITAS UTAMA: Jika sedang dalam animasi 'Kill', kunci semua update lainnya
        if (enemyAI.isPerformingKill) return; 

        HandleMovementAnimation();
        HandleStateTransitions();

        // Update state terakhir di akhir frame
        previousState = enemyAI.currentState;
    }

    private void HandleMovementAnimation()
    {
        // Gunakan kecepatan agent untuk menentukan animasi Walk/Run
        // Kita beri sedikit smoothing (Mathf.Lerp) agar transisi gerak lebih halus
        float currentSpeed = agent.velocity.magnitude;
        
        // Jika sedang menyerang atau diam, paksa speed ke 0 agar kaki tidak 'sliding'
        if (enemyAI.currentState == EnemyAI.EnemyState.Attacking || enemyAI.currentState == EnemyAI.EnemyState.Idle)
        {
            currentSpeed = 0;
        }

        anim.SetFloat("Speed", currentSpeed, 0.1f, Time.deltaTime);
    }

    private void HandleStateTransitions()
    {
        EnemyAI.EnemyState currentState = enemyAI.currentState;

        // Jika state tidak berubah, tidak perlu mengecek trigger
        if (currentState == previousState) return;

        // Trigger Serangan Biasa
        if (currentState == EnemyAI.EnemyState.Attacking)
        {
            anim.SetTrigger("Attack");
        }

        // Kontrol Investigasi (Boolean)
        bool isInvestigating = (currentState == EnemyAI.EnemyState.Investigating);
        anim.SetBool("Investigating", isInvestigating);
    }
}