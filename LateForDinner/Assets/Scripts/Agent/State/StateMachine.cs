using R3;
using UnityEngine;

public class StateMachine
{
    private readonly ReactiveProperty<State> state = new();
    public ReadOnlyReactiveProperty<State> OnStateChanged => state;
    public State curState
    {
        get => state.Value;
        private set => state.Value = value;
    }

    public void Init(State initState)
    {
        curState = initState;
        curState.Enter();
    }

    public void Change(State next, Vector2 input = default, bool force = false)
    {
        if (!force && curState == next) 
            return;

        if (!force && !curState.Transition(input))
            return;

        curState?.Exit();
        (curState = next).Enter();
    }
}