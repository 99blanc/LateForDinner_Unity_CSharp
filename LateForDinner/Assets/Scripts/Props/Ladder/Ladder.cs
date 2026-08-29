using UnityEngine;

public class Ladder : Prop, IInteractable
{
    [Header("Ladder Settings")]
    [SerializeField] private Collider2D _collider = default;
    [SerializeField] private InteractionType _interactionType = InteractionType.None;
    [SerializeField] private LocalizationKey _promptKey = default;
    [SerializeField] private bool _requireKeyInput = false;
    [SerializeField] private bool _triggerOnProximity = true;
    [SerializeField] private float _interactRadius = default;
    public Collider2D Collider => _collider;
    public PropKey PropKey => _propKey;
    public InteractionType InteractionType => _interactionType;
    public LocalizationKey LocalizationPromptKey => _promptKey;
    public bool RequireKeyInput => _requireKeyInput;
    public bool TriggerOnProximity => _triggerOnProximity;
    public float InteractRadius => _interactRadius;

    protected override bool UseSaveState => false;

    public void OnInteract(PlayableCharacter player)
    {

    }
}
