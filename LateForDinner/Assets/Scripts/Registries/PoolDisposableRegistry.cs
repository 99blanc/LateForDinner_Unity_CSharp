using R3;
using System;
using System.Collections.Generic;

internal static class PoolDisposableRegistry
{
    private static readonly Dictionary<IPoolable, DisposableBag> _bags = new();

    public static void Register(IPoolable owner, IDisposable disposable)
    {
        if (!_bags.TryGetValue(owner, out var bag))
        {
            bag = new DisposableBag();
            _bags[owner] = bag;
        }

        bag.Add(disposable);
    }

    public static void Clear(IPoolable owner)
    {
        if (_bags.TryGetValue(owner, out var bag))
        {
            bag.Dispose();
            _bags.Remove(owner);
        }
    }
}