using UnityEngine;
using System.Collections.Generic;

public class BoardedWall : MonoBehaviour
{
   [Header("Boards")]
   public List<Rigidbody> boards;

   [Header("Settings")]
   public float fallForceMin = 1f;
   public float fallForceMax = 3f;

   private int boardsRemoved = 0;
   private bool isDone = false;

   public bool CanInteract(ItemType itemType, string keyID = "")
    {
        return itemType == ItemType.Crowbar && !isDone;
    }

    public void Interact(ItemType itemType)
    {
        if (isDone) return;
        if (boardsRemoved >= boards.Count) return;

        Rigidbody board = boards[boardsRemoved];
        DetachBoard(board);

        boardsRemoved++;
        Debug.Log($"Papan dilepas {boardsRemoved}/{boards.Count}");

        if (boardsRemoved >= boards.Count)
        {
            isDone = true;
            Debug.Log("Semua Papa terlepas! Akses terbuka");
        }

        void DetachBoard(Rigidbody board)
        {
            board.transform.SetParent(null);
            board.constraints = RigidbodyConstraints.None;

            board.isKinematic = false;
            board.useGravity = true;

            Vector3 randomForce = new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.2f, 0.2f),
                Random.Range(fallForceMin, fallForceMax)
            );
            board.AddForce(randomForce, ForceMode.Impulse);
            board.AddTorque(Random.insideUnitSphere * 1.5f, ForceMode.Impulse);
        }
    }
}
