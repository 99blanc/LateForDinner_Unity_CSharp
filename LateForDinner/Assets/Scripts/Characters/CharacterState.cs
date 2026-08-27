using System;
using UnityEngine;
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
    {
        base.OnEnter();

        if (Owner is IIdleableCharacter)
            Owner.CharacterAnimator?.PlayIdle();
    }

    public override void OnLogic()
    {
        base.OnLogic();

        if (Owner is IIdleableCharacter idleable)
            idleable.Idle();
    }
}

public class MoveState : CharacterState
{
    protected readonly Func<float> _inputProvider;

    public MoveState(Character owner, Func<float> inputProvider) : base(owner)
        => _inputProvider = inputProvider;

    public override void OnEnter()
    {
        base.OnEnter();

        if (Owner is IMovableCharacter)
            Owner.CharacterAnimator?.PlayMove();
    }

    public override void OnLogic()
    {
        base.OnLogic();

        if (_inputProvider == null)
            return;

        float moveInput = _inputProvider.Invoke();

        if (Owner is IMovableCharacter movable)
            movable.Move(moveInput);
    }
}

public class JumpState : CharacterState
{
    protected readonly Func<float> _inputProvider;

    public JumpState(Character owner, Func<float> inputProvider) : base(owner)
        => _inputProvider = inputProvider;

    public override void OnEnter()
    {
        base.OnEnter();

        if (Owner is IJumpableCharacter jumpable)
        {
            if (jumpable.RemainingJumpCount < 0)
                jumpable.RemainingJumpCount = jumpable.MaxJumpCount;

            Owner.CharacterAnimator?.PlayJump();
            float directionX = _inputProvider?.Invoke() ?? 0f;
            jumpable.Jump(directionX);
        }
    }

    public override void OnLogic()
    {
        base.OnLogic();

        if (Owner is not IJumpableCharacter jumpable)
            return;

        float directionX = _inputProvider?.Invoke() ?? 0f;

        if (Owner is IMovableCharacter movable)
            movable.Move(directionX);
    }
}
