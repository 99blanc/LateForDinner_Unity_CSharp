using Cysharp.Threading.Tasks;
using System;

public static class NotifyExtensions
{
    public static async UniTask Lock(this Func<UniTask> task)
        => await Managers.Notify.LockAsync(task);

    public static async UniTask Lock(this UniTask task)
        => await Managers.Notify.LockAsync(task);

    public static async UniTask Load(this Func<UILoadDisplay, UniTask> task)
        => await Managers.Notify.LoadAsync(task);

    public static async UniTask Load(this Func<UniTask> task)
        => await Managers.Notify.LoadAsync(task);

    public static async UniTask Load(this UniTask task)
        => await Managers.Notify.LoadAsync(task);
}
