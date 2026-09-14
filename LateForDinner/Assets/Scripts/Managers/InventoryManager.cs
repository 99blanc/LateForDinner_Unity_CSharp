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
    public Observable<Unit> OnInventoryChanged 
        => _onInventoryChanged;
    private readonly Subject<Unit> _onInventoryChanged = new Subject<Unit>();
    private List<InventorySlot> _totalSlots = new List<InventorySlot>(Define.Amount.MaxInventorySlot);
    private List<InventorySlot> _equipmentTabSlots = new List<InventorySlot>(Define.Amount.InventoryTabSize);
    private List<InventorySlot> _consumptionTabSlots = new List<InventorySlot>(Define.Amount.InventoryTabSize);
    private List<InventorySlot> _etcTabSlots = new List<InventorySlot>(Define.Amount.InventoryTabSize);
    private List<InventorySlot> _equipmentSlots = new List<InventorySlot>(Define.Amount.MaxEquipmentSlot);
    private List<InventorySlot> _quickSlots = new List<InventorySlot>(Define.Amount.MaxQuickSlot);
    private List<EquipmentInstance> _unlockedEquipments = new List<EquipmentInstance>();

    public void InitInventory(SaveData data)
    {
        _totalSlots = data.TotalSlots ?? new List<InventorySlot>();
        EnsureSlotCapacity(_totalSlots, Define.Amount.MaxInventorySlot, true);
        _equipmentSlots = data.EquipmentSlots ?? new List<InventorySlot>();
        EnsureSlotCapacity(_equipmentSlots, Define.Amount.MaxEquipmentSlot, false);
        _quickSlots = data.QuickSlots ?? new List<InventorySlot>();
        EnsureSlotCapacity(_quickSlots, Define.Amount.MaxQuickSlot, false);
        _equipmentTabSlots = data.EquipmentTabSlots ?? new List<InventorySlot>();
        EnsureTabCapacity(_equipmentTabSlots);
        _consumptionTabSlots = data.ConsumptionTabSlots ?? new List<InventorySlot>();
        EnsureTabCapacity(_consumptionTabSlots);
        _etcTabSlots = data.EtcTabSlots ?? new List<InventorySlot>();
        EnsureTabCapacity(_etcTabSlots);
        _unlockedEquipments = data.UnlockedEquipments ?? new List<EquipmentInstance>();
        RebuildTabsFromTotal();
        SyncQuickSlotsAfterItemChanged();
    }

    private void EnsureSlotCapacity(List<InventorySlot> slots, int maxCapacity, bool updateGlobalIndex)
    {
        while (slots.Count < maxCapacity)
        {
            int index = slots.Count;
            slots.Add(new InventorySlot { GlobalIndex = updateGlobalIndex ? index : -1, SlotIndex = index });
        }

        int currentIndex = 0;

        foreach (var slot in slots)
        {
            if (updateGlobalIndex)
                slot.GlobalIndex = currentIndex;

            slot.SlotIndex = currentIndex++;
        }
    }

    private void EnsureTabCapacity(List<InventorySlot> tabSlots)
    {
        while (tabSlots.Count < Define.Amount.InventoryTabSize)
        {
            int index = tabSlots.Count;
            tabSlots.Add(new InventorySlot { GlobalIndex = -1, SlotIndex = index });
        }

        int currentIndex = 0;

        foreach (var slot in tabSlots)
            slot.SlotIndex = currentIndex++;
    }

    public bool AddItem(int itemID, int quantity, string instanceID = "")
    {
        if (!itemID.TryGetValidItemData(out var itemData, out var itemCategory))
            return false;

        if (!_totalSlots.HasEnoughSpaceForCategory(itemID, itemData.MaxStack, quantity, itemCategory))
            return false;

        FillExistingItemSlots(_totalSlots, itemID, itemData.MaxStack, ref quantity);
        FillEmptySlots(_totalSlots, itemID, itemData.MaxStack, ref quantity, instanceID);
        RebuildTabsFromTotal();
        FinalizeInventoryChange();
        return true;
    }

    public bool RemoveItem(int itemID, int quantity)
    {
        if (!itemID.TryGetValidItemData(out _, out _) || _totalSlots.Where(slot => slot.ItemID == itemID).Sum(slot => slot.Quantity) < quantity)
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
                slot.ClearSlot();
        }

        RebuildTabsFromTotal();
        FinalizeInventoryChange();
        return true;
    }

    public bool RemoveItem(InventorySlot targetSlot, int quantity)
    {
        if (targetSlot == null || targetSlot.ItemID <= 0 || targetSlot.Quantity < quantity)
            return false;

        var slotToModify = _totalSlots.FirstOrDefault(slot => slot == targetSlot || slot.GlobalIndex == targetSlot.GlobalIndex);

        if (slotToModify == null || slotToModify.ItemID <= 0 || slotToModify.Quantity < quantity)
            return false;

        slotToModify.Quantity -= quantity;

        if (slotToModify.Quantity <= 0)
            slotToModify.ClearSlot();

        RebuildTabsFromTotal();
        FinalizeInventoryChange(slotToModify);
        return true;
    }

    public async UniTask<bool> DropItem(InventorySlot targetSlot, int quantity, Vector3 dropPosition)
    {
        if (targetSlot == null || targetSlot.ItemID <= 0 || targetSlot.Quantity < quantity)
            return false;

        int itemID = targetSlot.ItemID;
        var masterSlot = _totalSlots.FirstOrDefault(slot => slot == targetSlot || slot.GlobalIndex == targetSlot.GlobalIndex);

        if (masterSlot == null || masterSlot.ItemID <= 0 || masterSlot.Quantity < quantity)
            return false;

        string droppedInstanceID = masterSlot.InstanceID;
        masterSlot.Quantity -= quantity;

        if (masterSlot.Quantity <= 0)
            masterSlot.ClearSlot();

        await SpawnItemProp(itemID, quantity, droppedInstanceID, dropPosition);
        RebuildTabsFromTotal();
        FinalizeInventoryChange(masterSlot);
        return true;
    }

    private async UniTask SpawnItemProp(int itemID, int quantity, string instanceID, Vector3 position)
    {
        if (!Managers.Data.Items.TryGetValue(itemID, out _))
            return;

        var (propInstance, _) = await Managers.Pool.PopAsync<ItemProp>();

        if (propInstance != null)
        {
            propInstance.transform.position = position;
            await propInstance.Setup(itemID, quantity, instanceID);
        }
    }

    public bool EquipItem(ItemCategory? currentTabType, InventorySlot sourceSlot, InventorySlot targetEquipmentSlot)
    {
        if (sourceSlot == null || sourceSlot.ItemID <= 0 || targetEquipmentSlot == null)
            return false;

        EquipmentSlotType targetSlotType = (EquipmentSlotType)targetEquipmentSlot.SlotIndex;

        if (!sourceSlot.ItemID.TryGetValidItemData(out var itemData, out _) || !itemData.IsEquipmentCategory() || !itemData.CanEquipInSlot(targetSlotType))
            return false;

        if (!_equipmentSlots.Contains(targetEquipmentSlot))
            return false;

        Character myCharacter = Managers.Game?.Player;

        if (myCharacter == null)
            return false;

        InventorySlot masterSlot = null;

        if (!currentTabType.HasValue)
            masterSlot = _totalSlots.FirstOrDefault(slot => slot == sourceSlot || slot.GlobalIndex == sourceSlot.GlobalIndex);
        else
        {
            var targetTabSlots = GetSlotsByType(currentTabType);

            if (targetTabSlots != null && targetTabSlots.Contains(sourceSlot))
                masterSlot = _totalSlots.FirstOrDefault(slot => slot.GlobalIndex == sourceSlot.GlobalIndex);
        }

        if (masterSlot == null || masterSlot.ItemID <= 0)
            masterSlot = _totalSlots.FirstOrDefault(slot => slot.ItemID == sourceSlot.ItemID && slot.InstanceID == sourceSlot.InstanceID);

        if (masterSlot == null || masterSlot.ItemID <= 0)
            return false;

        if (targetEquipmentSlot.ItemID > 0)
        {
            UnequipItemInternal(targetEquipmentSlot, myCharacter);
            var tempEquipmentData = new InventorySlot();
            tempEquipmentData.AssignSlotData(targetEquipmentSlot);
            targetEquipmentSlot.AssignSlotData(masterSlot);
            targetEquipmentSlot.GlobalIndex = -1;
            masterSlot.AssignSlotData(tempEquipmentData);
            masterSlot.GlobalIndex = masterSlot.SlotIndex;
        }
        else
        {
            targetEquipmentSlot.AssignSlotData(masterSlot);
            targetEquipmentSlot.GlobalIndex = -1;
            masterSlot.ClearSlot();
        }

        var equipInstance = _unlockedEquipments.FirstOrDefault(equip => equip.InstanceID == targetEquipmentSlot.InstanceID);

        if (equipInstance == null && !string.IsNullOrEmpty(targetEquipmentSlot.InstanceID))
        {
            equipInstance = new EquipmentInstance { InstanceID = targetEquipmentSlot.InstanceID, ItemID = targetEquipmentSlot.ItemID, UpgradeLevel = 0, ExtraOptionValue = 0 };
            _unlockedEquipments.Add(equipInstance);
        }

        if (equipInstance != null)
            equipInstance.ApplyEquipmentEffects(itemData, myCharacter);

        RebuildTabsFromTotal();
        FinalizeInventoryChange();
        return true;
    }

    public bool UnEquipItem(ItemCategory? currentTabType, InventorySlot targetEquipmentSlot, InventorySlot targetSlot = null)
    {
        if (targetEquipmentSlot == null || targetEquipmentSlot.ItemID <= 0)
            return false;

        if (!_equipmentSlots.Contains(targetEquipmentSlot))
            return false;

        Character myCharacter = Managers.Game?.Player;

        if (myCharacter == null)
            return false;

        InventorySlot masterSlot = null;

        if (targetSlot != null)
        {
            if (targetSlot.ItemID > 0)
            {
                EquipmentSlotType targetSlotType = (EquipmentSlotType)targetEquipmentSlot.SlotIndex;

                if (!targetSlot.ItemID.TryGetValidItemData(out var targetItemData, out _) || !targetItemData.IsEquipmentCategory() || !targetItemData.CanEquipInSlot(targetSlotType))
                    return false;

                masterSlot = _totalSlots.FirstOrDefault(slot => slot == targetSlot || slot.GlobalIndex == targetSlot.GlobalIndex);

                if (masterSlot == null)
                    masterSlot = _totalSlots.FirstOrDefault(slot => slot.ItemID == targetSlot.ItemID && slot.InstanceID == targetSlot.InstanceID);

                if (masterSlot == null)
                    return false;

                UnequipItemInternal(targetEquipmentSlot, myCharacter);
                var tempEquipmentData = new InventorySlot();
                tempEquipmentData.AssignSlotData(targetEquipmentSlot);
                targetEquipmentSlot.AssignSlotData(masterSlot);
                targetEquipmentSlot.GlobalIndex = -1;
                masterSlot.AssignSlotData(tempEquipmentData);
                masterSlot.GlobalIndex = masterSlot.SlotIndex;
                var newEquipInstance = _unlockedEquipments.FirstOrDefault(equip => equip.InstanceID == targetEquipmentSlot.InstanceID);

                if (newEquipInstance != null)
                    newEquipInstance.ApplyEquipmentEffects(targetItemData, myCharacter);

                RebuildTabsFromTotal();
                FinalizeInventoryChange();
                return true;
            }
            else
                masterSlot = _totalSlots.FirstOrDefault(slot => slot == targetSlot || (slot.GlobalIndex == targetSlot.GlobalIndex && slot.ItemID <= 0)) ?? _totalSlots.FirstOrDefault(slot => slot.ItemID <= 0);
        }
        else
            masterSlot = _totalSlots.FirstOrDefault(slot => slot.ItemID <= 0);

        if (masterSlot == null || masterSlot.ItemID > 0)
            return false;

        UnequipItemInternal(targetEquipmentSlot, myCharacter);
        masterSlot.AssignSlotData(targetEquipmentSlot);
        masterSlot.GlobalIndex = masterSlot.SlotIndex;
        targetEquipmentSlot.ClearSlot();
        RebuildTabsFromTotal();
        FinalizeInventoryChange();
        return true;
    }

    private void UnequipItemInternal(InventorySlot equipmentSlot, Character character)
    {
        if (equipmentSlot.ItemID <= 0)
            return;

        if (equipmentSlot.ItemID.TryGetValidItemData(out var itemData, out _))
        {
            var equipInstance = _unlockedEquipments.FirstOrDefault(equip => equip.InstanceID == equipmentSlot.InstanceID);

            if (equipInstance != null)
                equipInstance.RemoveEquipmentEffects(itemData, character);
        }
    }

    public bool UseConsumableItem(InventorySlot targetSlot, GameObject targetObject = null)
    {
        if (targetSlot == null || targetSlot.ItemID <= 0 || !targetSlot.IsValidAndCategory(ItemCategory.Consumption))
            return false;

        if (!targetSlot.ItemID.TryGetValidItemData(out var itemData, out _))
            return false;

        if (!itemData.CanUseConsumption(targetSlot))
            return false;

        Character myCharacter = Managers.Game?.Player;

        if (myCharacter == null)
            return false;

        if (itemData.ShouldConsumeOnUse())
        {
            var masterSlot = _totalSlots.FirstOrDefault(slot => slot == targetSlot || slot.GlobalIndex == targetSlot.GlobalIndex);

            if (masterSlot == null || !RemoveItem(masterSlot, 1))
                return false;

            SyncQuickSlotsAfterItemChanged(masterSlot);
        }

        itemData.ApplyConsumptionEffects(myCharacter, targetObject);
        itemData.PostProcessConsumption(targetSlot);
        RebuildTabsFromTotal();
        FinalizeInventoryChange();
        return true;
    }

    public bool UseQuickSlot(int quickSlotIndex, GameObject targetObject = null)
    {
        if (!quickSlotIndex.IsValidIndex(_quickSlots.Count))
            return false;

        var quickSlot = _quickSlots[quickSlotIndex];
        return UseQuickSlot(quickSlot, targetObject);
    }

    public bool UseQuickSlot(InventorySlot quickSlot, GameObject targetObject = null)
    {
        if (quickSlot == null || quickSlot.ItemID <= 0 || !quickSlot.IsValidAndCategory(ItemCategory.Consumption))
            return false;

        if (!quickSlot.ItemID.TryGetValidItemData(out var itemData, out _))
            return false;

        if (!itemData.CanUseConsumption(quickSlot))
            return false;

        Character myCharacter = Managers.Game?.Player;

        if (myCharacter == null)
            return false;

        if (itemData.ShouldConsumeOnUse())
        {
            bool success = false;
            InventorySlot masterSlot = null;

            if (quickSlot.GlobalIndex >= 0)
            {
                masterSlot = _totalSlots.FirstOrDefault(slot => slot.GlobalIndex == quickSlot.GlobalIndex);

                if (masterSlot != null && masterSlot.ItemID == quickSlot.ItemID)
                    success = RemoveItem(masterSlot, 1);
            }

            if (!success)
            {
                success = RemoveItem(quickSlot.ItemID, 1);
                masterSlot = _totalSlots.FirstOrDefault(slot => slot.ItemID == quickSlot.ItemID);
            }

            if (!success)
                return false;

            SyncQuickSlotsAfterItemChanged(masterSlot);
        }

        itemData.ApplyConsumptionEffects(myCharacter, targetObject);
        itemData.PostProcessConsumption(quickSlot);
        RebuildTabsFromTotal();
        FinalizeInventoryChange();
        return true;
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
        var sourceSlot = GetSourceSlot(currentTab, sourceArea, sourceIndex);
        var targetSlot = GetTargetSlot(currentTab, targetArea, targetIndex);
        return HandleItemMoveByTab(currentTab, sourceArea, sourceSlot, targetArea, targetSlot);
    }

    public bool HandleItemMoveByTab(ItemCategory? currentTab, SlotArea sourceArea, InventorySlot sourceSlot, SlotArea targetArea, InventorySlot targetSlot)
    {
        if (sourceSlot == null || targetSlot == null)
            return false;

        if (sourceArea != targetArea)
            return HandleCrossAreaMove(currentTab, sourceArea, sourceSlot, targetArea, targetSlot);

        if (!currentTab.HasValue)
            return HandleTotalTabMove(sourceSlot, targetSlot);

        return HandleCategoryTabMove(currentTab.Value, sourceSlot, targetSlot);
    }

    private bool HandleTotalTabMove(InventorySlot sourceMaster, InventorySlot targetMaster)
    {
        if (sourceMaster == targetMaster)
            return false;

        if (sourceMaster.TryMergeSlots(targetMaster))
        {
            RebuildTabsFromTotal();
            FinalizeInventoryChange();
            return true;
        }

        int globalA = sourceMaster.GlobalIndex;
        int globalB = targetMaster.GlobalIndex;
        sourceMaster.SwapValues(targetMaster);
        (sourceMaster.GlobalIndex, targetMaster.GlobalIndex) = (targetMaster.GlobalIndex, sourceMaster.GlobalIndex);
        SwapQuickSlotReferences(globalA, globalB);
        RebuildTabsFromTotal();
        FinalizeInventoryChange();
        return true;
    }

    private bool HandleCategoryTabMove(ItemCategory currentTab, InventorySlot sourceTabSlot, InventorySlot targetTabSlot)
    {
        if (sourceTabSlot == targetTabSlot)
            return false;

        if (sourceTabSlot.TryMergeSlots(targetTabSlot))
        {
            SyncTotalSlotsByTabMerge(sourceTabSlot, targetTabSlot);
            RebuildTabsFromTotal();
            FinalizeInventoryChange();
            return true;
        }

        sourceTabSlot.SwapValues(targetTabSlot);
        (sourceTabSlot.GlobalIndex, targetTabSlot.GlobalIndex) = (targetTabSlot.GlobalIndex, sourceTabSlot.GlobalIndex);
        SyncTotalSlotsByTabMove(sourceTabSlot, targetTabSlot);
        RebuildTabsFromTotal();
        FinalizeInventoryChange();
        return true;
    }

    public bool HandleCrossAreaMove(ItemCategory? currentTabType, SlotArea sourceArea, InventorySlot sourceSlot, SlotArea targetArea, InventorySlot targetSlot)
    {
        if (targetArea == SlotArea.Equipment)
        {
            if (sourceSlot == null || sourceSlot.ItemID <= 0)
                return false;

            if (!sourceSlot.ItemID.TryGetValidItemData(out var itemData, out _) || !itemData.IsEquipmentCategory())
                return false;

            return EquipItem(currentTabType, sourceSlot, targetSlot);
        }

        if (targetArea == SlotArea.Quick)
            return HandleQuickSlotMove(sourceArea, sourceSlot, targetSlot);

        if (sourceArea == SlotArea.Equipment)
        {
            if (targetSlot == null)
                return false;

            return UnEquipItem(currentTabType, sourceSlot, targetSlot);
        }

        if (sourceArea == SlotArea.Quick)
        {
            if (sourceSlot == null || targetSlot == null)
                return false;

            sourceSlot.SwapValues(targetSlot);
            FinalizeInventoryChange();
            return true;
        }

        if (sourceSlot.TryMergeSlots(targetSlot))
        {
            SyncTotalSlotsByTabMerge(sourceSlot, targetSlot);
            RebuildTabsFromTotal();
            FinalizeInventoryChange();
            return true;
        }

        int globalA = sourceSlot.GlobalIndex;
        int globalB = targetSlot.GlobalIndex;
        sourceSlot.SwapValues(targetSlot);
        (sourceSlot.GlobalIndex, targetSlot.GlobalIndex) = (targetSlot.GlobalIndex, sourceSlot.GlobalIndex);
        SyncTotalSlotsByTabMove(sourceSlot, targetSlot);
        SwapQuickSlotReferences(globalA, globalB);
        RebuildTabsFromTotal();
        FinalizeInventoryChange();
        return true;
    }

    public bool HandleQuickSlotMove(SlotArea sourceArea, InventorySlot sourceSlot, InventorySlot targetQuickSlot)
    {
        if (sourceSlot == null || sourceSlot.ItemID <= 0 || targetQuickSlot == null)
            return false;

        if (!sourceSlot.ItemID.TryGetValidItemData(out var itemData, out _) || itemData.IsEtc())
            return false;

        if (!_quickSlots.Contains(targetQuickSlot))
            return false;

        if (sourceArea == SlotArea.Quick)
        {
            sourceSlot.SwapValues(targetQuickSlot);
            (sourceSlot.GlobalIndex, targetQuickSlot.GlobalIndex) = (targetQuickSlot.GlobalIndex, sourceSlot.GlobalIndex);
        }
        else
        {
            targetQuickSlot.AssignSlotData(sourceSlot);
            targetQuickSlot.GlobalIndex = sourceSlot.GlobalIndex;
        }

        FinalizeInventoryChange();
        return true;
    }

    private InventorySlot GetSourceSlot(ItemCategory? currentTabType, SlotArea sourceArea, int sourceIndex)
    {
        return sourceArea switch
        {
            SlotArea.Equipment => _equipmentSlots.ElementAtOrDefault(sourceIndex),
            SlotArea.Quick => _quickSlots.ElementAtOrDefault(sourceIndex),
            _ => GetInventorySlotByTab(currentTabType, sourceIndex)
        };
    }

    private InventorySlot GetTargetSlot(ItemCategory? currentTabType, SlotArea targetArea, int targetIndex)
    {
        return targetArea switch
        {
            SlotArea.Equipment => _equipmentSlots.ElementAtOrDefault(targetIndex),
            SlotArea.Quick => _quickSlots.ElementAtOrDefault(targetIndex),
            _ => GetInventorySlotByTab(currentTabType, targetIndex, true)
        };
    }

    private InventorySlot GetInventorySlotByTab(ItemCategory? currentTabType, int index, bool isTarget = false)
    {
        if (!currentTabType.HasValue)
            return _totalSlots.ElementAtOrDefault(index);

        var targetList = GetSlotsByType(currentTabType);
        return targetList?.ElementAtOrDefault(index);
    }

    public InventorySlot GetEquipmentSlotByType(EquipmentSlotType slotType)
    {
        int index = (int)slotType;
        return _equipmentSlots.ElementAtOrDefault(index);
    }

    private void FinalizeInventoryChange(InventorySlot modifiedSlot = null)
    {
        SyncQuickSlotsAfterItemChanged(modifiedSlot);
        _onInventoryChanged.OnNext(Unit.Default);
    }

    private void SyncTotalSlotsByTabMerge(InventorySlot sourceTabSlot, InventorySlot targetTabSlot)
    {
        var masterSource = _totalSlots.FirstOrDefault(slot => slot.GlobalIndex == sourceTabSlot.GlobalIndex);
        var masterTarget = _totalSlots.FirstOrDefault(slot => slot.GlobalIndex == targetTabSlot.GlobalIndex);

        if (masterSource != null && masterTarget != null)
        {
            masterSource.TryMergeSlots(masterTarget);
            SyncQuickSlotsAfterItemChanged(masterSource);
            SyncQuickSlotsAfterItemChanged(masterTarget);
        }
    }

    private void SyncQuickSlotsAfterItemChanged(InventorySlot modifiedInventorySlot = null)
    {
        foreach (var quickSlot in _quickSlots)
        {
            if (quickSlot == null || quickSlot.ItemID <= 0)
                continue;

            if (modifiedInventorySlot != null)
            {
                if (quickSlot.IsRelatedTo(modifiedInventorySlot))
                {
                    if (modifiedInventorySlot.ItemID <= 0 || modifiedInventorySlot.Quantity <= 0)
                        quickSlot.ClearSlot();
                    else
                        quickSlot.Quantity = modifiedInventorySlot.Quantity;
                }
                continue;
            }

            var masterSlot = _totalSlots.FirstOrDefault(slot => slot.GlobalIndex == quickSlot.GlobalIndex && slot.ItemID == quickSlot.ItemID);

            if (masterSlot == null || masterSlot.ItemID <= 0 || masterSlot.ItemID != quickSlot.ItemID)
            {
                masterSlot = _totalSlots.FirstOrDefault(slot => slot.ItemID == quickSlot.ItemID);

                if (masterSlot != null)
                    quickSlot.GlobalIndex = masterSlot.GlobalIndex;
            }

            if (masterSlot != null && masterSlot.ItemID > 0 && masterSlot.ItemID == quickSlot.ItemID)
                quickSlot.Quantity = masterSlot.Quantity;
            else
                quickSlot.ClearSlot();
        }
    }

    public void ClearQuickSlot(InventorySlot quickSlot)
    {
        if (quickSlot != null && _quickSlots.Contains(quickSlot))
            quickSlot.ClearSlot();
    }

    public void ClearQuickSlot(int index)
    {
        var slot = _quickSlots.ElementAtOrDefault(index);
        ClearQuickSlot(slot);
    }

    private void SwapQuickSlotReferences(int globalIndexA, int globalIndexB)
    {
        foreach (var quickSlot in _quickSlots)
        {
            if (quickSlot == null || quickSlot.ItemID <= 0)
                continue;

            if (quickSlot.GlobalIndex == globalIndexA || quickSlot.GlobalIndex == globalIndexB)
                quickSlot.GlobalIndex = (quickSlot.GlobalIndex == globalIndexA) ? globalIndexB : globalIndexA;
        }
    }

    public IReadOnlyList<InventorySlot> GetEquipmentSlots() 
        => _equipmentSlots;

    public IReadOnlyList<InventorySlot> GetQuickSlots() 
        => _quickSlots;

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

    private void FillEmptySlots(List<InventorySlot> slots, int itemID, int maxStack, ref int remaining, string instanceID = "")
    {
        bool isEquipment = itemID.TryGetValidItemData(out _, out var category) && category == ItemCategory.Equipment;

        foreach (var slot in slots)
        {
            if (remaining <= 0)
                break;

            if (slot.ItemID != 0)
                continue;

            slot.InstanceID = null;
            int add = Math.Min(remaining, maxStack);
            slot.ItemID = itemID;
            slot.Quantity = add;
            remaining -= add;

            if (!isEquipment)
                continue;

            string targetInstanceID = string.IsNullOrEmpty(instanceID) ? Guid.NewGuid().ToString() : instanceID;
            slot.InstanceID = targetInstanceID;

            if (!_unlockedEquipments.Any(equip => equip.InstanceID == targetInstanceID))
                _unlockedEquipments.Add(new EquipmentInstance { InstanceID = targetInstanceID, ItemID = itemID, UpgradeLevel = 0, ExtraOptionValue = 0 });
        }
    }

    private void RebuildTabsFromTotal()
    {
        SyncTabWithCategory(_equipmentTabSlots, ItemCategory.Equipment);
        SyncTabWithCategory(_consumptionTabSlots, ItemCategory.Consumption);
        SyncTabWithCategory(_etcTabSlots, ItemCategory.Etc);
    }

    private void SyncTotalSlotsByTabMove(InventorySlot slotA, InventorySlot slotB)
    {
        var masterA = _totalSlots.FirstOrDefault(slot => slot.GlobalIndex == slotA.GlobalIndex);
        var masterB = _totalSlots.FirstOrDefault(slot => slot.GlobalIndex == slotB.GlobalIndex);

        if (masterA != null && masterB != null)
        {
            masterA.SwapValues(masterB);
            (masterA.GlobalIndex, masterB.GlobalIndex) = (masterB.GlobalIndex, masterA.GlobalIndex);
            SwapQuickSlotReferences(masterA.GlobalIndex, masterB.GlobalIndex);
        }
    }

    private void SyncTabWithCategory(List<InventorySlot> tabSlots, ItemCategory category)
    {
        EnsureTabCapacity(tabSlots);
        var masterItems = _totalSlots
        .Where(slot => slot.IsValidAndCategory(category))
        .ToList();

        foreach (var tabSlot in tabSlots)
        {
            if (tabSlot.ItemID <= 0)
            {
                tabSlot.GlobalIndex = -1;
                continue;
            }

            var matchedMaster = masterItems.FirstOrDefault(m => m.ItemID == tabSlot.ItemID && m.InstanceID == tabSlot.InstanceID);

            if (matchedMaster == null && tabSlot.GlobalIndex >= 0)
                matchedMaster = masterItems.FirstOrDefault(m => m.GlobalIndex == tabSlot.GlobalIndex);

            if (matchedMaster != null)
            {
                tabSlot.AssignSlotData(matchedMaster);
                tabSlot.GlobalIndex = matchedMaster.GlobalIndex;
                masterItems.Remove(matchedMaster);
            }
            else
                tabSlot.ClearTabSlot();
        }

        foreach (var master in masterItems)
        {
            var emptyTabSlot = tabSlots.FirstOrDefault(t => t.ItemID <= 0);

            if (emptyTabSlot != null)
            {
                emptyTabSlot.AssignSlotData(master);
                emptyTabSlot.GlobalIndex = master.GlobalIndex;
            }
        }
    }

    public void SortInventory(ItemCategory? currentTabType)
    {
        if (!currentTabType.HasValue)
        {
            _totalSlots.SortSlots();

            int index = 0;

            foreach (var slot in _totalSlots)
                slot.GlobalIndex = index++;

            RebuildTabsFromTotal();
        }
        else
        {
            var targetSlots = GetSlotsByType(currentTabType);

            if (targetSlots == null)
                return;

            targetSlots.SortSlots();
            SyncTabWithCategory(targetSlots, currentTabType.Value);
        }

        FinalizeInventoryChange();
    }

    public void ClearInventory()
    {
        foreach (var slot in _totalSlots)
            slot.ClearSlot();

        RebuildTabsFromTotal();

        foreach (var slot in _equipmentSlots)
            slot.ClearSlot();

        FinalizeInventoryChange();
    }

    public List<InventorySlot> ExportTotalSlotSaveData()
        => _totalSlots.DeepCopySlots();

    public List<InventorySlot> ExportEquipmentTabSaveData()
        => _equipmentTabSlots.DeepCopySlots();

    public List<InventorySlot> ExportConsumptionTabSaveData()
        => _consumptionTabSlots.DeepCopySlots();

    public List<InventorySlot> ExportEtcTabSaveData()
        => _etcTabSlots.DeepCopySlots();

    public List<InventorySlot> ExportEquipmentSlotSaveData()
        => _equipmentSlots.DeepCopySlots();

    public List<InventorySlot> ExportQuickSlotSaveData()
        => _quickSlots.DeepCopySlots();

    public List<EquipmentInstance> ExportUnlockedEquipmentsSaveData()
        => _unlockedEquipments.Select(equip => new EquipmentInstance { InstanceID = equip.InstanceID, ItemID = equip.ItemID, UpgradeLevel = equip.UpgradeLevel, ExtraOptionValue = equip.ExtraOptionValue }).ToList();
}
