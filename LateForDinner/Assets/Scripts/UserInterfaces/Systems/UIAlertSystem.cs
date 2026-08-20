using System.Collections.Generic;

public class UIAlertSystem : UISystem
{
    private enum Panels
    {
        AlertPanel
    }

    private readonly Queue<UIAlertSlot> _slots = new Queue<UIAlertSlot>();
    private const int MaxAlertCount = Define.Alert.Count;

    public override void Init()
    {
        base.Init();
        BindPanel(typeof(Panels));
    }

    public void PushAlert(string message)
    {
        var containerTransform = GetPanel((int)Panels.AlertPanel)?.transform;

        if (containerTransform == null)
            return;

        var (slot, _) = Managers.Pool.Pop<UIAlertSlot>(containerTransform);

        if (slot == null)
            return;

        if (_slots.Count >= MaxAlertCount)
        {
            var oldSlot = _slots.Dequeue();
            oldSlot.Release();
        }

        _slots.Enqueue(slot);
        slot.Setup(message, () => RemoveSlot(slot));
    }

    private void RemoveSlot(UIAlertSlot slot)
    {
        if (slot == null)
            return;

        slot.Release();
    }
}
