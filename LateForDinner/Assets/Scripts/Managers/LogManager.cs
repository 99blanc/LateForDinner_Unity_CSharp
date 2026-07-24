using Cysharp.Text;
using Cysharp.Threading.Tasks;
using R3;
using System;
using UnityEngine;

public class LogManager
{
    private readonly UniTaskCompletionSource _source = new UniTaskCompletionSource();
    private readonly Subject<string> _logSubject = new Subject<string>();
    public Observable<string> OnLogAdded => _logSubject;

    public async UniTask InitAsync()
    => await UniTask.CompletedTask;

    public void Notify()
        => _source.TrySetResult();

    private void Publish(string log, LogType type)
    {
        switch (type)
        {
            case LogType.Warning:
                Debug.LogWarning(log);
                break;
            case LogType.Error:
                Debug.LogError(log);
                break;
            default:
                Debug.Log(log);
                break;
        }
    }

    public void Write(string message, LogType type = LogType.Info)
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

    private async UniTaskVoid LogAsync(Localization key, LogType type, Func<string, string> action)
    {
        await _source.Task;

        string newKey = key.ToString();
        string raw = (Managers.Localization == null) ? newKey : Managers.Localization.Get(newKey);
        bool isFallback = string.IsNullOrEmpty(raw) || raw == newKey;
        string message = isFallback ? newKey : raw;
        string newMessage = action(message);

        if (isFallback)
            newMessage = $"{Literal.Messages.Fallback} {newMessage}";

        Write(newMessage, type);
    }

    public void Write(Localization key, LogType type)
        => LogAsync(key, type, raw => raw).Forget();

    public void Write<T0>(Localization key, LogType type, T0 arg0)
        => LogAsync(key, type, raw => ZString.Format(raw, arg0)).Forget();

    public void Write<T0, T1>(Localization key, LogType type, T0 arg0, T1 arg1)
        => LogAsync(key, type, raw => ZString.Format(raw, arg0, arg1)).Forget();

    public void Write<T0, T1, T2>(Localization key, LogType type, T0 arg0, T1 arg1, T2 arg2)
        => LogAsync(key, type, raw => ZString.Format(raw, arg0, arg1, arg2)).Forget();

    public void Write(Localization key, LogType type, params object[] args)
        => LogAsync(key, type, raw => (args != null && args.Length > 0) ? ZString.Format(raw, args) : raw).Forget();
}
