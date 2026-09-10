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
    private Func<string> _titleProvider;
    private Func<string> _messageProvider;

    public override void OnInit()
    {
        base.OnInit();
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
    }

    public override void OnGet()
    {
        base.OnGet();
        GetImage(Images.ConfirmButtonImage).BindState(_confirmButtonState, Define.Atlas.Common, this);
        GetImage(Images.CancelButtonImage).BindState(_cancelButtonState, Define.Atlas.Common, this);
        GetButton(Buttons.ConfirmButton).BindViewAsButton(_ => OnClickConfirm(), ViewEvent.LeftClick, this, _confirmButtonState);
        GetButton(Buttons.CancelButton).BindViewAsButton(_ => OnClickCancel(), ViewEvent.LeftClick, this, _cancelButtonState);
        this.BindKey(Literal.Hotkeys.Cancel, InputEventType.Triggered, OnClickCancel).RegisterToPool(this);
    }

    public override void Refresh()
    {
        base.Refresh();
        GetText(Texts.AlertText).text = _titleProvider?.Invoke() ?? string.Empty;
        GetText(Texts.MessageText).text = _messageProvider?.Invoke() ?? string.Empty;
    }

    public override void OnRelease()
    {
        base.OnRelease();
        _titleProvider = null;
        _messageProvider = null;
        _onConfirm = null;
        _onCancel = null;
    }

    public void Setup(LocalizationKey titleKey, LocalizationKey messageKey, Action onConfirm, Action onCancel = null)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;
        _titleProvider = () => Managers.Localization.Get(titleKey);
        _messageProvider = () => Managers.Localization.Get(messageKey);
        Refresh();
    }

    public void Setup<T1>(LocalizationKey titleKey, LocalizationKey messageKey, Action onConfirm, Action onCancel, T1 arg1)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;
        _titleProvider = () => Managers.Localization.Get(titleKey);
        _messageProvider = () => Managers.Localization.Get(messageKey, arg1);
        Refresh();
    }

    public void Setup<T1, T2>(LocalizationKey titleKey, LocalizationKey messageKey, Action onConfirm, Action onCancel, T1 arg1, T2 arg2)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;
        _titleProvider = () => Managers.Localization.Get(titleKey);
        _messageProvider = () => Managers.Localization.Get(messageKey, arg1, arg2);
        Refresh();
    }

    public void Setup<T1, T2, T3>(LocalizationKey titleKey, LocalizationKey messageKey, Action onConfirm, Action onCancel, T1 arg1, T2 arg2, T3 arg3)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;
        _titleProvider = () => Managers.Localization.Get(titleKey);
        _messageProvider = () => Managers.Localization.Get(messageKey, arg1, arg2, arg3);
        Refresh();
    }

    public void Setup(LocalizationKey titleKey, LocalizationKey messageKey, Action onConfirm, Action onCancel = null, params object[] args)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;
        _titleProvider = () => Managers.Localization.Get(titleKey);
        _messageProvider = () => (args != null && args.Length > 0) ? Managers.Localization.Get(messageKey, args) : Managers.Localization.Get(messageKey);
        Refresh();
    }

    private void OnClickConfirm()
        => _onConfirm?.Invoke();

    private void OnClickCancel()
        => _onCancel?.Invoke();
}
