using UnityEngine;
using Token.PRIORITY;

public class Box : CollisionProp, IBoxProp
{
    protected Rigidbody2D body { get; private set; }
    public override PropPriority priority => PropPriority.BOX;

    protected override void Awake()
    {
        base.Awake();
        body = gameObject.GetOrAddComponentAssert<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.useAutoMass = true;
        PhysicsMaterial2D mat = new(Define.Layer.BOX) 
        { 
            friction = 0.4f, 
            bounciness = 0
        };
        sensor.sharedMaterial = mat;
    }
}
