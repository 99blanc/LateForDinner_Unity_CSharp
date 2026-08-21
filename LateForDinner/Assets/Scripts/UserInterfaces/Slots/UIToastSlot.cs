using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class UIToastSlot : UISlot
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

    public override void Get()
    {
        base.Get();

        var canvasGroup = GetPanel((int)Panels.SlotPanel);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
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
            await canvasGroup.FadeAsync(0f, 1f, 0.15f, 1f, token);
            await UniTask.Delay(TimeSpan.FromSeconds(duration), ignoreTimeScale: true, cancellationToken: token);
            await canvasGroup.FadeAsync(1f, 0f, Define.Toast.Delay, 1f, token);
            onExpire?.Invoke();
        }
        catch (OperationCanceledException)
        {
            // DESC ::: 갱신되거나 풀로 반환되어 취소된 경우 예외 무시
        }
    }
}
