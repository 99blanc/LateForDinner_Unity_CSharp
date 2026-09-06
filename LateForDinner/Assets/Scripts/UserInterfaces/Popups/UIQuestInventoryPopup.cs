using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIQuestInventoryPopup : UIPopup
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
        ScrollDownButton,
    }

    private enum ScrollRects
    {
        InventoryScrollRects
    }

    private enum Panels
    {
        AttributePanel
    }

    private readonly List<UIInventorySlot> _createdSlots = new List<UIInventorySlot>();
    private readonly List<UIInventorySlot> _equipmentCreatedSlots = new List<UIInventorySlot>();
    private ItemType? _currentTabType = null;
    private bool _isAttributePanelOpen = false;

    public override void OnInit()
    {
        base.OnInit();
        BindRectTransform(typeof(RectTransforms));
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindScrollRect(typeof(ScrollRects));
        BindPanel(typeof(Panels));
        GetButton(Buttons.TotalButton).BindView(OnClickTotalTab, ViewEvent.LeftClick, this);
        GetButton(Buttons.EquipmentButton).BindView(OnClickEquipmentTab, ViewEvent.LeftClick, this);
        GetButton(Buttons.ConsumptionButton).BindView(OnClickConsumptionTab, ViewEvent.LeftClick, this);
        GetButton(Buttons.EtcButton).BindView(OnClickEtcTab, ViewEvent.LeftClick, this);
        GetButton(Buttons.AttributeButton).BindView(OnClickAttributeTab, ViewEvent.LeftClick, this);
        InitInventorySlots();
        InitEquipmentSlots();
    }

    private void InitInventorySlots()
    {
        var content = GetScrollRect(ScrollRects.InventoryScrollRects).content;

        for (int index = 0; index < Define.Amount.MaxInventorySlot; index++)
        {
            var (slot, _) = Managers.Pool.Pop<UIInventorySlot>(content);

            if (slot != null)
            {
                slot.SetActive(false);
                _createdSlots.Add(slot);
            }
        }
    }

    private void InitEquipmentSlots()
    {
        var equipmentContent = GetRectTransform(RectTransforms.EquipmentContent);

        for (int index = 0; index < Define.Amount.MaxEquipmentSlot; index++)
        {
            var (slot, _) = Managers.Pool.Pop<UIInventorySlot>(equipmentContent);

            if (slot != null)
            {
                slot.SetActive(false);
                _equipmentCreatedSlots.Add(slot);
            }
        }
    }

    public override void Refresh()
    {
        base.Refresh();
        RefreshInventory(_currentTabType);
        RefreshEquipmentSlots();
        RefreshPlayerInfo();
        GetPanel(Panels.AttributePanel).SetActive(_isAttributePanelOpen);

        if (_isAttributePanelOpen)
            RefreshPlayerInfo();
    }

    private void RefreshInventory(ItemType? type)
    {
        var slotDataList = Managers.Inventory.GetSlotsByType(type).ToList();

        for (int index = 0; index < _createdSlots.Count; index++)
        {
            if (index < slotDataList.Count)
            {
                _createdSlots[index].SetActive(true);
                _createdSlots[index].Setup(slotDataList[index].SlotIndex, slotDataList[index], false);
            }
            else
                _createdSlots[index].SetActive(false);
        }
    }

    private void RefreshEquipmentSlots()
    {
        var equipmentDataList = Managers.Inventory.GetEquipmentSlots();

        for (int index = 0; index < _equipmentCreatedSlots.Count; index++)
        {
            if (index < equipmentDataList.Count)
            {
                _equipmentCreatedSlots[index].SetActive(true);
                _equipmentCreatedSlots[index].Setup(equipmentDataList[index].SlotIndex, equipmentDataList[index], true);
            }
            else
            {
                _equipmentCreatedSlots[index].SetActive(false);
            }
        }
    }

    private void RefreshPlayerInfo()
    {
        var saveData = Managers.Save.CurrentData;

        if (saveData != null)
        {
            GetText(Texts.GoldText).text = Managers.Save.CurrentData.Gold.ToString("N0");
            GetText(Texts.DayText).text = Managers.Localization.Get(LocalizationKey.Slot_Day_Format, Managers.Save.CurrentData.Day);

            var mealImage = GetImage(Images.MealTimeImage);

            if (mealImage != null)
            {
                mealImage.SetActive(true);
                string spriteName = saveData.Meal.ToSpriteAsMealTime();
                mealImage.sprite = Managers.Resource.GetSprite(Define.Atlas.Common, spriteName);
            }
        }

        var player = Managers.Game.Player;

        if (player != null && player.Attributes != null)
        {
            int currentHealth = player.Attributes.Get<int>(AttributeType.Health).Value;
            GetText(Texts.HealthTabText).text = currentHealth.ToString();
            float moveSpeed = player.Attributes.Get<float>(AttributeType.MoveSpeed).Value;
            GetText(Texts.MoveSpeedTabText).text = moveSpeed.ToString("F1");
            float damage = player.Attributes.Get<float>(AttributeType.Damage).Value;
            GetText(Texts.DamageTabText).text = damage.ToString("N0");
            float attackSpeed = player.Attributes.Get<float>(AttributeType.AttackSpeed).Value;
            GetText(Texts.AttackSpeedTabText).text = attackSpeed.ToString("F2");
            float jumpForce = player.Attributes.Get<float>(AttributeType.JumpForce).Value;
            GetText(Texts.JumpForceTabText).text = jumpForce.ToString("F1");
            int jumpCount = player.Attributes.Get<int>(AttributeType.JumpCount).Value;
            GetText(Texts.JumpCountTabText).text = jumpCount.ToString();
            float dashDistance = player.Attributes.Get<float>(AttributeType.DashDistance).Value;
            GetText(Texts.DashDistanceTabText).text = dashDistance.ToString("F1");
        }
    }

    private void OnClickTotalTab(PointerEventData data)
    {
        _currentTabType = null;
        RefreshInventory(_currentTabType);
    }

    private void OnClickEquipmentTab(PointerEventData data)
    {
        _currentTabType = ItemType.Equipment;
        RefreshInventory(_currentTabType);
    }

    private void OnClickConsumptionTab(PointerEventData data)
    {
        _currentTabType = ItemType.Consumption;
        RefreshInventory(_currentTabType);
    }

    private void OnClickEtcTab(PointerEventData data)
    {
        _currentTabType = ItemType.Etc;
        RefreshInventory(_currentTabType);
    }

    private void OnClickAttributeTab(PointerEventData data)
    {
        bool isOpen = !GetPanel(Panels.AttributePanel).IsActive();
        GetPanel(Panels.AttributePanel).SetActive(isOpen);

        if (isOpen)
            RefreshPlayerInfo();
    }
}
