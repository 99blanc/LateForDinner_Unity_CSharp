using UnityEngine;

public class StateMachine
{
    public State curState { get; private set; }

    public void Init(State initState)
    {
        curState = initState;
        curState.Enter();
    }

    public void Change(State next, Vector2 input, bool force = false)
    {
        if (force || (curState != next && curState.Transition(input)))
        {
            curState?.Exit();
            (curState = next).Enter();
        }
    }
}