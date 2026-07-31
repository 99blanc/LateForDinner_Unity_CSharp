public class UIScreen : UserInterface
{
    public override void Init()
    {
        base.Init();
        var elements = GetComponentsInChildren<UIElement>(true);

        for (int index = 0; index < elements.Length; index++)
            elements[index]?.Init();
    }

    public virtual void Close()
    {
        var elements = GetComponentsInChildren<UIElement>(true);

        foreach (var element in elements)
            element?.Close();

        Managers.UI.Close(this);
    }
}
