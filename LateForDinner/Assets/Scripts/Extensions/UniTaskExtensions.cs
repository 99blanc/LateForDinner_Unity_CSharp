using Cysharp.Threading.Tasks;

public static class UniTaskExtensions
{
    public static async UniTask Lock(this UniTask task)
        => await Managers.Feedback.LockAsync(task);

    public static async UniTask Release<T>(this UniTask<T> task) where T : UserInterface
    {
        var user = await task;

        if (user != null)
            user.Release();
    }

    public static async UniTask Release(this UniTask task)
        => await task;
}
