using R3;
using System;

public class UIConfirmPopup : UIPopup, IDraggablePopup, IFocusablePopup
{
    private enum Images
    {
        ConfirmButtonImage,
        CancelButtonImage
    }

    private enum Texts
    {
        AlertText,
        MessageText,
        ConfirmButtonText,
        CancelButtonText
    }

    private enum Buttons
    {
        ConfirmButton,
        CancelButton
    }

    private readonly ReactiveProperty<ButtonState> _confirmButtonState = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _cancelButtonState = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private Action _onConfirm;
    private Action _onCancel;
    private LocalizationKey _cachedTitleKey;
    private Func<string> _messageProvider;

    public override void OnInit()
    {
        base.OnInit();
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        GetImage(Images.ConfirmButtonImage).BindState(_confirmButtonState, Define.Atlas.Common, this);
        GetImage(Images.CancelButtonImage).BindState(_cancelButtonState, Define.Atlas.Common, this);
        GetButton(Buttons.ConfirmButton).BindViewAsButton(_ => OnClickConfirm(), ViewEvent.LeftClick, this, _confirmButtonState);
        GetButton(Buttons.CancelButton).BindViewAsButton(_ => OnClickCancel(), ViewEvent.LeftClick, this, _cancelButtonState);
        Managers.Control.Subscribe(Literal.Hotkeys.Cancel, OnClickCancel).RegisterToPool(this);
    }

    public override void Refresh()
    {
        base.Refresh();
        GetText(Texts.AlertText).text = Managers.Localization.Get(_cachedTitleKey);
        GetText(Texts.MessageText).text = _messageProvider();
    }

    public void Setup(LocalizationKey titleKey, LocalizationKey messageKey, Action onConfirm, Action onCancel = null)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;
        _cachedTitleKey = titleKey;
        _messageProvider = () => Managers.Localization.Get(messageKey);
        Refresh();
    }

    public void Setup<T1>(LocalizationKey titleKey, LocalizationKey messageKey, Action onConfirm, Action onCancel, T1 arg1)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;
        _cachedTitleKey = titleKey;
        _messageProvider = () => Managers.Localization.Get(messageKey, arg1);
        Refresh();
    }

    public void Setup<T1, T2>(LocalizationKey titleKey, LocalizationKey messageKey, Action onConfirm, Action onCancel, T1 arg1, T2 arg2)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;
        _cachedTitleKey = titleKey;
        _messageProvider = () => Managers.Localization.Get(messageKey, arg1, arg2);
        Refresh();
    }

    public void Setup<T1, T2, T3>(LocalizationKey titleKey, LocalizationKey messageKey, Action onConfirm, Action onCancel, T1 arg1, T2 arg2, T3 arg3)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;
        _cachedTitleKey = titleKey;
        _messageProvider = () => Managers.Localization.Get(messageKey, arg1, arg2, arg3);
        Refresh();
    }

    public void Setup(LocalizationKey titleKey, LocalizationKey messageKey, Action onConfirm, Action onCancel = null, params object[] args)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;
        _cachedTitleKey = titleKey;
        _messageProvider = () => (args != null && args.Length > 0) ? Managers.Localization.Get(messageKey, args) : Managers.Localization.Get(messageKey);
        Refresh();
    }

    private void OnClickConfirm()
        => _onConfirm?.Invoke();

    private void OnClickCancel()
        => _onCancel?.Invoke();
}
