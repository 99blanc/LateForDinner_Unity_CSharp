using UnityEngine;

public interface IRollableCharacter
{
    SpriteRenderer Renderer { get; }
    Rigidbody2D Rigidbody { get; }
    AttributeRegistry Attributes { get; }

    public void Roll(float directionX)
    {
        if (this is not Character || Rigidbody == null || Attributes == null)
            return;

        Renderer.FlipX(directionX);
        float maxMoveSpeed = Attributes.Get<float>(AttributeType.MoveSpeed).CurrentValue;
        float acceleration = Attributes.Get<float>(AttributeType.Acceleration).CurrentValue;
        float deceleration = Attributes.Get<float>(AttributeType.Deceleration).CurrentValue;
        float turnDeceleration = Attributes.Get<float>(AttributeType.TurnDeceleration).CurrentValue;
        float targetVelocityX = directionX * maxMoveSpeed;
        float currentVelocityX = Rigidbody.linearVelocity.x;
        float rate = DetermineRate(currentVelocityX, directionX, acceleration, deceleration, turnDeceleration);
        float newVelocityX = Mathf.MoveTowards(currentVelocityX, targetVelocityX, rate * Time.fixedDeltaTime);
        Vector2 velocity = Rigidbody.linearVelocity;
        velocity.x = newVelocityX;
        Rigidbody.linearVelocity = velocity;
    }

    private float DetermineRate(float currentVelocityX, float directionX, float acceleration, float deceleration, float turnDeceleration)
    {
        if (Mathf.Abs(directionX) > 0.01f)
        {
            bool isTurning = Mathf.Sign(currentVelocityX) != Mathf.Sign(directionX) && Mathf.Abs(currentVelocityX) > 0.1f;
            return isTurning ? turnDeceleration : acceleration;
        }

        return deceleration;
    }
}
