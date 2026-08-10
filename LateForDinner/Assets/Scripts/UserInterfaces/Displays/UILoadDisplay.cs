using Cysharp.Text;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class UILoadDisplay : UIDisplay, IAnimatable
{
    private enum Texts
    {
        MessageText
    }

    private enum Images
    {
        RotateImage
    }

    private float _current;

    public override void Init()
    {
        base.Init();
        BindText(typeof(Texts));
        BindImage(typeof(Images));
    }

    public override void Get()
        => _current = 0f;

    public async UniTask LoadAsync(float targetProgress, string message)
    {
        var token = GetToken("LoadTask");
        var messageText = GetText((int)Texts.MessageText);

        if (messageText == null)
            return;

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
                int percent = Mathf.RoundToInt(_current * 100f);
                messageText.text = ZString.Format("{0} {1}%", message, percent);

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            _current = targetProgress;
            messageText.text = ZString.Format("{0} {1}%", message, Mathf.RoundToInt(targetProgress * 100f));
        }
        catch (OperationCanceledException)
        {
            // DESC ::: 새로운 값이 들어와 기존 애니메이션 중단하는 경우
        }
    }

    public async UniTask PlayAsync()
    {
        var image = GetImage((int)Images.RotateImage);
        var token = GetToken("RotateTask");

        if (image == null)
            return;

        while (!token.IsCancellationRequested)
        {
            image.transform.Rotate(0f, 0f, -300f * Time.unscaledDeltaTime);

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }
}
