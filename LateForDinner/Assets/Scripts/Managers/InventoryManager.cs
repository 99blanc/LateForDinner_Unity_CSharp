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
            slots.Add(new InventorySlot
            {
                GlobalIndex = updateGlobalIndex ? index : slots.Count,
                SlotIndex = index,
                ItemID = 0,
                Quantity = 0,
                InstanceID = null
            });
        }

        for (int index = 0; index < slots.Count; index++)
        {
            if (updateGlobalIndex)
                slots[index].GlobalIndex = index;

            slots[index].SlotIndex = index;
        }
    }

    private void EnsureTabCapacity(List<InventorySlot> tabSlots)
    {
        while (tabSlots.Count < Define.Amount.InventoryTabSize)
        {
            int index = tabSlots.Count;
            tabSlots.Add(new InventorySlot
            {
                GlobalIndex = -1,
                SlotIndex = index,
                ItemID = 0,
                Quantity = 0,
                InstanceID = null
            });
        }

        for (int index = 0; index < tabSlots.Count; index++)
            tabSlots[index].SlotIndex = index;
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
        SyncQuickSlotsAfterItemChanged();
        _onInventoryChanged.OnNext(Unit.Default);
        return true;
    }

    public bool RemoveItem(int itemID, int quantity)
    {
        if (!itemID.TryGetValidItemData(out _, out _))
            return false;

        int totalExistingQuantity = _totalSlots.Where(slot => slot.ItemID == itemID).Sum(slot => slot.Quantity);

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
                slot.ClearSlot();
        }

        RebuildTabsFromTotal();
        SyncQuickSlotsAfterItemChanged();
        _onInventoryChanged.OnNext(Unit.Default);
        return true;
    }

    public bool RemoveItem(InventorySlot targetSlot, int quantity)
    {
        if (targetSlot == null || targetSlot.ItemID <= 0 || targetSlot.Quantity < quantity)
            return false;

        var slotToModify = _totalSlots.FirstOrDefault(slot => slot.GlobalIndex == targetSlot.GlobalIndex) ?? targetSlot;

        if (slotToModify.Quantity < quantity)
            return false;

        slotToModify.Quantity -= quantity;

        if (slotToModify.Quantity <= 0)
            slotToModify.ClearSlot();

        RebuildTabsFromTotal();
        SyncQuickSlotsAfterItemChanged(slotToModify);
        _onInventoryChanged.OnNext(Unit.Default);
        return true;
    }

    public async UniTask<bool> DropItem(InventorySlot targetSlot, int quantity, Vector3 dropPosition)
    {
        if (targetSlot == null || targetSlot.ItemID <= 0 || targetSlot.Quantity < quantity)
            return false;

        int itemID = targetSlot.ItemID;
        var masterSlot = _totalSlots.FirstOrDefault(slot => slot.GlobalIndex == targetSlot.GlobalIndex);

        if (masterSlot == null || masterSlot.Quantity < quantity)
            return false;

        string droppedInstanceID = masterSlot.InstanceID;
        masterSlot.Quantity -= quantity;

        if (masterSlot.Quantity <= 0)
            masterSlot.ClearSlot();

        RebuildTabsFromTotal();
        await SpawnItemProp(itemID, quantity, droppedInstanceID, dropPosition);
        SyncQuickSlotsAfterItemChanged(masterSlot);
        _onInventoryChanged.OnNext(Unit.Default);
        return true;
    }

    private async UniTask SpawnItemProp(int itemID, int quantity, string instanceID, Vector3 position)
    {
        if (!Managers.Data.Items.TryGetValue(itemID, out var itemData))
            return;

        var (propInstance, rentHandle) = await Managers.Pool.PopAsync<ItemProp>();

        if (propInstance != null)
        {
            propInstance.transform.position = position;
            await propInstance.Setup(itemID, quantity, instanceID);
        }
    }

    public bool EquipItem(SlotArea sourceArea, int sourceIndex, EquipmentSlotType targetSlotType, ItemCategory? currentTabType = null)
    {
        var sourceSlot = GetSourceSlot(currentTabType, sourceArea, sourceIndex);

        if (sourceSlot == null || sourceSlot.ItemID <= 0)
            return false;

        if (!Managers.Data.Items.TryGetValue(sourceSlot.ItemID, out var itemData) || !itemData.IsEquipmentCategory())
            return false;

        if (!itemData.CanEquipInSlot(targetSlotType))
            return false;

        int targetIndex = (int)targetSlotType;

        if (_equipmentSlots == null || targetIndex < 0 || targetIndex >= _equipmentSlots.Count)
            return false;

        var equipmentSlot = _equipmentSlots[targetIndex];
        Character myCharacter = Managers.Game?.Player;

        if (myCharacter == null)
            return false;

        if (equipmentSlot.ItemID > 0)
            UnequipItemInternal(equipmentSlot, myCharacter);

        var equipInstance = _unlockedEquipments.FirstOrDefault(equip => equip.InstanceID == sourceSlot.InstanceID);

        if (equipInstance == null && !string.IsNullOrEmpty(sourceSlot.InstanceID))
        {
            equipInstance = new EquipmentInstance
            {
                InstanceID = sourceSlot.InstanceID,
                ItemID = sourceSlot.ItemID,
                UpgradeLevel = 0,
                ExtraOptionValue = 0
            };
            _unlockedEquipments.Add(equipInstance);
        }

        SwapSlotsValues(sourceSlot, equipmentSlot);

        if (equipmentSlot.ItemID > 0 && equipInstance != null)
            equipInstance.ApplyEquipmentEffects(itemData, myCharacter);

        PostProcessCrossMove(sourceArea, SlotArea.Equipment);
        return true;
    }

    public bool UnequipItem(EquipmentSlotType targetSlotType, ItemCategory? currentTabType = null, int? targetIndex = null)
    {
        if (!targetIndex.HasValue)
            return false;

        int sourceIndex = (int)targetSlotType;

        if (_equipmentSlots == null || sourceIndex < 0 || sourceIndex >= _equipmentSlots.Count)
            return false;

        var equipmentSlot = _equipmentSlots[sourceIndex];

        if (equipmentSlot.ItemID <= 0)
            return false;

        var targetList = GetSlotsByType(currentTabType);

        if (targetList == null || !targetIndex.Value.IsValidIndex(targetList.Count))
            return false;

        var targetSlot = targetList[targetIndex.Value];

        if (targetSlot.ItemID > 0)
            return false;

        Character myCharacter = Managers.Game?.Player;

        if (myCharacter == null)
            return false;

        UnequipItemInternal(equipmentSlot, myCharacter);
        targetSlot.ItemID = equipmentSlot.ItemID;
        targetSlot.Quantity = equipmentSlot.Quantity;
        targetSlot.InstanceID = equipmentSlot.InstanceID;

        if (currentTabType.HasValue)
            SyncTotalFromTab(currentTabType.Value);

        equipmentSlot.ClearSlot();
        RebuildTabsFromTotal();

        SyncQuickSlotsAfterItemChanged();
        _onInventoryChanged.OnNext(Unit.Default);
        return true;
    }

    private void UnequipItemInternal(InventorySlot equipmentSlot, Character character)
    {
        if (equipmentSlot.ItemID <= 0)
            return;

        if (Managers.Data.Items.TryGetValue(equipmentSlot.ItemID, out var itemData))
        {
            var equipInstance = _unlockedEquipments.FirstOrDefault(equip => equip.InstanceID == equipmentSlot.InstanceID);

            if (equipInstance != null)
                equipInstance.RemoveEquipmentEffects(itemData, character);
        }
    }

    public bool UseConsumableItem(InventorySlot targetSlot, GameObject targetObject = null)
    {
        if (targetSlot == null || targetSlot.ItemID <= 0)
            return false;

        int itemID = targetSlot.ItemID;

        if (!itemID.TryGetValidItemData(out var itemData, out var itemCategory) || itemCategory != ItemCategory.Consumption)
            return false;

        if (!itemData.CanUseConsumption(targetSlot))
            return false;

        Character myCharacter = Managers.Game?.Player;

        if (myCharacter == null)
            return false;

        if (itemData.ShouldConsumeOnUse())
        {
            var masterSlot = _totalSlots.FirstOrDefault(slot => slot.GlobalIndex == targetSlot.GlobalIndex) ?? targetSlot;

            if (!RemoveItem(targetSlot, 1))
                return false;

            SyncQuickSlotsAfterItemChanged(masterSlot);
        }

        itemData.ApplyConsumptionEffects(myCharacter, targetObject);
        itemData.PostProcessConsumption(targetSlot);

        SyncQuickSlotsAfterItemChanged();
        _onInventoryChanged.OnNext(Unit.Default);
        return true;
    }

    public bool UseQuickSlot(int quickSlotIndex, GameObject targetObject = null)
    {
        if (!quickSlotIndex.IsValidIndex(_quickSlots.Count))
            return false;

        var quickSlot = _quickSlots[quickSlotIndex];

        if (quickSlot == null || quickSlot.ItemID <= 0)
            return false;

        if (!quickSlot.ItemID.TryGetValidItemData(out var itemData, out var itemCategory))
            return false;

        if (itemCategory != ItemCategory.Consumption)
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

            if (quickSlot.GlobalIndex >= 0 && quickSlot.GlobalIndex < _totalSlots.Count)
            {
                masterSlot = _totalSlots[quickSlot.GlobalIndex];

                if (masterSlot.ItemID == quickSlot.ItemID)
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
        SyncQuickSlotsAfterItemChanged();
        _onInventoryChanged.OnNext(Unit.Default);
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
        if (sourceArea != targetArea)
            return HandleCrossAreaMove(currentTab, sourceArea, sourceIndex, targetArea, targetIndex);

        if (!currentTab.HasValue)
            return HandleTotalTabMove(sourceIndex, targetIndex);

        return HandleCategoryTabMove(currentTab.Value, sourceIndex, targetIndex);
    }

    private bool HandleTotalTabMove(int sourceIndex, int targetIndex)
    {
        if (!sourceIndex.IsValidIndex(_totalSlots.Count) || !targetIndex.IsValidIndex(_totalSlots.Count))
            return false;

        var sourceMaster = _totalSlots[sourceIndex];
        var targetMaster = _totalSlots[targetIndex];

        if (sourceMaster == targetMaster)
            return false;

        if (sourceMaster.TryMergeSlots(targetMaster))
        {
            FinalizeMove();
            return true;
        }

        int globalA = sourceMaster.GlobalIndex;
        int globalB = targetMaster.GlobalIndex;
        SwapSlotsValues(sourceMaster, targetMaster);
        SwapQuickSlotReferences(globalA, globalB);
        SyncQuickSlotsAfterItemChanged();
        FinalizeMove();
        return true;

        void FinalizeMove()
        {
            RebuildTabsFromTotal();
            SyncQuickSlotsAfterItemChanged();
            _onInventoryChanged.OnNext(Unit.Default);
        }
    }

    private bool HandleCategoryTabMove(ItemCategory currentTab, int sourceIndex, int targetIndex)
    {
        var targetList = GetSlotsByType(currentTab);

        if (targetList == null || !sourceIndex.IsValidIndex(targetList.Count) || !targetIndex.IsValidIndex(targetList.Count))
            return false;

        var sourceTabSlot = targetList[sourceIndex];
        var targetTabSlot = targetList[targetIndex];

        if (sourceTabSlot == targetTabSlot)
            return false;

        if (sourceTabSlot.TryMergeSlots(targetTabSlot))
        {
            FinalizeTabMove(currentTab);
            return true;
        }

        SwapTabSlotValues(sourceTabSlot, targetTabSlot);
        FinalizeTabMove(currentTab);
        return true;

        void FinalizeTabMove(ItemCategory category)
        {
            SyncTotalFromTab(category);
            RebuildTabsFromTotal();
            SyncQuickSlotsAfterItemChanged();
            _onInventoryChanged.OnNext(Unit.Default);
        }
    }

    public bool HandleCrossAreaMove(ItemCategory? currentTabType, SlotArea sourceArea, int sourceIndex, SlotArea targetArea, int targetIndex)
    {
        if (targetArea == SlotArea.Equipment)
        {
            var sourceSlot = GetSourceSlot(currentTabType, sourceArea, sourceIndex);

            if (sourceSlot == null || sourceSlot.ItemID <= 0)
                return false;

            if (!Managers.Data.Items.TryGetValue(sourceSlot.ItemID, out var itemData) || !itemData.IsEquipmentCategory())
                return false;

            return EquipItem(sourceArea, sourceIndex, (EquipmentSlotType)targetIndex, currentTabType);
        }

        if (targetArea == SlotArea.Quick)
            return HandleQuickSlotMove(sourceArea, sourceIndex, currentTabType, targetIndex);

        if (sourceArea == SlotArea.Equipment)
        {
            var sourceSlot = GetSourceSlot(currentTabType, sourceArea, sourceIndex);

            if (sourceSlot == null || sourceSlot.ItemID <= 0)
                return false;

            return UnequipItem((EquipmentSlotType)sourceIndex, currentTabType, targetIndex);
        }

        if (sourceArea == SlotArea.Quick)
        {
            var sourceQuickSlot = GetSourceSlot(currentTabType, sourceArea, sourceIndex);
            var targetNormalSlot = GetTargetSlot(currentTabType, targetArea, targetIndex);

            if (sourceQuickSlot == null || targetNormalSlot == null)
                return false;

            SwapSlotsValues(sourceQuickSlot, targetNormalSlot);
            SyncQuickSlotsAfterItemChanged();
            _onInventoryChanged.OnNext(Unit.Default);
            return true;
        }

        InventorySlot normalSourceSlot = GetSourceSlot(currentTabType, sourceArea, sourceIndex);
        InventorySlot normalTargetSlot = GetTargetSlot(currentTabType, targetArea, targetIndex);

        if (normalSourceSlot == null || normalTargetSlot == null)
            return false;

        if (normalSourceSlot.ItemID > 0 && normalTargetSlot.ItemID > 0)
        {
            if (normalSourceSlot.ItemID.TryGetValidItemData(out _, out var sourceCategory) && normalTargetSlot.ItemID.TryGetValidItemData(out _, out var targetCategory))
            {
                if (sourceCategory != targetCategory)
                    return false;
            }
        }

        if (currentTabType.HasValue)
        {
            if (currentTabType.Value != ItemCategory.Equipment)
            {
                if (normalSourceSlot.ItemID > 0 && normalSourceSlot.ItemID.TryGetValidItemData(out _, out var sCategory) && sCategory == ItemCategory.Equipment)
                    return false;

                if (normalTargetSlot.ItemID > 0 && normalTargetSlot.ItemID.TryGetValidItemData(out _, out var tCategory) && tCategory == ItemCategory.Equipment)
                    return false;
            }
        }

        int globalA = normalSourceSlot.GlobalIndex;
        int globalB = normalTargetSlot.GlobalIndex;
        SwapSlotsValues(normalSourceSlot, normalTargetSlot);
        SwapQuickSlotReferences(globalA, globalB);
        SyncQuickSlotsAfterItemChanged();
        PostProcessCrossMove(sourceArea, targetArea);
        return true;
    }

    public bool HandleQuickSlotMove(SlotArea sourceArea, int sourceIndex, ItemCategory? currentTabType, int targetQuickIndex)
    {
        var sourceSlot = GetSourceSlot(currentTabType, sourceArea, sourceIndex);

        if (sourceSlot == null || sourceSlot.ItemID <= 0)
            return false;

        if (!sourceSlot.ItemID.TryGetValidItemData(out var itemData, out _))
            return false;

        if (itemData.IsEtc())
            return false;

        if (_quickSlots == null || !targetQuickIndex.IsValidIndex(_quickSlots.Count))
            return false;

        var targetQuickSlot = _quickSlots[targetQuickIndex];

        if (sourceArea == SlotArea.Quick)
        {
            SwapSlotsValues(sourceSlot, targetQuickSlot);
            (sourceSlot.GlobalIndex, targetQuickSlot.GlobalIndex) = (targetQuickSlot.GlobalIndex, sourceSlot.GlobalIndex);
        }
        else
        {
            targetQuickSlot.ItemID = sourceSlot.ItemID;
            targetQuickSlot.Quantity = sourceSlot.Quantity;
            targetQuickSlot.InstanceID = sourceSlot.InstanceID;
            targetQuickSlot.GlobalIndex = sourceSlot.GlobalIndex;
        }

        SyncQuickSlotsAfterItemChanged();
        _onInventoryChanged.OnNext(Unit.Default);
        return true;
    }

    private InventorySlot GetSourceSlot(ItemCategory? currentTabType, SlotArea sourceArea, int sourceIndex)
    {
        return sourceArea switch
        {
            SlotArea.Equipment => sourceIndex.IsValidIndex(_equipmentSlots?.Count ?? 0) ? _equipmentSlots[sourceIndex] : null,
            SlotArea.Quick => sourceIndex.IsValidIndex(_quickSlots?.Count ?? 0) ? _quickSlots[sourceIndex] : null,
            _ => GetInventorySlotByTab(currentTabType, sourceIndex)
        };
    }

    private InventorySlot GetTargetSlot(ItemCategory? currentTabType, SlotArea targetArea, int targetIndex)
    {
        return targetArea switch
        {
            SlotArea.Equipment => targetIndex.IsValidIndex(_equipmentSlots?.Count ?? 0) ? _equipmentSlots[targetIndex] : null,
            SlotArea.Quick => targetIndex.IsValidIndex(_quickSlots?.Count ?? 0) ? _quickSlots[targetIndex] : null,
            _ => GetInventorySlotByTab(currentTabType, targetIndex, true)
        };
    }

    private InventorySlot GetInventorySlotByTab(ItemCategory? currentTabType, int index, bool isTarget = false)
    {
        if (!currentTabType.HasValue)
            return index.IsValidIndex(_totalSlots.Count) ? _totalSlots[index] : null;

        var targetList = GetSlotsByType(currentTabType);

        if (targetList == null || !index.IsValidIndex(targetList.Count))
            return null;

        var tabSlot = targetList[index];

        if (tabSlot.GlobalIndex >= 0 && tabSlot.GlobalIndex < _totalSlots.Count)
            return _totalSlots[tabSlot.GlobalIndex];

        return isTarget ? _totalSlots.FirstOrDefault(s => s.ItemID == 0) : null;
    }

    private void PostProcessCrossMove(SlotArea sourceArea, SlotArea targetArea)
    {
        RebuildTabsFromTotal();
        SyncQuickSlotsAfterItemChanged();
        _onInventoryChanged.OnNext(Unit.Default);
    }

    private void SyncTotalFromTab(ItemCategory category)
    {
        var tabSlots = GetSlotsByType(category);

        if (tabSlots == null)
            return;

        for (int index = 0; index < _totalSlots.Count; index++)
        {
            var slot = _totalSlots[index];

            if (slot.ItemID > 0 && slot.ItemID.TryGetValidItemData(out _, out var slotCategory) && slotCategory == category)
                slot.ClearSlot();
        }

        foreach (var tabSlot in tabSlots)
        {
            if (tabSlot.GlobalIndex >= 0 && tabSlot.GlobalIndex < _totalSlots.Count)
            {
                var masterSlot = _totalSlots[tabSlot.GlobalIndex];
                masterSlot.ItemID = tabSlot.ItemID;
                masterSlot.Quantity = tabSlot.Quantity;
                masterSlot.InstanceID = tabSlot.InstanceID;
            }
        }

        foreach (var tabSlot in tabSlots)
        {
            if (tabSlot.ItemID > 0 && (tabSlot.GlobalIndex < 0 || tabSlot.GlobalIndex >= _totalSlots.Count || _totalSlots[tabSlot.GlobalIndex].ItemID != tabSlot.ItemID))
            {
                var emptyMaster = _totalSlots.FirstOrDefault(slot => slot.ItemID == 0);

                if (emptyMaster != null)
                {
                    emptyMaster.ItemID = tabSlot.ItemID;
                    emptyMaster.Quantity = tabSlot.Quantity;
                    emptyMaster.InstanceID = tabSlot.InstanceID;
                    tabSlot.GlobalIndex = emptyMaster.GlobalIndex;
                }
            }
        }
    }

    private void SyncQuickSlotsAfterItemChanged(InventorySlot modifiedInventorySlot = null)
    {
        for (int index = 0; index < _quickSlots.Count; index++)
        {
            var quickSlot = _quickSlots[index];

            if (quickSlot == null || quickSlot.ItemID <= 0)
                continue;

            if (modifiedInventorySlot != null)
            {
                if (quickSlot.GlobalIndex == modifiedInventorySlot.GlobalIndex || quickSlot.ItemID == modifiedInventorySlot.ItemID)
                {
                    if (modifiedInventorySlot.ItemID <= 0 || modifiedInventorySlot.Quantity <= 0)
                        ClearQuickSlot(index);
                    else
                        quickSlot.Quantity = modifiedInventorySlot.Quantity;
                }

                continue;
            }

            var masterSlot = (quickSlot.GlobalIndex >= 0 && quickSlot.GlobalIndex < _totalSlots.Count) ? _totalSlots[quickSlot.GlobalIndex] : null;

            if (masterSlot == null || masterSlot.ItemID <= 0 || masterSlot.ItemID != quickSlot.ItemID)
            {
                masterSlot = _totalSlots.FirstOrDefault(slot => slot.ItemID == quickSlot.ItemID);

                if (masterSlot != null)
                    quickSlot.GlobalIndex = masterSlot.GlobalIndex;
            }

            if (masterSlot != null && masterSlot.ItemID > 0 && masterSlot.ItemID == quickSlot.ItemID)
                quickSlot.Quantity = masterSlot.Quantity;
            else
                ClearQuickSlot(index);
        }
    }

    private void SwapQuickSlotReferences(int globalIndexA, int globalIndexB)
    {
        if (_quickSlots == null)
            return;

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
            {
                _unlockedEquipments.Add(new EquipmentInstance
                {
                    InstanceID = targetInstanceID,
                    ItemID = itemID,
                    UpgradeLevel = 0,
                    ExtraOptionValue = 0
                });
            }
        }
    }

    private void SwapSlotsValues(InventorySlot source, InventorySlot target)
    {
        (source.ItemID, target.ItemID) = (target.ItemID, source.ItemID);
        (source.Quantity, target.Quantity) = (target.Quantity, source.Quantity);
        (source.InstanceID, target.InstanceID) = (target.InstanceID, source.InstanceID);
    }

    private void SwapTabSlotValues(InventorySlot source, InventorySlot target)
    {
        SwapSlotsValues(source, target);
        (source.GlobalIndex, target.GlobalIndex) = (target.GlobalIndex, source.GlobalIndex);
    }

    private void RebuildTabsFromTotal()
    {
        SyncTabWithCategory(_equipmentTabSlots, ItemCategory.Equipment);
        SyncTabWithCategory(_consumptionTabSlots, ItemCategory.Consumption);
        SyncTabWithCategory(_etcTabSlots, ItemCategory.Etc);
    }

    private void SyncTabWithCategory(List<InventorySlot> tabSlots, ItemCategory category)
    {
        EnsureTabCapacity(tabSlots);
        var masterItems = _totalSlots
        .Where(slot => slot.ItemID > 0 && slot.ItemID.TryGetValidItemData(out _, out var slotCategory) && slotCategory == category)
        .ToList();
        var existingMap = new Dictionary<int, InventorySlot>();

        foreach (var tabSlot in tabSlots)
        {
            if (tabSlot.GlobalIndex >= 0 && tabSlot.ItemID > 0)
                existingMap[tabSlot.GlobalIndex] = tabSlot;

            tabSlot.ClearTabSlot();
        }

        var unplacedMasters = new List<InventorySlot>();

        foreach (var master in masterItems)
        {
            bool placed = false;

            foreach (var tabSlot in tabSlots)
            {
                if (tabSlot.ItemID == 0)
                {
                    if (existingMap.TryGetValue(master.GlobalIndex, out var mappedSlot) && mappedSlot == tabSlot)
                    {
                        tabSlot.AssignSlotData(master);
                        tabSlot.GlobalIndex = master.GlobalIndex;
                        existingMap.Remove(master.GlobalIndex);
                        placed = true;
                        break;
                    }
                }
            }

            if (!placed)
                unplacedMasters.Add(master);
        }

        foreach (var master in unplacedMasters)
        {
            foreach (var tabSlot in tabSlots)
            {
                if (tabSlot.ItemID == 0)
                {
                    tabSlot.AssignSlotData(master);
                    tabSlot.GlobalIndex = master.GlobalIndex;
                    break;
                }
            }
        }
    }

    public void SortInventory(ItemCategory? currentTabType)
    {
        if (!currentTabType.HasValue)
        {
            _totalSlots.SortSlots();
            RebuildTabsFromTotal();
        }
        else
        {
            var targetSlots = GetSlotsByType(currentTabType);

            if (targetSlots != null)
            {
                targetSlots.SortSlots();
                SyncTotalFromTab(currentTabType.Value);
                RebuildTabsFromTotal();
            }
        }

        SyncQuickSlotsAfterItemChanged();
        _onInventoryChanged.OnNext(Unit.Default);
    }

    public void ClearInventory()
    {
        foreach (var slot in _totalSlots)
            slot.ClearSlot();

        RebuildTabsFromTotal();

        foreach (var slot in _equipmentSlots)
            slot.ClearSlot();

        SyncQuickSlotsAfterItemChanged();
        _onInventoryChanged.OnNext(Unit.Default);
    }

    public bool ClearQuickSlot(int quickSlotIndex)
    {
        if (!quickSlotIndex.IsValidIndex(_quickSlots.Count))
            return false;

        var quickSlot = _quickSlots[quickSlotIndex];

        if (quickSlot == null || quickSlot.ItemID <= 0)
            return false;

        quickSlot.ClearSlot();
        _onInventoryChanged.OnNext(Unit.Default);
        return true;
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
    {
        return _unlockedEquipments.Select(equip => new EquipmentInstance
        {
            InstanceID = equip.InstanceID,
            ItemID = equip.ItemID,
            UpgradeLevel = equip.UpgradeLevel,
            ExtraOptionValue = equip.ExtraOptionValue
        }).ToList();
    }
}
