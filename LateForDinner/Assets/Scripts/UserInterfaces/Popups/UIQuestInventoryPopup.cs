using LateForDinner.Data;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using ZLinq;

public class UIQuestInventoryPopup : UIPopup, IDraggablePopup, IFocusablePopup
{
    private enum RectTransforms
    {
        EquipmentContent
    }

    private enum Images
    {
        AttributeButtonImage,
        TotalButtonImage,
        EquipmentButtonImage,
        ConsumptionButtonImage,
        EtcButtonImage,
        SortButtonImage,
        ScrollUpArrowImage,
        ScrollDownArrowImage,
        MealTimeImage
    }

    private enum Texts
    {
        AttributeTabText,
        HealthTabText,
        JumpForceTabText,
        JumpCountTabText,
        DashDistanceTabText,
        MoveSpeedTabText,
        DamageTabText,
        AttackSpeedTabText,
        GoldText,
        DayText
    }

    private enum Buttons
    {
        AttributeButton,
        TotalButton,
        EquipmentButton,
        ConsumptionButton,
        EtcButton,
        SortButton,
        ScrollUpButton,
        ScrollDownButton
    }

    private enum ScrollRects
    {
        InventoryScrollRect
    }

    private enum Panels
    {
        AttributePanel
    }

    public ItemCategory? CurrentTabType => _currentTabType;
    private ItemCategory? _currentTabType = null;
    private readonly ReactiveProperty<ButtonState> _attributeButtonState = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _totalButtonState = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _equipmentButtonState = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _consumptionButtonState = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _etcButtonState = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _sortButtonState = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _scrollUpButtonState = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _scrollDownButtonState = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly List<UIInventorySlot> _createdSlots = new List<UIInventorySlot>();
    private readonly List<UIInventorySlot> _equipmentCreatedSlots = new List<UIInventorySlot>();
    private bool _isAttributePanelOpen = true;

    public override void OnInit()
    {
        base.OnInit();
        BindRectTransform(typeof(RectTransforms));
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindScrollRect(typeof(ScrollRects));
        BindPanel(typeof(Panels));
        InitInventorySlots();
        InitEquipmentSlots();
    }

    private void InitInventorySlots()
    {
        var content = GetScrollRect(ScrollRects.InventoryScrollRect).content;

        for (int index = 0; index < Define.Amount.MaxInventorySlot; index++)
        {
            var (slot, _) = Managers.Pool.Pop<UIInventorySlot>(content);

            if (slot != null)
                _createdSlots.Add(slot);
        }
    }

    private void InitEquipmentSlots()
    {
        var equipmentContent = GetRectTransform(RectTransforms.EquipmentContent);

        for (int index = 0; index < Define.Amount.MaxEquipmentSlot; index++)
        {
            var (slot, _) = Managers.Pool.Pop<UIInventorySlot>(equipmentContent);

            if (slot != null)
                _equipmentCreatedSlots.Add(slot);
        }
    }

    public override void OnGet()
    {
        base.OnGet();
        BindButtonStates();
        BindButtonActions();
        Managers.Inventory.OnInventoryChanged
        .Subscribe(_ => Refresh())
        .RegisterToPool(this);
        Refresh();
    }

    public override void OnRelease()
    {
        base.OnRelease();
        Managers.UI.Close<UIItemDetailPopup>();
        Managers.UI.Close<UIItemDropPopup>();
    }

    private void BindButtonStates()
    {
        GetImage(Images.AttributeButtonImage).BindState(_attributeButtonState, Define.Atlas.Common, this);
        GetImage(Images.TotalButtonImage).BindState(_totalButtonState, Define.Atlas.Common, this);
        GetImage(Images.EquipmentButtonImage).BindState(_equipmentButtonState, Define.Atlas.Common, this);
        GetImage(Images.ConsumptionButtonImage).BindState(_consumptionButtonState, Define.Atlas.Common, this);
        GetImage(Images.EtcButtonImage).BindState(_etcButtonState, Define.Atlas.Common, this);
        GetImage(Images.SortButtonImage).BindState(_sortButtonState, Define.Atlas.Common, this);
        GetImage(Images.ScrollUpArrowImage).BindStateAsArrow(_scrollUpButtonState, Define.Atlas.Common, this);
        GetImage(Images.ScrollDownArrowImage).BindStateAsArrow(_scrollDownButtonState, Define.Atlas.Common, this);
    }

    private void BindButtonActions()
    {
        GetButton(Buttons.AttributeButton).BindViewAsButton(OnClickAttributeTab, ViewEvent.LeftClick, this, _attributeButtonState);
        GetButton(Buttons.TotalButton).BindViewAsButton(OnClickTotalTab, ViewEvent.LeftClick, this, _totalButtonState);
        GetButton(Buttons.EquipmentButton).BindViewAsButton(OnClickEquipmentTab, ViewEvent.LeftClick, this, _equipmentButtonState);
        GetButton(Buttons.ConsumptionButton).BindViewAsButton(OnClickConsumptionTab, ViewEvent.LeftClick, this, _consumptionButtonState);
        GetButton(Buttons.EtcButton).BindViewAsButton(OnClickEtcTab, ViewEvent.LeftClick, this, _etcButtonState);
        GetButton(Buttons.SortButton).BindViewAsButton(OnClickSortTab, ViewEvent.LeftClick, this, _sortButtonState);
        GetButton(Buttons.ScrollUpButton).BindViewAsButton(OnClickScrollUp, ViewEvent.LeftClick, this, _scrollUpButtonState);
        GetButton(Buttons.ScrollDownButton).BindViewAsButton(OnClickScrollDown, ViewEvent.LeftClick, this, _scrollDownButtonState);
    }

    public override void Refresh()
    {
        base.Refresh();
        RefreshInventory(_currentTabType);
        RefreshEquipmentSlots();

        if (_isAttributePanelOpen)
            RefreshPlayerInfo();
    }

    private void RefreshInventory(ItemCategory? type)
    {
        var displaySlots = Managers.Inventory.GetSlotsByType(type).ToList();

        for (int index = 0; index < _createdSlots.Count; index++)
        {
            var targetSlot = _createdSlots[index];

            if (index >= displaySlots.Count)
            {
                targetSlot.SetActive(false);
                continue;
            }

            var slotData = displaySlots[index];
            targetSlot.SetActive(true);

            if (IsFilteredOut(slotData, type))
                targetSlot.SetupAsFilteredOut(index, slotData);
            else
                targetSlot.Setup(index, slotData, false);
        }
    }

    private bool IsFilteredOut(InventorySlot slotData, ItemCategory? type)
    {
        if (!type.HasValue) 
            return false;

        if (slotData.ItemID <= 0) 
            return false;

        if (!Managers.Data.Items.TryGetValue(slotData.ItemID, out var itemData)) 
            return false;

        if (!Enum.TryParse<ItemCategory>(itemData.ItemCategory, true, out var parsedItemCategory)) 
            return false;

        return parsedItemCategory != type.Value;
    }

    private void RefreshEquipmentSlots()
    {
        var equipmentDataList = Managers.Inventory.GetEquipmentSlots();

        for (int index = 0; index < _equipmentCreatedSlots.Count; index++)
        {
            InventorySlot targetData = equipmentDataList.FirstOrDefault(x => x.SlotIndex == index);
            _equipmentCreatedSlots[index].Setup(index, targetData, true);
        }
    }

    private void RefreshPlayerInfo()
    {
        var saveData = Managers.Save.CurrentData;

        if (saveData != null)
        {
            GetText(Texts.GoldText).text = saveData.Gold.ToString("N0");
            GetText(Texts.DayText).text = Managers.Localization.Get(LocalizationKey.Slot_Day_Format, saveData.Day);
            string spriteName = saveData.Meal.ToSpriteAsMealTime();
            GetImage(Images.MealTimeImage).sprite = Managers.Resource.GetSprite(Define.Atlas.Common, spriteName);
        }

        var player = Managers.Game.Player;

        if (player != null && player.Attributes != null)
        {
            GetText(Texts.HealthTabText).text = player.Attributes.Get<int>(AttributeType.Health).Value.ToString();
            GetText(Texts.MoveSpeedTabText).text = player.Attributes.Get<float>(AttributeType.MoveSpeed).Value.ToString("F1");
            GetText(Texts.DamageTabText).text = player.Attributes.Get<float>(AttributeType.Damage).Value.ToString("N0");
            GetText(Texts.AttackSpeedTabText).text = player.Attributes.Get<float>(AttributeType.AttackSpeed).Value.ToString("F2");
            GetText(Texts.JumpForceTabText).text = player.Attributes.Get<float>(AttributeType.JumpForce).Value.ToString("F1");
            GetText(Texts.JumpCountTabText).text = player.Attributes.Get<int>(AttributeType.JumpCount).Value.ToString();
            GetText(Texts.DashDistanceTabText).text = player.Attributes.Get<float>(AttributeType.DashDistance).Value.ToString("F1");
        }
    }

    private void OnClickAttributeTab(PointerEventData data)
    {
        _isAttributePanelOpen = !GetPanel(Panels.AttributePanel).IsActive();
        GetPanel(Panels.AttributePanel).SetActive(_isAttributePanelOpen);

        if (_isAttributePanelOpen)
            RefreshPlayerInfo();
    }

    private void OnClickTotalTab(PointerEventData data)
    {
        _currentTabType = null;
        RefreshInventory(_currentTabType);
    }

    private void OnClickEquipmentTab(PointerEventData data)
    {
        _currentTabType = ItemCategory.Equipment;
        OnClickScrollUp(data);
        RefreshInventory(_currentTabType);
    }

    private void OnClickConsumptionTab(PointerEventData data)
    {
        _currentTabType = ItemCategory.Consumption;
        OnClickScrollUp(data);
        RefreshInventory(_currentTabType);
    }

    private void OnClickEtcTab(PointerEventData data)
    {
        _currentTabType = ItemCategory.Etc;
        OnClickScrollUp(data);
        RefreshInventory(_currentTabType);
    }

    private void OnClickSortTab(PointerEventData data)
    {
        Managers.Inventory.SortInventory(_currentTabType);
        Refresh();
    }

    private void OnClickScrollUp(PointerEventData data)
        => GetScrollRect(ScrollRects.InventoryScrollRect).verticalNormalizedPosition = 1f;

    private void OnClickScrollDown(PointerEventData data)
        => GetScrollRect(ScrollRects.InventoryScrollRect).verticalNormalizedPosition = 0f;
}
