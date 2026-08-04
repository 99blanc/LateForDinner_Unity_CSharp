using Cysharp.Threading.Tasks;

public class UIOptionPopup : UIPopup, IDraggable, IFocusable
{
    private enum Buttons
    {
        TabSoundButton,
        TabGraphicButton,
        TabAccessButton,
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

    private UI_OptionState _state = UI_OptionState.Sound;

    public override void Init()
    {
        base.Init();
        BindButton(typeof(Buttons));
        BindCanvasGroup(typeof(Panels));

        // 탭 버튼 바인딩
        //GetButton((int)Buttons.TabSoundButton).onClick.AddListener(() => Switch(UI_OptionState.Sound));
        //GetButton((int)Buttons.TabGraphicButton).onClick.AddListener(() => Switch(UI_OptionState.Graphic));
        //GetButton((int)Buttons.TabAccessButton).onClick.AddListener(() => Switch(UI_OptionState.Access));

        // 하단 공통 버튼 바인딩
        GetButton((int)Buttons.ApplyButton).onClick.AddListener(() => OnApplyClick().Forget());
        GetButton((int)Buttons.CompleteButton).onClick.AddListener(() => OnCompleteClick().Forget());
        GetButton((int)Buttons.CancelButton).onClick.AddListener(() => Close());
        GetButton((int)Buttons.DefaultButton).onClick.AddListener(() => OnDefaultClick().Forget());

        // 초기 탭 설정 및 UI 데이터 동기화
        //Switch(UI_OptionState.Sound);
        Refresh();
    }

    private void SwitchTab(UI_OptionState state)
    {
        _state = state;
        bool isSound = _state == UI_OptionState.Sound;
        bool isGraphic = _state == UI_OptionState.Graphic;
        bool isAccess = _state == UI_OptionState.Access;
        GetCanvasGroup((int)Panels.SoundPanel)?.SetActivePanel(isSound);
        GetCanvasGroup((int)Panels.GraphicPanel)?.SetActivePanel(isGraphic);
        GetCanvasGroup((int)Panels.AccessPanel)?.SetActivePanel(isAccess);
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

    private async UniTaskVoid OnApplyClick()
    {
        await Managers.UI.LockAsync(async () =>
        {
            Sync();
            await Managers.Config.SaveAsync();
        });
    }

    private async UniTaskVoid OnCompleteClick()
    {
        await Managers.UI.LockAsync(async () =>
        {
            Sync();
            await Managers.Config.SaveAsync();
            Close();
        });
    }

    private async UniTaskVoid OnDefaultClick()
    {
        await Managers.UI.LockAsync(async () =>
        {
            await Managers.Config.ResetAsync();
            Refresh();
        });
    }
}
