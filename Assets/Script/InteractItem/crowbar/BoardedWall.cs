using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using TMPro;

public class BoardedWall : MonoBehaviour, IInteractable
{
   [Header("Boards")]
   public List<Rigidbody> boards;

   [Header("Settings")]
   public float fallForceMin = 1f;
   public float fallForceMax = 3f;
   [SerializeField] private float uiDisplayDistance = 3.0f;

    [Header("UI System (Direct TMP)")]
    [SerializeField] private TextMeshProUGUI globalInteractText;

   private int boardsRemoved = 0;
   private bool isDone = false;
    private bool _isPlayerNear = false;
    private Transform _playerTransform;
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

        if (playerObj != null) _playerTransform = playerObj.transform;

        if (globalInteractText != null) globalInteractText.text = "";
    }

    void Update()
    {
        HandleUIDisplay();
    }

    private void HandleUIDisplay()
    {
        if (_playerTransform == null || globalInteractText == null || isDone) 
        {
            if (_isPlayerNear && isDone) 
            {
                globalInteractText.text = "";
                _isPlayerNear = false;
            }
            return;
        }

        float distance = Vector3.Distance(transform.position, _playerTransform.position);

        if (distance <= uiDisplayDistance)
        {
            _isPlayerNear = true;
            UpdateBoardUIText();
        }
        else if (_isPlayerNear)
        {
            _isPlayerNear = false;
            globalInteractText.text = "";
        }
    }

    private void UpdateBoardUIText()
    {
        PlayerInteraction interaction = _playerTransform.GetComponent<PlayerInteraction>();

        if (interaction != null)
        {
            globalInteractText.text = "Pres [F] Dismantable The Board";
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

            if (globalInteractText != null) globalInteractText.text = "";
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
