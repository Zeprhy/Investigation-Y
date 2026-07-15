using UnityEngine;
using System.Collections;

public class ElevatorSlidingDoor : BaseDoor
{
    [Header("== Komponen Lift ==")]
    [SerializeField] private Animator elevatorAnimator;

    protected override void Awake()
    {
        base.Awake(); // Menjalankan setup radius bawaan BaseDoor
        if (elevatorAnimator == null) elevatorAnimator = GetComponent<Animator>();
    }

    protected override void Initialize()
    {
        base.Initialize();
        UpdateAnimator();
    }

   
    protected override void Update()
    {
        HandleUIDistance(); 
    }

    
    public override void Interact(GameObject player)
    {
        float distSqr = (transform.position - player.transform.position).sqrMagnitude;
        if (distSqr > _interactRadiusSqr) return;

        if (!isLocked)
        {
            ToggleDoor(player.transform.position);
        }
    }

   
    public override void ToggleDoor(Vector3 interactorPosition)
    {
        if (autoCloseCoroutine != null) StopCoroutine(autoCloseCoroutine);
    
        isOpen = !isOpen;
        UpdateAnimator();
    
        if (_doorObstacle != null) _doorObstacle.enabled = !isOpen;

        if (isOpen && useAutoClose) 
            autoCloseCoroutine = StartCoroutine(AutoCloseTimerLift());
        
        UpdateUIText(); 
    }

    public override void CloseDoor()
    {
        isOpen = false;
        UpdateAnimator();
        if (_doorObstacle != null) _doorObstacle.enabled = true;
        UpdateUIText();
    }

    private void UpdateAnimator()
    {
        if (elevatorAnimator != null)
            elevatorAnimator.SetBool("isOpen", isOpen);
    }

    private IEnumerator AutoCloseTimerLift()
    {
        yield return _autoCloseWait;
        if (isOpen) CloseDoor();
        autoCloseCoroutine = null;
    }

   
    protected override void UpdateUIText()
    {
        if (!_isPlayerNear || GameManager.Instance.interactionUIManager == null) return;

        string hintText = "";
        
        if (isLocked)
        {
            hintText = "[Locked] Elevator is Disabled";
        }
        else
        {
            hintText = isOpen ? "Press [F] To Close Elevator" : "Press [F] To Call Elevator";
        }

        GameManager.Instance.interactionUIManager.ShowText(hintText);
    }

    protected override void HideUIText()
    {
        if (GameManager.Instance.interactionUIManager != null)
            GameManager.Instance.interactionUIManager.HideText();
    }
}