public class UIPopup : UserInterface
{
    public override void Get()
    {
        base.Get();
        Managers.UI.Focus(this);
    }

    public override void Release()
    {
        base.Release();
        Managers.UI.Close(this);
    }
}
