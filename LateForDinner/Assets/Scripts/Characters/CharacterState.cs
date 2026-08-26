using System;
using UnityHFSM;

public abstract class CharacterState : StateBase<CharacterStateType>
{
    protected readonly Character Owner;

    protected CharacterState(Character owner, bool hasExitTime = false) : base(hasExitTime)
        => Owner = owner;
}

public class IdleState : CharacterState
{
    public IdleState(Character owner) : base(owner) { }

    public override void OnEnter()
        => base.OnEnter();

    public override void OnLogic()
    {
        base.OnLogic();

        if (Owner is IIdleable idleable)
            idleable.Idle();
    }
}

public class MoveState : CharacterState
{
    private readonly Func<float> _inputProvider;

    public MoveState(Character owner, Func<float> inputProvider) : base(owner)
        => _inputProvider = inputProvider;

    public override void OnEnter()
        => base.OnEnter();

    public override void OnLogic()
    {
        base.OnLogic();

        if (_inputProvider == null)
            return;

        float moveInput = _inputProvider.Invoke();

        if (Owner is IMovable movable)
            movable.Move(moveInput);
    }
}