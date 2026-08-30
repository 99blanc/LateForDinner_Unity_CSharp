using System.Runtime.CompilerServices;
using UnityEngine;

public interface IClimbableCharacter
{
    private static readonly ConditionalWeakTable<IClimbableCharacter, ClimbStateValue> _climbValue = new ConditionalWeakTable<IClimbableCharacter, ClimbStateValue>();
    private class ClimbStateValue
    {
        public Ladder CurrentLadder = null;
        public bool IsClimbing = false;
        public CooldownRegistry ExitCooldown = new CooldownRegistry();
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
    public bool CanForceExit => this is Character character && character.IsGrounded();
    public bool IsExitLocked => _climbValue.GetOrCreateValue(this).ExitCooldown.IsOnCooldown && !CanForceExit;

    public void StartClimbing(Ladder ladder)
    {
        if (this is not Character || Rigidbody == null || ladder == null)
            return;

        var val = _climbValue.GetOrCreateValue(this);
        val.CurrentLadder = ladder;
        val.IsClimbing = true;
        Rigidbody.gravityScale = 0f;
        float bufferTime = Define.Scaler.Buffer;
        val.ExitCooldown.CooldownTime = bufferTime;
        val.ExitCooldown.CurrentCooldown = bufferTime;
        val.ExitCooldown.IsOnCooldown = true;
        Managers.Cooldown.Register(val.ExitCooldown);
    }

    public void StopClimbing()
    {
        if (this is not Character || Rigidbody == null)
            return;

        var val = _climbValue.GetOrCreateValue(this);
        val.IsClimbing = false;
        val.CurrentLadder = null;
        Rigidbody.gravityScale = 1f;
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
