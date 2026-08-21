using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class FeedbackManager
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public async UniTask LockAsync(Func<UniTask> task)
    {
        var timer = UniTask.Delay(TimeSpan.FromSeconds(0.2f), ignoreTimeScale: true);
        var locker = await Managers.UI.OpenSystemAsync<UILock>(Layer.Lock);

        if (locker == null)
        {
            await task();
            return;
        }

        locker.PlayAsync().Forget();
        await _semaphore.WaitAsync();

        try
        {
            await task();
        }
        finally
        {
            await timer;
            Managers.UI.Close(locker);
            _semaphore.Release();
        }
    }

    public async UniTask LockAsync(UniTask task)
        => await LockAsync(async () => await task);

    public async UniTask ToastAsync(string message)
    {
        var toastSystem = await Managers.UI.OpenSystemAsync<UIToastSystem>(Layer.System);

        if (toastSystem == null)
            return;

        await toastSystem.PushToastAsync(message);
    }
}

