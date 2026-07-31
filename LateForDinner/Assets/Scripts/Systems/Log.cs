public static class Log
{
    private static void Write(string message, LogType type, bool condition)
    {
        if (!condition) 
            return;

        Managers.Log.Write(message, type);
    }

    private static void Write(Localization key, LogType type, bool condition)
    {
        if (!condition) 
            return;

        Managers.Log.Write(key, type);
    }

    private static void Write(Localization key, LogType type, bool condition, params object[] args)
    {
        if (!condition) 
            return;

        if (args != null && args.Length > 0)
            Managers.Log.Write(key, type, args);
        else
            Managers.Log.Write(key, type);
    }

    public static void Info(string message, bool condition = true)
        => Write(message, LogType.Info, condition);

    public static void Info(Localization key, bool condition = true)
        => Write(key, LogType.Info, condition);

    public static void Info(Localization key, bool condition, params object[] args)
        => Write(key, LogType.Info, condition, args);

    public static void Warning(string message, bool condition = true)
        => Write(message, LogType.Warning, condition);

    public static void Warning(Localization key, bool condition = true)
        => Write(key, LogType.Warning, condition);

    public static void Warning(Localization key, bool condition, params object[] args)
        => Write(key, LogType.Warning, condition, args);

    public static void Error(string message, bool condition = true)
        => Write(message, LogType.Error, condition);

    public static void Error(Localization key, bool condition = true)
        => Write(key, LogType.Error, condition);

    public static void Error(Localization key, bool condition, params object[] args)
        => Write(key, LogType.Error, condition, args);

    public static void System(string message, bool condition = true)
        => Write(message, LogType.System, condition);

    public static void System(Localization key, bool condition = true)
        => Write(key, LogType.System, condition);

    public static void System(Localization key, bool condition, params object[] args)
        => Write(key, LogType.System, condition, args);
}