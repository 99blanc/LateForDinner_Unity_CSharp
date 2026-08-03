using Cysharp.Text;
using R3;
using System;
using UnityEngine;

public class LogManager
{
    private readonly Subject<string> _logSubject = new Subject<string>();
    public Observable<string> OnLogAdded => _logSubject;

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
        string prefix = type switch
        {
            LogType.Warning => Literal.Logs.Warning,
            LogType.Error => Literal.Logs.Error,
            LogType.System => Literal.Logs.System,
            _ => Literal.Logs.Info
        };
        string log = ZString.Format(Literal.Messages.Format, DateTime.Now, prefix, message);
        _logSubject.OnNext(log);
        Publish(log, type);
    }

    public void Write(Localization key, LogType type)
    {
        string message = GetMessage(key);
        Write(message, type);
    }

    public void Write<T1>(Localization key, LogType type, T1 arg1)
    {
        string message = GetMessage(key);
        string formatted = ZString.Format(message, arg1);
        Write(formatted, type);
    }

    public void Write<T1, T2>(Localization key, LogType type, T1 arg1, T2 arg2)
    {
        string message = GetMessage(key);
        string formatted = ZString.Format(message, arg1, arg2);
        Write(formatted, type);
    }

    public void Write<T1, T2, T3>(Localization key, LogType type, T1 arg1, T2 arg2, T3 arg3)
    {
        string message = GetMessage(key);
        string formatted = ZString.Format(message, arg1, arg2, arg3);
        Write(formatted, type);
    }
    public void Write(Localization key, LogType type, params object[] args)
    {
        string message = GetMessage(key);
        string formatted = (args != null && args.Length > 0) ? ZString.Format(message, args) : message;
        Write(formatted, type);
    }

    private string GetMessage(Localization key)
    {
        string newKey = key.ToString();
        string raw = (Managers.Localization == null) ? newKey : Managers.Localization.Get(newKey);

        return string.IsNullOrEmpty(raw) ? newKey : raw;
    }
}
