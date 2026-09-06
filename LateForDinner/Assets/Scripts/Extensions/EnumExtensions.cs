public static class EnumExtensions
{
    public static string ToSpriteAsMealTime(this MealTime mealTime)
    {
        return mealTime switch
        {
            MealTime.Breakfast => Define.Sprite.MealTime_Breakfast,
            MealTime.Lunch => Define.Sprite.MealTime_Lunch,
            MealTime.Dinner => Define.Sprite.MealTime_Dinner,
            _ => Define.Sprite.MealTime_Breakfast
        };
    }

    public static string ToSpriteAsEquipmentCover(this EquipmentSlotType slotType)
    {
        return slotType switch
        {
            EquipmentSlotType.Head => Define.Sprite.InventoryHead,
            EquipmentSlotType.Chest => Define.Sprite.InventoryChest,
            EquipmentSlotType.Pants => Define.Sprite.InventoryPants,
            EquipmentSlotType.Boots => Define.Sprite.InventoryBoots,
            EquipmentSlotType.Weapon => Define.Sprite.InventoryWeapon,
            _ => string.Empty
        };
    }
}
