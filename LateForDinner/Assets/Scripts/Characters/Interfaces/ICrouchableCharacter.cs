using System.Runtime.CompilerServices;
using UnityEngine;

public interface ICrouchableCharacter
{
    private static readonly ConditionalWeakTable<ICrouchableCharacter, CrouchStateValue> _crouchValue = new ConditionalWeakTable<ICrouchableCharacter, CrouchStateValue>();
    private class CrouchStateValue
    {
        public bool IsInitialized = false;
        public Vector2 OriginalOffset;
        public Vector2 OriginalSize;
    }
    Collider2D Collider { get; }
    SpriteRenderer Renderer { get; }
    Rigidbody2D Rigidbody { get; }
    CapsuleCollider2D CapsuleCollider => Collider as CapsuleCollider2D;
    private CrouchStateValue StateValue => _crouchValue.GetOrCreateValue(this);

    public void Crouch()
    {
        if (this is not Character || Rigidbody == null || Collider == null)
            return;

        var state = StateValue;

        if (!state.IsInitialized)
        {
            state.OriginalOffset = CapsuleCollider.offset;
            state.OriginalSize = CapsuleCollider.size;
            state.IsInitialized = true;
        }

        Vector2 velocity = Rigidbody.linearVelocity;
        velocity.x = 0f;
        Rigidbody.linearVelocity = velocity;
        float newHeight = state.OriginalSize.y * 0.5f;
        float heightDifference = state.OriginalSize.y - newHeight;
        Vector2 newOffset = new Vector2(state.OriginalOffset.x, state.OriginalOffset.y - (heightDifference * 0.5f));
        Vector2 newSize = new Vector2(state.OriginalSize.x, newHeight);
        CapsuleCollider.offset = newOffset;
        CapsuleCollider.size = newSize;
    }

    public void StandUp()
    {
        if (CapsuleCollider == null)
            return;

        var state = StateValue;

        if (!state.IsInitialized)
            return;

        CapsuleCollider.offset = state.OriginalOffset;
        CapsuleCollider.size = state.OriginalSize;
    }
}
