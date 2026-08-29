using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class NotifyManager
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public async UniTask LockAsync(Func<UniTask> task)
    {
        var timer = UniTask.Delay(TimeSpan.FromSeconds(0.2f), ignoreTimeScale: true);
        await _semaphore.WaitAsync();
        var locker = Managers.UI.OpenSystem<UILockSystem>(LayerType.Lock);

        try
        {
            locker.PlayAsync().Forget();
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

    public async UniTask LoadAsync(Func<UILoadDisplay, UniTask> task)
    {
        Managers.UI.CloseAll();
        var load = await Managers.UI.OpenDisplayAsync<UILoadDisplay>();
        load?.PlayAsync().Forget();

        try
        {
            await task(load);
        }
        finally
        {
            load?.Release();
        }
    }

    public async UniTask LoadAsync(Func<UniTask> task)
        => await LoadAsync(async load => await task());

    public async UniTask LoadAsync(UniTask task)
        => await LoadAsync(async () => await task);

    public async UniTask ToastAsync(LocalizationKey key)
    {
        var toastSystem = await GetToastSystemAsync();
        await toastSystem.PushToastAsync(key);
    }

    public async UniTask ToastAsync<T1>(LocalizationKey key, T1 arg1)
    {
        var toastSystem = await GetToastSystemAsync();
        await toastSystem.PushToastAsync(key, arg1);
    }

    public async UniTask ToastAsync<T1, T2>(LocalizationKey key, T1 arg1, T2 arg2)
    {
        var toastSystem = await GetToastSystemAsync();
        await toastSystem.PushToastAsync(key, arg1, arg2);
    }

    public async UniTask ToastAsync<T1, T2, T3>(LocalizationKey key, T1 arg1, T2 arg2, T3 arg3)
    {
        var toastSystem = await GetToastSystemAsync();
        await toastSystem.PushToastAsync(key, arg1, arg2, arg3);
    }

    public async UniTask ToastAsync(LocalizationKey key, params object[] args)
    {
        var toastSystem = await GetToastSystemAsync();
        await toastSystem.PushToastAsync(key, args);
    }

    private async UniTask<UIAlertPopup> OpenAlertPopupAsync()
        => await Managers.UI.OpenPopupAsync<UIAlertPopup>(true);

    private async UniTask<UIConfirmPopup> OpenConfirmPopupAsync()
        => await Managers.UI.OpenPopupAsync<UIConfirmPopup>(true);

    private async UniTask<UIToastSystem> GetToastSystemAsync()
        => await Managers.UI.OpenSystemAsync<UIToastSystem>(LayerType.System);

    private async UniTask AlertInternalAsync(UserInterface owner, LocalizationKey titleKey, LocalizationKey messageKey, params object[] messageArgs)
    {
        var popup = await OpenAlertPopupAsync();

        if (IsPopupNull(popup))
            return;

        bool isClosed = false;
        popup.Setup(titleKey, messageKey, () => { isClosed = true; }, messageArgs);

        try
        {
            await UniTask.WaitUntil(() => isClosed || owner.IsPooled());
        }
        catch (OperationCanceledException)
        {
            Log.System(LocalizationKey.Log_Feedback_AlertPopup_Cancelled);
        }

        if (IsPopupNotPooled(popup))
            Managers.UI.Close(popup);
    }

    public async UniTask AlertAsync(UserInterface owner, LocalizationKey titleKey, LocalizationKey messageKey)
        => await AlertInternalAsync(owner, titleKey, messageKey);

    public async UniTask AlertAsync<T1>(UserInterface owner, LocalizationKey titleKey, LocalizationKey messageKey, T1 arg1)
        => await AlertInternalAsync(owner, titleKey, messageKey, arg1);

    public async UniTask AlertAsync<T1, T2>(UserInterface owner, LocalizationKey titleKey, LocalizationKey messageKey, T1 arg1, T2 arg2)
        => await AlertInternalAsync(owner, titleKey, messageKey, arg1, arg2);

    public async UniTask AlertAsync<T1, T2, T3>(UserInterface owner, LocalizationKey titleKey, LocalizationKey messageKey, T1 arg1, T2 arg2, T3 arg3)
        => await AlertInternalAsync(owner, titleKey, messageKey, arg1, arg2, arg3);

    public async UniTask AlertAsync(UserInterface owner, LocalizationKey titleKey, LocalizationKey messageKey, params object[] messageArgs)
        => await AlertInternalAsync(owner, titleKey, messageKey, messageArgs);

    private async UniTask<bool> ConfirmInternalAsync(UserInterface owner, LocalizationKey titleKey, LocalizationKey messageKey, params object[] messageArgs)
    {
        var popup = await OpenConfirmPopupAsync();

        if (IsPopupNull(popup))
            return false;

        bool result = false;
        bool isClosed = false;
        popup.Setup(titleKey, messageKey, onConfirm: () => { result = true; isClosed = true; }, onCancel: () => { result = false; isClosed = true; }, messageArgs);

        try
        {
            await UniTask.WaitUntil(() => isClosed || owner.IsPooled());
        }
        catch (OperationCanceledException)
        {
            Log.System(LocalizationKey.Log_Feedback_ConfirmPopup_Cancelled);
        }

        if (IsPopupNotPooled(popup))
            Managers.UI.Close(popup);

        return result;
    }

    public async UniTask<bool> ConfirmAsync(UserInterface owner, LocalizationKey titleKey, LocalizationKey messageKey)
        => await ConfirmInternalAsync(owner, titleKey, messageKey);

    public async UniTask<bool> ConfirmAsync<T1>(UserInterface owner, LocalizationKey titleKey, LocalizationKey messageKey, T1 arg1)
        => await ConfirmInternalAsync(owner, titleKey, messageKey, arg1);

    public async UniTask<bool> ConfirmAsync<T1, T2>(UserInterface owner, LocalizationKey titleKey, LocalizationKey messageKey, T1 arg1, T2 arg2)
        => await ConfirmInternalAsync(owner, titleKey, messageKey, arg1, arg2);

    public async UniTask<bool> ConfirmAsync<T1, T2, T3>(UserInterface owner, LocalizationKey titleKey, LocalizationKey messageKey, T1 arg1, T2 arg2, T3 arg3)
        => await ConfirmInternalAsync(owner, titleKey, messageKey, arg1, arg2, arg3);

    public async UniTask<bool> ConfirmAsync(UserInterface owner, LocalizationKey titleKey, LocalizationKey messageKey, params object[] messageArgs)
        => await ConfirmInternalAsync(owner, titleKey, messageKey, messageArgs);

    private bool IsPopupNull(UserInterface popup)
        => popup == null;

    private bool IsPopupNotPooled(UserInterface popup)
        => !popup.IsPooled();
}
