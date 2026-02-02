using UnityEngine;

public class CollisionProp : Prop
{
    protected override void Awake()
    {
        base.Awake();
        sensor.isTrigger = false;
    }

    private void OnCollisionEnter2D(Collision2D collision) => HandleEnter(collision.gameObject);

    private void OnCollisionExit2D(Collision2D collision) => HandleExit(collision.gameObject);
}
