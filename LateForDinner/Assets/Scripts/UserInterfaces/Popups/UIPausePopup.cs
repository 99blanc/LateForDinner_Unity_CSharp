using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIPausePopup : UIPopup
{
    private enum Images
    {
        ContinueButtonImage,
        OptionButtonImage,
        TitleButtonImage
    }

    private enum Texts
    {
        ContinueButtonText,
        OptionButtonText,
        TitleButtonText,
    }

    private enum Buttons
    {
        ContinueButton,
        OptionButton,
        TitleButton,
    }

    private readonly ReactiveProperty<ButtonState> _continueButtonState = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _optionButtonState = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _titleButtonState = new ReactiveProperty<ButtonState>(ButtonState.Normal);

    public override void OnInit()
    {
        base.OnInit();
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindButtonStates();
        BindButtonActions();
    }

    public override void Refresh()
    {
        base.Refresh();
        InitStaticTexts();
    }

    private void BindButtonStates()
    {
        GetImage(Images.ContinueButtonImage).BindState(_continueButtonState, Define.Atlas.Common, this);
        GetImage(Images.OptionButtonImage).BindState(_optionButtonState, Define.Atlas.Common, this);
        GetImage(Images.TitleButtonImage).BindState(_titleButtonState, Define.Atlas.Common, this);
    }

    private void BindButtonActions()
    {
        GetButton(Buttons.ContinueButton).BindViewAsButton(OnClickContinue, ViewEvent.LeftClick, this, _continueButtonState);
        GetButton(Buttons.OptionButton).BindViewAsButton(OnClickOption, ViewEvent.LeftClick, this, _optionButtonState);
        GetButton(Buttons.TitleButton).BindViewAsButton(async (data) => await OnClickTitle(data), ViewEvent.LeftClick, this, _titleButtonState);
    }

    private void InitStaticTexts()
    {
        SetText(Texts.ContinueButtonText, LocalizationKey.UI_Pause_Popup_Text_Continue);
        SetText(Texts.OptionButtonText, LocalizationKey.UI_Pause_Popup_Text_Option);
        SetText(Texts.TitleButtonText, LocalizationKey.UI_Pause_Popup_Text_Title);
    }

    public override void OnGet()
    {
        base.OnGet();
        Refresh();
        Managers.Control.DisableActionMap(Literal.Maps.User);
        Managers.Game.Pause();
    }

    public override void OnRelease()
    {
        base.OnRelease();
        Managers.Control.EnableActionMap(Literal.Maps.User);
        Time.timeScale = 1f;
        Managers.Game.Resume();
    }

    private void OnClickContinue(PointerEventData data)
        => Close();

    private void OnClickOption(PointerEventData data)
        => Managers.UI.OpenPopup<UIOptionPopup>();

    private async UniTask OnClickTitle(PointerEventData data)
    {
        bool confirmed = await Managers.Notify.ConfirmAsync(this, LocalizationKey.UI_Pause_Popup_Confirm_Title, LocalizationKey.UI_Pause_Popup_Confirm_Desc);

        if (confirmed)
            await Managers.Game.TitleGameAsync();
    }

    private void SetText(Texts textEnum, LocalizationKey key)
        => GetText(textEnum).text = Managers.Localization.Get(key);
}
