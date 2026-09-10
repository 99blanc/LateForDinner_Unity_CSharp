using LateForDinner.Data;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using ZLinq;

public class InventoryManager
{
    public Observable<Unit> OnInventoryChanged => _onInventoryChanged;
    private readonly Subject<Unit> _onInventoryChanged = new Subject<Unit>();
    private List<InventorySlot> _totalSlots = new List<InventorySlot>(Define.Amount.MaxInventorySlot);
    private List<InventorySlot> _equipmentTabSlots = new List<InventorySlot>(Define.Amount.InventoryTabSize);
    private List<InventorySlot> _consumptionTabSlots = new List<InventorySlot>(Define.Amount.InventoryTabSize);
    private List<InventorySlot> _etcTabSlots = new List<InventorySlot>(Define.Amount.InventoryTabSize);
    private List<InventorySlot> _equipmentSlots = new List<InventorySlot>(Define.Amount.MaxEquipmentSlot);
    private List<InventorySlot> _quickSlots = new List<InventorySlot>(Define.Amount.MaxQuickSlot);

    public void InitInventory(List<InventorySlot> savedTotalSlots, List<InventorySlot> savedEquipmentTabSlots, List<InventorySlot> savedConsumptionTabSlots, List<InventorySlot> savedEtcTabSlots, List<InventorySlot> savedEquipmentSlots, List<InventorySlot> savedQuickSlots)
    {
        _totalSlots = savedTotalSlots ?? new List<InventorySlot>();
        EnsureSlotCapacity(_totalSlots, Define.Amount.MaxInventorySlot);
        _equipmentTabSlots = savedEquipmentTabSlots ?? new List<InventorySlot>();
        EnsureSlotCapacity(_equipmentTabSlots, Define.Amount.InventoryTabSize);
        _consumptionTabSlots = savedConsumptionTabSlots ?? new List<InventorySlot>();
        EnsureSlotCapacity(_consumptionTabSlots, Define.Amount.InventoryTabSize);
        _etcTabSlots = savedEtcTabSlots ?? new List<InventorySlot>();
        EnsureSlotCapacity(_etcTabSlots, Define.Amount.InventoryTabSize);
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
            slots.Add(new InventorySlot
            {
                GlobalIndex = index,
                SlotIndex = index,
                ItemID = 0,
                Quantity = 0
            });
        }

        for (int index = 0; index < slots.Count; index++)
            slots[index].GlobalIndex = index;
    }

    public bool AddItem(int itemID, int quantity)
    {
        if (!TryGetValidItemData(itemID, out var itemData, out var itemCategory))
            return false;

        if (!HasEnoughSpaceInAll(_totalSlots, itemID, itemData.MaxStack, quantity))
            return false;

        var tabSlots = GetSlotsByType(itemCategory);

        if (tabSlots != _totalSlots && !HasEnoughSpaceInAll(tabSlots, itemID, itemData.MaxStack, quantity))
            return false;

        int addQuantity = quantity;
        FillExistingItemSlots(_totalSlots, itemID, itemData.MaxStack, ref quantity);
        FillEmptySlots(_totalSlots, itemID, itemData.MaxStack, ref quantity);
        SyncAddedItemToTab(itemID, addQuantity, itemData.MaxStack);
        _onInventoryChanged.OnNext(Unit.Default);
        return true;
    }

    private void SyncAddedItemToTab(int itemID, int quantity, int maxStack)
    {
        if (!TryGetValidItemData(itemID, out _, out var itemCategory))
            return;

        var tabSlots = GetSlotsByType(itemCategory);

        if (tabSlots == null)
            return;

        int remaining = quantity;
        FillExistingItemSlots(tabSlots, itemID, maxStack, ref remaining);
        FillEmptySlots(tabSlots, itemID, maxStack, ref remaining);
    }

    public bool RemoveItem(int itemID, int quantity)
    {
        if (!TryGetValidItemData(itemID, out var itemData, out var itemCategory))
            return false;

        int totalExistingQuantity = _totalSlots.Where(s => s.ItemID == itemID).Sum(s => s.Quantity);

        if (totalExistingQuantity < quantity)
            return false;

        int remainingToRemove = quantity;

        foreach (var slot in _totalSlots)
        {
            if (remainingToRemove <= 0)
                break;

            if (slot.ItemID != itemID)
                continue;

            int removeQty = Math.Min(remainingToRemove, slot.Quantity);
            slot.Quantity -= removeQty;
            remainingToRemove -= removeQty;

            if (slot.Quantity <= 0)
            {
                slot.ItemID = 0;
                slot.Quantity = 0;
            }
        }

        SyncRemovedItemToTab(itemID, quantity);
        _onInventoryChanged.OnNext(Unit.Default);
        return true;
    }

    private void SyncRemovedItemToTab(int itemID, int quantity)
    {
        if (!TryGetValidItemData(itemID, out _, out var itemCategory)) 
            return;

        var tabSlots = GetSlotsByType(itemCategory);

        if (tabSlots == null) 
            return;

        int remainingToRemove = quantity;

        foreach (var slot in tabSlots)
        {
            if (remainingToRemove <= 0) 
                break;

            if (slot.ItemID == itemID)
            {
                int removeQty = Math.Min(remainingToRemove, slot.Quantity);
                slot.Quantity -= removeQty;
                remainingToRemove -= removeQty;

                if (slot.Quantity <= 0)
                {
                    slot.ItemID = 0;
                    slot.Quantity = 0;
                }
            }
        }
    }

    public List<InventorySlot> GetSlotsByType(ItemCategory? type)
    {
        if (!type.HasValue)
            return _totalSlots;

        return type.Value switch
        {
            ItemCategory.Equipment => _equipmentTabSlots,
            ItemCategory.Consumption => _consumptionTabSlots,
            ItemCategory.Etc => _etcTabSlots,
            _ => _totalSlots
        };
    }

    public bool HandleItemMoveByTab(ItemCategory? currentTab, SlotArea sourceArea, int sourceIndex, SlotArea targetArea, int targetIndex)
    {
        if (sourceArea != targetArea)
            return HandleCrossAreaMove(sourceArea, sourceIndex, targetArea, targetIndex);

        var targetList = GetSlotsByType(currentTab);

        if (targetList == null || sourceIndex < 0 || sourceIndex >= targetList.Count || targetIndex < 0 || targetIndex >= targetList.Count)
            return false;

        var sourceSlot = targetList[sourceIndex];
        var targetSlot = targetList[targetIndex];

        if (sourceSlot == targetSlot)
            return false;

        SwapSlots(sourceSlot, targetSlot);
        _onInventoryChanged.OnNext(Unit.Default);
        return true;
    }

    private bool HandleCrossAreaMove(SlotArea sourceArea, int sourceIndex, SlotArea targetArea, int targetIndex)
    {
        var sourceList = GetSlotList(sourceArea);
        var targetList = GetSlotList(targetArea);

        if (sourceList == null || targetList == null) 
            return false;

        var sourceSlot = sourceList.FirstOrDefault(s => s.SlotIndex == sourceIndex || s.GlobalIndex == sourceIndex);
        var targetSlot = targetList.FirstOrDefault(s => s.SlotIndex == targetIndex || s.GlobalIndex == targetIndex);

        if (sourceSlot == null || targetSlot == null) 
            return false;

        SwapSlots(sourceSlot, targetSlot);
        _onInventoryChanged.OnNext(Unit.Default);
        return true;
    }

    private List<InventorySlot> GetSlotList(SlotArea area)
    {
        return area switch
        {
            SlotArea.Inventory => _totalSlots,
            SlotArea.Equipment => _equipmentSlots,
            SlotArea.Quick => _quickSlots,
            _ => null
        };
    }

    public IReadOnlyList<InventorySlot> GetEquipmentSlots() 
        => _equipmentSlots;

    public IReadOnlyList<InventorySlot> GetQuickSlots() 
        => _quickSlots;

    private bool TryGetValidItemData(int itemID, out ItemData itemData, out ItemCategory itemCategory)
    {
        itemData = null;
        itemCategory = ItemCategory.Etc;

        if (!Managers.Data.Items.ContainsKey(itemID))
            return false;

        itemData = Managers.Data.Items[itemID];
        Enum.TryParse(itemData.ItemCategory, true, out itemCategory);
        return true;
    }

    private bool HasEnoughSpaceInAll(List<InventorySlot> slots, int itemID, int maxStack, int quantity)
    {
        int required = quantity;

        foreach (var slot in slots)
        {
            if (required <= 0) 
                break;

            if (slot.ItemID == itemID && slot.Quantity < maxStack)
            {
                required -= (maxStack - slot.Quantity);
                continue;
            }

            if (slot.ItemID == 0) 
                required -= maxStack;
        }

        return required <= 0;
    }

    private void FillExistingItemSlots(List<InventorySlot> slots, int itemID, int maxStack, ref int remaining)
    {
        foreach (var slot in slots)
        {
            if (remaining <= 0) 
                break;

            if (slot.ItemID != itemID || slot.Quantity >= maxStack) 
                continue;

            int add = Math.Min(remaining, maxStack - slot.Quantity);
            slot.Quantity += add;
            remaining -= add;
        }
    }

    private void FillEmptySlots(List<InventorySlot> slots, int itemID, int maxStack, ref int remaining)
    {
        foreach (var slot in slots)
        {
            if (remaining <= 0) 
                break;

            if (slot.ItemID != 0) 
                continue;

            int add = Math.Min(remaining, maxStack);
            slot.ItemID = itemID;
            slot.Quantity = add;
            remaining -= add;
        }
    }

    private void SwapSlots(InventorySlot a, InventorySlot b)
    {
        (a.ItemID, b.ItemID) = (b.ItemID, a.ItemID);
        (a.Quantity, b.Quantity) = (b.Quantity, a.Quantity);
    }

    public void SortInventory(ItemCategory? currentTabType)
    {
        var targetSlots = GetSlotsByType(currentTabType);

        if (targetSlots == null)
            return;

        SortSlotList(targetSlots);
        _onInventoryChanged.OnNext(Unit.Default);
    }

    private void SortSlotList(List<InventorySlot> slots)
    {
        var sortedItems = slots
        .Where(s => s.ItemID != 0)
        .Select(s => (s.ItemID, s.Quantity))
        .OrderBy(x => x.ItemID)
        .ThenByDescending(x => x.Quantity)
        .ToList();
        int index = 0;

        foreach (var item in sortedItems)
        {
            slots[index].ItemID = item.ItemID;
            slots[index].Quantity = item.Quantity;
            index++;
        }

        while (index < slots.Count)
        {
            slots[index].ItemID = 0;
            slots[index].Quantity = 0;
            index++;
        }
    }

    public void ClearInventory()
    {
        foreach (var slot in _totalSlots) 
        { 
            slot.ItemID = 0; 
            slot.Quantity = 0;
        }

        foreach (var slot in _equipmentTabSlots) 
        { 
            slot.ItemID = 0; 
            slot.Quantity = 0; 
        }

        foreach (var slot in _consumptionTabSlots) 
        { 
            slot.ItemID = 0; 
            slot.Quantity = 0; 
        }

        foreach (var slot in _etcTabSlots) 
        { 
            slot.ItemID = 0; 
            slot.Quantity = 0; 
        }

        foreach (var slot in _equipmentSlots) 
        { 
            slot.ItemID = 0; 
            slot.Quantity = 0; 
        }

        foreach (var slot in _quickSlots) 
        { 
            slot.ItemID = 0; 
            slot.Quantity = 0; 
        }

        _onInventoryChanged.OnNext(Unit.Default);
    }

    public List<InventorySlot> ExportTotalSlotSaveData() 
        => ExportSlotList(_totalSlots);

    public List<InventorySlot> ExportEquipmentTabSaveData() 
        => ExportSlotList(_equipmentTabSlots);

    public List<InventorySlot> ExportConsumptionTabSaveData() 
        => ExportSlotList(_consumptionTabSlots);

    public List<InventorySlot> ExportEtcTabSaveData() 
        => ExportSlotList(_etcTabSlots);

    public List<InventorySlot> ExportEquipmentSlotSaveData() 
        => ExportSlotList(_equipmentSlots);

    public List<InventorySlot> ExportQuickSlotSaveData() 
        => ExportSlotList(_quickSlots);

    private List<InventorySlot> ExportSlotList(List<InventorySlot> slots)
    {
        return slots.Select(slot => new InventorySlot
        {
            GlobalIndex = slot.GlobalIndex,
            SlotIndex = slot.SlotIndex,
            ItemID = slot.ItemID,
            Quantity = slot.Quantity
        }).ToList();
    }
}
