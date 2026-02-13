namespace Token.PRIORITY
{
    public enum ModulePrority
    {
        NULL,
        _GROUP_CONTROL = 100,
        PLAYER_CONTROL,
        _GROUP_ANIMATOR = 200,
        PLAYER_ANIMATOR
    }

    public enum PropPriority
    {
        NULL,
        _GROUP_ENVIRONMENT = 100,
        ONEWAY_PLATFORM,
        TWOWAY_PLATFORM,
        _GROUP_OBJECTIVE = 200,
        LADDER,
        BOX,
        _GROUP_INTERACTION = 300,
        TRAY,
        NPC,
        PORTAL
    }
}
