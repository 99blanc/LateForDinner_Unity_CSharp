using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

public class UIToastSystem : UISystem
{
    private enum Panels
    {
        ToastPanel
    }

    private readonly Queue<string> _messageQueue = new Queue<string>();
    private readonly HashSet<UIToastSlot> _activeSlots = new HashSet<UIToastSlot>();
    private const int _maxCount = Define.Toast.Count;
    private bool _isProcessingQueue;

    public override void Init()
    {
        base.Init();
        BindPanel(typeof(Panels));
    }

    public async UniTask PushToastAsync(string message)
    {
        _messageQueue.Enqueue(message);

        if (_isProcessingQueue)
            return;

        await ProcessQueueAsync();
    }

    private async UniTask ProcessQueueAsync()
    {
        _isProcessingQueue = true;

        while (_messageQueue.Count > 0)
        {
            var containerTransform = GetPanel((int)Panels.ToastPanel)?.transform;

            if (containerTransform == null)
                break;

            while (_activeSlots.Count >= _maxCount)
                await UniTask.Yield(PlayerLoopTiming.Update);

            string message = _messageQueue.Dequeue();
            var (slot, _) = await Managers.Pool.PopAsync<UIToastSlot>(containerTransform);

            if (slot == null)
                continue;

            _activeSlots.Add(slot);
            slot.Setup(message, () =>
            {
                _activeSlots.Remove(slot);
                slot.Close();
            });
            await UniTask.Delay(TimeSpan.FromSeconds(Define.Toast.Delay), ignoreTimeScale: true);
        }

        _isProcessingQueue = false;
    }
}
