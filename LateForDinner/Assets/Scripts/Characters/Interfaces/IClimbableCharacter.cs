using System.Runtime.CompilerServices;
using UnityEngine;

public interface IClimbableCharacter
{
    private static readonly ConditionalWeakTable<IClimbableCharacter, ClimbStateValue> _climbValue = new ConditionalWeakTable<IClimbableCharacter, ClimbStateValue>();
    private class ClimbStateValue
    {
        public Ladder CurrentLadder = null;
        public bool IsClimbing = false;
        public float OriginalGravityScale = 1f;
        public CooldownRegistry EnterCooldownProxy = new CooldownRegistry();
        public CooldownRegistry ExitCooldownProxy = new CooldownRegistry();
        public CooldownRegistry GroundBufferCooldown = new CooldownRegistry();
    }

    SpriteRenderer Renderer { get; }
    Rigidbody2D Rigidbody { get; }
    AttributeRegistry Attributes { get; }

    public Ladder CurrentLadder
    {
        get => _climbValue.GetOrCreateValue(this).CurrentLadder;
        set => _climbValue.GetOrCreateValue(this).CurrentLadder = value;
    }

    public bool IsClimbing
    {
        get => _climbValue.GetOrCreateValue(this).IsClimbing;
        set => _climbValue.GetOrCreateValue(this).IsClimbing = value;
    }

    public bool CanClimb => !_climbValue.GetOrCreateValue(this).EnterCooldownProxy.IsOnCooldown && !_climbValue.GetOrCreateValue(this).ExitCooldownProxy.IsOnCooldown;

    public void StartGroundBuffer()
    {
        var val = _climbValue.GetOrCreateValue(this);

        if (!val.GroundBufferCooldown.IsOnCooldown)
        {
            float bufferTime = Define.Scaler.Buffer;
            val.GroundBufferCooldown.CooldownTime = bufferTime;
            val.GroundBufferCooldown.CurrentCooldown = bufferTime;
            val.GroundBufferCooldown.IsOnCooldown = true;
            Managers.Cooldown.Register(val.GroundBufferCooldown);
        }
    }

    public void ResetGroundBuffer()
    {
        var val = _climbValue.GetOrCreateValue(this);
        val.GroundBufferCooldown.IsOnCooldown = false;
        Managers.Cooldown.Unregister(val.GroundBufferCooldown);
    }

    public bool HasExceededGroundBuffer()
    {
        var val = _climbValue.GetOrCreateValue(this);
        bool hasExceeded = val.GroundBufferCooldown.IsOnCooldown == false && val.GroundBufferCooldown.CurrentCooldown <= 0f;

        if (hasExceeded && IsClimbing)
            StopClimbing();

        return val.GroundBufferCooldown.IsOnCooldown == false && val.GroundBufferCooldown.CurrentCooldown <= 0f;
    }

    public void StartClimbing(Ladder ladder)
    {
        if (this is not Character || Rigidbody == null || ladder == null || !CanClimb)
            return;

        var val = _climbValue.GetOrCreateValue(this);
        val.CurrentLadder = ladder;
        val.IsClimbing = true;
        float enterCooldownTime = Define.Scaler.Buffer;
        val.EnterCooldownProxy.CooldownTime = enterCooldownTime;
        val.EnterCooldownProxy.CurrentCooldown = enterCooldownTime;
        val.EnterCooldownProxy.IsOnCooldown = true;
        Managers.Cooldown.Register(val.EnterCooldownProxy);
    }

    public void StopClimbing()
    {
        if (this is not Character || Rigidbody == null)
            return;

        var val = _climbValue.GetOrCreateValue(this);
        val.IsClimbing = false;
        val.CurrentLadder = null;
        ResetGroundBuffer();

        float exitCooldownTime = Define.Scaler.Buffer;
        val.ExitCooldownProxy.CooldownTime = exitCooldownTime;
        val.ExitCooldownProxy.CurrentCooldown = exitCooldownTime;
        val.ExitCooldownProxy.IsOnCooldown = true;
        Managers.Cooldown.Register(val.ExitCooldownProxy);
    }

    public void Climb(float verticalInput)
    {
        if (this is not Character || Rigidbody == null || Attributes == null || !IsClimbing)
            return;

        ApplyClimbMovement(verticalInput);
    }

    private void ApplyClimbMovement(float verticalInput)
    {
        var val = _climbValue.GetOrCreateValue(this);
        float targetVelocityX = 0f;

        if (val.CurrentLadder != null)
        {
            float targetX = val.CurrentLadder.transform.position.x;
            float currentX = Rigidbody.position.x;
            float xDifference = targetX - currentX;
            targetVelocityX = xDifference * 15f;
        }

        float maxClimbSpeed = Attributes.Get<float>(AttributeType.MoveSpeed).Value * 0.5f;
        float targetVelocityY = verticalInput * maxClimbSpeed;
        Rigidbody.linearVelocity = new Vector2(targetVelocityX, targetVelocityY);
    }
}
