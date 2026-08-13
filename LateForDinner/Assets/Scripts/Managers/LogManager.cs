using Cysharp.Text;
using R3;
using System;
using UnityEngine;

public class LogManager
{
    private readonly Subject<string> _logSubject = new Subject<string>();
    public Observable<string> OnLogAdded 
        => _logSubject;

    private void Publish(string log, LogType type)
    {
        Action<string> logAction = type switch
        {
            LogType.Warning => Debug.LogWarning,
            LogType.Error => Debug.LogError,
            _ => Debug.Log
        };
        logAction(log);
    }

    public void Write(string message, LogType type)
    {
        string prefix = GetLogPrefix(type);
        string log = ZString.Format(Literal.Messages.Format, DateTime.Now, prefix, message);
        _logSubject.OnNext(log);
        Publish(log, type);
    }

    public void Write(Localization key, LogType type) 
        => Write(GetMessage(key), type);

    public void Write<T1>(Localization key, LogType type, T1 arg1) 
        => WriteFormatted(key, type, message => ZString.Format(message, arg1));

    public void Write<T1, T2>(Localization key, LogType type, T1 arg1, T2 arg2) 
        => WriteFormatted(key, type, message => ZString.Format(message, arg1, arg2));

    public void Write<T1, T2, T3>(Localization key, LogType type, T1 arg1, T2 arg2, T3 arg3) 
        => WriteFormatted(key, type, message => ZString.Format(message, arg1, arg2, arg3));

    public void Write(Localization key, LogType type, params object[] args)
    {
        string message = GetMessage(key);
        string formatted = (args != null && args.Length > 0) ? ZString.Format(message, args) : message;
        Write(formatted, type);
    }

    private void WriteFormatted(Localization key, LogType type, Func<string, string> formatAction)
    {
        string message = GetMessage(key);
        string formatted = formatAction(message);
        Write(formatted, type);
    }

    private string GetLogPrefix(LogType type)
    {
        return type switch
        {
            LogType.Warning => Literal.Logs.Warning,
            LogType.Error => Literal.Logs.Error,
            LogType.System => Literal.Logs.System,
            _ => Literal.Logs.Info
        };
    }

    private string GetMessage(Localization key)
    {
        string newKey = key.ToString();
        string raw = Managers.Localization == null ? newKey : Managers.Localization.Get(newKey);
        return string.IsNullOrEmpty(raw) ? newKey : raw;
    }
}
