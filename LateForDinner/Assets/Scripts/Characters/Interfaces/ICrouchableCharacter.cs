using UnityEngine;

public interface ICrouchableCharacter
{
    SpriteRenderer Renderer { get; }
    Rigidbody2D Rigidbody { get; }

    public void Crouch()
    {
        if (this is not Character || Rigidbody == null)
            return;

        Vector2 velocity = Rigidbody.linearVelocity;
        velocity.x = 0f;
        Rigidbody.linearVelocity = velocity;
    }
}
