using UnityEngine;
using Token.DATA;

public abstract class Character<TComponent, TView, TData, TKey> : Agent<TComponent, TView, TData, TKey> where TComponent : Component where TView : class, IViewProvider where TData : IData<TKey>
{
    public ICharacterView characterView => registry;

    public override void Init(TData data) => base.Init(data);

    protected override void Components() { }

    protected override void ApplyRegistry(TData data) { }

    public virtual void RestoreHealth(short amount)
    {
        var cur = registry.Get<short>(StatType.CURRENT_HEALTH);
        var max = registry.Get<short>(StatType.MAX_HEALTH).Value;
        cur.Value = (short)Mathf.Min(cur.Value + amount, max);
    }

    public virtual void TakeDamage(short damage)
    {
        var cur = registry.Get<short>(StatType.CURRENT_HEALTH);
        var temp = registry.Get<short>(StatType.CURRENT_TEMP_HEALTH);
        short absorb = (short)Mathf.Min(damage, temp.Value);
        temp.Value = (short)Mathf.Max(temp.Value - absorb, 0);
        short remain = (short)(damage - absorb);
        cur.Value = (short)Mathf.Max(cur.Value - remain, 0);
    }
}
