using R3;
using UnityEngine;
using System;

public class UIFPSSystem : UISystem
{
    private enum Texts
    {
        FPSText
    }

    private float _pollingTime = Define.Framerate.PollingTime;
    private IDisposable _fpsSubscription;

    public override void Init()
    {
        base.Init();
        BindText(typeof(Texts));
    }

    public override void Get()
    {
        base.Get();
        _fpsSubscription?.Dispose();
        _fpsSubscription = Observable.EveryUpdate()
        .Scan((Accumulator: 0f, FrameCount: 0), (state, _) =>
        {
            var newTime = state.Accumulator + Time.unscaledDeltaTime;
            var newCount = state.FrameCount + 1;

            if (newTime >= _pollingTime)
                return (0f, 0);

            return (newTime, newCount);
        })
        .Where(state => state.Accumulator == 0f && state.FrameCount == 0)
        .Subscribe(_ =>
        {
            int fps = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
            GetText(Texts.FPSText).text = Managers.Localization.Get(LocalizationKey.UI_FPS_System_Indicator, fps);
        });
    }

    public override void Release()
    {
        base.Release();
        _fpsSubscription?.Dispose();
        _fpsSubscription = null;
    }
}