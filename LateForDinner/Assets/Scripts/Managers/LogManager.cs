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
        string newKey = key.ToString();
        string raw = (Managers.Localization == null) ? newKey : Managers.Localization.Get(newKey);
        string message = string.IsNullOrEmpty(raw) ? newKey : raw;
        Write(message, type);
    }

    public void Write(Localization key, LogType type, params object[] args)
    {
        string newKey = key.ToString();
        string raw = (Managers.Localization == null) ? newKey : Managers.Localization.Get(newKey);
        string message = string.IsNullOrEmpty(raw) ? newKey : raw;
        string formattedMessage = (args != null && args.Length > 0) ? ZString.Format(message, args) : message;
        Write(formattedMessage, type);
    }
}
