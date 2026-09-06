using LateForDinner.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

public class UIInventorySlot : UISlot
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

    private InventorySlot _data;
    private int _slotIndex;
    private bool _isEquipmentSlot;

    public override void OnInit()
    {
        base.OnInit();
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        GetButton(Buttons.SlotButton).BindView(OnClickSlot, ViewEvent.LeftClick, this);
    }

    public void Setup(int slotIndex, InventorySlot slotData, bool isEquipmentSlot = false)
    {
        _slotIndex = slotIndex;
        _data = slotData;
        _isEquipmentSlot = isEquipmentSlot;
        Refresh();
    }

    public override void Refresh()
    {
        base.Refresh();

        if (_isEquipmentSlot && (_data == null || _data.ItemID <= 0))
        {
            GetImage(Images.SlotCoverImage).SetActive(true);
            EquipmentSlotType slotType = (EquipmentSlotType)_slotIndex;
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

    public void Clear()
    {
        _data = null;
        _slotIndex = -1;
        GetImage(Images.SlotItemImage).SetActive(false);
        GetText(Texts.SlotQuantityText).text = string.Empty;
        GetImage(Images.SlotCooldownImage).SetActive(false);
    }

    private void OnClickSlot(PointerEventData data)
    {
        Debug.Log($"Clicked Slot Index: {_slotIndex}");
    }

    private void SetEquipmentImageSprite(string spriteName)
    {
        var image = GetImage(Images.SlotCoverImage);

        if (image != null)
            image.sprite = Managers.Resource.GetSprite(Define.Atlas.Common, spriteName);
    }
}
