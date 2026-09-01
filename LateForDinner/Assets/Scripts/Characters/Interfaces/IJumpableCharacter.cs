using UnityEngine;

public interface IJumpableCharacter
{
    SpriteRenderer Renderer { get; }
    Rigidbody2D Rigidbody { get; }
    AttributeRegistry Attributes { get; }
    int MaxJumpCount => Attributes.GetBase<int>(AttributeType.JumpCount).Value;
    int RemainJumpCount
    {
        get => Attributes.Get<int>(AttributeType.JumpCount).Value;
        set => Attributes.Set(AttributeType.JumpCount, value);
    }

    public void RestoreJumpCount()
        => RemainJumpCount = Mathf.Min(RemainJumpCount + 1, MaxJumpCount);

    public void Jump(float directionX)
    {
        if (this is not Character || Rigidbody == null || Attributes == null)
            return;

        if (RemainJumpCount < 0)
            RemainJumpCount = MaxJumpCount;

        Renderer.FlipX(directionX);
        float jumpForce = Attributes.Get<float>(AttributeType.JumpForce).Value;
        Vector2 velocity = Rigidbody.linearVelocity;
        velocity.y = jumpForce;
        Rigidbody.linearVelocity = velocity;
        RemainJumpCount--;
    }
}
