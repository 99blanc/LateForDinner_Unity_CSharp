using R3;
using System;
using UnityEngine;

public class CooldownRegistry : ICooldownable
{
    public string ID { get; set; }
    public float CooldownTime { get; set; }
    public float CurrentCooldown { get; set; }
    public bool IsOnCooldown { get; set; }
    public readonly ReactiveProperty<float> CooldownProgress = new ReactiveProperty<float>(default);
    private Action _onComplete;

    public CooldownRegistry(Action onComplete = null)
        => _onComplete = onComplete;

    public CooldownRegistry(float cooldownTime, Action onComplete = null)
    {
        CooldownTime = cooldownTime;
        CurrentCooldown = cooldownTime;
        IsOnCooldown = true;
        CooldownProgress.Value = 1f;
        _onComplete = onComplete;
    }

    public virtual void TickCooldown(float deltaTime)
    {
        if (!IsOnCooldown)
            return;

        CurrentCooldown -= deltaTime;

        if (CooldownTime > 0f)
            CooldownProgress.Value = Mathf.Clamp01(CurrentCooldown / CooldownTime);

        if (CurrentCooldown <= 0f)
        {
            CurrentCooldown = 0f;
            CooldownProgress.Value = 0f;
            IsOnCooldown = false;
            OnCooldownComplete();
            Managers.Cooldown.Unregister(this);
        }
    }

    public void OnCooldownComplete()
    {
        IsOnCooldown = false;
        CooldownProgress.Value = 0f;
        _onComplete?.Invoke();
    }
}

public class BuffCooldownRegistry : CooldownRegistry
{
    public int ItemID { get; private set; }
    public string AttributeKey { get; private set; }
    public string Value { get; private set; }
    public int RemainingTicks { get; set; }
    private Character _targetCharacter;
    private float _tickTimer;

    public BuffCooldownRegistry(string id, int itemID, string attributeKey, string value, float duration, int ticks, Character character) : base()
    {
        ID = id;
        ItemID = itemID;
        AttributeKey = attributeKey;
        Value = value;
        CooldownTime = duration;
        CurrentCooldown = duration;
        IsOnCooldown = true;
        RemainingTicks = ticks;
        _targetCharacter = character;
        _tickTimer = 1f;
    }

    public override void TickCooldown(float deltaTime)
    {
        if (!IsOnCooldown)
            return;

        CurrentCooldown -= deltaTime;
        _tickTimer -= deltaTime;

        if (_tickTimer <= 0f && RemainingTicks > 0)
        {
            _tickTimer = 1f;
            ExecuteTick();
        }

        if (CurrentCooldown <= 0f)
        {
            CurrentCooldown = 0f;
            IsOnCooldown = false;
            OnCooldownComplete();
            Managers.Cooldown.Unregister(this);
        }
    }

    private void ExecuteTick()
    {
        if (_targetCharacter?.Attributes == null || Enum.TryParse<AttributeType>(AttributeKey, out var attributeType) == false)
            return;

        _targetCharacter.Attributes.AddValue(attributeType, Value);
        RemainingTicks--;
    }
}
