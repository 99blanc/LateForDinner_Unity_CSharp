public abstract class UIPopup : UserInterface
{
    public override void Get()
    {
        base.Get();
        Managers.UI.Focus(this);
    }
}
