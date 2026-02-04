namespace Token.PRIORITY
{
    public enum ModulePrority
    {
        NULL,
        AGENT_CONTROL,
        PLAYER_CONTROL
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
        _GROUP_INTERACTION = 300
    }
}
