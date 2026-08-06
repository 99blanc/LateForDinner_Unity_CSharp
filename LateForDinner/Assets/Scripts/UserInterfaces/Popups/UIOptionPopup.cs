using Cysharp.Threading.Tasks;
using R3;
using UnityEngine.EventSystems;

public class UIOptionPopup : UIPopup, IDraggable, IFocusable
{
    private readonly ReactiveProperty<ButtonState> _soundButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _graphicButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _accessButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _applyButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _completeButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _cancelButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _defaultButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);

    private enum Images
    {
        SoundButtonImage,
        GraphicButtonImage,
        AccessButtonImage,
        ApplyButtonImage,
        CompleteButtonImage,
        CancelButtonImage,
        DefaultButtonImage
    }

    private enum Buttons
    {
        SoundButton,
        GraphicButton,
        AccessButton,
        ApplyButton,
        CompleteButton,
        CancelButton,
        DefaultButton
    }

    private enum Panels
    {
        SoundPanel,
        GraphicPanel,
        AccessPanel
    }

    private enum UI_OptionState
    {
        Sound,
        Graphic,
        Access
    }

    private UI_OptionState _state;

    public override void Init()
    {
        base.Init();
        BindButton(typeof(Buttons));
        BindImage(typeof(Images));
        BindCanvasGroup(typeof(Panels));
        GetImage((int)Images.SoundButtonImage).BindState(_soundButton, Define.Atlas.UI_Common, this);
        GetImage((int)Images.GraphicButtonImage).BindState(_graphicButton, Define.Atlas.UI_Common, this);
        GetImage((int)Images.AccessButtonImage).BindState(_accessButton, Define.Atlas.UI_Common, this);
        GetImage((int)Images.ApplyButtonImage).BindState(_applyButton, Define.Atlas.UI_Common, this);
        GetImage((int)Images.CompleteButtonImage).BindState(_completeButton, Define.Atlas.UI_Common, this);
        GetImage((int)Images.CancelButtonImage).BindState(_cancelButton, Define.Atlas.UI_Common, this);
        GetImage((int)Images.DefaultButtonImage).BindState(_defaultButton, Define.Atlas.UI_Common, this);
        GetButton((int)Buttons.SoundButton).BindViewAsButton(OnSoundClicked, ViewEvent.LeftClick, this, _soundButton);
        GetButton((int)Buttons.GraphicButton).BindViewAsButton(OnGraphicClicked, ViewEvent.LeftClick, this, _graphicButton);
        GetButton((int)Buttons.AccessButton).BindViewAsButton(OnAccessClicked, ViewEvent.LeftClick, this, _accessButton);
        GetButton((int)Buttons.ApplyButton).BindViewAsButton(async (data) => await OnApplyClicked(data), ViewEvent.LeftClick, this, _applyButton);
        GetButton((int)Buttons.CompleteButton).BindViewAsButton(async (data) => await OnCompleteClicked(data), ViewEvent.LeftClick, this, _completeButton);
        GetButton((int)Buttons.CancelButton).BindViewAsButton(OnCancelClicked, ViewEvent.LeftClick, this, _cancelButton);
        GetButton((int)Buttons.DefaultButton).BindViewAsButton(async (data) => await OnDefaultClick(data), ViewEvent.LeftClick, this, _defaultButton);
        Switch(UI_OptionState.Sound);
    }

    public override void Get()
    {
        Switch(_state);
        Refresh();
    }

    private void OnSoundClicked(PointerEventData data)
    {
        if (_state != UI_OptionState.Sound)
            Switch(UI_OptionState.Sound);
    }

    private void OnGraphicClicked(PointerEventData data)
    {
        if (_state != UI_OptionState.Graphic)
            Switch(UI_OptionState.Graphic);
    }

    private void OnAccessClicked(PointerEventData data)
    {
        if (_state != UI_OptionState.Access)
            Switch(UI_OptionState.Access);
    }

    private void Switch(UI_OptionState state)
    {
        _state = state;
        bool isSound = _state == UI_OptionState.Sound;
        bool isGraphic = _state == UI_OptionState.Graphic;
        bool isAccess = _state == UI_OptionState.Access;
        GetCanvasGroup((int)Panels.SoundPanel).SetActivePanel(isSound);
        GetCanvasGroup((int)Panels.GraphicPanel).SetActivePanel(isGraphic);
        GetCanvasGroup((int)Panels.AccessPanel).SetActivePanel(isAccess);
        UpdateTab();
    }

    private void UpdateTab()
    {
        _soundButton.Value = (_state == UI_OptionState.Sound) ? ButtonState.Press : ButtonState.Normal;
        _graphicButton.Value = (_state == UI_OptionState.Graphic) ? ButtonState.Press : ButtonState.Normal;
        _accessButton.Value = (_state == UI_OptionState.Access) ? ButtonState.Press : ButtonState.Normal;
    }

    private void Refresh()
    {
        var option = Managers.Config.Option;

        // TODO ::: Managers.Config.Option 안의 SoundOption, GraphicOption, AccessOption 값을 
        // 각 패널 내의 슬라이더, 토글, 드롭다운 등에 반영
    }

    private void Sync()
    {
        var option = Managers.Config.Option;

        // TODO ::: 유저가 팝업 내에서 변경한 UI 값을 Managers.Config.Option 에 거꾸로 반영
    }

    private async UniTask OnApplyClicked(PointerEventData data)
    {
        await Managers.Config.SaveAsync().Lock();

        Sync();
    }

    private async UniTask OnCompleteClicked(PointerEventData data)
    {
        await Managers.Config.SaveAsync().Lock();

        Sync();
        Release();
    }

    private void OnCancelClicked(PointerEventData data)
        => Release();

    private async UniTask OnDefaultClick(PointerEventData data)
    {
        await Managers.Config.ResetAsync().Lock();

        Refresh();
    }
}
