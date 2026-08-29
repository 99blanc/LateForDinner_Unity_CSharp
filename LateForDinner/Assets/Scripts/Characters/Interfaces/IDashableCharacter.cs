using System.Runtime.CompilerServices;
using UnityEngine;

public interface IDashableCharacter : ICooldownable
{
    private static readonly ConditionalWeakTable<IDashableCharacter, DashStateValue> _dashValue = new ConditionalWeakTable<IDashableCharacter, DashStateValue>();
    private class DashStateValue
    {
        public int RemainingDashCount = -1;
        public float CooldownTimer = 0f;
        public float CooldownTime = 0f;
        public bool IsCoolingDown = false;
    }
    SpriteRenderer Renderer { get; }
    Rigidbody2D Rigidbody { get; }
    AttributeRegistry Attributes { get; }
    int RemainingDashCount
    {
        get
        {
            var val = _dashValue.GetOrCreateValue(this);
            if (val.RemainingDashCount < 0)
                val.RemainingDashCount = Attributes.Get<short>(AttributeType.DashCount).Value;
            return val.RemainingDashCount;
        }
        set => _dashValue.GetOrCreateValue(this).RemainingDashCount = value;
    }
    float ICooldownable.CooldownTime
    {
        get => _dashValue.GetOrCreateValue(this).CooldownTime;
        set => _dashValue.GetOrCreateValue(this).CooldownTime = value;
    }
    float ICooldownable.CurrentCooldown
    {
        get => _dashValue.GetOrCreateValue(this).CooldownTimer;
        set => _dashValue.GetOrCreateValue(this).CooldownTimer = value;
    }
    bool ICooldownable.IsOnCooldown
    {
        get => _dashValue.GetOrCreateValue(this).IsCoolingDown;
        set => _dashValue.GetOrCreateValue(this).IsCoolingDown = value;
    }

    void ICooldownable.OnCooldownComplete()
        => RemainingDashCount = Attributes.Get<short>(AttributeType.DashCount).Value;

    public void Dash(Vector2 direction)
    {
        if (this is not Character || Rigidbody == null || Attributes == null)
            return;

        var cooldownable = (ICooldownable)this;

        if (direction.x != 0)
            Renderer.FlipX(direction.x);

        float dashSpeed = Attributes.Get<float>(AttributeType.DashDistance).Value / Define.Scaler.Duration;
        Rigidbody.linearVelocity = direction.normalized * dashSpeed;
        RemainingDashCount--;

        if (RemainingDashCount <= 0)
        {
            cooldownable.CooldownTime = Attributes.Get<float>(AttributeType.DashCooldown).Value;
            cooldownable.CurrentCooldown = Attributes.Get<float>(AttributeType.DashCooldown).Value;
            cooldownable.IsOnCooldown = true;
            Managers.Cooldown.Register(cooldownable);
        }
    }
}
