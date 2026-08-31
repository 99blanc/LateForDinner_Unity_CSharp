using System;
using UnityEngine;

public class PlayableCharacterCrouchState : CharacterState
{
    private readonly PlayableCharacter _player;

    public PlayableCharacterCrouchState(Character owner) : base(owner)
        => _player = owner as PlayableCharacter;

    public override void OnEnter()
    {
        base.OnEnter();

        if (_player is not ICrouchableCharacter crouchable)
            return;

        crouchable.Crouch();
        PlayCrouchAnimation();
    }

    public override void OnExit()
    {
        base.OnExit();

        if (_player is not ICrouchableCharacter crouchable)
            return;

        crouchable.StandUp();
    }

    private void PlayCrouchAnimation()
    {
        if (_player?.CharacterAnimator is not PlayableCharacterAnimator playableAnimator)
            return;

        playableAnimator.PlayState(CharacterStateType.Crouch);
    }
}

public class PlayableCharacterJumpState : JumpState
{
    private readonly PlayableCharacter _player;

    public PlayableCharacterJumpState(Character owner, Func<float> inputProvider) : base(owner, inputProvider)
        => _player = Owner as PlayableCharacter;

    public override void OnEnter()
    {
        base.OnEnter();
        PlayJumpAnimation();
    }

    private void PlayJumpAnimation()
    {
        if (_player?.CharacterAnimator is not PlayableCharacterAnimator playableAnimator)
            return;

        if (_player is not IJumpableCharacter jumpable)
            return;

        bool isHoldingProp = _player is ICarriableCharacter carriable && carriable.IsHoldingProp;

        if (jumpable.RemainJumpCount >= jumpable.MaxJumpCount - 1 || isHoldingProp)
        {
            playableAnimator.PlayState(CharacterStateType.Jump);
            return;
        }

        playableAnimator.PlayState(CharacterStateType.DoubleJump);
    }
}

public class PlayableCharacterRollState : CharacterState
{
    protected readonly Func<float> _inputProvider;
    private readonly PlayableCharacter _player;

    public PlayableCharacterRollState(Character owner, Func<float> inputProvider) : base(owner)
    {
        _inputProvider = inputProvider;
        _player = Owner as PlayableCharacter;
    }

    public override void OnEnter()
    {
        base.OnEnter();

        if (_player is not IRollableCharacter rollable)
            return;

        float directionX = _inputProvider?.Invoke() ?? 0f;
        rollable.Roll(directionX);
        PlayRollAnimation();
    }

    public override void OnLogic()
    {
        base.OnLogic();

        if (_player is not IRollableCharacter rollable)
            return;

        if (_inputProvider == null)
            return;

        float moveInput = _inputProvider.Invoke();

        if (Mathf.Abs(moveInput) > 0.01f)
            rollable.Roll(moveInput);
    }

    private void PlayRollAnimation()
    {
        if (_player?.CharacterAnimator is not PlayableCharacterAnimator playableAnimator)
            return;

        playableAnimator.PlayState(CharacterStateType.Roll);
    }
}

public class PlayableCharacterDashState : DashState
{
    private PlayableCharacter _player;

    public PlayableCharacterDashState(Character owner, Func<Vector2> inputProvider) : base(owner, inputProvider)
        => _player = Owner as PlayableCharacter;

    public override void OnEnter()
    {
        base.OnEnter();

        if (_player is not IDashableCharacter dashable)
            return;

        PlayDashAnimation(dashable);
    }

    public override void OnExit()
    {
        base.OnExit();

        if (_player is not IDashableCharacter dashable)
            return;

        if (_player.IsGrounded() && _player is IJumpableCharacter jumpable)
            jumpable.RemainJumpCount = jumpable.MaxJumpCount;
    }

    private void PlayDashAnimation(IDashableCharacter dashable)
    {
        if (_player?.CharacterAnimator is not PlayableCharacterAnimator playableAnimator)
            return;

        Vector2 dir = dashable.DashDirection;
        Action playAnimation = (dir == Vector2.down || dir.y < -0.5f) ? () => playableAnimator.PlayState(CharacterStateType.DownDash) : () => playableAnimator.PlayState(CharacterStateType.Dash);
        playAnimation?.Invoke();
    }
}
