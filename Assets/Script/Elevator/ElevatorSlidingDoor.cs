using UnityEngine;
using System.Collections;

public class ElevatorSlidingDoor : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator elevatorAnimator;
    [SerializeField] private UnityEngine.AI.NavMeshObstacle doorObstacle;

    [Header("Settings")]
    public bool isOpen = false;

    [Header("Auto Close")]
    [SerializeField] private bool useAutoClose = true;
    [SerializeField] private float autoCloseDelay = 3f;
    private Coroutine autoCloseCoroutine;

    private WaitForSeconds _autoCloseWait;

    void Awake()
    {
        _autoCloseWait = new WaitForSeconds(autoCloseDelay);
        if (elevatorAnimator == null) elevatorAnimator = GetComponent<Animator>();
    }

    void Start()
    {
        UpdateAnimator();
        if (doorObstacle != null) doorObstacle.enabled = !isOpen;
    }

    // This is now purely for logic, no UI text here
    public void Interact()
    {
        ToggleDoor();
    }

    private void ToggleDoor()
    {
        if (autoCloseCoroutine != null) StopCoroutine(autoCloseCoroutine);
    
        isOpen = !isOpen;
        UpdateAnimator();
    
        if (doorObstacle != null) doorObstacle.enabled = !isOpen;

        if (isOpen && useAutoClose) 
            autoCloseCoroutine = StartCoroutine(AutoCloseTimer());
    }

    private void UpdateAnimator()
    {
        if (elevatorAnimator != null)
            elevatorAnimator.SetBool("isOpen", isOpen);
    }

    private IEnumerator AutoCloseTimer()
    {
        yield return _autoCloseWait;
        if (isOpen) ToggleDoor();
        autoCloseCoroutine = null;
    }
}