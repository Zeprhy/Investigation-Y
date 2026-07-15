using System;
using UnityEngine;

public class GameplayStateManager : MonoBehaviour
{
    public GameplayState CurrentState { get; private set; }

    public event Action<GameplayState> OnStateChanged;

    public void Initialize()
    {
        SetState(GameplayState.Gameplay);
    }

    public void SetState(GameplayState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;

        OnStateChanged?.Invoke(CurrentState);

        Debug.Log($"Gameplay State -> {CurrentState}");
    }
}
