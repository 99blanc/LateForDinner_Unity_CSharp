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
    }
}
