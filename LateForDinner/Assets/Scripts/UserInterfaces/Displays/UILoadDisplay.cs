using Cysharp.Text;
using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;

public class UILoadDisplay : UIDisplay, IAnimationUIView
{
    private enum Images
    {
        RotateImage
    }

    private enum Texts
    {
        MessageText
    }

    private float _current;
    private LocalizationKey _cachedKey;
    private Func<string> _messageProvider;

    public override void OnInit()
    {
        base.OnInit();
        BindImage(typeof(Images));
        BindText(typeof(Texts));
    }

    public override void OnGet()
    {
        base.OnGet();
        _current = 0f;
    }


    public override void OnRelease()
    {
        base.OnRelease();
        _cachedKey = LocalizationKey.None;
        _messageProvider = null;
    }

    public void Setup(LocalizationKey key)
    {
        _cachedKey = key;
        _messageProvider = () => Managers.Localization.Get(key);
    }

    public void Setup<T1>(LocalizationKey key, T1 arg1)
    {
        _cachedKey = key;
        _messageProvider = () => Managers.Localization.Get(key, arg1);
    }

    public void Setup<T1, T2>(LocalizationKey key, T1 arg1, T2 arg2)
    {
        _cachedKey = key;
        _messageProvider = () => Managers.Localization.Get(key, arg1, arg2);
    }

    public void Setup<T1, T2, T3>(LocalizationKey key, T1 arg1, T2 arg2, T3 arg3)
    {
        _cachedKey = key;
        _messageProvider = () => Managers.Localization.Get(key, arg1, arg2, arg3);
    }

    public void Setup(LocalizationKey key, params object[] args)
    {
        _cachedKey = key;
        _messageProvider = () => (args != null && args.Length > 0) ? Managers.Localization.Get(key, args) : Managers.Localization.Get(key);
    }

    public async UniTask LoadAsync(float targetProgress, LocalizationKey key = LocalizationKey.None)
    {
        if (key != default && key != _cachedKey)
            Setup(key);

        var messageText = GetText(Texts.MessageText);
        var token = GetToken("LoadTask");
        float start = _current;
        float duration = 0.3f;
        float elapsedTime = 0f;

        try
        {
            while (elapsedTime < duration)
            {
                token.ThrowIfCancellationRequested();
                elapsedTime += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                _current = Mathf.Lerp(start, targetProgress, t);
                UpdateMessageText(messageText, _current);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            _current = targetProgress;
            UpdateMessageText(messageText, targetProgress);
        }
        catch (OperationCanceledException)
        {
            // DESC ::: 새로운 값이 들어와 기존 애니메이션 중단하는 경우
        }
        catch (Exception)
        {
            Log.Error(LocalizationKey.Log_Load_Display_AnimationFailed);
        }
    }

    public async UniTask PlayAsync()
    {
        var image = GetImage(Images.RotateImage);
        var token = GetToken("RotateTask");

        try
        {
            while (!token.IsCancellationRequested)
            {
                image?.transform.Rotate(0f, 0f, -300f * Time.unscaledDeltaTime);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        catch (OperationCanceledException)
        {
            // DESC ::: 비동기 실행 후 탈출
        }
        catch (Exception)
        {
            Log.Error(LocalizationKey.Log_Load_Display_RotateFailed);
        }
    }

    private void UpdateMessageText(TMP_Text textComponent, float progress)
    {
        int percent = Mathf.RoundToInt(progress * 100f);
        string message = _messageProvider != null ? _messageProvider() : string.Empty;
        textComponent.text = ZString.Format("{0} {1}%", message, percent);
        Log.System(_cachedKey);
    }
}
