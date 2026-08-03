public class UISlot : UserInterface
{
    public virtual void Close()
        => Managers.Pool.Push(this);
}
