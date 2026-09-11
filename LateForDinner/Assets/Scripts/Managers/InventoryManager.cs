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
        if (!TryGetValidItemData(itemID, out var itemData, out var itemCategory))
            return false;

        if (!HasEnoughSpaceForCategory(_totalSlots, itemID, itemData.MaxStack, quantity, itemCategory))
            return false;

        FillExistingItemSlots(_totalSlots, itemID, itemData.MaxStack, ref quantity);
        FillEmptySlots(_totalSlots, itemID, itemData.MaxStack, ref quantity, instanceID);
        RebuildTabsFromTotal();
        _onInventoryChanged.OnNext(Unit.Default);
        return true;
    }

    public bool RemoveItem(int itemID, int quantity)
    {
        if (!TryGetValidItemData(itemID, out _, out _))
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
                ClearSlot(slot);
        }

        RebuildTabsFromTotal();
        _onInventoryChanged.OnNext(Unit.Default);
        return true;
    }

    public bool RemoveItem(InventorySlot targetSlot, int quantity)
    {
        if (targetSlot == null || targetSlot.ItemID <= 0 || targetSlot.Quantity < quantity)
            return false;

        var slotToModify = _totalSlots.FirstOrDefault(s => s.GlobalIndex == targetSlot.GlobalIndex) ?? targetSlot;

        if (slotToModify.Quantity < quantity)
            return false;

        slotToModify.Quantity -= quantity;

        if (slotToModify.Quantity <= 0)
            ClearSlot(slotToModify);

        RebuildTabsFromTotal();
        _onInventoryChanged.OnNext(Unit.Default);
        return true;
    }

    public async UniTask<bool> DropItem(InventorySlot targetSlot, int quantity, Vector3 dropPosition)
    {
        if (targetSlot == null || targetSlot.ItemID <= 0 || targetSlot.Quantity < quantity)
            return false;

        int itemID = targetSlot.ItemID;
        var masterSlot = _totalSlots.FirstOrDefault(s => s.GlobalIndex == targetSlot.GlobalIndex);

        if (masterSlot == null || masterSlot.Quantity < quantity)
            return false;

        string droppedInstanceID = masterSlot.InstanceID;
        masterSlot.Quantity -= quantity;

        if (masterSlot.Quantity <= 0)
            ClearSlot(masterSlot);

        RebuildTabsFromTotal();
        await SpawnItemProp(itemID, quantity, droppedInstanceID, dropPosition);
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

        var equipInstance = _unlockedEquipments.FirstOrDefault(eq => eq.InstanceID == sourceSlot.InstanceID);

        if (equipInstance == null && !string.IsNullOrEmpty(sourceSlot.InstanceID))
        {
            equipInstance = new EquipmentInstance
            {
                InstanceID = sourceSlot.InstanceID,
                ItemID = sourceSlot.ItemID,
                UpgradeLevel = 0,
                ExtraOptionValue = 0,
                Flag = false
            };
            _unlockedEquipments.Add(equipInstance);
        }

        SwapSlotsValues(sourceSlot, equipmentSlot);

        if (equipmentSlot.ItemID > 0 && equipInstance != null)
            equipInstance.ApplyEquipmentEffects(itemData, myCharacter);

        PostProcessCrossMove(sourceArea, SlotArea.Equipment);
        return true;
    }

    public bool UnequipItem(EquipmentSlotType targetSlotType)
    {
        int sourceIndex = (int)targetSlotType;

        if (_equipmentSlots == null || sourceIndex < 0 || sourceIndex >= _equipmentSlots.Count)
            return false;

        var equipmentSlot = _equipmentSlots[sourceIndex];

        if (equipmentSlot.ItemID <= 0)
            return false;

        var emptySlot = _totalSlots.FirstOrDefault(s => s.ItemID == 0);

        if (emptySlot == null)
            return false;

        Character myCharacter = Managers.Game?.Player;

        if (myCharacter == null)
            return false;

        UnequipItemInternal(equipmentSlot, myCharacter);
        emptySlot.ItemID = equipmentSlot.ItemID;
        emptySlot.Quantity = equipmentSlot.Quantity;
        emptySlot.InstanceID = equipmentSlot.InstanceID;
        ClearSlot(equipmentSlot);
        RebuildTabsFromTotal();
        _onInventoryChanged.OnNext(Unit.Default);
        return true;
    }

    private void UnequipItemInternal(InventorySlot equipmentSlot, Character character)
    {
        if (equipmentSlot.ItemID <= 0)
            return;

        if (Managers.Data.Items.TryGetValue(equipmentSlot.ItemID, out var itemData))
        {
            var equipInstance = _unlockedEquipments.FirstOrDefault(eq => eq.InstanceID == equipmentSlot.InstanceID);

            if (equipInstance != null)
                equipInstance.RemoveEquipmentEffects(itemData, character);
        }
    }

    public bool UseConsumableItem(InventorySlot targetSlot, GameObject targetObject = null)
    {
        if (targetSlot == null || targetSlot.ItemID <= 0)
            return false;

        int itemID = targetSlot.ItemID;

        if (!TryGetValidItemData(itemID, out var itemData, out var itemCategory) || itemCategory != ItemCategory.Consumption)
            return false;

        if (!itemData.TryGetConsumptionData(out var consumptionData))
            return false;

        float cooldownTime = consumptionData.Cooldown;
        string itemCooldownKey = Define.Key.GetItemCooldownKey(itemID);

        if (cooldownTime > 0f)
        {
            var existingCooldown = Managers.Cooldown.GetSlotCooldown(itemCooldownKey);

            if (existingCooldown != null && existingCooldown.IsOnCooldown)
                return false;
        }

        Character myCharacter = Managers.Game?.Player;

        if (myCharacter == null)
            return false;

        var saveData = Managers.Save.CurrentData;
        List<ItemTemplateData> templates = null;

        if (Managers.Data.ItemTemplates != null && Managers.Data.ItemTemplates.Contains(itemID))
            templates = Managers.Data.ItemTemplates[itemID].ToList();

        if (templates != null)
        {
            foreach (var template in templates)
            {
                if (template.Flag)
                {
                    string flagKey = Define.Key.GetConsumableFlagKey(itemID, template.AttributeKey);

                    if (saveData?.AppliedFlagItems != null && saveData.AppliedFlagItems.Contains(flagKey))
                        return false;
                }
            }
        }

        if (!RemoveItem(targetSlot, 1))
            return false;

        itemData.ApplyConsumptionEffects(myCharacter, targetObject);

        if (cooldownTime > 0f)
            Managers.Cooldown.RegisterSlotCooldown(itemCooldownKey, cooldownTime);

        if (templates != null && saveData != null)
        {
            foreach (var template in templates)
            {
                if (template.Flag)
                {
                    if (saveData.AppliedFlagItems == null)
                        saveData.AppliedFlagItems = new HashSet<string>();

                    string flagKey = Define.Key.GetConsumableFlagKey(itemID, template.AttributeKey);
                    saveData.AppliedFlagItems.Add(flagKey);
                }
            }
        }

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
        if (!IsValidIndex(sourceIndex, _totalSlots.Count) || !IsValidIndex(targetIndex, _totalSlots.Count))
            return false;

        var sourceMaster = _totalSlots[sourceIndex];
        var targetMaster = _totalSlots[targetIndex];

        if (sourceMaster == targetMaster)
            return false;

        if (TryMergeOrSwapSlots(sourceMaster, targetMaster))
        {
            FinalizeMove();
            return true;
        }

        SwapSlotsValues(sourceMaster, targetMaster);
        FinalizeMove();
        return true;

        void FinalizeMove()
        {
            RebuildTabsFromTotal();
            _onInventoryChanged.OnNext(Unit.Default);
        }
    }

    private bool HandleCategoryTabMove(ItemCategory currentTab, int sourceIndex, int targetIndex)
    {
        var targetList = GetSlotsByType(currentTab);

        if (targetList == null || !IsValidIndex(sourceIndex, targetList.Count) || !IsValidIndex(targetIndex, targetList.Count))
            return false;

        var sourceTabSlot = targetList[sourceIndex];
        var targetTabSlot = targetList[targetIndex];

        if (sourceTabSlot == targetTabSlot)
            return false;

        if (TryMergeSameItemInTab(sourceTabSlot, targetTabSlot))
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
            _onInventoryChanged.OnNext(Unit.Default);
        }
    }

    private bool TryMergeSameItemInTab(InventorySlot sourceTabSlot, InventorySlot targetTabSlot)
    {
        if (sourceTabSlot.ItemID <= 0 || sourceTabSlot.ItemID != targetTabSlot.ItemID)
            return false;

        if (!TryGetValidItemData(sourceTabSlot.ItemID, out var itemData, out var itemCategory) || itemCategory == ItemCategory.Equipment)
            return false;

        int maxStack = itemData.MaxStack;

        if (targetTabSlot.Quantity >= maxStack)
            return false;

        int space = maxStack - targetTabSlot.Quantity;
        int transfer = Math.Min(space, sourceTabSlot.Quantity);
        targetTabSlot.Quantity += transfer;
        sourceTabSlot.Quantity -= transfer;

        if (sourceTabSlot.Quantity <= 0)
            ClearTabSlot(sourceTabSlot);

        return true;
    }

    private bool HandleCrossAreaMove(ItemCategory? currentTabType, SlotArea sourceArea, int sourceIndex, SlotArea targetArea, int targetIndex)
    {
        if (sourceArea != SlotArea.Equipment && targetArea == SlotArea.Equipment)
            return EquipItem(sourceArea, sourceIndex, (EquipmentSlotType)targetIndex, currentTabType);

        if (sourceArea == SlotArea.Equipment)
        {
            var sourceSlot = GetSourceSlot(currentTabType, sourceArea, sourceIndex);
            var targetSlot = GetTargetSlot(currentTabType, targetArea, targetIndex);

            if (sourceSlot == null || targetSlot == null || sourceSlot.ItemID <= 0)
                return false;

            Character myCharacter = Managers.Game?.Player;

            if (myCharacter != null)
                UnequipItemInternal(sourceSlot, myCharacter);

            SwapSlotsValues(sourceSlot, targetSlot);
            PostProcessCrossMove(sourceArea, targetArea);
            return true;
        }

        InventorySlot normalSourceSlot = GetSourceSlot(currentTabType, sourceArea, sourceIndex);
        InventorySlot normalTargetSlot = GetTargetSlot(currentTabType, targetArea, targetIndex);

        if (normalSourceSlot == null || normalTargetSlot == null)
            return false;

        SwapSlotsValues(normalSourceSlot, normalTargetSlot);
        PostProcessCrossMove(sourceArea, targetArea);
        return true;
    }

    private InventorySlot GetSourceSlot(ItemCategory? currentTabType, SlotArea sourceArea, int sourceIndex)
    {
        return sourceArea switch
        {
            SlotArea.Equipment => IsValidIndex(sourceIndex, _equipmentSlots?.Count ?? 0) ? _equipmentSlots[sourceIndex] : null,
            SlotArea.Quick => IsValidIndex(sourceIndex, _quickSlots?.Count ?? 0) ? _quickSlots[sourceIndex] : null,
            _ => GetInventorySlotByTab(currentTabType, sourceIndex)
        };
    }

    private InventorySlot GetTargetSlot(ItemCategory? currentTabType, SlotArea targetArea, int targetIndex)
    {
        return targetArea switch
        {
            SlotArea.Equipment => IsValidIndex(targetIndex, _equipmentSlots?.Count ?? 0) ? _equipmentSlots[targetIndex] : null,
            SlotArea.Quick => IsValidIndex(targetIndex, _quickSlots?.Count ?? 0) ? _quickSlots[targetIndex] : null,
            _ => GetInventorySlotByTab(currentTabType, targetIndex, true)
        };
    }

    private InventorySlot GetInventorySlotByTab(ItemCategory? currentTabType, int index, bool isTarget = false)
    {
        if (!currentTabType.HasValue)
            return IsValidIndex(index, _totalSlots.Count) ? _totalSlots[index] : null;

        var targetList = GetSlotsByType(currentTabType);

        if (targetList == null || !IsValidIndex(index, targetList.Count))
            return null;

        var tabSlot = targetList[index];

        if (tabSlot.GlobalIndex >= 0 && tabSlot.GlobalIndex < _totalSlots.Count)
            return _totalSlots[tabSlot.GlobalIndex];

        return isTarget ? _totalSlots.FirstOrDefault(s => s.ItemID == 0) : null;
    }

    private void PostProcessCrossMove(SlotArea sourceArea, SlotArea targetArea)
    {
        RebuildTabsFromTotal();
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

            if (slot.ItemID > 0 && TryGetValidItemData(slot.ItemID, out _, out var cat) && cat == category)
                ClearSlot(slot);
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
                var emptyMaster = _totalSlots.FirstOrDefault(s => s.ItemID == 0);

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

    private bool TryMergeOrSwapSlots(InventorySlot source, InventorySlot target)
    {
        if (source.ItemID <= 0 || source.ItemID != target.ItemID)
            return false;

        if (!TryGetValidItemData(source.ItemID, out var itemData, out var itemCategory) || itemCategory == ItemCategory.Equipment)
            return false;

        int maxStack = itemData.MaxStack;

        if (target.Quantity >= maxStack)
            return false;

        int space = maxStack - target.Quantity;
        int transfer = Math.Min(space, source.Quantity);
        target.Quantity += transfer;
        source.Quantity -= transfer;

        if (source.Quantity <= 0)
            ClearSlot(source);

        return true;
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
            if (required <= 0) 
                break;

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

    private void FillEmptySlots(List<InventorySlot> slots, int itemID, int maxStack, ref int remaining, string instanceID = "")
    {
        bool isEquipment = TryGetValidItemData(itemID, out _, out var category) && category == ItemCategory.Equipment;

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

            if (!_unlockedEquipments.Any(eq => eq.InstanceID == targetInstanceID))
            {
                _unlockedEquipments.Add(new EquipmentInstance
                {
                    InstanceID = targetInstanceID,
                    ItemID = itemID,
                    UpgradeLevel = 0,
                    ExtraOptionValue = 0,
                    Flag = false
                });
            }
        }
    }

    private void SwapSlotsValues(InventorySlot a, InventorySlot b)
    {
        (a.ItemID, b.ItemID) = (b.ItemID, a.ItemID);
        (a.Quantity, b.Quantity) = (b.Quantity, a.Quantity);
        (a.InstanceID, b.InstanceID) = (b.InstanceID, a.InstanceID);
    }

    private void SwapTabSlotValues(InventorySlot a, InventorySlot b)
    {
        SwapSlotsValues(a, b);
        (a.GlobalIndex, b.GlobalIndex) = (b.GlobalIndex, a.GlobalIndex);
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
        .Where(s => s.ItemID > 0 && TryGetValidItemData(s.ItemID, out _, out var cat) && cat == category)
        .ToList();
        var existingMap = new Dictionary<int, InventorySlot>();

        foreach (var tabSlot in tabSlots)
        {
            if (tabSlot.GlobalIndex >= 0 && tabSlot.ItemID > 0)
                existingMap[tabSlot.GlobalIndex] = tabSlot;

            ClearTabSlot(tabSlot);
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
                        AssignSlotData(tabSlot, master);
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
                    AssignSlotData(tabSlot, master);
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
            SortSlotList(_totalSlots);
            RebuildTabsFromTotal();
        }
        else
        {
            var targetSlots = GetSlotsByType(currentTabType);

            if (targetSlots != null)
            {
                SortSlotList(targetSlots);
                SyncTotalFromTab(currentTabType.Value);
                RebuildTabsFromTotal();
            }
        }

        _onInventoryChanged.OnNext(Unit.Default);
    }

    private void SortSlotList(List<InventorySlot> slots)
    {
        var sortedItems = slots
        .Where(s => s.ItemID != 0)
        .Select(s => (s.ItemID, s.Quantity, s.InstanceID, s.GlobalIndex))
        .OrderBy(x => x.ItemID)
        .ThenByDescending(x => x.Quantity)
        .ThenBy(x => x.InstanceID)
        .ToList();
        int index = 0;

        foreach (var item in sortedItems)
        {
            slots[index].ItemID = item.ItemID;
            slots[index].Quantity = item.Quantity;
            slots[index].InstanceID = item.InstanceID;
            slots[index].GlobalIndex = item.GlobalIndex;
            index++;
        }

        while (index < slots.Count)
        {
            ClearSlot(slots[index]);
            index++;
        }
    }

    public void ClearInventory()
    {
        foreach (var slot in _totalSlots)
            ClearSlot(slot);

        RebuildTabsFromTotal();

        foreach (var slot in _equipmentSlots)
            ClearSlot(slot);
    }

    private bool IsValidIndex(int index, int count) 
        => index >= 0 && index < count;

    private void ClearSlot(InventorySlot slot)
    {
        slot.ItemID = 0;
        slot.Quantity = 0;
        slot.InstanceID = null;
    }

    private void ClearTabSlot(InventorySlot slot)
    {
        ClearSlot(slot);
        slot.GlobalIndex = -1;
    }

    private void AssignSlotData(InventorySlot target, InventorySlot source)
    {
        target.ItemID = source.ItemID;
        target.Quantity = source.Quantity;
        target.InstanceID = source.InstanceID;
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

    public List<EquipmentInstance> ExportUnlockedEquipmentsSaveData()
    {
        return _unlockedEquipments.Select(equip => new EquipmentInstance
        {
            InstanceID = equip.InstanceID,
            ItemID = equip.ItemID,
            UpgradeLevel = equip.UpgradeLevel,
            ExtraOptionValue = equip.ExtraOptionValue,
            Flag = equip.Flag
        }).ToList();
    }

    private List<InventorySlot> ExportSlotList(List<InventorySlot> slots)
    {
        return slots.Select(slot => new InventorySlot
        {
            GlobalIndex = slot.GlobalIndex,
            SlotIndex = slot.SlotIndex,
            ItemID = slot.ItemID,
            Quantity = slot.Quantity,
            InstanceID = slot.InstanceID
        }).ToList();
    }
}
