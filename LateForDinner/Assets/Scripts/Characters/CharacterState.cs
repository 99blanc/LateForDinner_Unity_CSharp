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

        Owner?.CharacterAnimator?.PlayState(CharacterStateType.Idle);
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

        Owner?.CharacterAnimator?.PlayState(CharacterStateType.Move);
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

        Owner?.CharacterAnimator?.PlayState(CharacterStateType.Fall);
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

        Owner?.CharacterAnimator?.PlayState(CharacterStateType.Jump);
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

    public DashState(Character owner, Func<Vector2> inputProvider) : base(owner)
        => _inputProvider = inputProvider;

    public override void OnEnter()
    {
        base.OnEnter();

        if (Owner is not IDashableCharacter dashable)
            return;

        Vector2 inputDir = _inputProvider?.Invoke() ?? Vector2.right;
        Owner?.CharacterAnimator?.PlayState(CharacterStateType.Dash);
        dashable.StartDashing(inputDir);
    }

    public override void OnLogic()
    {
        base.OnLogic();

        if (Owner is not IDashableCharacter dashable)
            return;

        dashable.UpdateDashing(Time.deltaTime);
    }

    public override void OnExit()
    {
        base.OnExit();

        if (Owner is not IDashableCharacter dashable)
            return;

        dashable.StopDashing();
    }
}

public class ClimbState : CharacterState
{
    protected readonly Func<float> _inputProvider;

    public ClimbState(Character owner, Func<float> inputProvider) : base(owner)
        => _inputProvider = inputProvider;

    public override void OnEnter()
    {
        base.OnEnter();

        if (Owner is not IClimbableCharacter climbable)
            return;

        if (Owner.CurrentInteractable is Ladder ladder)
            climbable.StartClimbing(ladder);

        Owner?.CharacterAnimator?.PlayState(CharacterStateType.Climb);
        Owner?.CharacterAnimator?.SetAnimatorSpeed(1f);
    }

    public override void OnLogic()
    {
        base.OnLogic();

        if (Owner is not IClimbableCharacter climbable)
            return;

        if (Owner.IsGrounded())
            climbable.StartGroundBuffer();
        else
            climbable.ResetGroundBuffer();

        if (_inputProvider == null)
            return;

        float verticalInput = _inputProvider.Invoke();
        climbable.Climb(verticalInput);
        UpdateClimbAnimation(verticalInput);
    }

    public override void OnExit()
    {
        base.OnExit();

        if (Owner is not IClimbableCharacter climbable)
            return;

        climbable.ResetGroundBuffer();
        Owner?.CharacterAnimator?.SetAnimatorSpeed(1f);
        climbable.StopClimbing();
    }

    private void UpdateClimbAnimation(float verticalInput)
    {
        if (Mathf.Abs(verticalInput) > 0.01f)
            Owner?.CharacterAnimator?.SetAnimatorSpeed(1f);
        else
            Owner?.CharacterAnimator?.SetAnimatorSpeed(0f);
    }
}
