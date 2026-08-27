using System;

public class ProtagonistJumpState : JumpState
{
    private Protagonist _protagonist;

    public ProtagonistJumpState(Character owner, Func<float> inputProvider) : base(owner, inputProvider) { }

    public override void OnEnter()
    {
        base.OnEnter();

        if (_protagonist == null)
            _protagonist = Owner as Protagonist;

        PlayJumpAnimation();

        if (_protagonist is IJumpableCharacter jumpable && jumpable.RemainingJumpCount < jumpable.MaxJumpCount - 1)
        {
            if (_protagonist.CharacterAnimator is ProtagonistAnimator protagonistAnimator)
                protagonistAnimator.PlayRoll();
        }
    }

    public override void OnLogic()
    {
        base.OnLogic();

        if (_protagonist == null)
            _protagonist = Owner as Protagonist;
    }

    private void PlayJumpAnimation()
    {
        if (_protagonist?.CharacterAnimator is not ProtagonistAnimator protagonistAnimator)
            return;

        if (_protagonist is not IJumpableCharacter jumpable)
            return;

        int maxJumps = jumpable.MaxJumpCount;

        if (jumpable.RemainingJumpCount == maxJumps - 1)
            protagonistAnimator.PlayJump();
        else
            protagonistAnimator.PlayDoubleJump();
    }
}
