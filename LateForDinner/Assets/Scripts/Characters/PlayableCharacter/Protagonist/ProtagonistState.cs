using System;
using UnityEngine;

public class ProtagonistJumpState : JumpState
{
    private Protagonist _protagonist;

    public ProtagonistJumpState(Character owner, Func<float> inputProvider) : base(owner, inputProvider) { }

    public override void OnEnter()
    {
        base.OnEnter();
        _protagonist ??= Owner as Protagonist;
        PlayJumpAnimation();
    }

    public override void OnLogic()
    {
        base.OnLogic();
        _protagonist ??= Owner as Protagonist;
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
    protected readonly Func<float> _inputProvider;
    private Protagonist _protagonist;

    public ProtagonistRollState(Character owner, Func<float> inputProvider) : base(owner)
        => _inputProvider = inputProvider;

    public override void OnEnter()
    {
        base.OnEnter();
        _protagonist ??= Owner as Protagonist;

        if (_protagonist?.CharacterAnimator is not ProtagonistAnimator protagonistAnimator)
            return;

        if (Owner is not IRollableCharacter rollable)
            return;

        float directionX = _inputProvider?.Invoke() ?? 0f;
        rollable.Roll(directionX);
        protagonistAnimator.PlayRoll();
    }

    public override void OnLogic()
    {
        base.OnLogic();

        if (Owner is not IRollableCharacter rollable)
            return;

        if (_inputProvider == null)
            return;

        float moveInput = _inputProvider.Invoke();

        if (Mathf.Abs(moveInput) > 0.01f)
            rollable.Roll(moveInput);
    }
}

public class ProtagonistDashState : DashState
{
    private Protagonist _protagonist;

    public ProtagonistDashState(Character owner, Func<Vector2> inputProvider) : base(owner, inputProvider) { }

    public override void OnEnter()
    {
        base.OnEnter();
        _protagonist ??= Owner as Protagonist;

        if (_protagonist is not IDashableCharacter dashable)
            return;

        dashable.Rigidbody.gravityScale = 0f;
        PlayDashAnimation();
    }

    public override void OnLogic()
    {
        base.OnLogic();
        _protagonist ??= Owner as Protagonist;
    }

    public override void OnExit()
    {
        base.OnExit();

        if (_protagonist is not IDashableCharacter dashable)
            return;

        if (_protagonist.IsGrounded() && _protagonist is IJumpableCharacter jumpable)
            jumpable.RemainingJumpCount = jumpable.MaxJumpCount;

        dashable.Rigidbody.gravityScale = 1f;
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
