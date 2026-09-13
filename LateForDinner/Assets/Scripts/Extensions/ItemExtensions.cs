using LateForDinner.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZLinq;

public static class ItemExtensions
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
            var buffRegistry = new BuffCooldownRegistry(
                id: Guid.NewGuid().ToString(),
                itemID: itemID,
                attributeKey: attributeType.ToString(),
                value: valueString,
                duration: duration,
                ticks: tickCount,
                character: character
            );

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

    public static bool HandleDoubleClick(this InventorySlot slot, bool isEquipmentSlot, int sourceIndex, ItemCategory? currentTabType)
    {
        if (slot == null || slot.ItemID <= 0)
            return false;

        if (!Managers.Data.Items.TryGetValue(slot.ItemID, out var itemData))
            return false;

        if (isEquipmentSlot)
        {
            EquipmentSlotType targetEquipmentSlot = (EquipmentSlotType)sourceIndex;
            int? emptyIndex = GetFirstEmptyInventoryIndex(currentTabType);
            return Managers.Inventory.UnequipItem(targetEquipmentSlot, currentTabType, emptyIndex);
        }

        if (itemData.IsConsumption())
            return Managers.Inventory.UseConsumableItem(slot);

        if (itemData.IsEquipmentCategory() && itemData.TryGetEquipmentSlotType(out var slotType))
        {
            SlotArea sourceArea = isEquipmentSlot ? SlotArea.Equipment : SlotArea.Inventory;
            return Managers.Inventory.EquipItem(sourceArea, sourceIndex, slotType, currentTabType);
        }

        return false;
    }

    private static int? GetFirstEmptyInventoryIndex(ItemCategory? currentTabType)
    {
        var slots = Managers.Inventory.GetSlotsByType(currentTabType);

        for (int index = 0; index < slots.Count; index++)
        {
            if (slots[index].ItemID <= 0)
                return index;
        }

        return null;
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
