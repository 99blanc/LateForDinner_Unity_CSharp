using UnityEngine;

public class TriggerProp : Prop
{
    protected override void Awake()
    {
        base.Awake();
        sensor.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collider) => HandleEnter(collider.gameObject);

    private void OnTriggerExit2D(Collider2D collider) => HandleExit(collider.gameObject);
}
