using UnityEngine;

public interface IIdleable
{
    Rigidbody2D Rigidbody { get; }

    public void Idle()
    {
        if (this is not Character || Rigidbody == null)
            return;

        Vector2 velocity = Rigidbody.linearVelocity;
        velocity.x = 0f;
        Rigidbody.linearVelocity = velocity;
    }
}
