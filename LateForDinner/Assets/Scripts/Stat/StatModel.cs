using R3;
using System.Collections.Generic;
using Token.DATA;

public partial class StatModel : IAllView
{
    private readonly Dictionary<StatType, IStatView> stats = new();

    public ReadOnlyReactiveProperty<T> Stream<T>(StatType type) where T : struct => Get<T>(type);

    public ReactiveProperty<T> Get<T>(StatType type, T defaultValue = default) where T : struct
    {
        if (!stats.TryGetValue(type, out var stat))
        {
            stat = new StatView<T>(defaultValue);
            stats[type] = stat;
        }

        return ((StatView<T>)stat).property;
    }

    public void Set<T>(StatType type, T value) where T : struct => Get<T>(type).Value = value;
}
