using UnityEngine;

public class UIScene : UserInterface
{
    private Canvas _canvas;
    public Canvas Canvas
    {
        get
        {
            if (_canvas == null)
                _canvas = gameObject.GetComponentAssert<Canvas>();

            return _canvas;
        }
    }

    public override void Init()
        => base.Init();

    public virtual void Close()
        => Managers.UI.CloseScene(this);
}
