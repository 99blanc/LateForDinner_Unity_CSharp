using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class UIAlertSlot : UISlot
{
    private enum Texts
    {
        MessageText
    }

    private enum Panels
    {
        SlotPanel
    }

    public override void Init()
    {
        base.Init();
        BindText(typeof(Texts));
        BindPanel(typeof(Panels));
    }

    public void Setup(string message, Action onExpire)
    {
        SetMessageText(message);
        var token = GetToken("AlertExpireTask");
        FadeAndExpireAsync(3f, onExpire, token).Forget();
    }

    private void SetMessageText(string message)
    {
        var textComponent = GetText((int)Texts.MessageText);

        if (textComponent != null)
            textComponent.text = message;
    }

    private async UniTaskVoid FadeAndExpireAsync(float duration, Action onExpire, CancellationToken token)
    {
        var canvasGroup = GetPanel((int)Panels.SlotPanel);

        try
        {
            if (canvasGroup != null)
                await canvasGroup.FadeAsync(0f, 1f, 0.15f, 1f, token);

            await UniTask.Delay(TimeSpan.FromSeconds(duration), ignoreTimeScale: true, cancellationToken: token);

            if (canvasGroup != null)
                await canvasGroup.FadeAsync(1f, 0f, 0.2f, 1f, token);

            onExpire?.Invoke();
        }
        catch (OperationCanceledException)
        {
            // DESC ::: 갱신되거나 풀로 반환되어 취소된 경우 예외 무시
        }
    }
}
