using R3;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.TextCore.Text;

public interface IDashableCharacter
{
    private static readonly ConditionalWeakTable<IDashableCharacter, DashStateValue> _dashValue = new ConditionalWeakTable<IDashableCharacter, DashStateValue>();
    private class DashStateValue
    {
        public float DurationTimer = 0f;
        public float OriginalGravityScale = 1f;
        public Vector2 DashDirection = Vector2.right;
        public CooldownRegistry CooldownRegistry;
        public bool IsInitialized = false;
    }
    SpriteRenderer Renderer { get; }
    Rigidbody2D Rigidbody { get; }
    AttributeRegistry Attributes { get; }
    int MaxDashCount => Attributes.GetBase<int>(AttributeType.DashCount).CurrentValue;
    int RemainDashCount
    {
        get => Attributes.Get<int>(AttributeType.DashCount).Value;
        set => Attributes.Set(AttributeType.DashCount, value);
    }
    public bool IsOnCooldown
    {
        get
        {
            var val = _dashValue.GetOrCreateValue(this);
            return val.CooldownRegistry != null && val.CooldownRegistry.IsOnCooldown;
        }
    }
    public bool IsDurationEnded => _dashValue.GetOrCreateValue(this).DurationTimer <= 0f;
    public Vector2 DashDirection => _dashValue.GetOrCreateValue(this).DashDirection;

    private static DashStateValue GetOrCreateDashState(IDashableCharacter character)
    {
        var val = _dashValue.GetOrCreateValue(character);

        if (!val.IsInitialized && character is MonoBehaviour mono)
        {
            val.IsInitialized = true;
            val.CooldownRegistry = new CooldownRegistry(() =>
            {
                character.RemainDashCount = character.MaxDashCount;
            });
            character.Attributes.Get<int>(AttributeType.DashCount)
            .Where(count => count <= 0)
            .Subscribe(_ =>
            {
                if (!val.CooldownRegistry.IsOnCooldown)
                {
                    float cooldownTime = character.Attributes.Get<float>(AttributeType.DashCooldown).CurrentValue;
                    val.CooldownRegistry.CooldownTime = cooldownTime;
                    val.CooldownRegistry.CurrentCooldown = cooldownTime;
                    val.CooldownRegistry.IsOnCooldown = true;
                    Managers.Cooldown.Register(val.CooldownRegistry);
                }
            }).RegisterToPool(character as IPoolable);
        }

        return val;
    }

    public void ResetDashCooldownAndRestoreCount()
    {
        var val = GetOrCreateDashState(this);

        if (val.CooldownRegistry != null && val.CooldownRegistry.IsOnCooldown)
        {
            Managers.Cooldown.Unregister(val.CooldownRegistry);
            val.CooldownRegistry.IsOnCooldown = false;
            val.CooldownRegistry.CurrentCooldown = 0f;
        }

        RemainDashCount = Mathf.Min(RemainDashCount + 1, MaxDashCount);
    }

    public void StartDashing(Vector2 inputDirection)
    {
        if (this is not Character || Rigidbody == null || Attributes == null)
            return;

        var val = GetOrCreateDashState(this);

        val.OriginalGravityScale = Rigidbody.gravityScale;
        val.DurationTimer = Define.Scaler.Duration;
        val.DashDirection = inputDirection;

        if (val.DashDirection == Vector2.zero)
            val.DashDirection = Renderer.flipX ? Vector2.left : Vector2.right;

        if (val.DashDirection.x != 0)
            Renderer.FlipX(val.DashDirection.x);

        float dashSpeed = Attributes.Get<float>(AttributeType.DashDistance).CurrentValue / Define.Scaler.Duration;
        Rigidbody.gravityScale = 0f;
        Rigidbody.linearVelocity = val.DashDirection.normalized * dashSpeed;
        RemainDashCount--;
    }

    public void UpdateDashing(float deltaTime)
    {
        var val = GetOrCreateDashState(this);
        if (val.DurationTimer > 0f)
            val.DurationTimer -= deltaTime;
    }

    public void StopDashing()
    {
        if (this is not Character || Rigidbody == null)
            return;

        var val = GetOrCreateDashState(this);
        Rigidbody.gravityScale = val.OriginalGravityScale;
        Rigidbody.linearVelocity = Vector2.zero;
    }
}
