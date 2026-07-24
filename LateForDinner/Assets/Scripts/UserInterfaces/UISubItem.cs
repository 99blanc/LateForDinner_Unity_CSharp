public class UISubItem : UserInterface
{
    public override void Init()
        => base.Init();

    public virtual void Close()
        => Managers.Pool.Push(this);
}
