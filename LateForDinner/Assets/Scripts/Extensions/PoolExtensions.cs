using System.Runtime.CompilerServices;

public static class PoolableExtensions
{
    private static readonly ConditionalWeakTable<IPoolable, Box<bool>> _pooledStates = new();

    private class Box<T> 
    { 
        public T Value; 
    }

    public static bool IsPooled(this IPoolable poolable) 
        => _pooledStates.TryGetValue(poolable, out var box) && box.Value;

    public static void SetPooled(this IPoolable poolable, bool value)
    {
        var box = _pooledStates.GetOrCreateValue(poolable);
        box.Value = value;
    }
}
