using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using TMPro;


public class BoardedWall : MonoBehaviour, IInteractable
{
    [Header("Boards")]
    public List<Rigidbody> boards;
 
    [Header ("Audio")]
    [SerializeField] private AudioClip PryingSound;
 
    [Header("Settings")]
    public float fallForceMin = 1f;
    public float fallForceMax = 3f;
 
    [Header("Objective Settings")]
    [SerializeField] ObjectiveManager objectiveManager;
 
    private int boardsRemoved = 0;
    private bool isDone = false;
    private NavMeshObstacle wallObstacle;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        wallObstacle = GetComponent<NavMeshObstacle>();

        if (wallObstacle != null)
        {
            wallObstacle.enabled = true;
            wallObstacle.carving = true;
        }
    }

   public bool CanInteract(ItemType itemType, string keyID = "")
    {
        return itemType == ItemType.Crowbar && !isDone;
    }

    public void Interact(ItemType itemType)
    {
        if (isDone) return;
        if (boardsRemoved >= boards.Count) return;

        if (GameManager.Instance != null && PryingSound != null)
        {
            GameManager.Instance.audioManager.PlaySFX(PryingSound);
        }

        Rigidbody board = boards[boardsRemoved];
        DetachBoard(board);
        boardsRemoved++;

        if (boardsRemoved >= boards.Count)
        {
            isDone = true;

            if (wallObstacle != null) 
            {
                wallObstacle.enabled = false;
            }
        }
    }
     void DetachBoard(Rigidbody board)
        {
            board.transform.SetParent(null);
            board.constraints = RigidbodyConstraints.None;

            board.isKinematic = false;
            board.useGravity = true;
            
            Vector3 randomForce = transform.forward * Random.Range(fallForceMin, fallForceMax);
            board.AddForce(randomForce, ForceMode.Impulse);
            board.AddTorque(Random.insideUnitSphere * 1.5f, ForceMode.Impulse);
        }
}
