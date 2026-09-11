using LateForDinner.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ItemExtensions
{
    public static void ApplyEquipmentEffects(this EquipmentInstance equipInstance, ItemData itemData, Character character)
    {
        if (equipInstance == null || itemData == null || character?.Attributes == null)
            return;

        if (Managers.Data.ItemTemplates == null || !Managers.Data.ItemTemplates.Contains(itemData.ID))
            return;

        var templates = Managers.Data.ItemTemplates[itemData.ID];

        foreach (var template in templates)
        {
            if (!Enum.TryParse<ApplyType>(template.ApplyType, true, out var applyType))
                continue;

            if (!Enum.TryParse<AttributeType>(template.AttributeKey, out var attributeType))
                continue;

            bool isPassive = applyType == ApplyType.Passive;
            bool isOneTime = template.Flag;

            if (isOneTime && equipInstance.Flag)
                continue;

            ApplyEffectInternal(character, attributeType, template.Value.ToString(), template.Duration, isPassive, itemData.ID);

            if (isOneTime)
                equipInstance.Flag = true;
        }
    }

    public static void RemoveEquipmentEffects(this EquipmentInstance equipInstance, ItemData itemData, Character character)
    {
        if (equipInstance == null || itemData == null || character?.Attributes == null)
            return;

        if (Managers.Data.ItemTemplates == null || !Managers.Data.ItemTemplates.Contains(itemData.ID))
            return;

        var templates = Managers.Data.ItemTemplates[itemData.ID];

        foreach (var template in templates)
        {
            if (!Enum.TryParse<ApplyType>(template.ApplyType, true, out var applyType))
                continue;

            if (!Enum.TryParse<AttributeType>(template.AttributeKey, out var attributeType))
                continue;

            bool isPassive = (applyType == ApplyType.Passive);
            bool isOneTime = template.Flag;

            if (isOneTime)
                continue;

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
                    ApplyPotionTemplates(targetCharacter, templates);
                    break;
                    // TODO ::: Buff, Scroll 등 추가 확장
            }
        }
    }

    private static void ApplyPotionTemplates(Character character, List<ItemTemplateData> templates)
    {
        foreach (var template in templates)
        {
            if (!Enum.TryParse<AttributeType>(template.AttributeKey, out var attributeType))
                continue;

            character.Attributes.AddValue(attributeType, template.Value.ToString());
        }
    }

    private static void ApplyBuffTemplates(Character character, List<ItemTemplateData> templates, int itemID)
    {
        foreach (var template in templates)
        {
            if (!Enum.TryParse<AttributeType>(template.AttributeKey, out var attributeType))
                continue;

            int tickCount = Mathf.RoundToInt(template.Duration);

            if (tickCount > 0)
            {
                var buffRegistry = new BuffCooldownRegistry(
                id: Guid.NewGuid().ToString(),
                itemID: itemID,
                attributeKey: attributeType.ToString(),
                value: template.Value.ToString(),
                duration: template.Duration,
                ticks: tickCount,
                character: character);
                Managers.Cooldown.Register(buffRegistry);
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
}
