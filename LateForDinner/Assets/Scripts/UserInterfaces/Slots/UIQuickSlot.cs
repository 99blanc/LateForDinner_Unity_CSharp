using LateForDinner.Data;
using R3;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIQuickSlot : UISlot, IDraggableSlot
{
    private enum Images
    {
        QuickSlotItemImage,
        QuickSlotCooldownImage,
        QuickSlotFrameImage
    }

    private enum Texts
    {
        QuickSlotQuantityText
    }

    private enum Buttons
    {
        QuickSlotButton
    }

    public Sprite DragSprite
    {
        get
        {
            var itemImage = GetImage(Images.QuickSlotItemImage);
            return itemImage.gameObject.activeSelf ? itemImage.sprite : null;
        }
    }
    public SlotArea CurrentSlotArea 
        => SlotArea.Quick;
    public InventorySlot Data 
        => _data;
    private InventorySlot _data;
    private int _index;
    private IDisposable _disposable;

    public override void OnInit()
    {
        base.OnInit();
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        GetImage(Images.QuickSlotItemImage).raycastTarget = false;
        GetImage(Images.QuickSlotFrameImage).raycastTarget = false;
        GetText(Texts.QuickSlotQuantityText).raycastTarget = false;
    }

    public override void OnGet()
    {
        base.OnGet();
        GetButton(Buttons.QuickSlotButton).BindView(OnDoubleClickQuickSlot, ViewEvent.DoubleClick, this);
        Refresh();
    }

    private void BindCooldown()
    {
        _disposable?.Dispose();
        _disposable = null;

        if (_data == null || _data.ItemID <= 0)
        {
            SetCooldown(0f);
            return;
        }

        string itemKey = _data.GetItemCooldownKey();
        var cooldownRegistry = Managers.Cooldown.GetSlotCooldown(itemKey);

        if (cooldownRegistry != null && cooldownRegistry.IsOnCooldown)
        {
            _disposable = cooldownRegistry.CooldownProgress
            .Subscribe(SetCooldown)
            .RegisterToPool(this);
        }
        else
            SetCooldown(0f);
    }

    private void SetCooldown(float fillAmount)
    {
        var cooldownImage = GetImage(Images.QuickSlotCooldownImage);

        if (fillAmount > 0f)
        {
            cooldownImage.SetActive(true);
            cooldownImage.fillAmount = fillAmount;
        }
        else
        {
            cooldownImage.SetActive(false);
            cooldownImage.fillAmount = 0f;
        }
    }

    public void Setup(int index, InventorySlot slotData)
    {
        _index = index;
        _data = slotData;
        Refresh();
    }

    public override void Refresh()
    {
        base.Refresh();
        BindCooldown();

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
            GetImage(Images.QuickSlotCooldownImage).SetActive(false);
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

    private void OnDoubleClickQuickSlot(PointerEventData data)
        => Managers.Inventory.UseQuickSlot(_index);

    public void OnDropItem(UISlot targetSlot)
    {
        if (targetSlot == null || targetSlot == this)
            return;

        if (targetSlot is UIQuickSlot targetQuickSlot)
            Managers.Inventory.HandleQuickSlotMove(CurrentSlotArea, _data, targetQuickSlot.Data);
    }

    public void OnDropOutside()
    {
        if (_data != null && _data.ItemID > 0)
        {
            Managers.Inventory.ClearQuickSlot(_index);
            Refresh();
        }
    }
}
