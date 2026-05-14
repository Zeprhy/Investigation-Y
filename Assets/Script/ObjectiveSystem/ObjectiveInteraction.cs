using UnityEngine;
using System.Collections.Generic;

public class ObjectiveInteraction : MonoBehaviour
{
    public enum InteractionType {Obstacle, Evidence, Tool}

    [Header("Configuration")]
    [SerializeField] private InteractionType type;
    [SerializeField] private string itemName;
    [SerializeField] private string DialogText;
    [SerializeField] private ObjectiveSO relatedObjective;

    [Header("Requirement")]
    [SerializeField] private bool needItem;
    [SerializeField] private string requiredItemName;

    private static HashSet<string> _CollectedItems = new HashSet<string>();

    private void Awake()
    {
        _CollectedItems.Clear();    
    }

    public void Interaction()
    {
        Debug.Log("Interaksi terjadi pada: " + gameObject.name);
        switch (type)
        {
            case InteractionType.Obstacle:
                HandleObstacle();
                break;
            case InteractionType.Evidence:
                HandleEvidence();
                break;
            case InteractionType.Tool:
                HandleTool();
                break;
        }
    }

    private void HandleObstacle()
    {
        bool hasRequiredItem = !needItem ||_CollectedItems.Contains(requiredItemName);

        if (!hasRequiredItem)
        {
            DialogueManager.Instance.ShowDialogue(DialogText);
            if (relatedObjective != null)
                ObjectiveManager.Instance.SetNewObjective(relatedObjective);
        }
        else
        {
            if (relatedObjective != null)
                ObjectiveManager.Instance.CompleteObjetive(relatedObjective.objectiveID); 

            gameObject.SetActive(false);
        }
    }

    private void HandleTool()
    {
        if (!string.IsNullOrEmpty(itemName))
        {
            _CollectedItems.Add(itemName);
        }
        gameObject.SetActive(false);
    }

    private void HandleEvidence()
    {
        if (relatedObjective != null)
            ObjectiveManager.Instance.SetNewObjective(relatedObjective);
    }
}
