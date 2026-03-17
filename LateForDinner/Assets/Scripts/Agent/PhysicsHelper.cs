using UnityEngine;

public class PhysicsHelper
{
    public static bool IsGrounded(CapsuleCollider2D collider, Rigidbody2D body)
    {
        Vector2 origin = new(collider.bounds.center.x, collider.bounds.min.y + Define.Physics.SNAP);
        Vector2 size = new(collider.size.x * Define.Physics.LIMIT, Define.Physics.DEADZONE);
        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0, Vector2.down, Define.Physics.OFFSET, Define.Layer.GROUND_MASKS);

        if (!hit) 
            return false;

        if (Vector2.Angle(hit.normal, Vector2.up) > Define.Physics.SLOPE)
            return false;

        return true;
    }

    public static Vector2 ToLookAt(Vector2 current, Vector2 target = default)
    {
        float x = current.x != 0 ? Mathf.Sign(current.x) : target.x;
        float y = 0;

        if (current.sqrMagnitude < Define.Physics.DEADZONE)
            return target != Vector2.zero ? target : Vector2.right;

        if (current.y > Define.Physics.HALF) 
            y = 1f;

        if (current.y < -Define.Physics.HALF) 
            y = -1f;

        Vector2 result = new(x, y);
        return new Vector2(x, y) != Vector2.zero ? result.normalized : Vector2.right;
    }
}
