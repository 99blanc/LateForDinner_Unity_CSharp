public class UISubItem : UserInterface
{
    public virtual void CloseSubItem() => Managers.Resource.Destroy(gameObject);
}
