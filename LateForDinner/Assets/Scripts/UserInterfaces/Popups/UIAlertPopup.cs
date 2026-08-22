using Cysharp.Threading.Tasks;
using R3;
using System;

public class UIAlertPopup : UIPopup, IDraggable, IFocusable
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

    public override void Init()
    {
        base.Init();
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        GetImage((int)Images.ConfirmButtonImage).BindState(_confirmButtonState, Define.Atlas.Common, this);
        GetButton((int)Buttons.ConfirmButton).BindViewAsButton(_ => OnClickConfirm(), ViewEvent.LeftClick, this, _confirmButtonState);
        Managers.Control.Subscribe(Literal.Hotkeys.Submit, OnClickConfirm).AddTo(this);
    }

    public void Setup(string title, string message, Action onConfirm = null)
    {
        _onConfirm = onConfirm;
        GetText((int)Texts.AlertText).text = title;
        GetText((int)Texts.MessageText).text = message;
    }

    private void OnClickConfirm()
        => _onConfirm?.Invoke();
}
