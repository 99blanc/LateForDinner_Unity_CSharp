using UnityEngine;
using Token.PRIORITY;

public class Box : CollisionProp, IPushProp
{
    protected Rigidbody2D rBody { get; private set; }
    public override PropPriority priority => PropPriority.BOX;

    protected override void Awake()
    {
        base.Awake();
        rBody = gameObject.GetOrAddComponentAssert<Rigidbody2D>();
        rBody.bodyType = RigidbodyType2D.Dynamic;
        rBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rBody.constraints = RigidbodyConstraints2D.FreezeRotation;
        rBody.useAutoMass = true;
        PhysicsMaterial2D mat = new PhysicsMaterial2D(Define.Layer.BOX) 
        { 
            friction = 0.4f, 
            bounciness = 0
        };
        sensor.sharedMaterial = mat;
    }
}
