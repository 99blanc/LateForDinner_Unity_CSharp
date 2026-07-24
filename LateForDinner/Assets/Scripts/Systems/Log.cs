public static class Log
{
    private static void Write(string message, LogType type = LogType.Info, bool condition = true)
    {
        if (!condition) 
            return;

        Managers.Log.Write(message, type);
    }

    private static void Write(Localization key, LogType type, bool condition = true)
    {
        if (!condition) 
            return;

        Managers.Log.Write(key, type);
    }

    private static void Write<T0>(Localization key, LogType type, bool condition, T0 arg0)
    {
        if (!condition) 
            return;

        Managers.Log.Write(key, type, arg0);
    }

    private static void Write<T0, T1>(Localization key, LogType type, bool condition, T0 arg0, T1 arg1)
    {
        if (!condition) 
            return;

        Managers.Log.Write(key, type, arg0, arg1);
    }

    private static void Write<T0, T1, T2>(Localization key, LogType type, bool condition, T0 arg0, T1 arg1, T2 arg2)
    {
        if (!condition) 
            return;

        Managers.Log.Write(key, type, arg0, arg1, arg2);
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

    public static void Info<T0>(Localization key, bool condition, T0 arg0) 
        => Write(key, LogType.Info, condition, arg0);

    public static void Info<T0, T1>(Localization key, bool condition, T0 arg0, T1 arg1) 
        => Write(key, LogType.Info, condition, arg0, arg1);

    public static void Info<T0, T1, T2>(Localization key, bool condition, T0 arg0, T1 arg1, T2 arg2) 
        => Write(key, LogType.Info, condition, arg0, arg1, arg2);

    public static void Info(Localization key, bool condition, params object[] args) 
        => Write(key, LogType.Info, condition, args);

    public static void Warning(string message, bool condition = true) 
        => Write(message, LogType.Warning, condition);

    public static void Warning(Localization key, bool condition = true) 
        => Write(key, LogType.Warning, condition);

    public static void Warning<T0>(Localization key, bool condition, T0 arg0) 
        => Write(key, LogType.Warning, condition, arg0);

    public static void Warning<T0, T1>(Localization key, bool condition, T0 arg0, T1 arg1) 
        => Write(key, LogType.Warning, condition, arg0, arg1);

    public static void Warning<T0, T1, T2>(Localization key, bool condition, T0 arg0, T1 arg1, T2 arg2) 
        => Write(key, LogType.Warning, condition, arg0, arg1, arg2);

    public static void Warning(Localization key, bool condition, params object[] args) 
        => Write(key, LogType.Warning, condition, args);

    public static void Error(string message, bool condition = true) 
        => Write(message, LogType.Error, condition);

    public static void Error(Localization key, bool condition = true) 
        => Write(key, LogType.Error, condition);

    public static void Error<T0>(Localization key, bool condition, T0 arg0) 
        => Write(key, LogType.Error, condition, arg0);

    public static void Error<T0, T1>(Localization key, bool condition, T0 arg0, T1 arg1) 
        => Write(key, LogType.Error, condition, arg0, arg1);

    public static void Error<T0, T1, T2>(Localization key, bool condition, T0 arg0, T1 arg1, T2 arg2) 
        => Write(key, LogType.Error, condition, arg0, arg1, arg2);

    public static void Error(Localization key, bool condition, params object[] args) 
        => Write(key, LogType.Error, condition, args);
    
    public static void System(string message, bool condition = true) 
        => Write(message, LogType.System, condition);

    public static void System(Localization key, bool condition = true) 
        => Write(key, LogType.System, condition);

    public static void System<T0>(Localization key, bool condition, T0 arg0) 
        => Write(key, LogType.System, condition, arg0);

    public static void System<T0, T1>(Localization key, bool condition, T0 arg0, T1 arg1) 
        => Write(key, LogType.System, condition, arg0, arg1);

    public static void System<T0, T1, T2>(Localization key, bool condition, T0 arg0, T1 arg1, T2 arg2) 
        => Write(key, LogType.System, condition, arg0, arg1, arg2);

    public static void System(Localization key, bool condition, params object[] args) 
        => Write(key, LogType.System, condition, args);
}