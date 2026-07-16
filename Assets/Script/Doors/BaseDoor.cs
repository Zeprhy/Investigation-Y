using UnityEngine;
using DG.Tweening;

public class BaseDoor : MonoBehaviour
{
    [Header("Base Settings")]
    public bool isOpen = false;
    public bool isLocked = true;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float animationDuration = 0.5f; 

    protected Quaternion _targetRotation;
    protected Quaternion _defaultRotation;
    protected UnityEngine.AI.NavMeshObstacle _doorObstacle;

    protected virtual void Awake()
    {
        _doorObstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();

        if (_doorObstacle != null) 
            _doorObstacle.enabled = !isOpen;

        _defaultRotation = transform.localRotation;
        _targetRotation = _defaultRotation;
    }

    public virtual void Interact(GameObject player)
    {
        if (!isLocked)
        {
            ToggleDoor(player.transform.position);
        }
    }

    public void UnlockDoor()
    {
        isLocked = false;
    }

    public virtual void ToggleDoor(Vector3 interactorPosition)
    {
        isOpen = !isOpen;
    
        if (_doorObstacle != null) 
        {
            _doorObstacle.enabled = !isOpen; 
        }

        transform.DOKill();

        if (isOpen)
        {
            Vector3 directionToInteractor = transform.position - interactorPosition;
            float dot = Vector3.Dot(transform.forward, directionToInteractor);
            float angle = dot >= 0 ? openAngle : -openAngle;
            
            _targetRotation = _defaultRotation * Quaternion.Euler(0, angle, 0);

            transform.DOLocalRotateQuaternion(_targetRotation, animationDuration).SetEase(Ease.OutQuad);
        }
        else
        {
            CloseDoor();
        }
    }

    public virtual void CloseDoor()
    {
        isOpen = false;
        _targetRotation = _defaultRotation;
        
        if (_doorObstacle != null) 
            _doorObstacle.enabled = true;

        // Animasi DOTween menutup pintu
        transform.DOKill();
        transform.DOLocalRotateQuaternion(_defaultRotation, animationDuration).SetEase(Ease.OutQuad);
    }
}