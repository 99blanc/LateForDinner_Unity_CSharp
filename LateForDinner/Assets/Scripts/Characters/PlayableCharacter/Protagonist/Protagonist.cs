using UnityHFSM;

public class Protagonist : PlayableCharacter
{
    public override CharacterAnimator CharacterAnimator => _protagonistAnimator;
    protected override CharacterID CharacterID => CharacterID.Protagonist;
    private ProtagonistAnimator _protagonistAnimator;

    protected override void RegisterStates(StateMachine<CharacterStateType> fsm)
    {
        base.RegisterStates(fsm);
        fsm.AddState(CharacterStateType.Jump, new ProtagonistJumpState(this, GetPlayerMoveInput));
    }

    protected override void CacheComponents()
    {
        base.CacheComponents();
        _protagonistAnimator = this.FindChildAssert<ProtagonistAnimator>(recursive: true);
    }
}
