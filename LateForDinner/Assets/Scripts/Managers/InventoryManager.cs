using Cysharp.Threading.Tasks;
using LateForDinner.Data;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        _equipmentSlots = savedEquipmentSlots ?? new List<InventorySlot>();
        EnsureSlotCapacity(_equipmentSlots, Define.Amount.MaxEquipmentSlot);
        _quickSlots = savedQuickSlots ?? new List<InventorySlot>();
        EnsureSlotCapacity(_quickSlots, Define.Amount.MaxQuickSlot);
        EnsureSlotCapacityOnly(_equipmentTabSlots, Define.Amount.InventoryTabSize);
        EnsureSlotCapacityOnly(_consumptionTabSlots, Define.Amount.InventoryTabSize);
        EnsureSlotCapacityOnly(_etcTabSlots, Define.Amount.InventoryTabSize);
        SyncAllTabsFromTotal();
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
        {
            slots[index].GlobalIndex = index;
            slots[index].SlotIndex = index;
        }
    }

    private void EnsureSlotCapacityOnly(List<InventorySlot> slots, int maxCapacity)
    {
        while (slots.Count < maxCapacity)
            slots.Add(new InventorySlot { ItemID = 0, Quantity = 0 });
    }

    public bool AddItem(int itemID, int quantity)
    {
        if (!TryGetValidItemData(itemID, out var itemData, out var itemCategory))
            return false;

        if (!HasEnoughSpaceForCategory(_totalSlots, itemID, itemData.MaxStack, quantity, itemCategory))
            return false;

        FillExistingItemSlots(_totalSlots, itemID, itemData.MaxStack, ref quantity);
        FillEmptySlots(_totalSlots, itemID, itemData.MaxStack, ref quantity);
        SyncAllTabsFromTotal();
        _onInventoryChanged.OnNext(Unit.Default);
        return true;
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

        SyncAllTabsFromTotal();
        _onInventoryChanged.OnNext(Unit.Default);
        return true;
    }

    public async UniTask<bool> DropItem(InventorySlot targetSlot, int quantity, Vector3 dropPosition)
    {
        if (targetSlot == null || targetSlot.ItemID <= 0 || targetSlot.Quantity < quantity)
            return false;

        int itemID = targetSlot.ItemID;
        int globalIndex = targetSlot.GlobalIndex;
        var masterSlot = _totalSlots.FirstOrDefault(s => s.GlobalIndex == globalIndex);

        if (masterSlot == null || masterSlot.Quantity < quantity)
            return false;

        masterSlot.Quantity -= quantity;

        if (masterSlot.Quantity <= 0)
        {
            masterSlot.ItemID = 0;
            masterSlot.Quantity = 0;
        }

        SyncAllTabsFromTotal();
        await SpawnItemProp(itemID, quantity, dropPosition);
        _onInventoryChanged.OnNext(Unit.Default);
        return true;
    }

    private async UniTask SpawnItemProp(int itemID, int quantity, Vector3 position)
    {
        if (!Managers.Data.Items.TryGetValue(itemID, out var itemData))
            return;

        var (propInstance, rentHandle) = await Managers.Pool.PopAsync<ItemProp>();

        if (propInstance != null)
        {
            propInstance.transform.position = position;
            await propInstance.Setup(itemID, quantity);
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

        if (currentTab.HasValue)
        {
            var masterSource = _totalSlots.FirstOrDefault(s => s.GlobalIndex == sourceSlot.GlobalIndex);
            var masterTarget = _totalSlots.FirstOrDefault(s => s.GlobalIndex == targetSlot.GlobalIndex);

            if (masterSource == null || masterTarget == null)
                return false;

            if (TryMergeOrSwapSlots(masterSource, masterTarget))
            {
                SyncAllTabsFromTotal();
                _onInventoryChanged.OnNext(Unit.Default);
                return true;
            }

            int toIndex = _totalSlots.IndexOf(masterSource);
            int fromIndex = _totalSlots.IndexOf(masterTarget);
            (_totalSlots[toIndex], _totalSlots[fromIndex]) = (_totalSlots[fromIndex], _totalSlots[toIndex]);
        }
        else
        {
            var sourceMaster = _totalSlots[sourceIndex];
            var targetMaster = _totalSlots[targetIndex];

            if (TryMergeOrSwapSlots(sourceMaster, targetMaster))
            {
                SyncAllTabsFromTotal();
                _onInventoryChanged.OnNext(Unit.Default);
                return true;
            }

            (_totalSlots[sourceIndex], _totalSlots[targetIndex]) = (_totalSlots[targetIndex], _totalSlots[sourceIndex]);
        }

        SyncAllTabsFromTotal();
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

        if (TryMergeOrSwapSlots(sourceSlot, targetSlot))
        {
            if (sourceArea == SlotArea.Inventory || targetArea == SlotArea.Inventory)
                SyncAllTabsFromTotal();

            _onInventoryChanged.OnNext(Unit.Default);
            return true;
        }

        SwapSlotsValues(sourceSlot, targetSlot);

        if (sourceArea == SlotArea.Inventory || targetArea == SlotArea.Inventory)
            SyncAllTabsFromTotal();

        _onInventoryChanged.OnNext(Unit.Default);
        return true;
    }

    private bool TryMergeOrSwapSlots(InventorySlot source, InventorySlot target)
    {
        if (source.ItemID <= 0 || source.ItemID != target.ItemID)
            return false;

        if (!TryGetValidItemData(source.ItemID, out var itemData, out _))
            return false;

        int maxStack = itemData.MaxStack;

        if (target.Quantity >= maxStack)
            return false;

        int space = maxStack - target.Quantity;
        int transfer = Math.Min(space, source.Quantity);
        target.Quantity += transfer;
        source.Quantity -= transfer;

        if (source.Quantity <= 0)
        {
            source.ItemID = 0;
            source.Quantity = 0;
        }

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

    private bool HasEnoughSpaceForCategory(List<InventorySlot> totalSlots, int itemID, int maxStack, int quantity, ItemCategory targetCategory)
    {
        var categorySlots = totalSlots
        .Where(s => s.ItemID > 0 && TryGetValidItemData(s.ItemID, out _, out var cat) && cat == targetCategory)
        .ToList();
        int usedCategorySlotsCount = categorySlots.Count;
        int maxCategorySlots = Define.Amount.InventoryTabSize;
        int emptyCategorySlotsCount = maxCategorySlots - usedCategorySlotsCount;
        int required = quantity;

        foreach (var slot in categorySlots)
        {
            if (required <= 0) break;

            if (slot.ItemID == itemID && slot.Quantity < maxStack)
                required -= (maxStack - slot.Quantity);
        }

        if (required > 0)
        {
            int neededSlots = (int)Math.Ceiling((double)required / maxStack);

            if (neededSlots > emptyCategorySlotsCount)
                return false;

            int totalEmptySlots = totalSlots.Count(s => s.ItemID == 0);

            if (neededSlots > totalEmptySlots)
                return false;
        }

        return true;
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

    private void SwapSlotsValues(InventorySlot a, InventorySlot b)
    {
        (a.ItemID, b.ItemID) = (b.ItemID, a.ItemID);
        (a.Quantity, b.Quantity) = (b.Quantity, a.Quantity);
    }

    private void SyncAllTabsFromTotal()
    {
        SyncSingleTabInstance(_equipmentTabSlots, ItemCategory.Equipment);
        SyncSingleTabInstance(_consumptionTabSlots, ItemCategory.Consumption);
        SyncSingleTabInstance(_etcTabSlots, ItemCategory.Etc);
    }

    private void SyncSingleTabInstance(List<InventorySlot> tabSlots, ItemCategory category)
    {
        EnsureSlotCapacityOnly(tabSlots, Define.Amount.InventoryTabSize);
        var matchedSlots = _totalSlots
        .Where(s => s.ItemID > 0 && TryGetValidItemData(s.ItemID, out _, out var cat) && cat == category)
        .ToList();
        int index = 0;

        foreach (var masterSlot in matchedSlots)
        {
            if (index >= tabSlots.Count) 
                break;

            tabSlots[index] = masterSlot;
            index++;
        }

        for (int i = index; i < tabSlots.Count; i++)
            tabSlots[i] = new InventorySlot { ItemID = 0, Quantity = 0 };
    }

    public void SortInventory(ItemCategory? currentTabType)
    {
        if (!currentTabType.HasValue)
            SortSlotList(_totalSlots);
        else
        {
            var targetSlots = GetSlotsByType(currentTabType);

            if (targetSlots != null)
                SortSlotList(targetSlots);
        }

        SyncAllTabsFromTotal();
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

        SyncAllTabsFromTotal();

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
