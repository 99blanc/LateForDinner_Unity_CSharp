using UnityEngine;

public abstract class PhysicsProp : TriggerProp
{
    protected BoxCollider2D physics { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        physics = gameObject.AddComponent<BoxCollider2D>();

        if (TryGetComponent<SpriteRenderer>(out var renderer))
        {
            physics.size = renderer.bounds.size;
            physics.offset = Vector2.zero;
        }
    }

    public override void SetActive(bool active)
    {
        base.SetActive(active);

        if (physics is not null) 
            physics.enabled = active;

        if (TryGetComponent<Rigidbody2D>(out var body))
            body.simulated = active;
    }
}
