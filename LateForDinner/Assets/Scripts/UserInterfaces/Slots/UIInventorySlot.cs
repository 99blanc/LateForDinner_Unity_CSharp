using Cysharp.Threading.Tasks;
using LateForDinner.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIInventorySlot : UISlot, IDraggableSlot<UIInventorySlot>
{
    private enum Images
    {
        SlotBackgroundImage,
        SlotCoverImage,
        SlotItemImage,
        SlotCooldownImage
    }

    private enum Texts
    {
        SlotQuantityText
    }

    private enum Buttons
    {
        SlotButton
    }

    public Sprite DragSprite
    {
        get
        {
            var itemImage = GetImage(Images.SlotItemImage);
            return itemImage.gameObject.activeSelf ? itemImage.sprite : null;
        }
    }
    private bool _isEquipmentSlot;
    private InventorySlot _data;
    public InventorySlot Data 
        => _data;
    public SlotArea CurrentSlotArea 
        => _isEquipmentSlot ? SlotArea.Equipment : SlotArea.Inventory;

    public override void OnInit()
    {
        base.OnInit();
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        GetImage(Images.SlotCoverImage).raycastTarget = false;
        GetImage(Images.SlotItemImage).raycastTarget = false;
        GetImage(Images.SlotCooldownImage).raycastTarget = false;
        GetText(Texts.SlotQuantityText).raycastTarget = false;
    }

    public override void OnGet()
    {
        base.OnGet();
        GetButton(Buttons.SlotButton).BindView(OnClickSlot, ViewEvent.DoubleClick, this);
        GetButton(Buttons.SlotButton).BindView(OnPointerEnterSlot, ViewEvent.Enter, this);
        GetButton(Buttons.SlotButton).BindView(OnPointerExitSlot, ViewEvent.Exit, this);
        Refresh();
    }

    public void Setup(int displayIndex, InventorySlot slotData, bool isEquipmentSlot = false)
    {
        _isEquipmentSlot = isEquipmentSlot;
        _data = slotData;
        var draggable = (IDraggableSlot<UIInventorySlot>)this;
        draggable.SlotIndex = displayIndex;
        Refresh();
    }

    public void SetupAsFilteredOut(int displayIndex, InventorySlot slotData)
    {
        _isEquipmentSlot = false;
        _data = slotData;
        var draggable = (IDraggableSlot<UIInventorySlot>)this;
        draggable.SlotIndex = displayIndex;
        GetImage(Images.SlotCoverImage).SetActive(false);
        GetImage(Images.SlotItemImage).SetActive(false);
        GetText(Texts.SlotQuantityText).text = string.Empty;
        GetImage(Images.SlotCooldownImage).SetActive(false);
    }

    public override void Refresh()
    {
        base.Refresh();

        if (_isEquipmentSlot && (_data == null || _data.ItemID <= 0))
        {
            GetImage(Images.SlotCoverImage).SetActive(true);
            EquipmentSlotType slotType = (EquipmentSlotType)((IDraggableSlot<UIInventorySlot>)this).SlotIndex;
            string coverSpriteName = slotType.ToSpriteAsEquipmentCover();

            if (!string.IsNullOrEmpty(coverSpriteName))
                SetEquipmentImageSprite(coverSpriteName);
        }
        else
            GetImage(Images.SlotCoverImage).SetActive(false);

        if (_data == null || _data.ItemID <= 0)
        {
            GetImage(Images.SlotItemImage).SetActive(false);
            GetText(Texts.SlotQuantityText).text = string.Empty;
            GetImage(Images.SlotCooldownImage).SetActive(false);
            return;
        }

        if (Managers.Data.Items.TryGetValue(_data.ItemID, out ItemData itemData))
        {
            GetImage(Images.SlotItemImage).SetActive(true);
            GetImage(Images.SlotItemImage).sprite = Managers.Resource.GetSprite(Define.Atlas.Item, itemData.AddressableKey);
        }
        else
        {
            GetImage(Images.SlotItemImage).SetActive(false);
            return;
        }

        if (_data.Quantity > 1)
        {
            GetText(Texts.SlotQuantityText).SetActive(true);
            GetText(Texts.SlotQuantityText).text = _data.Quantity.ToString();
        }
        else
            GetText(Texts.SlotQuantityText).SetActive(false);
    }

    public void OnDropItem(UIInventorySlot targetSlot)
    {
        if (targetSlot == null || targetSlot == this)
            return;

        if (_data == null || targetSlot.Data == null)
            return;

        ItemCategory? currentTabType = Managers.UI.GetPopup<UIQuestInventoryPopup>().CurrentTabType;
        Managers.Inventory.HandleItemMoveByTab(currentTabType, CurrentSlotArea, ((IDraggableSlot<UIInventorySlot>)this).SlotIndex, targetSlot.CurrentSlotArea, ((IDraggableSlot<UIInventorySlot>)targetSlot).SlotIndex);
    }

    public void OnDropOutside()
    {
        if (_isEquipmentSlot)
        {
            Managers.Notify.ToastAsync(LocalizationKey.Log_Inventory_Slot_DropFailed).Forget();
            return;
        }

        if (_data == null || _data.ItemID <= 0)
            return;

        if (!Managers.Data.Items.TryGetValue(_data.ItemID, out ItemData itemData))
            return;

        var dropPopup = Managers.UI.OpenPopup<UIItemDropPopup>();

        if (dropPopup == null)
            return;

        dropPopup.Setup(LocalizationKey.UI_Inventory_Slot_Drop_Confirm_Title, LocalizationKey.UI_Inventory_Slot_Drop_Confirm_Message, _data.Quantity, arg1: itemData.NameKey,
        onConfirm: async (selectedCount) =>
        {
            var player = Managers.Game.Player;
            Vector3 dropPosition = player != null ? player.transform.position + (player.Renderer.flipX ? Vector3.left : Vector3.right) : Vector3.zero;
            await Managers.Inventory.DropItem(_data, selectedCount, dropPosition);
        },
        onCancel: () => { });
    }

    private void OnClickSlot(PointerEventData data)
    {
        if (_data == null || _data.ItemID <= 0)
            return;

        if (!Managers.Data.ConsumptionItems.TryGetValue(_data.ItemID, out var consumptionItemData))
            return;
    }

    private void OnPointerEnterSlot(PointerEventData data)
    {
        if (_data == null || _data.ItemID <= 0)
            return;

        var detailPopup = Managers.UI.OpenPopup<UIItemDetailPopup>();
        detailPopup?.Setup(_data.ItemID, Mouse.current.position.ReadValue(), _data.InstanceID);
    }

    private void OnPointerExitSlot(PointerEventData data)
        => Managers.UI.Close<UIItemDetailPopup>();

    private void SetEquipmentImageSprite(string spriteName)
        => GetImage(Images.SlotCoverImage).sprite = Managers.Resource.GetSprite(Define.Atlas.Common, spriteName);
}
