using UnityEngine;

public class Tray : Prop, IInteractable, IPoolable
{
    [Header("Tray Settings")]
    [SerializeField] private Collider2D _collider;
    [SerializeField] private InteractionType _interactionType = InteractionType.Tray;
    [SerializeField] private LocalizationKey _localizationPromptKey;
    [SerializeField] private bool _requireKeyInput = true;
    [SerializeField] private bool _triggerOnProximity = false;
    public Collider2D Collider => _collider;
    public PropKey PropKey => _propKey;
    public InteractionType InteractionType => _interactionType;
    public LocalizationKey LocalizationPromptKey => _localizationPromptKey;
    public bool RequireKeyInput => _requireKeyInput;
    public bool TriggerOnProximity => _triggerOnProximity;
    protected override bool UseSaveState => true;

    public void OnInteract(Character character)
    {
        if (character is not ICarriableCharacter carriable)
            return;

        carriable.PickupProp(this);
    }
}
