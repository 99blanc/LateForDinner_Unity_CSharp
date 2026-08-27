using Cysharp.Text;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class CharacterExtensions
{
    private static readonly Dictionary<CharacterID, (Type characterType, Type animatorType)> _caches = new Dictionary<CharacterID, (Type characterType, Type animatorType)>();

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
