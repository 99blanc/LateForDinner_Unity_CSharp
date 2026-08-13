public class UISystem : UserInterface
{
    public override void Release()
    {
        base.Release();
        Managers.Pool.Push(this);
    }
}
