using LateForDinner.Data;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIQuickSlot : UISlot
{
    private enum Images
    {
        QuickSlotItemImage,
        QuickSlotCooldownImage
    }

    private enum Texts
    {
        QuickSlotQuantityText
    }

    private enum Buttons
    {
        QuickSlotButton
    }

    private int _quickSlotIndex;
    private InventorySlot _data;

    public override void OnInit()
    {
        base.OnInit();
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        GetButton(Buttons.QuickSlotButton).BindView(OnClickQuickSlot, ViewEvent.LeftClick, this);
    }

    public void Setup(int index, InventorySlot slotData)
    {
        _quickSlotIndex = index;
        _data = slotData;
        Refresh();
    }

    public override void Refresh()
    {
        base.Refresh();

        if (_data == null || _data.ItemID <= 0)
        {
            GetImage(Images.QuickSlotItemImage).SetActive(false);
            GetText(Texts.QuickSlotQuantityText).SetActive(false);
            GetImage(Images.QuickSlotCooldownImage).SetActive(false);
            return;
        }

        if (Managers.Data.Items.TryGetValue(_data.ItemID, out ItemData itemData))
        {
            GetImage(Images.QuickSlotItemImage).SetActive(true);
            GetImage(Images.QuickSlotItemImage).sprite = Managers.Resource.GetSprite(Define.Atlas.Item, itemData.AddressableKey);
        }
        else
        {
            GetImage(Images.QuickSlotItemImage).SetActive(false);
            GetText(Texts.QuickSlotQuantityText).SetActive(false);
            return;
        }

        if (_data.Quantity > 1)
        {
            GetText(Texts.QuickSlotQuantityText).SetActive(true);
            GetText(Texts.QuickSlotQuantityText).text = _data.Quantity.ToString();
        }
        else
            GetText(Texts.QuickSlotQuantityText).SetActive(false);
    }

    private void OnClickQuickSlot(PointerEventData data)
    {
        Debug.Log($"Clicked QuickSlot Index: {_quickSlotIndex}, ItemID: {_data?.ItemID ?? 0}");
    }
}
