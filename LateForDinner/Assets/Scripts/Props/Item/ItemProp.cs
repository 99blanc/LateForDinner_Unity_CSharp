using Cysharp.Threading.Tasks;
using LateForDinner.Data;
using System;
using UnityEngine;

public class ItemProp : Prop, IPoolable, IInteractable
{
    [Header("Item Settings")]
    [SerializeField] private Collider2D _collider;
    [SerializeField] private InteractionType _interactionType = InteractionType.Item;
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
    [HideInInspector] public int _itemID;
    [HideInInspector] public int _quantity;
    [HideInInspector] public string _instanceID = string.Empty;
    private UIItemIndicator _indicatorInstance;


    public virtual void OnGet()
    {
        if (GetSprite(_itemID) != null)
            Renderer.sprite = GetSprite(_itemID);
        else
            Renderer.sprite = Managers.Resource.GetSprite(Define.Atlas.Common, Define.Sprite.Empty);
    }

    public virtual void OnRelease()
    {
        CleanupIndicator();
        _itemID = 0;
        _quantity = 0;
        _instanceID = string.Empty;
    }

    public bool OnInteract(Character character)
    {
        bool success = Managers.Inventory.AddItem(_itemID, _quantity, _instanceID);

        if (success)
        {
            CleanupIndicator();
            Managers.Pool.Push(this, UniqueKey);
            return true;
        }
        else
        {
            // DESC ::: 인벤토리 공간 경고 토스트 출력
            Managers.Notify.ToastAsync(LocalizationKey.Log_Item_Prop_NotEnoughInventory).Forget();
            return false;
        }
    }

    public async UniTask Setup(int itemID, int quantity, string instanceID = "")
    {
        _itemID = itemID;
        _quantity = quantity;
        _instanceID = instanceID;

        if (GetSprite(_itemID) != null)
            Renderer.sprite = GetSprite(_itemID);

        await UpdateIndicatorContent();
    }

    private async UniTask UpdateIndicatorContent()
    {
        _indicatorInstance = await Managers.UI.OpenIndicatorAsync<UIItemIndicator>();

        if (_indicatorInstance == null)
            return;

        if (!Managers.Data.Items.TryGetValue(_itemID, out var itemData))
            return;

        string itemText = Managers.Localization.Get(LocalizationKey.Item_Drop_Format, itemData.NameKey, _quantity);
        _indicatorInstance.OnGet();
        _indicatorInstance.SetItemName(itemText);
        _indicatorInstance.SetTarget(transform, new Vector3(0f, 0.5f, 0f));
    }

    private void CleanupIndicator()
    {
        if (_indicatorInstance != null)
        {
            _indicatorInstance.Close();
            _indicatorInstance = null;
        }
    }

    private Sprite GetSprite(int itemID)
    {
        if (!Managers.Data.Items.TryGetValue(itemID, out var itemData))
            return null;

        return Managers.Resource.GetSprite(Define.Atlas.Item, itemData.AddressableKey);
    }
}
