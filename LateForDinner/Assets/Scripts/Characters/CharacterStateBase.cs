using System;
using UnityHFSM;

public abstract class CharacterStateBase : StateBase<CharacterState>
{
    protected readonly Character Owner;

    protected CharacterStateBase(Character owner, bool exitTime = false) : base(exitTime)
        => Owner = owner;
}

public class IdleState : CharacterStateBase
{
    public IdleState(Character owner) : base(owner) { }

    public override void OnEnter()
    {
        base.OnEnter();
        Owner?.Animator?.SetBool("IsMoving", false);
    }

    public override void OnLogic()
    {
        base.OnLogic();

        if (Owner is IIdleable idleable)
            idleable.Idle();
    }
}

public class MoveState : CharacterStateBase
{
    private readonly Func<float> _inputProvider;

    public MoveState(Character owner, Func<float> inputProvider) : base(owner)
        => _inputProvider = inputProvider;

    public override void OnEnter()
    {
        base.OnEnter();
        Owner?.Animator?.SetBool("IsMoving", true);
    }

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