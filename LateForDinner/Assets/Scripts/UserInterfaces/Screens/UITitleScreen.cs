using UnityEngine;

public class UITitleScreen : UIScreen
{
    private enum Panels
    {
        MainPanel,
        LoadPanel
    }

    public override void Init()
    {
        base.Init();
        BindCanvasGroup(typeof(Panels));
    }


}
