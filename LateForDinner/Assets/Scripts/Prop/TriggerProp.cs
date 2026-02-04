using UnityEngine;
using Token.PRIORITY;

public abstract class TriggerProp : Prop
{
    public override abstract PropPriority priority { get; }

    protected override void Awake()
    {
        base.Awake();
        sensor.isTrigger = true;
    }

    protected void OnTriggerEnter2D(Collider2D collider) => HandleEnter(collider.gameObject);

    protected void OnTriggerExit2D(Collider2D collider) => HandleExit(collider.gameObject);
}
