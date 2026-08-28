public class Protagonist : PlayableCharacter
{
    public override CharacterAnimator CharacterAnimator => _protagonistAnimator;
    protected override CharacterID CharacterID => CharacterID.Protagonist;
    private ProtagonistAnimator _protagonistAnimator;

    protected override void CacheComponents()
    {
        base.CacheComponents();
        _protagonistAnimator = this.FindChildAssert<ProtagonistAnimator>(recursive: true);
    }
}
