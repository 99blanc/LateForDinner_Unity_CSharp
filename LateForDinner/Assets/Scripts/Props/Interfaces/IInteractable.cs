using R3;
using System.Runtime.CompilerServices;
using UnityEngine;

public interface IInteractable
{
    CircleCollider2D Collider { get; }
    InteractionType InteractionType { get; }
    LocalizationKey LocalizationPromptKey { get; }
    bool RequireKeyInput { get; }
    bool TriggerOnProximity { get; }
    float InteractRadius => Define.Scaler.Interact;
    private static readonly ConditionalWeakTable<IInteractable, ReactiveProperty<bool>> _interactCaches = new();
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
                InteractionType.NPC => new NPCInteractAction(),
                InteractionType.Quest => new QuestInteractAction(),
                InteractionType.Shop => new ShopInteractAction(),
                InteractionType.Save => new SaveInteractAction(),
                InteractionType.Event => new EventInteractAction(),
                InteractionType.StageProgress => new StageProgressInteractAction(),
                InteractionType.Door => new DoorInteractAction(),
                InteractionType.TrayHolder => new TrayHolderInteractAction(),
                InteractionType.Tray => new TrayInteractAction(),
                InteractionType.DiningTable => new DiningTableInteractAction(),
                _ => null
            };

            if (newAction != null)
                _actionCaches.Add(this, newAction);

            return newAction;
        }
    }

    public void ProtectedInteract(PlayableCharacter player)
    {
        Action?.Execute(player);
        OnInteract(player);
    }

    virtual void OnInteract(PlayableCharacter player) { }
}
