public class UIDisplay : UserInterface
{
    public override void Release()
    {
        base.Release();
        Managers.UI.Close(this);
    }
}
