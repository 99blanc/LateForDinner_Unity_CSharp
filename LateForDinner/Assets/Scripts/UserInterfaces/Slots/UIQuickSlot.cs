using UnityEngine;

public class UIQuickSlot : UISlot
{
    private enum Images
    {
        QuickSlotImage
    }

    private int _index;

    public override void OnInit()
    {
        base.OnInit();
        BindImage(typeof(Images));
    }

    public void SetIndex(int index)
    {
        _index = index;
        Refresh();
    }

    public override void Refresh()
    {
        base.Refresh();
    }
}
