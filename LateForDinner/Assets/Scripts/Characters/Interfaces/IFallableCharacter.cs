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
    }
}
