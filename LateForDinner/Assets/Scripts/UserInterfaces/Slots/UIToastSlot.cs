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

    private Func<string> _messageProvider;

    public override void Init()
    {
        base.Init();
        BindText(typeof(Texts));
        BindPanel(typeof(Panels));
    }

    public override void Get()
    {
        base.Get();
        GetPanel(Panels.SlotPanel).alpha = 0f;
    }

    public override void Refresh()
    {
        base.Refresh();

        if (_messageProvider != null)
            GetText(Texts.MessageText).text = _messageProvider();
    }

    public void Setup(LocalizationKey messageKey, Action onExpire)
    {
        _messageProvider = () => Managers.Localization.Get(messageKey);
        Refresh();
        var token = GetToken("AlertExpireTask");
        FadeAndExpireAsync(3f, onExpire, token).Forget();
    }

    public void Setup<T1>(LocalizationKey messageKey, Action onExpire, T1 arg1)
    {
        _messageProvider = () => Managers.Localization.Get(messageKey, arg1);
        Refresh();
        var token = GetToken("AlertExpireTask");
        FadeAndExpireAsync(3f, onExpire, token).Forget();
    }

    public void Setup<T1, T2>(LocalizationKey messageKey, Action onExpire, T1 arg1, T2 arg2)
    {
        _messageProvider = () => Managers.Localization.Get(messageKey, arg1, arg2);
        Refresh();
        var token = GetToken("AlertExpireTask");
        FadeAndExpireAsync(3f, onExpire, token).Forget();
    }

    public void Setup<T1, T2, T3>(LocalizationKey messageKey, Action onExpire, T1 arg1, T2 arg2, T3 arg3)
    {
        _messageProvider = () => Managers.Localization.Get(messageKey, arg1, arg2, arg3);
        Refresh();
        var token = GetToken("AlertExpireTask");
        FadeAndExpireAsync(3f, onExpire, token).Forget();
    }

    public void Setup(LocalizationKey messageKey, Action onExpire, params object[] args)
    {
        _messageProvider = () => (args != null && args.Length > 0) ? Managers.Localization.Get(messageKey, args) : Managers.Localization.Get(messageKey);
        Refresh();
        var token = GetToken("AlertExpireTask");
        FadeAndExpireAsync(3f, onExpire, token).Forget();
    }

    private async UniTaskVoid FadeAndExpireAsync(float duration, Action onExpire, CancellationToken token)
    {
        var canvasGroup = GetPanel(Panels.SlotPanel);

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
