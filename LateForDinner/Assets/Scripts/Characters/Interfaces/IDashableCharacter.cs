using System.Runtime.CompilerServices;
using UnityEngine;

public interface IDashableCharacter
{
    private static readonly ConditionalWeakTable<IDashableCharacter, DashStateValue> _dashValue = new ConditionalWeakTable<IDashableCharacter, DashStateValue>();
    private class DashStateValue
    {
        public int RemainingDashCount = -1;
        public float DurationTimer = 0f;
        public float OriginalGravityScale = 1f;
        public Vector2 DashDirection = Vector2.right;
        public CooldownRegistry CooldownRegistry;
    }
    SpriteRenderer Renderer { get; }
    Rigidbody2D Rigidbody { get; }
    AttributeRegistry Attributes { get; }
    int MaxDashCount => Attributes.Get<short>(AttributeType.DashCount).Value;
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
    public bool IsOnCooldown
    {
        get
        {
            var val = _dashValue.GetOrCreateValue(this);
            return val.CooldownRegistry != null && val.CooldownRegistry.IsOnCooldown;
        }
    }
    public bool IsDurationEnded 
        => _dashValue.GetOrCreateValue(this).DurationTimer <= 0f;
    public Vector2 DashDirection 
        => _dashValue.GetOrCreateValue(this).DashDirection;

    public void StartDashing(Vector2 inputDirection)
    {
        if (this is not Character || Rigidbody == null || Attributes == null)
            return;

        var val = _dashValue.GetOrCreateValue(this);

        if (val.CooldownRegistry == null)
        {
            val.CooldownRegistry = new CooldownRegistry(() =>
            {
                RemainingDashCount = Attributes.Get<short>(AttributeType.DashCount).Value;
            });
        }

        val.OriginalGravityScale = Rigidbody.gravityScale;
        val.DurationTimer = Define.Scaler.Duration;
        val.DashDirection = inputDirection;

        if (val.DashDirection == Vector2.zero)
            val.DashDirection = Renderer.flipX ? Vector2.left : Vector2.right;

        if (val.DashDirection.x != 0)
            Renderer.FlipX(val.DashDirection.x);

        float dashSpeed = Attributes.Get<float>(AttributeType.DashDistance).Value / Define.Scaler.Duration;
        Rigidbody.gravityScale = 0f;
        Rigidbody.linearVelocity = val.DashDirection.normalized * dashSpeed;
        RemainingDashCount--;

        if (RemainingDashCount <= 0)
        {
            float cooldownTime = Attributes.Get<float>(AttributeType.DashCooldown).Value;
            val.CooldownRegistry.CooldownTime = cooldownTime;
            val.CooldownRegistry.CurrentCooldown = cooldownTime;
            val.CooldownRegistry.IsOnCooldown = true;
            Managers.Cooldown.Register(val.CooldownRegistry);
        }
    }

    public void UpdateDashing(float deltaTime)
    {
        var val = _dashValue.GetOrCreateValue(this);

        if (val.DurationTimer > 0f)
            val.DurationTimer -= deltaTime;
    }

    public void StopDashing()
    {
        if (this is not Character || Rigidbody == null)
            return;

        var val = _dashValue.GetOrCreateValue(this);
        Rigidbody.gravityScale = val.OriginalGravityScale;
        Rigidbody.linearVelocity = Vector2.zero;
    }
}
