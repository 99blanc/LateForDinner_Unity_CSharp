using Cysharp.Threading.Tasks;
using R3;
using System;

public class UIAlertPopup : UIPopup, IDraggablePopup, IFocusablePopup
{
    private readonly ReactiveProperty<ButtonState> _confirmButtonState = new ReactiveProperty<ButtonState>(ButtonState.Normal);

    private enum Images
    {
        ConfirmButtonImage
    }

    private enum Texts
    {
        AlertText,
        MessageText,
        ConfirmButtonText
    }

    private enum Buttons
    {
        ConfirmButton
    }

    private Action _onConfirm;
    private LocalizationKey _cachedTitleKey;
    private Func<string> _messageProvider;

    public override void Init()
    {
        base.Init();
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        GetImage(Images.ConfirmButtonImage).BindState(_confirmButtonState, Define.Atlas.Common, this);
        GetButton(Buttons.ConfirmButton).BindViewAsButton(_ => OnClickConfirm(), ViewEvent.LeftClick, this, _confirmButtonState);
        Managers.Control.Subscribe(Literal.Hotkeys.Submit, OnClickConfirm).AddToPool(this);
    }

    public override void Refresh()
    {
        base.Refresh();
        GetText(Texts.AlertText).text = Managers.Localization.Get(_cachedTitleKey);
        GetText(Texts.MessageText).text = _messageProvider();
    }

    public void Setup(LocalizationKey titleKey, LocalizationKey messageKey, Action onConfirm = null)
    {
        _onConfirm = onConfirm;
        _cachedTitleKey = titleKey;
        _messageProvider = () => Managers.Localization.Get(messageKey);
        Refresh();
    }

    public void Setup<T1>(LocalizationKey titleKey, LocalizationKey messageKey, Action onConfirm, T1 arg1)
    {
        _onConfirm = onConfirm;
        _cachedTitleKey = titleKey;
        _messageProvider = () => Managers.Localization.Get(messageKey, arg1);
        Refresh();
    }

    public void Setup<T1, T2>(LocalizationKey titleKey, LocalizationKey messageKey, Action onConfirm, T1 arg1, T2 arg2)
    {
        _onConfirm = onConfirm;
        _cachedTitleKey = titleKey;
        _messageProvider = () => Managers.Localization.Get(messageKey, arg1, arg2);
        Refresh();
    }

    public void Setup<T1, T2, T3>(LocalizationKey titleKey, LocalizationKey messageKey, Action onConfirm, T1 arg1, T2 arg2, T3 arg3)
    {
        _onConfirm = onConfirm;
        _cachedTitleKey = titleKey;
        _messageProvider = () => Managers.Localization.Get(messageKey, arg1, arg2, arg3);
        Refresh();
    }

    public void Setup(LocalizationKey titleKey, LocalizationKey messageKey, Action onConfirm, params object[] args)
    {
        _onConfirm = onConfirm;
        _cachedTitleKey = titleKey;
        _messageProvider = () => (args != null && args.Length > 0) ? Managers.Localization.Get(messageKey, args) : Managers.Localization.Get(messageKey);
        Refresh();
    }

    private void OnClickConfirm()
        => _onConfirm?.Invoke();
}
