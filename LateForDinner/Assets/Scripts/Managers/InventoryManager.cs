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
    private List<InventorySlot> _equipmentTabSlots = new List<InventorySlot>();
    private List<InventorySlot> _consumptionTabSlots = new List<InventorySlot>();
    private List<InventorySlot> _etcTabSlots = new List<InventorySlot>();
    private List<InventorySlot> _equipmentSlots = new List<InventorySlot>(Define.Amount.MaxEquipmentSlot);
    private List<InventorySlot> _quickSlots = new List<InventorySlot>(Define.Amount.MaxQuickSlot);
    private List<EquipmentInstance> _unlockedEquipments = new List<EquipmentInstance>();

    public void InitInventory(SaveData data)
    {
        _equipmentTabSlots = data.EquipmentTabSlots ?? new List<InventorySlot>();
        EnsureSlotCapacity(_equipmentTabSlots, data.InventoryTabCapacity > 0 ? data.InventoryTabCapacity : Define.Amount.DefaultInventorySlot);
        _consumptionTabSlots = data.ConsumptionTabSlots ?? new List<InventorySlot>();
        EnsureSlotCapacity(_consumptionTabSlots, data.InventoryTabCapacity > 0 ? data.InventoryTabCapacity : Define.Amount.DefaultInventorySlot);
        _etcTabSlots = data.EtcTabSlots ?? new List<InventorySlot>();
        EnsureSlotCapacity(_etcTabSlots, data.InventoryTabCapacity > 0 ? data.InventoryTabCapacity : Define.Amount.DefaultInventorySlot);
        _equipmentSlots = data.EquipmentSlots ?? new List<InventorySlot>();
        EnsureSlotCapacity(_equipmentSlots, Define.Amount.MaxEquipmentSlot);
        _quickSlots = data.QuickSlots ?? new List<InventorySlot>();
        EnsureSlotCapacity(_quickSlots, Define.Amount.MaxQuickSlot);
        _unlockedEquipments = data.UnlockedEquipments ?? new List<EquipmentInstance>();
        SyncQuickSlotsAfterItemChanged();
    }

    private void EnsureSlotCapacity(List<InventorySlot> slots, int maxCapacity)
    {
        while (slots.Count < maxCapacity)
        {
            int index = slots.Count;
            slots.Add(new InventorySlot { SlotIndex = index });
        }

        int currentIndex = 0;

        foreach (var slot in slots)
            slot.SlotIndex = currentIndex++;
    }

    public bool ExpandInventory(ItemCategory category, int addCount, int maxLimit = Define.Amount.MaxInventorySlot)
    {
        if (addCount <= 0)
            return false;

        var targetSlots = GetSlotsByType(category);

        if (targetSlots.Count >= maxLimit)
            return false;

        int newCapacity = Math.Min(targetSlots.Count + addCount, maxLimit);
        int actualAddCount = newCapacity - targetSlots.Count;

        for (int index = 0; index < actualAddCount; index++)
            targetSlots.Add(new InventorySlot { SlotIndex = targetSlots.Count });

        FinalizeInventoryChange();
        return true;
    }

    public bool AddItem(int itemID, int quantity, string instanceID = "")
    {
        if (!itemID.TryGetValidItemData(out var itemData, out var itemCategory))
            return false;

        var targetTabSlots = GetSlotsByType(itemCategory);

        if (!targetTabSlots.HasEnoughSpaceForCategory(itemID, itemData.MaxStack, quantity, itemCategory))
            return false;

        FillExistingItemSlots(targetTabSlots, itemID, itemData.MaxStack, ref quantity);
        FillEmptySlots(targetTabSlots, itemID, itemData.MaxStack, ref quantity, instanceID);
        FinalizeInventoryChange();
        return true;
    }

    public bool RemoveItem(int itemID, int quantity)
    {
        if (!itemID.TryGetValidItemData(out _, out var itemCategory))
            return false;

        var targetTabSlots = GetSlotsByType(itemCategory);
        int remainingToRemove = quantity;

        if (targetTabSlots.Where(slot => slot.ItemID == itemID).Sum(slot => slot.Quantity) < quantity)
            return false;

        foreach (var slot in targetTabSlots)
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

        FinalizeInventoryChange();
        return true;
    }

    public bool RemoveItem(InventorySlot targetSlot, int quantity)
    {
        if (targetSlot == null || targetSlot.ItemID <= 0 || targetSlot.Quantity < quantity)
            return false;

        if (targetSlot.ItemID.TryGetValidItemData(out var itemData, out _))
        {
            if (!itemData.Destroy)
            {
                Managers.Notify.ToastAsync(LocalizationKey.Log_Inventory_CannotDestroy).Forget();
                return false;
            }
        }

        targetSlot.Quantity -= quantity;

        if (targetSlot.Quantity <= 0)
            targetSlot.ClearSlot();

        FinalizeInventoryChange();
        return true;
    }

    public async UniTask<bool> DropItem(InventorySlot targetSlot, int quantity, Vector3 dropPosition)
    {
        if (targetSlot == null || targetSlot.ItemID <= 0 || targetSlot.Quantity < quantity)
            return false;

        if (targetSlot.ItemID.TryGetValidItemData(out var itemData, out _))
        {
            if (!itemData.Destroy)
            {
                Managers.Notify.ToastAsync(LocalizationKey.Log_Inventory_CannotDestroy).Forget();
                return false;
            }
        }

        int itemID = targetSlot.ItemID;
        string droppedInstanceID = targetSlot.InstanceID;
        targetSlot.Quantity -= quantity;

        if (targetSlot.Quantity <= 0)
            targetSlot.ClearSlot();

        await SpawnItemProp(itemID, quantity, droppedInstanceID, dropPosition);
        FinalizeInventoryChange();
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

    public bool EquipItem(InventorySlot sourceSlot, InventorySlot targetEquipmentSlot)
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

        if (targetEquipmentSlot.ItemID > 0)
        {
            UnEquipItemInternal(targetEquipmentSlot, myCharacter);
            var tempEquipmentData = new InventorySlot();
            tempEquipmentData.AssignSlotData(targetEquipmentSlot);
            targetEquipmentSlot.AssignSlotData(sourceSlot);
            sourceSlot.AssignSlotData(tempEquipmentData);
        }
        else
        {
            targetEquipmentSlot.AssignSlotData(sourceSlot);
            sourceSlot.ClearSlot();
        }

        var equipInstance = _unlockedEquipments.FirstOrDefault(equip => equip.InstanceID == targetEquipmentSlot.InstanceID);

        if (equipInstance == null && !string.IsNullOrEmpty(targetEquipmentSlot.InstanceID))
        {
            equipInstance = new EquipmentInstance { InstanceID = targetEquipmentSlot.InstanceID, ItemID = targetEquipmentSlot.ItemID, UpgradeLevel = 0, ExtraOptionValue = 0 };
            _unlockedEquipments.Add(equipInstance);
        }

        if (equipInstance != null)
            equipInstance.ApplyEquipmentEffects(itemData, myCharacter);

        FinalizeInventoryChange();
        return true;
    }

    public bool UnEquipItem(InventorySlot targetEquipmentSlot, InventorySlot targetSlot = null)
    {
        if (targetEquipmentSlot == null || targetEquipmentSlot.ItemID <= 0)
            return false;

        if (!_equipmentSlots.Contains(targetEquipmentSlot))
            return false;

        Character myCharacter = Managers.Game?.Player;

        if (myCharacter == null)
            return false;

        var equipmentTabSlots = GetSlotsByType(ItemCategory.Equipment);
        InventorySlot slotToUse = targetSlot;

        if (slotToUse == null || !_equipmentTabSlots.Contains(slotToUse))
            slotToUse = equipmentTabSlots.FirstOrDefault(slot => slot.ItemID <= 0);

        if (slotToUse == null)
            return false;

        EquipmentSlotType targetSlotType = (EquipmentSlotType)targetEquipmentSlot.SlotIndex;

        if (slotToUse.ItemID > 0)
        {
            if (!slotToUse.ItemID.TryGetValidItemData(out var incomingItemData, out _) || !incomingItemData.IsEquipmentCategory() || !incomingItemData.CanEquipInSlot(targetSlotType))
                return false;
        }

        UnEquipItemInternal(targetEquipmentSlot, myCharacter);

        if (slotToUse.ItemID > 0)
        {
            var tempEquipmentData = new InventorySlot();
            tempEquipmentData.AssignSlotData(slotToUse);
            slotToUse.AssignSlotData(targetEquipmentSlot);
            targetEquipmentSlot.AssignSlotData(tempEquipmentData);

            if (slotToUse.ItemID.TryGetValidItemData(out var newItemData, out _))
            {
                var equipInstance = _unlockedEquipments.FirstOrDefault(equip => equip.InstanceID == targetEquipmentSlot.InstanceID);

                if (equipInstance != null)
                    equipInstance.ApplyEquipmentEffects(newItemData, myCharacter);
            }
        }
        else
        {
            slotToUse.AssignSlotData(targetEquipmentSlot);
            targetEquipmentSlot.ClearSlot();
        }

        FinalizeInventoryChange();
        return true;
    }

    private void UnEquipItemInternal(InventorySlot equipmentSlot, Character character)
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
            if (!RemoveItem(targetSlot, 1))
                return false;

            SyncQuickSlotsAfterItemChanged();
        }

        itemData.ApplyConsumptionEffects(myCharacter, targetObject);
        itemData.PostProcessConsumption(targetSlot);
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
        if (quickSlot == null || quickSlot.ItemID <= 0)
            return false;

        if (!quickSlot.ItemID.TryGetValidItemData(out var itemData, out var itemCategory))
            return false;

        Character myCharacter = Managers.Game?.Player;

        if (myCharacter == null)
            return false;

        var allTabSlots = GetAllTabSlots();

        if (itemCategory == ItemCategory.Consumption)
        {
            if (!itemData.CanUseConsumption(quickSlot))
                return false;

            if (itemData.ShouldConsumeOnUse())
            {
                var targetSlot = allTabSlots.FirstOrDefault(slot => (!string.IsNullOrEmpty(quickSlot.InstanceID) && slot.InstanceID == quickSlot.InstanceID) || (string.IsNullOrEmpty(quickSlot.InstanceID) && slot.ItemID == quickSlot.ItemID));

                if (targetSlot == null || targetSlot.ItemID <= 0 || targetSlot.Quantity <= 0)
                    return false;

                if (!RemoveItem(targetSlot, 1))
                    return false;

                SyncQuickSlotsAfterItemChanged();
            }

            itemData.ApplyConsumptionEffects(myCharacter, targetObject);
            itemData.PostProcessConsumption(quickSlot);
            FinalizeInventoryChange();
            return true;
        }

        if (itemCategory == ItemCategory.Equipment)
        {
            if (!itemData.TryGetEquipmentSlotType(out var slotTypeToEquip))
                return false;

            var targetEquipmentSlot = GetEquipmentSlotByType(slotTypeToEquip);

            if (targetEquipmentSlot == null)
                return false;

            bool isAlreadyEquipped = (!string.IsNullOrEmpty(quickSlot.InstanceID) && targetEquipmentSlot.InstanceID == quickSlot.InstanceID) || (string.IsNullOrEmpty(quickSlot.InstanceID) && targetEquipmentSlot.ItemID == quickSlot.ItemID);

            if (isAlreadyEquipped)
            {
                Managers.Notify.ToastAsync(LocalizationKey.Log_Inventory_AlreadyEquipped).Forget();
                return false;
            }

            var targetSlot = allTabSlots.FirstOrDefault(slot => (!string.IsNullOrEmpty(quickSlot.InstanceID) && slot.InstanceID == quickSlot.InstanceID) || (string.IsNullOrEmpty(quickSlot.InstanceID) && slot.ItemID == quickSlot.ItemID));

            if (targetSlot == null || targetSlot.ItemID <= 0)
                return false;

            int previousEquippedItemID = targetEquipmentSlot.ItemID;
            string previousEquippedInstanceID = targetEquipmentSlot.InstanceID;
            int previousEquippedQuantity = targetEquipmentSlot.Quantity;
            bool equipResult = EquipItem(targetSlot, targetEquipmentSlot);

            if (equipResult && previousEquippedItemID > 0)
            {
                quickSlot.ItemID = previousEquippedItemID;
                quickSlot.InstanceID = previousEquippedInstanceID;
                quickSlot.Quantity = previousEquippedQuantity;
            }

            return equipResult;
        }

        return false;
    }

    public bool HandleItemMoveByTab(ItemCategory currentTab, SlotArea sourceArea, InventorySlot sourceSlot, SlotArea targetArea, InventorySlot targetSlot)
    {
        if (sourceSlot == null || targetSlot == null)
            return false;

        if (sourceArea != targetArea)
            return HandleCrossAreaMove(currentTab, sourceArea, sourceSlot, targetArea, targetSlot);

        if (sourceSlot.TryMergeSlots(targetSlot))
        {
            FinalizeInventoryChange();
            return true;
        }

        sourceSlot.SwapValues(targetSlot);
        FinalizeInventoryChange();
        return true;
    }

    public bool HandleCrossAreaMove(ItemCategory currentTab, SlotArea sourceArea, InventorySlot sourceSlot, SlotArea targetArea, InventorySlot targetSlot)
    {
        if (sourceSlot == null || targetSlot == null)
            return false;

        if (targetArea == SlotArea.Quick)
            return HandleQuickSlotMove(sourceArea, sourceSlot, targetSlot);

        if (sourceArea == SlotArea.Equipment)
        {
            if (currentTab != ItemCategory.Equipment)
                return false;

            return UnEquipItem(sourceSlot, targetSlot);
        }

        if (targetArea == SlotArea.Equipment)
        {
            if (sourceSlot.ItemID <= 0)
                return false;

            if (!sourceSlot.ItemID.TryGetValidItemData(out var itemData, out _) || !itemData.IsEquipmentCategory())
                return false;

            return EquipItem(sourceSlot, targetSlot);
        }

        if (targetArea == SlotArea.Quick)
            return HandleQuickSlotMove(sourceArea, sourceSlot, targetSlot);

        if (sourceArea == SlotArea.Quick)
        {
            sourceSlot.SwapValues(targetSlot);
            FinalizeInventoryChange();
            return true;
        }

        if (sourceSlot.ItemID > 0)
        {
            if (!sourceSlot.ItemID.TryGetValidItemData(out var sourceItemData, out var sourceCategory) || sourceCategory != currentTab)
                return false;
        }

        if (targetSlot.ItemID > 0)
        {
            if (!targetSlot.ItemID.TryGetValidItemData(out var targetItemData, out var targetCategory) || targetCategory != currentTab)
                return false;
        }

        if (sourceSlot.TryMergeSlots(targetSlot))
        {
            FinalizeInventoryChange();
            return true;
        }

        sourceSlot.SwapValues(targetSlot);
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

        if (sourceArea == SlotArea.Equipment)
        {
            targetQuickSlot.AssignSlotData(sourceSlot);
            FinalizeInventoryChange();
            return true;
        }

        if (sourceArea == SlotArea.Quick)
        {
            sourceSlot.SwapValues(targetQuickSlot);
            FinalizeInventoryChange();
            return true;
        }

        targetQuickSlot.AssignSlotData(sourceSlot);
        FinalizeInventoryChange();
        return true;
    }

    public InventorySlot GetEquipmentSlotByType(EquipmentSlotType slotType)
    {
        int index = (int)slotType;
        return _equipmentSlots.ElementAtOrDefault(index);
    }

    private void FinalizeInventoryChange()
    {
        SyncQuickSlotsAfterItemChanged();
        _onInventoryChanged.OnNext(Unit.Default);
    }

    private void SyncQuickSlotsAfterItemChanged()
    {
        var allTabSlots = GetAllTabSlots();

        foreach (var quickSlot in _quickSlots)
        {
            if (quickSlot == null || quickSlot.ItemID <= 0)
                continue;

            var targetEquipment = _equipmentSlots.FirstOrDefault(slot => (!string.IsNullOrEmpty(quickSlot.InstanceID) && slot.InstanceID == quickSlot.InstanceID) || (string.IsNullOrEmpty(quickSlot.InstanceID) && slot.ItemID == quickSlot.ItemID));

            if (targetEquipment != null && targetEquipment.ItemID > 0)
            {
                quickSlot.ItemID = targetEquipment.ItemID;
                quickSlot.InstanceID = targetEquipment.InstanceID;
                quickSlot.Quantity = targetEquipment.Quantity;
                continue;
            }

            List<InventorySlot> targetMasters = null;

            if (!string.IsNullOrEmpty(quickSlot.InstanceID))
                targetMasters = allTabSlots.Where(slot => slot.ItemID == quickSlot.ItemID && slot.InstanceID == quickSlot.InstanceID).ToList();

            if (string.IsNullOrEmpty(quickSlot.InstanceID) || targetMasters == null || targetMasters.Count == 0)
                targetMasters = allTabSlots.Where(slot => slot.ItemID == quickSlot.ItemID && string.IsNullOrEmpty(slot.InstanceID)).ToList();

            if (targetMasters.Any(slot => slot.ItemID > 0))
            {
                var representativeMaster = targetMasters.First(slot => slot.ItemID > 0);
                quickSlot.ItemID = representativeMaster.ItemID;
                quickSlot.InstanceID = representativeMaster.InstanceID;
                quickSlot.Quantity = targetMasters.Sum(slot => slot.Quantity);
                continue;
            }

            quickSlot.ClearSlot();
        }
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
            string targetInstanceID = string.IsNullOrEmpty(instanceID) ? Guid.NewGuid().ToString() : instanceID;
            slot.InstanceID = targetInstanceID;

            if (!_unlockedEquipments.Any(equip => equip.InstanceID == targetInstanceID))
                _unlockedEquipments.Add(new EquipmentInstance { InstanceID = targetInstanceID, ItemID = itemID, UpgradeLevel = 0, ExtraOptionValue = 0 });
        }
    }


    public void SortInventory(ItemCategory currentTabType)
    {
        GetSlotsByType(currentTabType).SortSlots();
        FinalizeInventoryChange();
    }

    public void ClearInventory()
    {
        Character myCharacter = Managers.Game?.Player;

        if (myCharacter != null)
        {
            foreach (var equipmentSlot in _equipmentSlots)
            {
                if (equipmentSlot.ItemID > 0)
                    UnEquipItemInternal(equipmentSlot, myCharacter);
            }
        }

        foreach (var slot in _equipmentTabSlots)
            slot.ClearSlot();

        foreach (var slot in _consumptionTabSlots)
            slot.ClearSlot();

        foreach (var slot in _etcTabSlots)
            slot.ClearSlot();

        foreach (var slot in _equipmentSlots)
            slot.ClearSlot();

        foreach (var quickSlot in _quickSlots)
        {
            if (quickSlot != null)
                quickSlot.ClearSlot();
        }

        FinalizeInventoryChange();
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

    private List<InventorySlot> GetAllTabSlots()
    {
        var allSlots = new List<InventorySlot>();
        allSlots.AddRange(_equipmentTabSlots);
        allSlots.AddRange(_consumptionTabSlots);
        allSlots.AddRange(_etcTabSlots);
        return allSlots;
    }

    public List<InventorySlot> GetSlotsByType(ItemCategory type)
    {
        return type switch
        {
            ItemCategory.Equipment => _equipmentTabSlots,
            ItemCategory.Consumption => _consumptionTabSlots,
            ItemCategory.Etc => _etcTabSlots,
            _ => _equipmentTabSlots
        };
    }

    public IReadOnlyList<InventorySlot> GetEquipmentSlots()
        => _equipmentSlots;

    public IReadOnlyList<InventorySlot> GetQuickSlots()
        => _quickSlots;

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
