using UnityEngine;

public interface IFallableCharacter
{
    SpriteRenderer Renderer { get; }
    Rigidbody2D Rigidbody { get; }
    AttributeRegistry Attributes { get; }

    public void Fall(float directionX)
    {
        if (this is not Character || Rigidbody == null || Attributes == null)
            return;

        Renderer.FlipX(directionX);
        float maxMoveSpeed = Attributes.Get<float>(AttributeType.MoveSpeed).Value;
        float currentVelocityX = Rigidbody.linearVelocity.x;
        float targetVelocityX = directionX * maxMoveSpeed;
        float newVelocityX = Mathf.MoveTowards(currentVelocityX, targetVelocityX, Attributes.Get<float>(AttributeType.Acceleration).Value * 0.5f * Time.fixedDeltaTime);
        Vector2 velocity = Rigidbody.linearVelocity;
        velocity.x = newVelocityX;
        Rigidbody.linearVelocity = velocity;
    }
}
