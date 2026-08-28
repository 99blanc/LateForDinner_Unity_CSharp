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

        if (Owner is not IIdleableCharacter)
            return;

        Owner.CharacterAnimator?.PlayIdle();
    }

    public override void OnLogic()
    {
        base.OnLogic();

        if (Owner is not IIdleableCharacter idleable)
            return;

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

        if (Owner is not IMovableCharacter)
            return;

        Owner.CharacterAnimator?.PlayMove();
    }

    public override void OnLogic()
    {
        base.OnLogic();

        if (Owner is not IMovableCharacter)
            return;

        if (_inputProvider == null)
            return;

        float moveInput = _inputProvider.Invoke();

        if (Owner is IMovableCharacter movable)
            movable.Move(moveInput);
    }
}

public class FallState : CharacterState
{
    protected readonly Func<float> _inputProvider;

    public FallState(Character owner, Func<float> inputProvider) : base(owner)
        => _inputProvider = inputProvider;

    public override void OnEnter()
    {
        base.OnEnter();

        if (Owner is not IFallableCharacter)
            return;

        Owner.CharacterAnimator?.PlayFall();
    }

    public override void OnLogic()
    {
        base.OnLogic();

        if (Owner is not IFallableCharacter)
            return;

        if (_inputProvider == null)
            return;

        float moveInput = _inputProvider.Invoke();

        if (Owner is IFallableCharacter fallable)
            fallable.Fall(moveInput);
        else if (Owner is IMovableCharacter movable)
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

        if (Owner is not IJumpableCharacter jumpable)
            return;

        if (jumpable.RemainingJumpCount < 0)
            jumpable.RemainingJumpCount = jumpable.MaxJumpCount;

        Owner.CharacterAnimator?.PlayJump();
        float directionX = _inputProvider?.Invoke() ?? 0f;
        jumpable.Jump(directionX);
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

public class DashState : CharacterState
{
    protected readonly Func<Vector2> _inputProvider;
    protected Vector2 _dashDirection;
    private float _durationTimer;
    public bool IsDurationEnded => _durationTimer <= 0f;

    public DashState(Character owner, Func<Vector2> inputProvider) : base(owner)
        => _inputProvider = inputProvider;

    public override void OnEnter()
    {
        base.OnEnter();

        if (Owner is not IDashableCharacter dashable) 
            return;

        _durationTimer = Define.Scaler.Duration;
        _dashDirection = _inputProvider?.Invoke() ?? Vector2.right;

        if (_dashDirection == Vector2.zero)
            _dashDirection = Owner.Renderer.flipX ? Vector2.left : Vector2.right;

        dashable.Dash(_dashDirection);
        Owner.CharacterAnimator?.PlayDash();
    }

    public override void OnLogic()
    {
        base.OnLogic();

        if (Owner is not IDashableCharacter dashable)
            return;

        _durationTimer -= Time.deltaTime;
    }
}
