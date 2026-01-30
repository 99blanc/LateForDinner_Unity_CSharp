public class StateMachine
{
    public State curState { get; private set; }

    public void Init(State initState)
    {
        curState = initState;
        curState.Enter();
    }

    public void ChangeState(State newState)
    {
        if (curState == newState)
            return;

        curState.Exit();
        curState = newState;
        curState.Enter();
    }
}