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
        EnsureTotalSlotCapacity(_totalSlots, Define.Amount.MaxInventorySlot);

        _equipmentSlots = data.EquipmentSlots ?? new List<InventorySlot>();
        EnsureSlotCapacity(_equipmentSlots, Define.Amount.MaxEquipmentSlot);

        _quickSlots = data.QuickSlots ?? new List<InventorySlot>();
        EnsureSlotCapacity(_quickSlots, Define.Amount.MaxQuickSlot);

        _equipmentTabSlots = data.EquipmentTabSlots ?? new List<InventorySlot>();
        EnsureTabCapacity(_equipmentTabSlots, ItemCategory.Equipment);

        _consumptionTabSlots = data.ConsumptionTabSlots ?? new List<InventorySlot>();
        EnsureTabCapacity(_consumptionTabSlots, ItemCategory.Consumption);

        _etcTabSlots = data.EtcTabSlots ?? new List<InventorySlot>();
        EnsureTabCapacity(_etcTabSlots, ItemCategory.Etc);

        _unlockedEquipments = data.UnlockedEquipments ?? new List<EquipmentInstance>();

        // 데이터가 처음 로드되었을 때 탭과 전체 슬롯 간의 매핑을 정합성 있게 맞춤
        RebuildTabsFromTotal();
    }

    private void EnsureTotalSlotCapacity(List<InventorySlot> slots, int maxCapacity)
    {
        while (slots.Count < maxCapacity)
        {
            int index = slots.Count;
            slots.Add(new InventorySlot
            {
                GlobalIndex = index,
                SlotIndex = index,
                ItemID = 0,
                Quantity = 0,
                InstanceID = null
            });
        }

        for (int index = 0; index < slots.Count; index++)
        {
            slots[index].GlobalIndex = index;
            slots[index].SlotIndex = index;
        }
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
                Quantity = 0,
                InstanceID = null
            });
        }

        for (int index = 0; index < slots.Count; index++)
        {
            slots[index].SlotIndex = index;
        }
    }

    private void EnsureTabCapacity(List<InventorySlot> tabSlots, ItemCategory category)
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
        {
            tabSlots[index].SlotIndex = index;
        }
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
            {
                slot.ItemID = 0;
                slot.Quantity = 0;
                slot.InstanceID = null;
            }
        }

        RebuildTabsFromTotal();
        _onInventoryChanged.OnNext(Unit.Default);
        return true;
    }

    public bool RemoveItem(InventorySlot targetSlot, int quantity)
    {
        if (targetSlot == null || targetSlot.ItemID <= 0 || targetSlot.Quantity < quantity)
            return false;

        var masterSlot = _totalSlots.FirstOrDefault(s => s.GlobalIndex == targetSlot.GlobalIndex);
        var slotToModify = masterSlot ?? targetSlot;

        if (slotToModify.Quantity < quantity)
            return false;

        slotToModify.Quantity -= quantity;

        if (slotToModify.Quantity <= 0)
        {
            slotToModify.ItemID = 0;
            slotToModify.Quantity = 0;
            slotToModify.InstanceID = null;
        }

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
        {
            masterSlot.ItemID = 0;
            masterSlot.Quantity = 0;
            masterSlot.InstanceID = null;
        }

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
        if (sourceIndex < 0 || sourceIndex >= _totalSlots.Count || targetIndex < 0 || targetIndex >= _totalSlots.Count)
            return false;

        var sourceMaster = _totalSlots[sourceIndex];
        var targetMaster = _totalSlots[targetIndex];

        if (sourceMaster == targetMaster)
            return false;

        if (TryMergeOrSwapSlots(sourceMaster, targetMaster))
        {
            RebuildTabsFromTotal();
            _onInventoryChanged.OnNext(Unit.Default);
            return true;
        }

        SwapSlotsValues(sourceMaster, targetMaster);
        RebuildTabsFromTotal();
        _onInventoryChanged.OnNext(Unit.Default);
        return true;
    }

    private bool HandleCategoryTabMove(ItemCategory currentTab, int sourceIndex, int targetIndex)
    {
        var targetList = GetSlotsByType(currentTab);
        if (targetList == null || sourceIndex < 0 || sourceIndex >= targetList.Count || targetIndex < 0 || targetIndex >= targetList.Count)
            return false;

        var sourceTabSlot = targetList[sourceIndex];
        var targetTabSlot = targetList[targetIndex];

        if (sourceTabSlot == targetTabSlot)
            return false;

        // 탭 내 병합 시도
        if (TryMergeSameItemInTab(sourceTabSlot, targetTabSlot))
        {
            SyncTotalFromTab(currentTab);
            RebuildTabsFromTotal();
            _onInventoryChanged.OnNext(Unit.Default);
            return true;
        }

        // 탭 내 자유로운 슬롯 간 교환 (Swap)
        SwapTabSlotValues(sourceTabSlot, targetTabSlot);
        SyncTotalFromTab(currentTab);
        RebuildTabsFromTotal();
        _onInventoryChanged.OnNext(Unit.Default);
        return true;
    }

    private void SwapTabSlotValues(InventorySlot a, InventorySlot b)
    {
        int tempItemID = a.ItemID;
        int tempQty = a.Quantity;
        string tempInstanceID = a.InstanceID;
        int tempGlobalIndex = a.GlobalIndex;

        a.ItemID = b.ItemID;
        a.Quantity = b.Quantity;
        a.InstanceID = b.InstanceID;
        a.GlobalIndex = b.GlobalIndex;

        b.ItemID = tempItemID;
        b.Quantity = tempQty;
        b.InstanceID = tempInstanceID;
        b.GlobalIndex = tempGlobalIndex;
    }

    private bool TryMergeSameItemInTab(InventorySlot sourceTabSlot, InventorySlot targetTabSlot)
    {
        if (sourceTabSlot.ItemID <= 0 || sourceTabSlot.ItemID != targetTabSlot.ItemID)
            return false;

        if (!TryGetValidItemData(sourceTabSlot.ItemID, out var itemData, out var itemCategory))
            return false;

        if (itemCategory == ItemCategory.Equipment)
            return false;

        int maxStack = itemData.MaxStack;

        if (targetTabSlot.Quantity >= maxStack)
            return false;

        int space = maxStack - targetTabSlot.Quantity;
        int transfer = Math.Min(space, sourceTabSlot.Quantity);
        targetTabSlot.Quantity += transfer;
        sourceTabSlot.Quantity -= transfer;

        if (sourceTabSlot.Quantity <= 0)
        {
            sourceTabSlot.ItemID = 0;
            sourceTabSlot.Quantity = 0;
            sourceTabSlot.InstanceID = null;
            sourceTabSlot.GlobalIndex = -1;
        }

        return true;
    }

    private bool HandleCrossAreaMove(ItemCategory? currentTabType, SlotArea sourceArea, int sourceIndex, SlotArea targetArea, int targetIndex)
    {
        InventorySlot sourceSlot = GetSourceSlot(currentTabType, sourceArea, sourceIndex);
        InventorySlot targetSlot = GetTargetSlot(currentTabType, targetArea, targetIndex);

        if (sourceSlot == null || targetSlot == null)
            return false;

        SwapSlotsValues(sourceSlot, targetSlot);
        PostProcessCrossMove(sourceArea, targetArea);
        return true;
    }

    private InventorySlot GetSourceSlot(ItemCategory? currentTabType, SlotArea sourceArea, int sourceIndex)
    {
        if (sourceArea == SlotArea.Inventory)
        {
            if (!currentTabType.HasValue)
            {
                if (sourceIndex < 0 || sourceIndex >= _totalSlots.Count)
                    return null;
                return _totalSlots[sourceIndex];
            }
            else
            {
                var sourceList = GetSlotsByType(currentTabType);
                if (sourceList == null || sourceIndex < 0 || sourceIndex >= sourceList.Count)
                    return null;

                var tabSlot = sourceList[sourceIndex];
                // GlobalIndex가 유효하면 _totalSlots에서 가져오고, 아니면 탭 슬롯 자체 반환
                if (tabSlot.GlobalIndex >= 0 && tabSlot.GlobalIndex < _totalSlots.Count)
                    return _totalSlots[tabSlot.GlobalIndex];
                return null;
            }
        }
        else
        {
            var sourceList = GetSlotList(sourceArea);
            if (sourceList == null || sourceIndex < 0 || sourceIndex >= sourceList.Count)
                return null;
            return sourceList.FirstOrDefault(s => s.SlotIndex == sourceIndex || s.GlobalIndex == sourceIndex);
        }
    }

    private InventorySlot GetTargetSlot(ItemCategory? currentTabType, SlotArea targetArea, int targetIndex)
    {
        if (targetArea == SlotArea.Inventory)
        {
            if (!currentTabType.HasValue)
            {
                if (targetIndex < 0 || targetIndex >= _totalSlots.Count)
                    return null;
                return _totalSlots[targetIndex];
            }
            else
            {
                var targetList = GetSlotsByType(currentTabType);
                if (targetList == null || targetIndex < 0 || targetIndex >= targetList.Count)
                    return null;

                var tabSlot = targetList[targetIndex];
                if (tabSlot.GlobalIndex >= 0 && tabSlot.GlobalIndex < _totalSlots.Count)
                    return _totalSlots[tabSlot.GlobalIndex];

                return _totalSlots.FirstOrDefault(s => s.ItemID == 0);
            }
        }
        else
        {
            var targetList = GetSlotList(targetArea);
            if (targetList == null || targetIndex < 0 || targetIndex >= targetList.Count)
                return null;
            return targetList.FirstOrDefault(s => s.SlotIndex == targetIndex || s.GlobalIndex == targetIndex);
        }
    }

    private void PostProcessCrossMove(SlotArea sourceArea, SlotArea targetArea)
    {
        RebuildTabsFromTotal();
        _onInventoryChanged.OnNext(Unit.Default);
    }

    // 특정 탭의 배치가 변경되었을 때, 그 내용을 _totalSlots 및 다른 탭들의 상태에 안전하게 반영
    private void SyncTotalFromTab(ItemCategory category)
    {
        var tabSlots = GetSlotsByType(category);
        if (tabSlots == null) return;

        // 해당 카테고리에 속하는 _totalSlots 항목들을 초기화 후 재구성
        // 우선 현재 탭에 있는 유효 아이템들을 추출
        var validTabSlots = tabSlots.Where(s => s.ItemID > 0).ToList();

        // _totalSlots 중에서 해당 카테고리인 항목들 지우기
        for (int i = 0; i < _totalSlots.Count; i++)
        {
            var slot = _totalSlots[i];
            if (slot.ItemID > 0 && TryGetValidItemData(slot.ItemID, out _, out var cat) && cat == category)
            {
                slot.ItemID = 0;
                slot.Quantity = 0;
                slot.InstanceID = null;
            }
        }

        // 탭 내에서 배치된 순서대로 비어있는 _totalSlots 공간에 차례대로 채워넣기 (또는 GlobalIndex 매핑 유지)
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

        // 만약 GlobalIndex가 꼬였거나 비어있는 경우 빈 _totalSlots 슬롯에 순차 배치
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

        if (!TryGetValidItemData(source.ItemID, out var itemData, out var itemCategory))
            return false;

        if (itemCategory == ItemCategory.Equipment)
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
            source.InstanceID = null;
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

    private void RebuildTabsFromTotal()
    {
        SyncTabWithCategory(_equipmentTabSlots, ItemCategory.Equipment);
        SyncTabWithCategory(_consumptionTabSlots, ItemCategory.Consumption);
        SyncTabWithCategory(_etcTabSlots, ItemCategory.Etc);
    }

    private void SyncTabWithCategory(List<InventorySlot> tabSlots, ItemCategory category)
    {
        EnsureTabCapacity(tabSlots, category);

        // 해당 카테고리에 속하는 전체 슬롯들의 항목 가져오기
        var masterItems = _totalSlots
            .Where(s => s.ItemID > 0 && TryGetValidItemData(s.ItemID, out _, out var cat) && cat == category)
            .ToList();

        // 기존 탭 슬롯에 이미 배치된 위치 정보를 최대한 보존하기 위해 사전 맵 구성
        var existingMap = new Dictionary<int, InventorySlot>(); // GlobalIndex를 Key로 보유
        foreach (var tabSlot in tabSlots)
        {
            if (tabSlot.GlobalIndex >= 0 && tabSlot.ItemID > 0)
            {
                existingMap[tabSlot.GlobalIndex] = tabSlot;
            }
        }

        // 탭 슬롯 초기화
        foreach (var tabSlot in tabSlots)
        {
            tabSlot.GlobalIndex = -1;
            tabSlot.ItemID = 0;
            tabSlot.Quantity = 0;
            tabSlot.InstanceID = null;
        }

        // 1단계: 기존에 배치되어 있던 위치(GlobalIndex)가 유효하면 그 자리에 그대로 배치
        var unplacedMasters = new List<InventorySlot>();
        foreach (var master in masterItems)
        {
            bool placed = false;
            foreach (var tabSlot in tabSlots)
            {
                // 이 탭 슬롯이 비어있고, 이전에 이 master의 GlobalIndex를 들고 있었거나 빈 자리에 매칭될 수 있는 경우
                if (tabSlot.ItemID == 0)
                {
                    // 기존에 이 GlobalIndex가 특정 tabSlot의 위치에 저장되어 있었다면 그 자리에 배치
                    // 또는 신규 아이템인 경우 빈 탭 슬롯에 순차 배치
                    if (existingMap.TryGetValue(master.GlobalIndex, out var mappedSlot) && mappedSlot == tabSlot)
                    {
                        tabSlot.GlobalIndex = master.GlobalIndex;
                        tabSlot.ItemID = master.ItemID;
                        tabSlot.Quantity = master.Quantity;
                        tabSlot.InstanceID = master.InstanceID;
                        existingMap.Remove(master.GlobalIndex);
                        placed = true;
                        break;
                    }
                }
            }
            if (!placed)
            {
                unplacedMasters.Add(master);
            }
        }

        // 2단계: 자리를 못 찾았거나 새로 들어온 아이템들을 탭의 빈 슬롯에 순서대로 채워넣기
        foreach (var master in unplacedMasters)
        {
            foreach (var tabSlot in tabSlots)
            {
                if (tabSlot.ItemID == 0)
                {
                    tabSlot.GlobalIndex = master.GlobalIndex;
                    tabSlot.ItemID = master.ItemID;
                    tabSlot.Quantity = master.Quantity;
                    tabSlot.InstanceID = master.InstanceID;
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
            slots[index].GlobalIndex = item.GlobalIndex; // GlobalIndex 유지
            index++;
        }

        while (index < slots.Count)
        {
            slots[index].ItemID = 0;
            slots[index].Quantity = 0;
            slots[index].InstanceID = null;
            slots[index].GlobalIndex = -1;
            index++;
        }
    }

    public void ClearInventory()
    {
        foreach (var slot in _totalSlots)
        {
            slot.ItemID = 0;
            slot.Quantity = 0;
            slot.InstanceID = null;
        }

        RebuildTabsFromTotal();

        foreach (var slot in _equipmentSlots)
        {
            slot.ItemID = 0;
            slot.Quantity = 0;
            slot.InstanceID = null;
        }

        foreach (var slot in _quickSlots)
        {
            slot.ItemID = 0;
            slot.Quantity = 0;
            slot.InstanceID = null;
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

    public List<EquipmentInstance> ExportUnlockedEquipmentsSaveData()
    {
        return _unlockedEquipments.Select(eq => new EquipmentInstance
        {
            InstanceID = eq.InstanceID,
            ItemID = eq.ItemID,
            UpgradeLevel = eq.UpgradeLevel,
            ExtraOptionValue = eq.ExtraOptionValue,
            Flag = eq.Flag
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