public class UISystem : UserInterface
{
    public virtual void Close()
    => Managers.Pool.Push(this);
}
