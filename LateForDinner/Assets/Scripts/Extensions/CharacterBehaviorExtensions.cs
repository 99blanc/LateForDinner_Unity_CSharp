using Cysharp.Text;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class CharacterBehaviorExtensions
{
    private static readonly Dictionary<CharacterID, (Type characterType, Type animatorType)> _caches = new Dictionary<CharacterID, (Type characterType, Type animatorType)>();

    public static bool IsGrounded(this Character character, Vector2? boxSize = null, LayerMask? groundLayer = null)
    {
        if (character == null)
            return false;

        Vector2 size = boxSize ?? new Vector2(0.25f, 0.05f);
        LayerMask layer = groundLayer ?? LayerMask.GetMask(Literal.Layers.Ground);
        Vector2 position = new Vector2(character.transform.position.x, character.Collider.bounds.min.y - 0.02f - (size.y * 0.5f));
        Collider2D collider = Physics2D.OverlapBox(position, size, 0f, layer);
        Color color = collider != null ? Color.green : Color.red;
        character.DrawDebugBoxLines(position, size, color, DebugExtensions.IsDebugDrawEnabled);
        return collider != null;
    }

    public static bool IsPlayable(this CharacterID characterID)
    {
        int id = (int)characterID;
        return Managers.Data.PlayableCharacters != null && Managers.Data.PlayableCharacters.ContainsKey(id);
    }

    public static string GetAnimatorOverrideControllerPath(this CharacterID characterID)
        => ZString.Concat(characterID.ToString(), Literal.Assets.Animator);


    public static void FlipX(this SpriteRenderer renderer, float directionX)
    {
        if (renderer == null)
            return;

        if (Mathf.Abs(directionX) > 0.01f)
            renderer.flipX = directionX < 0f;
    }

    public static float GetLookDirectionX(this SpriteRenderer renderer)
    {
        if (renderer == null)
            return 1f;

        return renderer.flipX ? -1f : 1f;
    }

    public static float GetLookDirectionX(this Character character)
    {
        if (character == null || character.Renderer == null)
            return 0f;

        return GetLookDirectionX(character.Renderer);
    }

    public static Vector2 GetLookDirection(this SpriteRenderer renderer)
        => new Vector2(renderer.GetLookDirectionX(), 0f);

    public static Vector2 GetLookDirection(this Character character)
    {
        if (character == null || character.Renderer == null)
            return Vector2.right;

        return character.Renderer.GetLookDirection();
    }
}
