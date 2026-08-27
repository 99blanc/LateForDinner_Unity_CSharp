using System;
using System.Runtime.CompilerServices;

public static class PoolableExtensions
{
    private static readonly ConditionalWeakTable<IPoolablePrefab, Box<bool>> _pooledStates = new ConditionalWeakTable<IPoolablePrefab, Box<bool>>();

    private class Box<T> 
    { 
        public T Value; 
    }

    public static bool IsPooled(this IPoolablePrefab poolable) 
        => _pooledStates.TryGetValue(poolable, out var box) && box.Value;

    public static void SetPooled(this IPoolablePrefab poolable, bool value)
    {
        var box = _pooledStates.GetOrCreateValue(poolable);
        box.Value = value;
    }

    public static T AddToPool<T>(this T disposable, IPoolablePrefab owner) where T : IDisposable
    {
        PoolDisposableRegistry.Register(owner, disposable);
        return disposable;
    }
}
