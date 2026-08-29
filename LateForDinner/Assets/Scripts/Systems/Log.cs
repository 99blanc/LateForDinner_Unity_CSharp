public static class Log
{
    private static void Write(string message, LogType type, bool condition)
    {
        if (!condition) 
            return;

        Managers.Log.Write(message, type);
    }

    private static void Write(LocalizationKey key, LogType type, bool condition)
    {
        if (!condition) 
            return;

        Managers.Log.Write(key, type);
    }

    private static void Write<T1>(LocalizationKey key, LogType type, bool condition, T1 arg1)
    {
        if (!condition) 
            return;

        Managers.Log.Write(key, type, arg1);
    }

    private static void Write<T1, T2>(LocalizationKey key, LogType type, bool condition, T1 arg1, T2 arg2)
    {
        if (!condition) 
            return;

        Managers.Log.Write(key, type, arg1, arg2);
    }

    private static void Write<T1, T2, T3>(LocalizationKey key, LogType type, bool condition, T1 arg1, T2 arg2, T3 arg3)
    {
        if (!condition) 
            return;

        Managers.Log.Write(key, type, arg1, arg2, arg3);
    }

    private static void Write(LocalizationKey key, LogType type, bool condition, params object[] args)
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

    public static void Info(LocalizationKey key, bool condition = true)
        => Write(key, LogType.Info, condition);

    public static void Info<T1>(LocalizationKey key, T1 arg1, bool condition = true) 
        => Write(key, LogType.Info, condition, arg1);

    public static void Info<T1, T2>(LocalizationKey key, T1 arg1, T2 arg2, bool condition = true) 
        => Write(key, LogType.Info, condition, arg1, arg2);

    public static void Info<T1, T2, T3>(LocalizationKey key, T1 arg1, T2 arg2, T3 arg3, bool condition = true) 
        => Write(key, LogType.Info, condition, arg1, arg2, arg3);

    public static void Info(LocalizationKey key, bool condition, params object[] args) 
        => Write(key, LogType.Info, condition, args);

    public static void Warning(string message, bool condition = true)
        => Write(message, LogType.Warning, condition);

    public static void Warning(LocalizationKey key, bool condition = true)
        => Write(key, LogType.Warning, condition);

    public static void Warning<T1>(LocalizationKey key, T1 arg1, bool condition = true) 
        => Write(key, LogType.Warning, condition, arg1);

    public static void Warning<T1, T2>(LocalizationKey key, T1 arg1, T2 arg2, bool condition = true) 
        => Write(key, LogType.Warning, condition, arg1, arg2);

    public static void Warning<T1, T2, T3>(LocalizationKey key, T1 arg1, T2 arg2, T3 arg3, bool condition = true) 
        => Write(key, LogType.Warning, condition, arg1, arg2, arg3);

    public static void Warning(LocalizationKey key, bool condition, params object[] args) 
        => Write(key, LogType.Warning, condition, args);

    public static void Error(string message, bool condition = true)
        => Write(message, LogType.Error, condition);

    public static void Error(LocalizationKey key, bool condition = true)
        => Write(key, LogType.Error, condition);

    public static void Error<T1>(LocalizationKey key, T1 arg1, bool condition = true) 
        => Write(key, LogType.Error, condition, arg1);

    public static void Error<T1, T2>(LocalizationKey key, T1 arg1, T2 arg2, bool condition = true) 
        => Write(key, LogType.Error, condition, arg1, arg2);

    public static void Error<T1, T2, T3>(LocalizationKey key, T1 arg1, T2 arg2, T3 arg3, bool condition = true) 
        => Write(key, LogType.Error, condition, arg1, arg2, arg3);

    public static void Error(LocalizationKey key, bool condition, params object[] args) 
        => Write(key, LogType.Error, condition, args);

    public static void System(string message, bool condition = true)
        => Write(message, LogType.System, condition);

    public static void System(LocalizationKey key, bool condition = true)
        => Write(key, LogType.System, condition);

    public static void System<T1>(LocalizationKey key, T1 arg1, bool condition = true) 
        => Write(key, LogType.System, condition, arg1);

    public static void System<T1, T2>(LocalizationKey key, T1 arg1, T2 arg2, bool condition = true) 
        => Write(key, LogType.System, condition, arg1, arg2);

    public static void System<T1, T2, T3>(LocalizationKey key, T1 arg1, T2 arg2, T3 arg3, bool condition = true) 
        => Write(key, LogType.System, condition, arg1, arg2, arg3);

    public static void System(LocalizationKey key, bool condition, params object[] args) 
        => Write(key, LogType.System, condition, args);
}
