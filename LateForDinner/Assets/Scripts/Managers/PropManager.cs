using R3;
using R3.Triggers;
using System.Collections.Generic;
using UnityEngine;

public class PropManager
{
    private readonly Dictionary<IInteractable, CircleCollider2D> _interactables = new Dictionary<IInteractable, CircleCollider2D>();

    public void Register(Prop prop)
    {
        if (prop is not IInteractable interactable)
            return;

        if (_interactables.ContainsKey(interactable))
            return;

        prop.OnDisableAsObservable()
        .Subscribe(_ =>
        {
            Unregister(prop);
            (prop as IPoolable).ProtectedRelease();
        }).RegisterToPool(prop as IPoolable);
        prop.OnDestroyAsObservable()
        .Subscribe(_ => PoolDisposableRegistry.Clear(prop as IPoolable))
        .RegisterToPool(prop as IPoolable);
        var check = prop.FindChild<Collider2D>()?.isTrigger;
        var transform = prop.FindChild(Literal.Objects.InteractTransform, recursive: false);

        if (check != null || transform != null)
            return;

        GameObject range = new GameObject { name = Literal.Objects.InteractTransform };
        range.transform.SetParent(prop.transform);
        range.transform.localPosition = Vector3.zero;
        var collider = range.GetComponent<CircleCollider2D>();

        if (collider == null)
            collider = range.AddComponent<CircleCollider2D>();

        collider.isTrigger = true;
        SpriteRenderer renderer = prop.Renderer;
        Vector2 spriteSize = renderer.sprite.bounds.size;
        float maxScale = Mathf.Max(prop.transform.localScale.x, prop.transform.localScale.y);
        float maxBounds = Mathf.Max(spriteSize.x, spriteSize.y) * maxScale * 0.5f;
        float calculatedRadius = maxBounds + 0.25f;
        collider.radius = calculatedRadius;
        interactable.InteractRadius = collider.radius;
        _interactables.Add(interactable, collider);
    }

    public void Unregister(Prop prop)
    {
        if (prop is not IInteractable interactable)
            return;

        _interactables.Remove(interactable);
    }

    public void Clear()
        => _interactables.Clear();
}