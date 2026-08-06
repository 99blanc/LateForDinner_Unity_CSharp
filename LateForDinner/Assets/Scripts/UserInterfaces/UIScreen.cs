public class UIScreen : UserInterface
{
    public override void Release()
    {
        base.Release();
        Managers.UI.Close(this);
    }
}
