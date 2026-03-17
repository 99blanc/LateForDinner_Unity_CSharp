using R3;
using System;
using System.Collections.Generic;

public class StateMachine
{
    private readonly Dictionary<Type, State> maps = new();
    private readonly ReactiveProperty<State> state = new();
    public ReadOnlyReactiveProperty<State> OnStateChange => state;
    public State curState
    {
        get => state.Value;
        private set => state.Value = value;
    }

    public void Setup(params State[] states)
    {
        foreach (var state in states)
            maps[state.GetType()] = state;
    }

    public T Get<T>() where T : State
    {
        if (!maps.TryGetValue(typeof(T), out var value))
            throw new();

        return value as T;
    }

    public void Init<T>() where T : State
    {
        curState = Get<T>();
        curState.Enter();
    }

    public void Change(State next)
    {
        if (curState == next)
        {
            curState.Exit();
            curState.Enter();
            return;
        }

        curState.Exit();
        curState = next;
        curState.Enter();
    }

    public void Change<T>() where T : State => Change(Get<T>());
}
