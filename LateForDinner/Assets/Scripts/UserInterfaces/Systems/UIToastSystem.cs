using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

public class UIToastSystem : UISystem
{
    private enum Panels
    {
        ToastPanel
    }

    private readonly Queue<Action<UIToastSlot>> _toastQueue = new Queue<Action<UIToastSlot>>();
    private readonly HashSet<UIToastSlot> _activeSlots = new HashSet<UIToastSlot>();
    private const int _maxCount = Define.Toast.Count;
    private bool _isProcessingQueue;

    public override void Init()
    {
        base.Init();
        BindPanel(typeof(Panels));
    }

    private async UniTask EnqueueToastAsync(Action<UIToastSlot> setupAction)
    {
        _toastQueue.Enqueue(setupAction);

        if (_isProcessingQueue)
            return;

        await ProcessQueueAsync();
    }

    private async UniTask ProcessQueueAsync()
    {
        _isProcessingQueue = true;

        while (_toastQueue.Count > 0)
        {
            var containerTransform = GetPanel(Panels.ToastPanel)?.transform;

            if (containerTransform == null)
                break;

            while (_activeSlots.Count >= _maxCount)
                await UniTask.Yield(PlayerLoopTiming.Update);

            var setupAction = _toastQueue.Dequeue();
            var (slot, _) = await Managers.Pool.PopAsync<UIToastSlot>(containerTransform);

            if (slot == null)
                continue;

            _activeSlots.Add(slot);
            setupAction(slot);
            await UniTask.Delay(TimeSpan.FromSeconds(Define.Toast.Delay), ignoreTimeScale: true);
        }

        _isProcessingQueue = false;
    }

    public async UniTask PushToastAsync(Localization key)
        => await EnqueueToastAsync(slot => slot.Setup(key, () => ReleaseSlot(slot)));
    public async UniTask PushToastAsync<T1>(Localization key, T1 arg1)
        => await EnqueueToastAsync(slot => slot.Setup(key, () => ReleaseSlot(slot), arg1));
    public async UniTask PushToastAsync<T1, T2>(Localization key, T1 arg1, T2 arg2)
        => await EnqueueToastAsync(slot => slot.Setup(key, () => ReleaseSlot(slot), arg1, arg2));
    public async UniTask PushToastAsync<T1, T2, T3>(Localization key, T1 arg1, T2 arg2, T3 arg3)
        => await EnqueueToastAsync(slot => slot.Setup(key, () => ReleaseSlot(slot), arg1, arg2, arg3));
    public async UniTask PushToastAsync(Localization key, params object[] args)
        => await EnqueueToastAsync(slot => slot.Setup(key, () => ReleaseSlot(slot), args));

    private void ReleaseSlot(UIToastSlot slot)
    {
        _activeSlots.Remove(slot);
        slot.Close();
    }
}
