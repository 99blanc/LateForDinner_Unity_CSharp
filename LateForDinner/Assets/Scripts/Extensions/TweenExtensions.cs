using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public static class TweenExtensions
{
    public static async UniTask<T> PlayAsync<T>(this UniTask<T> task) where T : UserInterface, IAnimatable
    {
        var display = await task;

        if (display != null)
            await display.PlayAsync();

        return display;
    }

    public static async UniTask<T> PlayAsync<T>(this T user) where T : UserInterface, IAnimatable
    {
        if (user != null)
            await user.PlayAsync();

        return user;
    }

    public static async UniTask FadeAsync(this Image image, float start, float end, float duration, float power = 1.0f, CancellationToken token = default)
    {
        if (image == null)
            return;

        SetImageAlpha(image, start);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            token.ThrowIfCancellationRequested();
            elapsedTime += Time.unscaledDeltaTime;
            float normalizedTime = CalculateNormalizedTime(elapsedTime, duration, power);
            SetImageAlpha(image, Mathf.Lerp(start, end, normalizedTime));
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        SetImageAlpha(image, end);
    }

    public static async UniTask FadeAsync(this CanvasGroup canvas, float start, float end, float duration, float power = 1.0f, CancellationToken token = default)
    {
        if (canvas == null)
            return;

        canvas.blocksRaycasts = true;
        canvas.alpha = start;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            token.ThrowIfCancellationRequested();
            elapsedTime += Time.unscaledDeltaTime;
            float normalizedTime = CalculateNormalizedTime(elapsedTime, duration, power);
            canvas.alpha = Mathf.Lerp(start, end, normalizedTime);
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        canvas.alpha = end;

        if (end <= 0f)
            canvas.blocksRaycasts = false;
    }

    public static async UniTask TogglePanelAsync(this CanvasGroup canvas, bool isActive, float duration = 0.2f, CancellationToken token = default)
    {
        if (canvas == null)
            return;

        if (isActive)
        {
            canvas.blocksRaycasts = true;
            await canvas.FadeAsync(canvas.alpha, 1f, duration, 1f, token);
            return;
        }

        await canvas.FadeAsync(canvas.alpha, 0f, duration, 1f, token);
        canvas.blocksRaycasts = false;
        canvas.interactable = false;
    }

    public static void SetActivePanel(this CanvasGroup canvas, bool isActive)
    {
        if (canvas == null)
            return;

        canvas.alpha = isActive ? 1f : 0f;
        canvas.interactable = isActive;
        canvas.blocksRaycasts = isActive;
    }

    private static float CalculateNormalizedTime(float elapsedTime, float duration, float power)
    {
        float rawTime = Mathf.Clamp01(elapsedTime / duration);
        return 1f - Mathf.Pow(1f - rawTime, power);
    }

    private static void SetImageAlpha(Image image, float alpha)
    {
        var color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
