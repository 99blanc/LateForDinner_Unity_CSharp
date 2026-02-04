using UnityEngine;

public class PhysicsHelper
{
    public static bool IsGrounded(CapsuleCollider2D collider, Rigidbody2D body)
    {
        const float shell = 0.02f;
        const float footWidthRatio = 0.85f;
        const float detectionDepth = 0.12f;
        float halfWidth = (collider.size.x * footWidthRatio) / 2f;
        Vector2 centerBottom = new Vector2(collider.bounds.center.x, collider.bounds.min.y + shell);
        Vector2 castSize = new Vector2(halfWidth * 2f, shell);
        int mask = Define.Layer.GROUND_MASKS;
        RaycastHit2D hit = Physics2D.BoxCast(centerBottom, castSize, 0f, Vector2.down, detectionDepth, Define.Layer.GROUND_MASKS);

        if (!hit || hit.collider.isTrigger)
            return false;

        if (Vector2.Angle(hit.normal, Vector2.up) > 45f)
            return false;

        if (body.linearVelocity.y > 0.1f)
            return false;

        return true;
    }

    public static Vector2 ToLookAt(Vector2 current, Vector2 target = default)
    {
        if (current.sqrMagnitude < 0.01f)
        {
            return target != Vector2.zero ? target : Vector2.right;
        }

        float x = current.x != 0 ? Mathf.Sign(current.x) : target.x;

        float y = 0f;

        if (current.y > 0.5f) 
            y = 1f;
        if (current.y < -0.5f) 
            y = -1f;

        Vector2 result = new(x, y);
        return result != Vector2.zero ? result.normalized : Vector2.right;
    }
}
