using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public static class TweenExtensions
{
    public static async UniTask FadeAsync(this Image image, float start, float end, float duration, float power = 1.0f, CancellationToken token = default)
    {
        float elapsedTime = 0f;
        Color color = image.color;
        color.a = start;
        image.color = color;

        while (elapsedTime < duration)
        {
            token.ThrowIfCancellationRequested();
            elapsedTime += Time.unscaledDeltaTime;
            float rawTime = Mathf.Clamp01(elapsedTime / duration);
            float normalizedTime = 1f - Mathf.Pow(1f - rawTime, power);
            color.a = Mathf.Lerp(start, end, normalizedTime);
            image.color = color;

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        color.a = end;
        image.color = color;
    }

    public static async UniTask FadeAsync(this CanvasGroup canvas, float start, float end, float duration, float power = 1.0f, CancellationToken token = default)
    {
        if (canvas == null) 
            return;

        canvas.blocksRaycasts = true;
        float elapsedTime = 0f;
        canvas.alpha = start;

        while (elapsedTime < duration)
        {
            token.ThrowIfCancellationRequested();
            elapsedTime += Time.unscaledDeltaTime;
            float rawTime = Mathf.Clamp01(elapsedTime / duration);
            float normalizedTime = 1f - Mathf.Pow(1f - rawTime, power);
            canvas.alpha = Mathf.Lerp(start, end, normalizedTime);

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        canvas.alpha = end;

        if (end <= 0f)
            canvas.blocksRaycasts = false;
    }
}
