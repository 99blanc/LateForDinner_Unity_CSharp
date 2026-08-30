using UnityHFSM;

public class Protagonist : PlayableCharacter
{
    public override CharacterAnimator CharacterAnimator => _protagonistAnimator;
    protected override CharacterID CharacterID => CharacterID.Protagonist;
    private PlayableCharacterAnimator _protagonistAnimator;

    protected override void RegisterStates(StateMachine<CharacterStateType> fsm)
    {
        base.RegisterStates(fsm);
        fsm.AddState(CharacterStateType.Crouch, new PlayableCharacterCrouchState(this));
        fsm.AddState(CharacterStateType.Jump, new PlayableCharacterJumpState(this, GetPlayerMoveInput));
        fsm.AddState(CharacterStateType.Roll, new PlayableCharacterRollState(this, GetPlayerMoveInput));
        fsm.AddState(CharacterStateType.Dash, new PlayableCharacterDashState(this, GetPlayerDashInput));
    }

    protected override void CacheComponents()
    {
        base.CacheComponents();
        _protagonistAnimator = this.FindChildAssert<PlayableCharacterAnimator>(recursive: true);
    }
}
