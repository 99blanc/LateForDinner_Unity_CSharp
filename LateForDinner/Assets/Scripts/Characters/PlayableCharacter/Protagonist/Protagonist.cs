public class Protagonist : PlayableCharacter
{
    protected override CharacterID CharacterID => CharacterID.Protagonist;
    protected override CharacterAnimator CharacterAnimator => _protagonistAnimator;
    private ProtagonistAnimator _protagonistAnimator;

    protected override void CacheComponents()
    {
        base.CacheComponents();
        _protagonistAnimator = this.FindChildAssert<ProtagonistAnimator>(recursive: true);
    }
}
