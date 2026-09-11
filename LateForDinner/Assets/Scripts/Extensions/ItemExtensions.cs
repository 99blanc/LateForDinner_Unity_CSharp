using LateForDinner.Data;
using System;
using UnityEngine;

public static class ItemExtensions
{
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

    public static bool IsEquipmentCategory(this ItemData itemData)
    {
        if (itemData == null) 
            return false;

        if (Enum.TryParse<ItemCategory>(itemData.ItemCategory, true, out var category))
            return category == ItemCategory.Equipment;

        return false;
    }
}
