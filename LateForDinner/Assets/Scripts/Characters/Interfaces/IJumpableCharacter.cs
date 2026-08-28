using System.Runtime.CompilerServices;
using UnityEngine;

public interface IJumpableCharacter
{
    private static readonly ConditionalWeakTable<IJumpableCharacter, JumpStateValue> _jumpValue = new();
    private class JumpStateValue
    {
        public int RemainingJumpCount = -1;
    }
    SpriteRenderer Renderer { get; }
    Rigidbody2D Rigidbody { get; }
    AttributeRegistry Attributes { get; }
    int MaxJumpCount => Attributes.Get<short>(AttributeType.JumpCount).Value;
    int RemainingJumpCount
    {
        get
        {
            var val = _jumpValue.GetOrCreateValue(this);

            if (val.RemainingJumpCount < 0)
                val.RemainingJumpCount = MaxJumpCount;

            return val.RemainingJumpCount;
        }
        set => _jumpValue.GetOrCreateValue(this).RemainingJumpCount = value;
    }

    public void Jump(float directionX)
    {
        if (this is not Character || Rigidbody == null || Attributes == null)
            return;

        Renderer.FlipX(directionX);
        float jumpForce = Attributes.Get<float>(AttributeType.JumpForce).Value;
        Vector2 velocity = Rigidbody.linearVelocity;
        velocity.y = jumpForce;
        Rigidbody.linearVelocity = velocity;
        RemainingJumpCount--;
    }
}
