using Cysharp.Threading.Tasks;
using System;

public static class UniTaskExtensions
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

    public static async UniTask Release<T>(this UniTask<T> task) where T : UserInterface
    {
        var user = await task;

        if (user != null)
            user.OnRelease();
    }

    public static async UniTask Release(this UniTask task)
        => await task;
}
