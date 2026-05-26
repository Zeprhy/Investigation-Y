using System;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class EnemyChasingEvent : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private NewEnemyAI enemyAI;

    [Header("Force Player to see the enemy")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float lookAtDuration = 3f;

    [Header("Setup Input Action")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private string lookActionName = "Look";

    private bool _isLookingAtEnemy = false;
    private bool _isReturningToOriginal = false;
    private Quaternion _originalRotation;
    private bool _eventTriggered = false; // Pengaman agar tidak terpicu 2 kali

    // FUNGSI UTAMA: Memicu event saat Player masuk ke area ini
    private void OnTriggerEnter(Collider other)
    {
        // Pastikan yang menabrak adalah Player (sesuaikan Tag object player kamu)
        if (other.CompareTag("Player") && !_eventTriggered)
        {
            _eventTriggered = true; 
            EnemyActivation();
        }
    }

    public void EnemyActivation()
    {
        if (enemyAI != null)
        {
            // 1. HIDUPKAN ENEMY DI SINI (Detik saat Player melewati trigger)
            enemyAI.gameObject.SetActive(true);

            // 2. Amankan posisi agen ke NavMesh
            UnityEngine.AI.NavMeshAgent agent = enemyAI.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.Warp(enemyAI.transform.position);

            // 3. Jalankan kecerdasan AI dan paksa kamera melihat ke musuh
            enemyAI.EnemyActivation();
            StartCoroutine(TriggerForceLookRoutine());
        }
    }

    private IEnumerator TriggerForceLookRoutine()
    {
        _originalRotation = playerCamera.rotation;
        
        if (playerInput != null)
        {
            playerInput.actions[lookActionName].Disable();
        }

        _isLookingAtEnemy = true;
        yield return new WaitForSeconds(lookAtDuration);

        _isLookingAtEnemy = false;
        _isReturningToOriginal = true;

        yield return new WaitForSeconds(1f); 

        _isReturningToOriginal = false;

        if (playerInput != null)
        {
            playerInput.actions[lookActionName].Enable();
        }

        DialogueManager.Instance.ShowDialogue("what the hell is that?");
    }

    void LateUpdate()
    {
        if (playerCamera == null) return;

        if (_isLookingAtEnemy && enemyAI != null)
        {            
            Vector3 targetDirection = enemyAI.transform.position - playerCamera.position;
            if (targetDirection != Vector3.zero)
            {                
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                playerCamera.rotation = Quaternion.Slerp(playerCamera.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else if (_isReturningToOriginal)
        {
            playerCamera.rotation = Quaternion.Slerp(playerCamera.rotation, _originalRotation, rotationSpeed * Time.deltaTime);
        }
    }
}