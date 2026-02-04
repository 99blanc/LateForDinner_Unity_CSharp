using UnityEngine;
using Token.PRIORITY;

public abstract class CollisionProp : Prop
{
    public override abstract PropPriority priority { get; }

    protected override void Awake()
    {
        base.Awake();
        sensor.isTrigger = false;
    }

    protected void OnCollisionEnter2D(Collision2D collision) => HandleEnter(collision.gameObject);

    protected void OnCollisionExit2D(Collision2D collision) => HandleExit(collision.gameObject);
}
