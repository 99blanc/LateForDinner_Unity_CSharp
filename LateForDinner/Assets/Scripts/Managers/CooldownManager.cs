using R3;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CooldownManager
{
    private readonly HashSet<ICooldownable> _cooldownables = new HashSet<ICooldownable>();

    public void Setup()
    {
        Observable.EveryUpdate()
        .Subscribe(_ =>
        {
            float deltaTime = Time.deltaTime;

            foreach (var item in _cooldownables.ToArray())
                item.TickCooldown(deltaTime);
        });
    }

    public void Register(ICooldownable cooldownable)
    {
        if (!_cooldownables.Contains(cooldownable))
            _cooldownables.Add(cooldownable);
    }

    public void Unregister(ICooldownable cooldownable)
    {
        if (_cooldownables.Contains(cooldownable))
            _cooldownables.Remove(cooldownable);
    }

    public void OnUpdate()
    {
        float deltaTime = Time.deltaTime;

        foreach (var item in _cooldownables.ToArray())
            item.TickCooldown(deltaTime);
    }
}
