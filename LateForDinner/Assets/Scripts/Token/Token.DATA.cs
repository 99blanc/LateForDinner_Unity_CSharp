namespace Token.DATA
{
    public enum EquipmentPart
    {
        NULL,
        WEAPON,
        HAT,
        TOP,
        BOTTOM,
        SHOES
    }

    public enum WeaponCategory
    {
        NULL,
        GREAT_SWORD,
        DAGGER,
        BLUNT,
        BOW,
        THROW
    }

    public enum ItemCategory
    {
        NULL,
        EQUIPMENT,
        CONSUMPTION,
        MISC,
        QUEST,
        EVENT
    }

    public enum StatType
    {
        NULL,
        CURRENT_HEALTH,
        MAX_HEALTH,
        CURRENT_TEMP_HEALTH,
        MAX_TEMP_HEALTH,
        MOVE_SPEED,
        DAMAGE,
        ATK_SPEED,
        ATK_RANGE,
        ATK_INTERVAL,
        CHARGE_MULTIPLE,
        PIERCE_COUNT,
        DASH_COUNT,
        DASH_COOLTIME,
        DASH_DISTANCE,
        JUMP_COUNT,
        JUMP_FORCE,
        GV_REDUCTION,
        INVUL_DURATION,
        EQUIPMENT_PART,
        WEAPON_CATEGORY,
        ITEM_CATEGORY
    }
}
