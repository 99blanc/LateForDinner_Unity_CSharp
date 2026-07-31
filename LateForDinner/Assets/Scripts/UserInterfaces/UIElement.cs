public class UIElement : UserInterface
{
    public virtual void Close()
        => Managers.Pool.Push(this);
}
