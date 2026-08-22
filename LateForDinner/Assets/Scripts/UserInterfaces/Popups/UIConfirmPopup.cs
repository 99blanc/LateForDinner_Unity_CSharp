using Cysharp.Threading.Tasks;
using R3;
using System;

public class UIConfirmPopup : UIPopup, IDraggable, IFocusable
{
    private readonly ReactiveProperty<ButtonState> _confirmButtonState = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _cancelButtonState = new ReactiveProperty<ButtonState>(ButtonState.Normal);

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

    private Action _onConfirm;
    private Action _onCancel;

    public override void Init()
    {
        base.Init();
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        GetImage((int)Images.ConfirmButtonImage).BindState(_confirmButtonState, Define.Atlas.Common, this);
        GetImage((int)Images.CancelButtonImage).BindState(_cancelButtonState, Define.Atlas.Common, this);
        GetButton((int)Buttons.ConfirmButton).BindViewAsButton(_ => OnClickConfirm(), ViewEvent.LeftClick, this, _confirmButtonState);
        GetButton((int)Buttons.CancelButton).BindViewAsButton(_ => OnClickCancel(), ViewEvent.LeftClick, this, _cancelButtonState);
        Managers.Control.Subscribe(Literal.Hotkeys.Cancel, OnClickCancel).AddTo(this);
    }

    public void Setup(string title, string message, Action onConfirm, Action onCancel = null)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;
        GetText((int)Texts.AlertText).text = title;
        GetText((int)Texts.MessageText).text = message;
    }

    private void OnClickConfirm()
    {
        Managers.UI.Close(this);
        _onConfirm?.Invoke();
    }

    private void OnClickCancel()
    {
        Managers.UI.Close(this);
        _onCancel?.Invoke();
    }
}
