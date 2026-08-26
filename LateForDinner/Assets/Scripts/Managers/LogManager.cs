using Cysharp.Text;
using R3;
using System;
using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class LogManager
{
    private readonly Subject<LogFormat> _logSubject = new Subject<LogFormat>();
    private readonly ReactiveProperty<bool> _isReady = new ReactiveProperty<bool>(false);
    private readonly Queue<Action> _pendingLogs = new Queue<Action>();
    private readonly List<LogFormat> _logs = new List<LogFormat>();
    public IReadOnlyList<LogFormat> Logs => _logs;
    public Observable<LogFormat> OnLogAdded 
        => _logSubject;

    public void Setup()
    {
        _isReady.Value = true;

        while (_pendingLogs.Count > 0)
            _pendingLogs.Dequeue()?.Invoke();

        Write(LocalizationKey.Log_Log_SetupCompleted, LogType.System);
    }

    private void ProcessLog(Action logAction)
    {
        if (!_isReady.Value)
        {
            _pendingLogs.Enqueue(logAction);
            return;
        }

        logAction();
    }

    private void Publish(string log, LogType type)
    {
        Action<string> logAction = type switch
        {
            LogType.Info => Debug.Log,
            LogType.Warning => Debug.LogWarning,
            LogType.Error => Debug.LogError,
            _ => Debug.Log
        };
        logAction(log);
    }

    public void Write(string message, LogType type)
    {
        ProcessLog(() =>
        {
            string prefix = GetLogPrefix(type);
            string log = ZString.Format(Literal.Messages.Format, DateTime.Now, prefix, message);
            var logData = new LogFormat { Message = log, Type = type };
            _logs.Add(logData);

            if (_logs.Count > Define.Log.Storage)
                _logs.RemoveAt(0);

            _logSubject.OnNext(logData);
            Publish(log, type);
        });
    }

    public void Write(LocalizationKey key, LogType type) 
        => Write(GetMessage(key), type);

    public void Write<T1>(LocalizationKey key, LogType type, T1 arg1) 
        => WriteFormatted(key, type, message => ZString.Format(message, arg1));

    public void Write<T1, T2>(LocalizationKey key, LogType type, T1 arg1, T2 arg2) 
        => WriteFormatted(key, type, message => ZString.Format(message, arg1, arg2));

    public void Write<T1, T2, T3>(LocalizationKey key, LogType type, T1 arg1, T2 arg2, T3 arg3) 
        => WriteFormatted(key, type, message => ZString.Format(message, arg1, arg2, arg3));

    public void Write(LocalizationKey key, LogType type, params object[] args)
    {
        ProcessLog(() =>
        {
            string message = GetMessage(key);
            string formatted = (args != null && args.Length > 0) ? ZString.Format(message, args) : message;
            Write(formatted, type);
        });
    }

    private void WriteFormatted(LocalizationKey key, LogType type, Func<string, string> formatAction)
    {
        ProcessLog(() =>
        {
            string message = GetMessage(key);
            string formatted = formatAction(message);
            Write(formatted, type);
        });
    }

    private string GetLogPrefix(LogType type)
    {
        return type switch
        {
            LogType.Info => Literal.Logs.Info,
            LogType.Warning => Literal.Logs.Warning,
            LogType.Error => Literal.Logs.Error,
            LogType.System => Literal.Logs.System,
            _ => Literal.Logs.Info
        };
    }

    private string GetMessage(LocalizationKey key)
    {
        string newKey = key.ToString();
        string raw = Managers.Localization == null ? newKey : Managers.Localization.Get(newKey);
        return string.IsNullOrEmpty(raw) ? newKey : raw;
    }
}
