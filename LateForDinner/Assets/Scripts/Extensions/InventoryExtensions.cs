using LateForDinner.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZLinq;

public static class InventoryExtensions
{
    public static void ApplyEquipmentEffects(this EquipmentInstance equipInstance, ItemData itemData, Character character)
    {
        if (equipInstance == null || itemData == null || character?.Attributes == null)
            return;

        if (Managers.Data.ItemTemplates == null || !Managers.Data.ItemTemplates.Contains(itemData.ID))
            return;

        var saveData = Managers.Save.CurrentData;
        var templates = Managers.Data.ItemTemplates[itemData.ID];

        foreach (var template in templates)
        {
            if (!Enum.TryParse<ApplyType>(template.ApplyType, true, out var applyType))
                continue;

            if (!Enum.TryParse<AttributeType>(template.AttributeKey, out var attributeType))
                continue;

            bool isPassive = applyType == ApplyType.Passive;
            bool isOneTime = template.Flag;

            if (isOneTime)
            {
                string flagKey = (equipInstance.InstanceID).GetEquipableFlagKey(template.AttributeKey);

                if (saveData.AppliedFlagItems.Contains(flagKey))
                    continue;
            }

            ApplyEffectInternal(character, attributeType, template.Value.ToString(), template.Duration, isPassive, itemData.ID);
        }
    }

    public static void RemoveEquipmentEffects(this EquipmentInstance equipInstance, ItemData itemData, Character character)
    {
        if (equipInstance == null || itemData == null || character?.Attributes == null)
            return;

        if (Managers.Data.ItemTemplates == null || !Managers.Data.ItemTemplates.Contains(itemData.ID))
            return;

        var saveData = Managers.Save.CurrentData;
        var templates = Managers.Data.ItemTemplates[itemData.ID];

        foreach (var template in templates)
        {
            if (!Enum.TryParse<ApplyType>(template.ApplyType, true, out var applyType))
                continue;

            if (!Enum.TryParse<AttributeType>(template.AttributeKey, out var attributeType))
                continue;

            bool isPassive = (applyType == ApplyType.Passive);

            if (isPassive)
                character.Attributes.SubBaseValue(attributeType, template.Value.ToString());
            else
                character.Attributes.SubValue(attributeType, template.Value.ToString());
        }
    }

    private static void ApplyEffectInternal(Character character, AttributeType attributeType, string valueString, float duration, bool isPassive, int itemID)
    {
        int tickCount = Mathf.RoundToInt(duration);

        if (tickCount > 0)
        {
            var buffRegistry = new BuffCooldownRegistry(Guid.NewGuid().ToString(), itemID, attributeType.ToString(), valueString, duration, tickCount, character);
            Managers.Cooldown.Register(buffRegistry);
        }
        else
        {
            if (isPassive)
                character.Attributes.AddBaseValue(attributeType, valueString);
            else
                character.Attributes.AddValue(attributeType, valueString);
        }
    }

    public static bool CanUseConsumption(this ItemData itemData, InventorySlot targetSlot)
    {
        if (itemData == null || targetSlot == null || targetSlot.ItemID <= 0)
            return false;

        if (!itemData.TryGetConsumptionData(out var consumptionData))
            return false;

        if (Enum.TryParse<ConsumptionType>(consumptionData.ConsumptionType, true, out var consumptionType) && consumptionType == ConsumptionType.Potion)
        {
            float cooldownTime = consumptionData.Cooldown;

            if (cooldownTime > 0f)
            {
                string itemCooldownKey = targetSlot.GetItemCooldownKey();
                var existingCooldown = Managers.Cooldown.GetSlotCooldown(itemCooldownKey);

                if (existingCooldown != null && existingCooldown.IsOnCooldown)
                    return false;
            }
        }

        if (Managers.Data.ItemTemplates.Contains(itemData.ID))
        {
            var saveData = Managers.Save.CurrentData;
            var templates = Managers.Data.ItemTemplates[itemData.ID];

            foreach (var template in templates)
            {
                if (template.Flag)
                {
                    string flagKey = targetSlot.GetConsumableFlagKey(template);

                    if (saveData.AppliedFlagItems.Contains(flagKey))
                        return false;
                }
            }
        }

        return true;
    }

    public static void PostProcessConsumption(this ItemData itemData, InventorySlot targetSlot)
    {
        if (itemData == null || !itemData.TryGetConsumptionData(out var consumptionData))
            return;

        if (Enum.TryParse<ConsumptionType>(consumptionData.ConsumptionType, true, out var consumptionType) && consumptionType == ConsumptionType.Potion)
        {
            float cooldownTime = consumptionData.Cooldown;

            if (cooldownTime > 0f)
            {
                string itemCooldownKey = targetSlot.GetItemCooldownKey();
                Managers.Cooldown.RegisterSlotCooldown(itemCooldownKey, cooldownTime);
            }
        }

        if (Managers.Data.ItemTemplates.Contains(itemData.ID))
        {
            var saveData = Managers.Save.CurrentData;
            var templates = Managers.Data.ItemTemplates[itemData.ID];

            foreach (var template in templates)
            {
                if (template.Flag)
                {
                    string flagKey = targetSlot.GetConsumableFlagKey(template);
                    saveData.AppliedFlagItems.Add(flagKey);
                }
            }
        }
    }

    public static void ApplyConsumptionEffects(this ItemData itemData, Character targetCharacter, GameObject targetObject = null)
    {
        if (itemData == null || targetCharacter?.Attributes == null)
            return;

        if (!itemData.TryGetConsumptionData(out var consumptionData))
            return;

        if (Managers.Data.ItemTemplates == null || !Managers.Data.ItemTemplates.Contains(itemData.ID))
            return;

        var templates = Managers.Data.ItemTemplates[itemData.ID].ToList();

        if (Enum.TryParse<ConsumptionType>(consumptionData.ConsumptionType, true, out var consumptionType))
        {
            switch (consumptionType)
            {
                case ConsumptionType.Potion:
                    ApplyPotionTemplates(targetCharacter, templates, itemData.ID);
                    break;
                    // TODO ::: Buff, Scroll 등 추가 확장
            }
        }
    }

    private static void ApplyPotionTemplates(Character character, List<ItemTemplateData> templates, int itemID)
    {
        var saveData = Managers.Save.CurrentData;

        foreach (var template in templates)
        {
            if (!Enum.TryParse<AttributeType>(template.AttributeKey, out var attributeType))
                continue;

            bool isOneTime = template.Flag;

            if (isOneTime)
            {
                string flagKey = itemID.GetConsumableFlagKey(template);

                if (saveData.AppliedFlagItems.Contains(flagKey))
                    continue;
            }

            character.Attributes.AddValue(attributeType, template.Value.ToString());

            if (isOneTime)
            {
                string flagKey = itemID.GetConsumableFlagKey(template);
                saveData.AppliedFlagItems.Add(flagKey);
            }
        }
    }

    public static bool IsWeapon(this ItemData itemData)
    {
        if (itemData == null)
            return false;

        return Managers.Data.WeaponItems.ContainsKey(itemData.ID);
    }

    public static bool IsArmor(this ItemData itemData)
    {
        if (itemData == null)
            return false;

        return Managers.Data.ArmorItems.ContainsKey(itemData.ID);
    }

    public static bool IsConsumption(this ItemData itemData)
    {
        if (itemData == null)
            return false;

        return Managers.Data.ConsumptionItems.ContainsKey(itemData.ID);
    }

    public static bool IsEtc(this ItemData itemData)
    {
        if (itemData == null)
            return false;

        if (Enum.TryParse<ItemCategory>(itemData.ItemCategory, true, out var category))
            return category == ItemCategory.Etc;

        return false;
    }

    public static bool IsPotion(this ItemData itemData)
    {
        if (itemData == null)
            return false;

        if (itemData.TryGetConsumptionData(out var consumptionData) && Enum.TryParse<ConsumptionType>(consumptionData.ConsumptionType, true, out var consumptionType) && consumptionType == ConsumptionType.Potion)
            return true;

        return false;
    }

    public static bool TryGetValidItemData(this int itemID, out ItemData itemData, out ItemCategory itemCategory)
    {
        itemData = null;
        itemCategory = ItemCategory.Etc;

        if (!Managers.Data.Items.ContainsKey(itemID))
            return false;

        itemData = Managers.Data.Items[itemID];
        Enum.TryParse(itemData.ItemCategory, true, out itemCategory);
        return true;
    }

    public static bool IsValidIndex(this int index, int count)
        => index >= 0 && index < count;

    public static bool ShouldConsumeOnUse(this ItemData itemData)
    {
        if (itemData == null)
            return false;

        return itemData.TryGetConsumptionData(out var consumptionData) && consumptionData.Disposable;
    }

    public static bool HasEnoughSpaceForCategory(this List<InventorySlot> totalSlots, int itemID, int maxStack, int quantity, ItemCategory targetCategory)
    {
        var categorySlots = totalSlots
        .Where(slot => slot.ItemID > 0 && slot.ItemID.TryGetValidItemData(out _, out var category) && category == targetCategory)
        .ToList();
        int usedCategorySlotsCount = categorySlots.Count;
        int emptyCategorySlotsCount = Define.Amount.DefaultInventorySlot - usedCategorySlotsCount;
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

            int totalEmptySlots = totalSlots.Count(slot => slot.ItemID == 0);

            if (neededSlots > totalEmptySlots)
                return false;
        }

        return true;
    }

    public static bool TryGetEquipmentSlotType(this ItemData itemData, out EquipmentSlotType slotType)
    {
        slotType = default;

        if (itemData == null)
            return false;

        if (itemData.IsWeapon())
        {
            slotType = EquipmentSlotType.Weapon;
            return true;
        }

        if (Managers.Data.ArmorItems.TryGetValue(itemData.ID, out var armorData))
        {
            if (Enum.TryParse(armorData.ArmorCategory, true, out slotType))
                return true;
        }

        return false;
    }

    public static bool CanEquipInSlot(this ItemData itemData, EquipmentSlotType targetSlotType)
    {
        if (itemData.TryGetEquipmentSlotType(out var itemSlotType))
            return itemSlotType == targetSlotType;

        return false;
    }

    public static bool TryGetConsumptionData(this ItemData itemData, out ConsumptionItemData consumptionData)
    {
        consumptionData = default;

        if (itemData == null)
            return false;

        return Managers.Data.ConsumptionItems.TryGetValue(itemData.ID, out consumptionData);
    }

    public static bool IsEquipmentCategory(this ItemData itemData)
    {
        if (itemData == null)
            return false;

        if (Enum.TryParse<ItemCategory>(itemData.ItemCategory, true, out var category))
            return category == ItemCategory.Equipment;

        return false;
    }

    public static bool IsValidAndCategory(this InventorySlot slot, ItemCategory category)
    {
        if (slot == null || slot.ItemID <= 0)
            return false;

        return slot.ItemID.TryGetValidItemData(out _, out var itemCategory) && itemCategory == category;
    }

    public static string GetFormattedCategoryText(this ItemData itemData)
    {
        if (itemData == null)
            return string.Empty;

        string categoryText = string.Empty;

        if (Managers.Data.ItemCategories.TryGetValue(itemData.ItemCategory, out var itemCategoryData))
            categoryText = Managers.Localization.Get(itemCategoryData.LocalizationKey);

        if (Managers.Data.ArmorItems.TryGetValue(itemData.ID, out var armorItem) && Managers.Data.ArmorCategories.TryGetValue(armorItem.ArmorCategory, out var armorCategoryData))
        {
            string itemCategoryText = Managers.Localization.Get(itemCategoryData.LocalizationKey);
            string armorCategoryText = Managers.Localization.Get(armorCategoryData.LocalizationKey);
            return Managers.Localization.Get(LocalizationKey.Item_Equipment_Format, itemCategoryText, armorCategoryText);
        }

        if (Managers.Data.WeaponItems.TryGetValue(itemData.ID, out var weaponItem) && Managers.Data.WeaponCategories.TryGetValue(weaponItem.WeaponCategory, out var weaponCategoryData))
        {
            string itemCategoryText = Managers.Localization.Get(itemCategoryData.LocalizationKey);
            string weaponCategoryText = Managers.Localization.Get(weaponCategoryData.LocalizationKey);
            return Managers.Localization.Get(LocalizationKey.Item_Equipment_Format, itemCategoryText, weaponCategoryText);
        }

        return categoryText;
    }

    public static bool HandleDoubleClick(this InventorySlot slot, bool isEquipmentSlot, int sourceIndex, ItemCategory currentTabType)
    {
        if (slot == null || slot.ItemID <= 0)
            return false;

        if (!Managers.Data.Items.TryGetValue(slot.ItemID, out var itemData))
            return false;

        if (isEquipmentSlot)
        {
            EquipmentSlotType slotType = (EquipmentSlotType)sourceIndex;
            var targetEquipmentSlot = Managers.Inventory.GetEquipmentSlotByType(slotType);

            if (targetEquipmentSlot == null || targetEquipmentSlot.ItemID <= 0)
                return false;

            var emptySlot = GetFirstEmptyInventorySlot(currentTabType);

            if (emptySlot == null)
                return false;

            return Managers.Inventory.UnEquipItem(targetEquipmentSlot, emptySlot);
        }

        if (itemData.IsConsumption())
            return Managers.Inventory.UseConsumableItem(slot);

        if (itemData.IsEquipmentCategory() && itemData.TryGetEquipmentSlotType(out var slotTypeToEquip))
        {
            var targetEquipmentSlot = Managers.Inventory.GetEquipmentSlotByType(slotTypeToEquip);
            if (targetEquipmentSlot == null)
                return false;

            return Managers.Inventory.EquipItem(slot, targetEquipmentSlot);
        }

        return false;
    }

    private static InventorySlot GetFirstEmptyInventorySlot(ItemCategory currentTabType)
        => Managers.Inventory.GetSlotsByType(currentTabType).FirstOrDefault(slot => slot.ItemID <= 0);

    public static void AssignSlotData(this InventorySlot target, InventorySlot source)
    {
        target.ItemID = source.ItemID;
        target.Quantity = source.Quantity;
        target.InstanceID = source.InstanceID;
    }

    public static void ClearSlot(this InventorySlot slot)
    {
        slot.ItemID = 0;
        slot.Quantity = 0;
        slot.InstanceID = null;
    }

    public static bool TryMergeSlots(this InventorySlot source, InventorySlot target)
    {
        if (source.ItemID <= 0 || source.ItemID != target.ItemID)
            return false;

        if (!source.ItemID.TryGetValidItemData(out var itemData, out var itemCategory) || itemCategory == ItemCategory.Equipment)
            return false;

        int maxStack = itemData.MaxStack;

        if (target.Quantity >= maxStack)
            return false;

        int space = maxStack - target.Quantity;
        int transfer = Math.Min(space, source.Quantity);
        target.Quantity += transfer;
        source.Quantity -= transfer;

        if (source.Quantity <= 0)
            source.ClearSlot();

        return true;
    }

    public static List<InventorySlot> DeepCopySlots(this List<InventorySlot> sourceSlots)
    {
        if (sourceSlots == null)
            return new List<InventorySlot>();

        return sourceSlots.Select(slot => new InventorySlot
        {
            SlotIndex = slot.SlotIndex,
            ItemID = slot.ItemID,
            Quantity = slot.Quantity,
            InstanceID = slot.InstanceID
        }).ToList();
    }

    public static void SortSlots(this List<InventorySlot> slots, Comparison<InventorySlot> comparison = null)
    {
        if (slots == null || slots.Count <= 0)
            return;

        var allItems = new List<(int itemID, int quantity, string instanceID)>();

        foreach (var slot in slots)
        {
            if (slot.ItemID > 0 && slot.Quantity > 0)
                allItems.Add((slot.ItemID, slot.Quantity, slot.InstanceID));
        }

        foreach (var slot in slots)
            slot.ClearSlot();

        var sortedItems = allItems
        .OrderBy(item =>
        {
            if (item.itemID.TryGetValidItemData(out var data, out _) && Managers.Localization != null)
                return Managers.Localization.Get(data.NameKey);
            return string.Empty;
        })
        .ThenByDescending(item => item.quantity)
        .ThenBy(item => item.itemID);

        foreach (var item in sortedItems)
        {
            int remainingQty = item.quantity;

            if (!item.itemID.TryGetValidItemData(out var itemData, out _))
                continue;

            while (remainingQty > 0)
            {
                var targetSlot = slots.FirstOrDefault(slot => slot.ItemID == item.itemID && slot.Quantity < itemData.MaxStack) ?? slots.FirstOrDefault(s => s.ItemID <= 0);

                if (targetSlot == null)
                    break;

                if (targetSlot.ItemID <= 0)
                {
                    targetSlot.ItemID = item.itemID;
                    targetSlot.InstanceID = string.IsNullOrEmpty(item.instanceID) ? Guid.NewGuid().ToString() : item.instanceID;
                }

                int addQty = Math.Min(remainingQty, itemData.MaxStack - targetSlot.Quantity);
                targetSlot.Quantity += addQty;
                remainingQty -= addQty;
            }
        }
    }

    public static void SwapValues(this InventorySlot source, InventorySlot target)
    {
        if (source == null || target == null)
            return;

        int tempItemID = source.ItemID;
        int tempQuantity = source.Quantity;
        string tempInstanceID = source.InstanceID;
        source.ItemID = target.ItemID;
        source.Quantity = target.Quantity;
        source.InstanceID = target.InstanceID;
        target.ItemID = tempItemID;
        target.Quantity = tempQuantity;
        target.InstanceID = tempInstanceID;
    }

    public static string GetItemCooldownKey(this InventorySlot slot)
        => slot == null || slot.ItemID <= 0 ? string.Empty : Define.Key.GetItemCooldownKey(slot.ItemID);

    public static string GetConsumableFlagKey(this InventorySlot slot, ItemTemplateData template)
        => slot == null || slot.ItemID <= 0 || template == null ? string.Empty : Define.Key.GetConsumableFlagKey(slot.ItemID, template.AttributeKey);

    public static string GetConsumableFlagKey(this int itemID, ItemTemplateData template)
        => itemID <= 0 || template == null ? string.Empty : Define.Key.GetConsumableFlagKey(itemID, template.AttributeKey);

    public static string GetEquipableFlagKey(this InventorySlot slot, ItemTemplateData template)
        => slot == null || slot.ItemID <= 0 || template == null ? string.Empty : Define.Key.GetEquipableFlagKey(slot.InstanceID, template.AttributeKey);

    public static string GetEquipableFlagKey(this string instanceID, ItemTemplateData template)
        => string.IsNullOrEmpty(instanceID) || template == null ? string.Empty : Define.Key.GetEquipableFlagKey(instanceID, template.AttributeKey);

    public static string GetEquipableFlagKey(this string instanceID, string attributeKey)
        => string.IsNullOrEmpty(instanceID) || string.IsNullOrEmpty(attributeKey) ? string.Empty : Define.Key.GetEquipableFlagKey(instanceID, attributeKey);
}
