using LateForDinner.Data;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZLinq;

public class CooldownManager
{
    private readonly HashSet<ICooldownable> _cooldownables = new HashSet<ICooldownable>();
    private readonly Dictionary<string, CooldownRegistry> _slotCooldowns = new Dictionary<string, CooldownRegistry>();

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

    public CooldownRegistry RegisterSlotCooldown(string slotKey, float duration, Action onComplete = null)
    {
        var registry = new CooldownRegistry(duration, () =>
        {
            _slotCooldowns.Remove(slotKey);
            onComplete?.Invoke();
        });

        registry.ID = slotKey;
        Register(registry);
        _slotCooldowns[slotKey] = registry;
        return registry;
    }

    public CooldownRegistry GetSlotCooldown(string slotKey)
    {
        _slotCooldowns.TryGetValue(slotKey, out var registry);
        return registry;
    }

    public void RestoreBuffs(List<ActiveBuffSaveData> savedBuffs, Character targetCharacter, Action<BuffCooldownRegistry> onBuffComplete = null)
    {
        if (savedBuffs == null || savedBuffs.Count == 0)
            return;

        foreach (var buffData in savedBuffs)
        {
            BuffCooldownRegistry buffRegistry = null;
            buffRegistry = new BuffCooldownRegistry(
            id: Guid.NewGuid().ToString(),
            itemID: buffData.ItemID,
            attributeKey: buffData.AttributeKey,
            value: buffData.Value,
            duration: buffData.RemainingTime,
            ticks: buffData.RemainingTicks,
            character: targetCharacter);
            Register(buffRegistry);
        }
    }

    public List<ActiveBuffSaveData> ExportBuffSaveData()
    {
        var list = new List<ActiveBuffSaveData>();

        foreach (var item in _cooldownables)
        {
            if (item is BuffCooldownRegistry buff)
            {
                list.Add(new ActiveBuffSaveData
                {
                    ItemID = buff.ItemID,
                    AttributeKey = buff.AttributeKey,
                    Value = buff.Value,
                    RemainingTime = buff.CurrentCooldown,
                    RemainingTicks = buff.RemainingTicks
                });
            }
        }

        return list;
    }
}
