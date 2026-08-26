using UnityEngine;

public static class CharacterExtensions
{
    public static void FlipX(this SpriteRenderer renderer, float directionX)
    {
        if (renderer == null)
            return;

        if (Mathf.Abs(directionX) > 0.01f)
            renderer.flipX = directionX < 0f;
    }
}
