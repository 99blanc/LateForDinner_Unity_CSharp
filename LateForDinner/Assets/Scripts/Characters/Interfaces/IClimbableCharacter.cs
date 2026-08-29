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
        public float XVelocity = 0f;
        public CooldownRegistry CooldownProxy = new CooldownRegistry();
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

    public bool CanClimb => !_climbValue.GetOrCreateValue(this).CooldownProxy.IsOnCooldown;

    public void StartClimbing(Ladder ladder)
    {
        if (this is not Character || Rigidbody == null || ladder == null)
            return;

        if (!CanClimb)
            return;

        var val = _climbValue.GetOrCreateValue(this);
        val.CurrentLadder = ladder;
        val.IsClimbing = true;
        val.OriginalGravityScale = Rigidbody.gravityScale;
        Rigidbody.gravityScale = 0f;
        Rigidbody.linearVelocity = Vector2.zero;
    }

    public void StopClimbing()
    {
        if (this is not Character || Rigidbody == null)
            return;

        var val = _climbValue.GetOrCreateValue(this);
        val.IsClimbing = false;
        val.CurrentLadder = null;
        Rigidbody.gravityScale = val.OriginalGravityScale;
        float exitCooldownTime = Define.Scaler.Threshold;
        val.CooldownProxy.CooldownTime = exitCooldownTime;
        val.CooldownProxy.CurrentCooldown = exitCooldownTime;
        val.CooldownProxy.IsOnCooldown = true;
        Managers.Cooldown.Register(val.CooldownProxy);
    }

    public void Climb(float verticalInput)
    {
        if (this is not Character || Rigidbody == null || Attributes == null || !IsClimbing)
            return;

        var val = _climbValue.GetOrCreateValue(this);

        if (val.CurrentLadder != null)
        {
            float targetX = val.CurrentLadder.transform.position.x;
            float currentX = Rigidbody.transform.position.x;
            float newX = Mathf.SmoothDamp(currentX, targetX, ref val.XVelocity, 0.08f, Mathf.Infinity, Time.fixedDeltaTime);
            Vector3 pos = Rigidbody.transform.position;
            pos.x = newX;
            Rigidbody.transform.position = pos;
        }

        float maxClimbSpeed = Attributes.Get<float>(AttributeType.MoveSpeed).Value * 0.5f;
        float targetVelocityY = verticalInput * maxClimbSpeed;
        Vector2 velocity = Rigidbody.linearVelocity;
        velocity.y = targetVelocityY;
        Rigidbody.linearVelocity = velocity;
    }
}
