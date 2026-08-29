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
        .Subscribe(_ => OnUpdate());
    }

    public void Register(ICooldownable cooldownable)
    {
        if (cooldownable != null && !_cooldownables.Contains(cooldownable))
            _cooldownables.Add(cooldownable);
    }

    public void Unregister(ICooldownable cooldownable)
    {
        if (cooldownable != null && _cooldownables.Contains(cooldownable))
            _cooldownables.Remove(cooldownable);
    }

    public void OnUpdate()
    {
        float deltaTime = Time.deltaTime;

        foreach (var item in _cooldownables.ToArray())
        {
            if (item == null) 
                continue;

            item.TickCooldown(deltaTime);

            if (!item.IsOnCooldown)
                Unregister(item);
        }
    }
}
