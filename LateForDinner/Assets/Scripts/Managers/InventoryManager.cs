using LateForDinner.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using ZLinq;

public class InventoryManager
{
    private List<InventorySlot> _slots = new List<InventorySlot>(Define.Amount.MaxInventorySlot);
    private List<InventorySlot> _equipmentSlots = new List<InventorySlot>(Define.Amount.MaxEquipmentSlot);
    private List<InventorySlot> _quickSlots = new List<InventorySlot>(Define.Amount.MaxQuickSlot);

    public void InitInventory(List<InventorySlot> savedSlots, List<InventorySlot> savedQuickSlots, List<InventorySlot> savedEquipmentSlots = null)
    {
        _slots = savedSlots ?? new List<InventorySlot>();
        EnsureSlotCapacity(_slots, Define.Amount.MaxInventorySlot);
        _equipmentSlots = savedEquipmentSlots ?? new List<InventorySlot>();
        EnsureSlotCapacity(_equipmentSlots, Define.Amount.MaxEquipmentSlot);
        _quickSlots = savedQuickSlots ?? new List<InventorySlot>();
        EnsureSlotCapacity(_quickSlots, Define.Amount.MaxQuickSlot);
    }

    private void EnsureSlotCapacity(List<InventorySlot> slots, int maxCapacity)
    {
        while (slots.Count < maxCapacity)
        {
            int index = slots.Count;
            slots.Add(new InventorySlot { SlotIndex = index, ItemID = 0, Quantity = 0 });
        }
    }

    public void SortInventory(ItemType? type)
    {
        var (startIndex, targetSize) = GetTabRange(type);
        var validItems = _slots
        .Skip(startIndex)
        .Take(targetSize)
        .Where(slot => slot.ItemID != 0 && slot.Quantity > 0)
        .ToList();
        ClearSlotRange(startIndex, targetSize);
        FillSlotRange(startIndex, validItems);
    }

    public bool AddItem(int itemID, int quantity)
    {
        if (!TryGetValidItemData(itemID, out var itemData, out var itemType))
            return false;

        var (startIndex, targetSize) = GetTabRange(itemType);

        if (!HasEnoughSpace(itemID, itemData.MaxStack, startIndex, targetSize, quantity))
            return false;

        FillExistingItemSlots(itemID, itemData.MaxStack, startIndex, targetSize, ref quantity);
        FillEmptySlots(itemID, itemData.MaxStack, startIndex, targetSize, ref quantity);
        return true;
    }

    public bool RemoveItem(int itemID, int quantity)
    {
        int remainingToRemove = quantity;

        for (int index = 0; index < _slots.Count; index++)
        {
            if (remainingToRemove <= 0)
                break;

            var slot = _slots[index];

            if (!IsTargetItemSlot(slot, itemID))
                continue;

            int removeAmount = Math.Min(remainingToRemove, slot.Quantity);
            slot.Quantity -= removeAmount;
            remainingToRemove -= removeAmount;

            if (slot.Quantity <= 0)
                ClearSlot(slot);
        }

        return remainingToRemove < quantity;
    }

    public void ClearInventory()
    {
        for (int index = 0; index < _slots.Count; index++)
        {
            _slots[index].ItemID = 0;
            _slots[index].Quantity = 0;
        }
    }

    public IEnumerable<InventorySlot> GetSlotsByType(ItemType? type)
    {
        if (!type.HasValue)
            return _slots;

        var (startIndex, targetSize) = GetTabRange(type);
        return _slots.Skip(startIndex).Take(targetSize);
    }

    public IReadOnlyList<InventorySlot> GetQuickSlots() 
        => _quickSlots;
    public IReadOnlyList<InventorySlot> GetEquipmentSlots() 
        => _equipmentSlots;
    public List<InventorySlot> ExportSaveData() 
        => _slots;
    public List<InventorySlot> ExportQuickSlotSaveData() 
        => _quickSlots;
    public List<InventorySlot> ExportEquipmentSlotSaveData() 
        => _equipmentSlots;

    private bool TryGetValidItemData(int itemID, out ItemData itemData, out ItemType itemType)
    {
        itemData = null;
        itemType = ItemType.Etc;

        if (!Managers.Data.Items.ContainsKey(itemID))
            return false;

        itemData = Managers.Data.Items[itemID];

        if (!Enum.TryParse(itemData.ItemType, true, out itemType))
            itemType = ItemType.Etc;

        return true;
    }

    private (int startIndex, int targetSize) GetTabRange(ItemType? type)
    {
        int tabSize = Define.Amount.InventoryTabSize;

        if (!type.HasValue)
            return (0, Define.Amount.MaxInventorySlot);

        int startIndex = type.Value switch
        {
            ItemType.Equipment => tabSize * 0,
            ItemType.Consumption => tabSize * 1,
            ItemType.Etc => tabSize * 2,
            _ => 0
        };
        return (startIndex, tabSize);
    }

    private bool HasEnoughSpace(int itemID, int maxStack, int startIndex, int targetSize, int quantity)
    {
        int requiredQuantity = quantity;

        for (int index = 0; index < targetSize; index++)
        {
            if (requiredQuantity <= 0) 
                break;

            var slot = _slots[startIndex + index];

            if (slot.ItemID != itemID || slot.Quantity >= maxStack) 
                continue;

            requiredQuantity -= (maxStack - slot.Quantity);
        }

        for (int index = 0; index < targetSize; index++)
        {
            if (requiredQuantity <= 0) 
                break;

            var slot = _slots[startIndex + index];

            if (slot.ItemID != 0) 
                continue;

            requiredQuantity -= maxStack;
        }

        return requiredQuantity <= 0;
    }

    private void FillExistingItemSlots(int itemID, int maxStack, int startIndex, int targetSize, ref int remainingQuantity)
    {
        for (int index = 0; index < targetSize; index++)
        {
            if (remainingQuantity <= 0) 
                break;

            var slot = _slots[startIndex + index];

            if (slot.ItemID != itemID || slot.Quantity >= maxStack) 
                continue;

            int availableSpace = maxStack - slot.Quantity;
            int addAmount = Math.Min(remainingQuantity, availableSpace);
            slot.Quantity += addAmount;
            remainingQuantity -= addAmount;
        }
    }

    private void FillEmptySlots(int itemID, int maxStack, int startIndex, int targetSize, ref int remainingQuantity)
    {
        for (int index = 0; index < targetSize; index++)
        {
            if (remainingQuantity <= 0) 
                break;

            var slot = _slots[startIndex + index];

            if (slot.ItemID != 0) 
                continue;

            int addAmount = Math.Min(remainingQuantity, maxStack);
            slot.ItemID = itemID;
            slot.Quantity = addAmount;
            remainingQuantity -= addAmount;
        }
    }

    private bool IsTargetItemSlot(InventorySlot slot, int itemID)
        => slot.ItemID == itemID && slot.Quantity > 0;

    private void ClearSlot(InventorySlot slot)
    {
        slot.ItemID = 0;
        slot.Quantity = 0;
    }

    private void ClearSlotRange(int startIndex, int targetSize)
    {
        for (int index = 0; index < targetSize; index++)
        {
            int currentIndex = startIndex + index;
            _slots[currentIndex] = new InventorySlot
            {
                SlotIndex = currentIndex,
                ItemID = 0,
                Quantity = 0
            };
        }
    }

    private void FillSlotRange(int startIndex, List<InventorySlot> validItems)
    {
        for (int index = 0; index < validItems.Count; index++)
        {
            int currentIndex = startIndex + index;
            _slots[currentIndex] = new InventorySlot
            {
                SlotIndex = currentIndex,
                ItemID = validItems[index].ItemID,
                Quantity = validItems[index].Quantity,
            };
        }
    }
}
