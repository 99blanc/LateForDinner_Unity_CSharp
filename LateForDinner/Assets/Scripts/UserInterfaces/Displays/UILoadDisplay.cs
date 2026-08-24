using Cysharp.Text;
using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;

public class UILoadDisplay : UIDisplay, IAnimatable
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

    public override void Init()
    {
        base.Init();
        BindImage(typeof(Images));
        BindText(typeof(Texts));
    }

    public override void Get()
    {
        base.Get();
        _current = 0f;
    }

    public async UniTask LoadAsync(float targetProgress, string message)
    {
        var messageText = GetText(Texts.MessageText);

        if (messageText == null)
            return;

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
                UpdateMessageText(messageText, message, _current);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            _current = targetProgress;
            UpdateMessageText(messageText, message, targetProgress);
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

    private void UpdateMessageText(TMP_Text textComponent, string message, float progress)
    {
        int percent = Mathf.RoundToInt(progress * 100f);
        textComponent.text = ZString.Format("{0} {1}%", message, percent);
    }
}
