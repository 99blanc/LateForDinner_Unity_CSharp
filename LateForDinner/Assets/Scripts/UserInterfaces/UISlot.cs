public class UISlot : UserInterface
{
    public override void Release()
    {
        base.Release();
        Managers.Pool.Push(this);
    }
}
