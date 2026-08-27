using R3;
using System;
using System.Collections.Generic;

internal static class PoolDisposableRegistry
{
    private static readonly Dictionary<IPoolablePrefab, DisposableBag> _bags = new Dictionary<IPoolablePrefab, DisposableBag>();

    public static void Register(IPoolablePrefab owner, IDisposable disposable)
    {
        if (!_bags.TryGetValue(owner, out var bag))
        {
            bag = new DisposableBag();
            _bags[owner] = bag;
        }

        bag.Add(disposable);
    }

    public static void Clear(IPoolablePrefab owner)
    {
        if (_bags.TryGetValue(owner, out var bag))
        {
            bag.Dispose();
            _bags.Remove(owner);
        }
    }
}