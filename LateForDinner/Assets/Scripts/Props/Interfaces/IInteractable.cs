using R3;
using System.Runtime.CompilerServices;
using UnityEngine;

public interface IInteractable
{
    private static readonly ConditionalWeakTable<IInteractable, InteractValue> _interactValue = new ConditionalWeakTable<IInteractable, InteractValue>();
    private class InteractValue
    {
        public float InteractRadius = 0f;
        public int Priority = 0;
    }

    Collider2D Collider { get; }
    PropKey PropKey { get; }
    InteractionType InteractionType { get; }
    LocalizationKey LocalizationPromptKey { get; }
    bool RequireKeyInput { get; }
    bool TriggerOnProximity { get; }
    public float InteractRadius
    {
        get => _interactValue.GetOrCreateValue(this).InteractRadius;
        set => _interactValue.GetOrCreateValue(this).InteractRadius = value;
    }
    public int Priority
    {
        get => _interactValue.GetOrCreateValue(this).Priority;
        set => _interactValue.GetOrCreateValue(this).Priority = value;
    }
    private static readonly ConditionalWeakTable<IInteractable, ReactiveProperty<bool>> _interactCaches = new ConditionalWeakTable<IInteractable, ReactiveProperty<bool>>();
    public ReactiveProperty<bool> CanInteract
    {
        get => _interactCaches.GetValue(this, _ => new ReactiveProperty<bool>(false));
    }
    private static readonly ConditionalWeakTable<IInteractable, IInteractAction> _actionCaches = new ConditionalWeakTable<IInteractable, IInteractAction>();
    private IInteractAction Action
    {
        get
        {
            if (_actionCaches.TryGetValue(this, out var cachedAction))
                return cachedAction;

            IInteractAction newAction = InteractionType switch
            {
                InteractionType.Ladder => new LadderInteractAction(),
                InteractionType.Tray => new TrayInteractAction(),
                InteractionType.Item => new ItemInteractAction(),
                _ => null
            };

            if (newAction != null)
                _actionCaches.Add(this, newAction);

            return newAction;
        }
    }

    public bool ProtectedInteract(Character character)
    {
        bool actionSuccess = Action?.Execute(character) ?? true;

        if (!actionSuccess) 
            return false;

        return OnInteract(character);
    }

    virtual bool OnInteract(Character character) { return true; }

    public void Reset()
    {
        if (_interactCaches.TryGetValue(this, out var reactiveProp))
            reactiveProp.Value = false;

        if (_interactValue.TryGetValue(this, out var val))
        {
            val.InteractRadius = 0f;
            val.Priority = 0;
        }

        _actionCaches.Remove(this);
    }
}
