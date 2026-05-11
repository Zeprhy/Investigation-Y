using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class ValvePuzzleManager : MonoBehaviour
{
    [Header("Valve List")]
    [SerializeField] private List<ValveInteraction> valves = new List<ValveInteraction>();

    [Header("succes Events")]
    public UnityEvent onAllValveComplete;

    private int _completedCount = 0;

    void Start()
    {
        foreach (ValveInteraction valve in valves)
        {
            if (valve != null)
            {
                valve.onValveComplete.AddListener(CheckValves);
            }
        }

    }

    private void CheckValves()
    {
        _completedCount = 0;

        foreach (ValveInteraction valve in valves)
        {
            if (valve.IsComplete)
            {
                _completedCount++;
            }
        }

        if (_completedCount >= valves.Count)
        {
            CompletePuzzle();
        }
    }

    private void CompletePuzzle()
    {
        onAllValveComplete?.Invoke();
        this.enabled = false;
    }
}
