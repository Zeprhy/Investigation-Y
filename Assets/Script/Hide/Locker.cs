using UnityEngine;

public class Locker : MonoBehaviour
{
    [Header("Settings")]
    // [SerializeField] memungkinkan variabel private muncul di Inspector
    [SerializeField] private float interactionRadius = 2.5f; 
    [SerializeField] private Color gizmoColor = Color.yellow;

    [Header("References")]
    [SerializeField] private Transform hidingPoint;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private GameObject interactUI;

    private bool _isOccupied = false;
    private Transform _playerTransform;

    private void Start()
    {
        // Mencari player satu kali saat start
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;
        
        if (interactUI != null) interactUI.SetActive(false);
    }

    private void Update()
    {
        // Mencegah error jika player tidak ditemukan atau sedang di dalam loker
        if (_playerTransform == null || _isOccupied) 
        {
            if (interactUI != null && interactUI.activeSelf) interactUI.SetActive(false);
            return;
        }

        // SEKARANG interactionRadius DIGUNAKAN DI SINI
        float distance = Vector3.Distance(transform.position, _playerTransform.position);

        // UI muncul hanya jika player di dalam radius
        if (interactUI != null)
        {
            interactUI.SetActive(distance <= interactionRadius);
        }
    }

    public void Interact(MovementPlayer player)
    {
        if (!_isOccupied)
            EnterLocker(player);
        else
            ExitLocker(player);
    }

    private void EnterLocker(MovementPlayer player)
    {
        _isOccupied = true;
        if (interactUI != null) interactUI.SetActive(false);

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.position = hidingPoint.position;
        player.transform.rotation = hidingPoint.rotation;

        player.SetHiddenStatus(true);
    }

    private void ExitLocker(MovementPlayer player)
    {
        _isOccupied = false;

        player.transform.position = exitPoint.position;

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = true;

        player.SetHiddenStatus(false);
    }

    private void OnDrawGizmos()
    {
        // interactionRadius juga digunakan di sini untuk visualisasi Editor
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}