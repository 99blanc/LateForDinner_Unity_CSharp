using System;
using UnityEngine;

public class ProtagonistCrouchState : CharacterState
{
    private readonly Protagonist _protagonist;

    public ProtagonistCrouchState(Character owner) : base(owner)
        => _protagonist = owner as Protagonist;

    public override void OnEnter()
    {
        base.OnEnter();

        if (_protagonist is not ICrouchableCharacter crouchable)
            return;

        crouchable.Crouch();
        PlayCrouchAnimation();
    }

    private void PlayCrouchAnimation()
    {
        if (_protagonist?.CharacterAnimator is not ProtagonistAnimator protagonistAnimator)
            return;

        protagonistAnimator.PlayCrouch();
    }
}

public class ProtagonistJumpState : JumpState
{
    private readonly Protagonist _protagonist;

    public ProtagonistJumpState(Character owner, Func<float> inputProvider) : base(owner, inputProvider)
        => _protagonist = Owner as Protagonist;

    public override void OnEnter()
    {
        base.OnEnter();
        PlayJumpAnimation();
    }

    private void PlayJumpAnimation()
    {
        if (_protagonist?.CharacterAnimator is not ProtagonistAnimator protagonistAnimator)
            return;

        if (_protagonist is not IJumpableCharacter jumpable)
            return;

        if (jumpable.RemainingJumpCount >= jumpable.MaxJumpCount - 1)
            protagonistAnimator.PlayJump();
        else
            protagonistAnimator.PlayDoubleJump();
    }
}

public class ProtagonistRollState : CharacterState
{
    private readonly Protagonist _protagonist;
    protected readonly Func<float> _inputProvider;

    public ProtagonistRollState(Character owner, Func<float> inputProvider) : base(owner)
    {
        _inputProvider = inputProvider;
        _protagonist = Owner as Protagonist;
    }

    public override void OnEnter()
    {
        base.OnEnter();

        if (_protagonist is not IRollableCharacter rollable)
            return;

        float directionX = _inputProvider?.Invoke() ?? 0f;
        rollable.Roll(directionX);
        PlayRollAnimation();
    }

    public override void OnLogic()
    {
        base.OnLogic();

        if (_protagonist is not IRollableCharacter rollable)
            return;

        if (_inputProvider == null)
            return;

        float moveInput = _inputProvider.Invoke();

        if (Mathf.Abs(moveInput) > 0.01f)
            rollable.Roll(moveInput);
    }

    private void PlayRollAnimation()
    {
        if (_protagonist?.CharacterAnimator is not ProtagonistAnimator protagonistAnimator)
            return;

        protagonistAnimator.PlayRoll();
    }
}

public class ProtagonistDashState : DashState
{
    private Protagonist _protagonist;

    public ProtagonistDashState(Character owner, Func<Vector2> inputProvider) : base(owner, inputProvider) 
        => _protagonist = Owner as Protagonist;

    public override void OnEnter()
    {
        base.OnEnter();

        if (_protagonist is not IDashableCharacter dashable)
            return;

        PlayDashAnimation();
    }

    public override void OnExit()
    {
        base.OnExit();

        if (_protagonist is not IDashableCharacter dashable)
            return;

        if (_protagonist.IsGrounded() && _protagonist is IJumpableCharacter jumpable)
            jumpable.RemainingJumpCount = jumpable.MaxJumpCount;
    }

    private void PlayDashAnimation()
    {
        if (_protagonist?.CharacterAnimator is not ProtagonistAnimator protagonistAnimator)
            return;

        if (_protagonist is not IDashableCharacter dashable)
            return;

        Action playAnimation = (_dashDirection == Vector2.down || _dashDirection.y < -0.5f) ? protagonistAnimator.PlayDownDash : protagonistAnimator.PlayDash;
        playAnimation?.Invoke();
    }
}
