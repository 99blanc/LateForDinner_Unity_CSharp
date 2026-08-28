using Cysharp.Text;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class CharacterExtensions
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

    public static string GetAnimatorOverrideControllerPath(this CharacterID characterID)
        => ZString.Concat(characterID.ToString(), Literal.Assets.Animator);

    public static (Type characterType, Type animatorType) GetCharacterTypes(this CharacterID characterID)
    {
        if (_caches.TryGetValue(characterID, out var cachedTypes))
            return cachedTypes;

        string charTypeName = characterID.ToString();
        string animTypeName = ZString.Concat(characterID.ToString(), Literal.Assets.Animator);
        Type charType = null;
        Type animType = null;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (charType == null)
            {
                var t = assembly.GetType(charTypeName);

                if (t != null && typeof(PlayableCharacter).IsAssignableFrom(t))
                    charType = t;
            }

            if (animType == null)
            {
                var t = assembly.GetType(animTypeName);

                if (t != null && typeof(CharacterAnimator).IsAssignableFrom(t))
                    animType = t;
            }

            if (charType != null && animType != null)
                break;
        }

        _caches[characterID] = (charType, animType);
        return (charType, animType);
    }

    public static void FlipX(this SpriteRenderer renderer, float directionX)
    {
        if (renderer == null)
            return;

        if (Mathf.Abs(directionX) > 0.01f)
            renderer.flipX = directionX < 0f;
    }
}
