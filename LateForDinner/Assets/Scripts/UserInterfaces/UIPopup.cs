public abstract class UIPopup : UserInterface
{
    public override void OnGet()
    {
        base.OnGet();
        Managers.UI.FocusPopup(this);
    }
}
