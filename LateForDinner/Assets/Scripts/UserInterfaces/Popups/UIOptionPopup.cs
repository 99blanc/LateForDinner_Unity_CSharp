using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZLinq;

public class UIOptionPopup : UIPopup, IDraggable, IFocusable
{
    private readonly ReactiveProperty<ButtonState> _soundButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _graphicButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _accessButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _applyButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _completeButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _cancelButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _defaultButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _resolutionArrowButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _fullscreenArrowButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _qualityArrowButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);

    private enum Texts
    {
        SoundButtonText,
        GraphicButtonText,
        AccessButtonText,
        ApplyButtonText,
        CompleteButtonText,
        CancelButtonText,
        DefaultButtonText,
        // TODO ::: BoxText를 InputField로 변경 후 Scrollbar와 연동
        // DESC ::: SoundPanel
        MasterText,
        MasterBoxText,
        BGMText,
        BGMBoxText,
        AmbientText,
        AmbientBoxText,
        SFXText,
        SFXBoxText,
        UIText,
        UIBoxText,
        MuteText,
        // DESC ::: GraphicPanel
        ResolutionText,
        FullscreenText,
        QualityText,
        VsyncText,
        AntialiasingText,
        BloomText,
        AOText
    }

    private enum Images
    {
        SoundButtonImage,
        GraphicButtonImage,
        AccessButtonImage,
        ApplyButtonImage,
        CompleteButtonImage,
        CancelButtonImage,
        DefaultButtonImage,
        // DESC ::: SoundPanel
        MasterBoxImage,
        MasterToggleImage,
        MasterCheckmarkImage,
        BGMBoxImage,
        BGMToggleImage,
        BGMCheckmarkImage,
        AmbientBoxImage,
        AmbientToggleImage,
        AmbientCheckmarkImage,
        SFXBoxImage,
        SFXToggleImage,
        SFXCheckmarkImage,
        UIBoxImage,
        UIToggleImage,
        UICheckmarkImage,
        MuteToggleImage,
        MuteCheckmarkImage,
        // DESC ::: GraphicPanel
        ResolutionBoxImage,
        ResolutionArrowImage,
        ResolutionCheckmarkImage,
        FullscreenBoxImage,
        FullscreenArrowImage,
        FullscreenCheckmarkImage,
        QualityBoxImage,
        QualityArrowImage,
        QualityCheckmarkImage,
        VsyncToggleImage,
        VsyncCheckmarkImage,
        AntialiasingToggleImage,
        AntialiasingCheckmarkImage,
        BloomToggleImage,
        BloomCheckmarkImage,
        AOToggleImage,
        AOCheckmarkImage
    }

    private enum Buttons
    {
        SoundButton,
        GraphicButton,
        AccessButton,
        ApplyButton,
        CompleteButton,
        CancelButton,
        DefaultButton,
        ResolutionButton,
        FullscreenButton,
        QualityButton
    }

    private enum Toggles
    {
        // DESC ::: SoundPanel
        MasterToggle,
        BGMToggle,
        AmbientToggle,
        SFXToggle,
        UIToggle,
        MuteToggle,
        // DESC ::: GraphicPanel
        ResolutionToggle,
        FullscreenToggle,
        QualityToggle,
        VsyncToggle,
        AntialiasingToggle,
        BloomToggle,
        AOToggle
    }

    private enum ScrollRects
    {
        KeybindScrollRect
    }

    private enum Scrollbars
    {
        MasterScrollbar,
        BGMScrollbar,
        AmbientScrollbar,
        SFXScrollbar,
        UIScrollbar
    }

    private enum Dropdowns
    {
        // DESC ::: GraphicPanel
        ResolutionDropdown,
        FullscreenDropdown,
        QualityDropdown,
        // DESC ::: AccessPanel
        LanguageDropdown
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
    private Resolution[] _resolutions;
    private List<UIKeybindSlot> _keybinds = new List<UIKeybindSlot>();
    private string _initialKeybindJson;
    private bool _initialModifierDash;

    public override void Init()
    {
        base.Init();
        BindText(typeof(Texts));
        BindImage(typeof(Images));
        BindButton(typeof(Buttons));
        BindToggle(typeof(Toggles));
        BindScrollRect(typeof(ScrollRects));
        BindScrollbar(typeof(Scrollbars));
        BindDropdown(typeof(Dropdowns));
        BindPanel(typeof(Panels));
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
        GetText((int)Texts.SoundButtonText).text = Managers.Localization.Get(Localization.UI_Option_Popup_Text_Sound);
        GetText((int)Texts.GraphicButtonText).text = Managers.Localization.Get(Localization.UI_Option_Popup_Text_Graphic);
        GetText((int)Texts.AccessButtonText).text = Managers.Localization.Get(Localization.UI_Option_Popup_Text_Access);
        GetText((int)Texts.ApplyButtonText).text = Managers.Localization.Get(Localization.Apply);
        GetText((int)Texts.CompleteButtonText).text = Managers.Localization.Get(Localization.Complete);
        GetText((int)Texts.CancelButtonText).text = Managers.Localization.Get(Localization.Cancel);
        GetText((int)Texts.DefaultButtonText).text = Managers.Localization.Get(Localization.Default);
        Switch(UI_OptionState.Sound);
        // DESC ::: SoundPanel
        InitSoundPanel();
        // DESC ::: GraphicPanel
        InitGraphicPanel();
        // DESC ::: AccessPanel
        InitAccessPanel();
    }

    private void InitSoundPanel()
    {
        GetToggle((int)Toggles.MasterToggle).BindView(_ =>
        {
            bool isOn = GetToggle((int)Toggles.MasterToggle).isOn;
            UpdateCheckmark(GetImage((int)Images.MasterCheckmarkImage), isOn);
            GetImage((int)Images.MasterBoxImage).SetVisual(GetImage((int)Images.MasterToggleImage), GetScrollbar((int)Scrollbars.MasterScrollbar), isOn);
        }, ViewEvent.LeftClick, this);
        GetToggle((int)Toggles.BGMToggle).BindView(_ =>
        {
            bool isOn = GetToggle((int)Toggles.BGMToggle).isOn;
            UpdateCheckmark(GetImage((int)Images.BGMCheckmarkImage), isOn);
            GetImage((int)Images.BGMBoxImage).SetVisual(GetImage((int)Images.BGMToggleImage), GetScrollbar((int)Scrollbars.BGMScrollbar), isOn);
        }, ViewEvent.LeftClick, this);
        GetToggle((int)Toggles.AmbientToggle).BindView(_ =>
        {
            bool isOn = GetToggle((int)Toggles.AmbientToggle).isOn;
            UpdateCheckmark(GetImage((int)Images.AmbientCheckmarkImage), isOn);
            GetImage((int)Images.AmbientBoxImage).SetVisual(GetImage((int)Images.AmbientToggleImage), GetScrollbar((int)Scrollbars.AmbientScrollbar), isOn);
        }, ViewEvent.LeftClick, this);
        GetToggle((int)Toggles.SFXToggle).BindView(_ =>
        {
            bool isOn = GetToggle((int)Toggles.SFXToggle).isOn;
            UpdateCheckmark(GetImage((int)Images.SFXCheckmarkImage), isOn);
            GetImage((int)Images.SFXBoxImage).SetVisual(GetImage((int)Images.SFXToggleImage), GetScrollbar((int)Scrollbars.SFXScrollbar), isOn);
        }, ViewEvent.LeftClick, this);
        GetToggle((int)Toggles.UIToggle).BindView(_ =>
        {
            bool isOn = GetToggle((int)Toggles.UIToggle).isOn;
            UpdateCheckmark(GetImage((int)Images.UICheckmarkImage), isOn);
            GetImage((int)Images.UIBoxImage).SetVisual(GetImage((int)Images.UIToggleImage), GetScrollbar((int)Scrollbars.UIScrollbar), isOn);
        }, ViewEvent.LeftClick, this);
        GetToggle((int)Toggles.MuteToggle).BindView(_ =>
        {
            bool isOn = GetToggle((int)Toggles.MuteToggle).isOn;
            UpdateCheckmark(GetImage((int)Images.MuteCheckmarkImage), isOn);
            GetImage((int)Images.MuteToggleImage).SetVisual(null, null, isOn);
        }, ViewEvent.LeftClick, this);
        GetScrollbar((int)Scrollbars.MasterScrollbar).BindScrollbar(val => UpdateVolume(GetText((int)Texts.MasterBoxText), val), this);
        GetScrollbar((int)Scrollbars.BGMScrollbar).BindScrollbar(val => UpdateVolume(GetText((int)Texts.BGMBoxText), val), this);
        GetScrollbar((int)Scrollbars.AmbientScrollbar).BindScrollbar(val => UpdateVolume(GetText((int)Texts.AmbientBoxText), val), this);
        GetScrollbar((int)Scrollbars.SFXScrollbar).BindScrollbar(val => UpdateVolume(GetText((int)Texts.SFXBoxText), val), this);
        GetScrollbar((int)Scrollbars.UIScrollbar).BindScrollbar(val => UpdateVolume(GetText((int)Texts.UIBoxText), val), this);
        GetText((int)Texts.MasterText).text = Managers.Localization.Get(Localization.UI_Option_Popup_Text_Master);
        GetText((int)Texts.BGMText).text = Managers.Localization.Get(Localization.UI_Option_Popup_Text_BGM);
        GetText((int)Texts.AmbientText).text = Managers.Localization.Get(Localization.UI_Option_Popup_Text_Ambient);
        GetText((int)Texts.SFXText).text = Managers.Localization.Get(Localization.UI_Option_Popup_Text_SFX);
        GetText((int)Texts.UIText).text = Managers.Localization.Get(Localization.UI_Option_Popup_Text_UI);
        GetText((int)Texts.MuteText).text = Managers.Localization.Get(Localization.UI_Option_Popup_Text_Mute);
    }

    private void InitGraphicPanel()
    {
        InitResolution();
        GetImage((int)Images.ResolutionArrowImage).BindStateAsArrow(_resolutionArrowButton, Define.Atlas.UI_Common, this);
        GetImage((int)Images.FullscreenArrowImage).BindStateAsArrow(_fullscreenArrowButton, Define.Atlas.UI_Common, this);
        GetImage((int)Images.QualityArrowImage).BindStateAsArrow(_qualityArrowButton, Define.Atlas.UI_Common, this);
        var resolutionDropdown = GetDropdown((int)Dropdowns.ResolutionDropdown);
        var fullscreenDropdown = GetDropdown((int)Dropdowns.FullscreenDropdown);
        var qualityDropdown = GetDropdown((int)Dropdowns.QualityDropdown);
        GetButton((int)Buttons.ResolutionButton).BindViewAsButton(_ => { }, ViewEvent.LeftClick, this, _resolutionArrowButton);
        GetButton((int)Buttons.FullscreenButton).BindViewAsButton(_ => { }, ViewEvent.LeftClick, this, _fullscreenArrowButton);
        GetButton((int)Buttons.QualityButton).BindViewAsButton(_ => { }, ViewEvent.LeftClick, this, _qualityArrowButton);
        fullscreenDropdown.ClearOptions();
        fullscreenDropdown.AddOptions(new List<string> 
        { 
            Managers.Localization.Get(Localization.UI_Option_Popup_Text_Fullscreen_FullscreenWindow), 
            Managers.Localization.Get(Localization.UI_Option_Popup_Text_Fullscreen_Windowed), 
            Managers.Localization.Get(Localization.UI_Option_Popup_Text_Fullscreen_ExclusiveFullscreen) 
        });
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string> 
        { 
            Managers.Localization.Get(Localization.UI_Option_Popup_Text_Quality_Low), 
            Managers.Localization.Get(Localization.UI_Option_Popup_Text_Quality_Medium), 
            Managers.Localization.Get(Localization.UI_Option_Popup_Text_Quality_High) 
        });
        GetToggle((int)Toggles.VsyncToggle).BindView(_ =>
        {
            bool isOn = GetToggle((int)Toggles.VsyncToggle).isOn;
            UpdateCheckmark(GetImage((int)Images.VsyncCheckmarkImage), isOn);
            GetImage((int)Images.VsyncToggleImage).SetVisual(isEnabled: isOn);
        }, ViewEvent.LeftClick, this);
        GetToggle((int)Toggles.AntialiasingToggle).BindView(_ =>
        {
            bool isOn = GetToggle((int)Toggles.AntialiasingToggle).isOn;
            UpdateCheckmark(GetImage((int)Images.AntialiasingCheckmarkImage), isOn);
            GetImage((int)Images.AntialiasingToggleImage).SetVisual(isEnabled: isOn);
        }, ViewEvent.LeftClick, this);
        GetToggle((int)Toggles.BloomToggle).BindView(_ =>
        {
            bool isOn = GetToggle((int)Toggles.BloomToggle).isOn;
            UpdateCheckmark(GetImage((int)Images.BloomCheckmarkImage), isOn);
            GetImage((int)Images.BloomToggleImage).SetVisual(isEnabled: isOn);
        }, ViewEvent.LeftClick, this);
        GetToggle((int)Toggles.AOToggle).BindView(_ =>
        {
            bool isOn = GetToggle((int)Toggles.AOToggle).isOn;
            UpdateCheckmark(GetImage((int)Images.AOCheckmarkImage), isOn);
            GetImage((int)Images.AOToggleImage).SetVisual(isEnabled: isOn);
        }, ViewEvent.LeftClick, this);
        GetText((int)Texts.ResolutionText).text = Managers.Localization.Get(Localization.UI_Option_Popup_Text_Resolution);
        GetText((int)Texts.FullscreenText).text = Managers.Localization.Get(Localization.UI_Option_Popup_Text_Fullscreen);
        GetText((int)Texts.QualityText).text = Managers.Localization.Get(Localization.UI_Option_Popup_Text_Quality);
        GetText((int)Texts.VsyncText).text = Managers.Localization.Get(Localization.UI_Option_Popup_Text_Vsync);
        GetText((int)Texts.AntialiasingText).text = Managers.Localization.Get(Localization.UI_Option_Popup_Text_Antialiasing);
        GetText((int)Texts.BloomText).text = Managers.Localization.Get(Localization.UI_Option_Popup_Text_Bloom);
        GetText((int)Texts.AOText).text = Managers.Localization.Get(Localization.UI_Option_Popup_Text_AO);
    }

    private void InitAccessPanel()
    {
        var languageDropdown = GetDropdown((int)Dropdowns.LanguageDropdown);

        if (languageDropdown != null)
        {
            languageDropdown.ClearOptions();
            List<string> languages = Managers.Localization.GetLanguages();
            List<string> displayOptions = new List<string>();

            foreach (var locale in languages)
                displayOptions.Add(locale.ToNative());

            languageDropdown.AddOptions(displayOptions);
        }

        var content = GetScrollRect((int)ScrollRects.KeybindScrollRect).content;
        var (dashSlot, dashRentHandle) = Managers.Pool.Pop<UIKeybindSlot>(content);
        _keybinds.Add(dashSlot);
        dashSlot.SetupDashCommand((name, json) => { });
        
        // TODO ::: 입력 키 중복 불가 처리
        foreach (var action in Managers.Control.GetBindableActions())
        {
            var (slot, rentHandle) = Managers.Pool.Pop<UIKeybindSlot>(content);
            _keybinds.Add(slot);
            string actionName = action.name;
            slot.Setup(actionName, action, (name, json) => { });
        }
    }

    private void InitResolution()
    {
        var resolutionDropdown = GetDropdown((int)Dropdowns.ResolutionDropdown);
        resolutionDropdown.ClearOptions();
        _resolutions = Screen.resolutions
        .Select(r => new Resolution
        {
            width = r.width,
            height = r.height,
            refreshRateRatio = r.refreshRateRatio
        })
        .GroupBy(r => new {
            r.width,
            r.height,
            hz = Mathf.RoundToInt((float)r.refreshRateRatio.numerator / r.refreshRateRatio.denominator)
        })
        .Select(g => g.First())
        .OrderBy(r => r.width)
        .ThenBy(r => r.height)
        .ThenBy(r => (double)r.refreshRateRatio.numerator / r.refreshRateRatio.denominator)
        .ToArray();
        List<string> resolutionOptions = new List<string>();

        foreach (var res in _resolutions)
        {
            int hz = Mathf.RoundToInt((float)res.refreshRateRatio.numerator / res.refreshRateRatio.denominator);
            resolutionOptions.Add(Managers.Localization.Get(Localization.UI_Option_Popup_Text_Resolution_Dropdown, res.width, res.height, hz));
        }

        resolutionDropdown.AddOptions(resolutionOptions);
    }

    public override void Get()
    {
        _initialKeybindJson = Managers.Config.Option.Access.keybind;
        _initialModifierDash = Managers.Config.Option.Access.modifierDash;

        if (!string.IsNullOrEmpty(_initialKeybindJson))
            Managers.Control.LoadBindingFromJson(_initialKeybindJson);

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
        GetPanel((int)Panels.SoundPanel).SetActivePanel(isSound);
        GetPanel((int)Panels.GraphicPanel).SetActivePanel(isGraphic);
        GetPanel((int)Panels.AccessPanel).SetActivePanel(isAccess);
        UpdateTab();
    }

    private void UpdateTab()
    {
        _soundButton.Value = (_state == UI_OptionState.Sound) ? ButtonState.Disable : ButtonState.Normal;
        _graphicButton.Value = (_state == UI_OptionState.Graphic) ? ButtonState.Disable : ButtonState.Normal;
        _accessButton.Value = (_state == UI_OptionState.Access) ? ButtonState.Disable     : ButtonState.Normal;
    }

    private void UpdateVolume(TMP_Text text, float value)
    {
        int percent = Mathf.RoundToInt(value * 100f);
        text.text = percent.ToString();
    }

    private void UpdateCheckmark(Image image, bool isOn)
    {
        if (image == null)
            return;

        string sprite = isOn ? Define.Sprite.Checkmark_Yes : Define.Sprite.Checkmark_No;
        image.sprite = Managers.Resource.GetSprite(Define.Atlas.UI_Common, sprite);
    }

    private void Refresh()
    {
        var option = Managers.Config.Option;
        // DESC :: SoundPanel
        GetScrollbar((int)Scrollbars.MasterScrollbar).value = option.Sound.vMaster;
        GetScrollbar((int)Scrollbars.BGMScrollbar).value = option.Sound.vBGM;
        GetScrollbar((int)Scrollbars.AmbientScrollbar).value = option.Sound.vAmbient;
        GetScrollbar((int)Scrollbars.SFXScrollbar).value = option.Sound.vSFX;
        GetScrollbar((int)Scrollbars.UIScrollbar).value = option.Sound.vUI;
        UpdateVolume(GetText((int)Texts.MasterBoxText), option.Sound.vMaster);
        UpdateVolume(GetText((int)Texts.BGMBoxText), option.Sound.vBGM);
        UpdateVolume(GetText((int)Texts.AmbientBoxText), option.Sound.vAmbient);
        UpdateVolume(GetText((int)Texts.SFXBoxText), option.Sound.vSFX);
        UpdateVolume(GetText((int)Texts.UIBoxText), option.Sound.vUI);
        GetToggle((int)Toggles.MasterToggle).isOn = option.Sound.mMaster;
        GetToggle((int)Toggles.BGMToggle).isOn = option.Sound.mBGM;
        GetToggle((int)Toggles.AmbientToggle).isOn = option.Sound.mAmbient;
        GetToggle((int)Toggles.SFXToggle).isOn = option.Sound.mSFX;
        GetToggle((int)Toggles.UIToggle).isOn = option.Sound.mUI;
        GetToggle((int)Toggles.MuteToggle).isOn = option.Sound.mute;
        GetImage((int)Images.MasterBoxImage).SetVisual(GetImage((int)Images.MasterToggleImage), GetScrollbar((int)Scrollbars.MasterScrollbar), option.Sound.mMaster);
        GetImage((int)Images.BGMBoxImage).SetVisual(GetImage((int)Images.BGMToggleImage), GetScrollbar((int)Scrollbars.BGMScrollbar), option.Sound.mBGM);
        GetImage((int)Images.AmbientBoxImage).SetVisual(GetImage((int)Images.AmbientToggleImage), GetScrollbar((int)Scrollbars.AmbientScrollbar), option.Sound.mAmbient);
        GetImage((int)Images.SFXBoxImage).SetVisual(GetImage((int)Images.SFXToggleImage), GetScrollbar((int)Scrollbars.SFXScrollbar), option.Sound.mSFX);
        GetImage((int)Images.UIBoxImage).SetVisual(GetImage((int)Images.UIToggleImage), GetScrollbar((int)Scrollbars.UIScrollbar), option.Sound.mUI);
        UpdateCheckmark(GetImage((int)Images.MasterCheckmarkImage), option.Sound.mMaster);
        UpdateCheckmark(GetImage((int)Images.BGMCheckmarkImage), option.Sound.mBGM);
        UpdateCheckmark(GetImage((int)Images.AmbientCheckmarkImage), option.Sound.mAmbient);
        UpdateCheckmark(GetImage((int)Images.SFXCheckmarkImage), option.Sound.mSFX);
        UpdateCheckmark(GetImage((int)Images.UICheckmarkImage), option.Sound.mUI);
        UpdateCheckmark(GetImage((int)Images.MuteCheckmarkImage), option.Sound.mute);

        // DESC :: GraphicPanel
        for (int index = 0; index < _resolutions.Length; index++)
        {
            int hz = Mathf.RoundToInt((float)_resolutions[index].refreshRateRatio.numerator / _resolutions[index].refreshRateRatio.denominator);

            if (_resolutions[index].width == option.Graphic.rWidth && _resolutions[index].height == option.Graphic.rHeight && hz == option.Graphic.rRefreshRate)
            {
                GetDropdown((int)Dropdowns.ResolutionDropdown).value = index;
                break;
            }
        }

        GetDropdown((int)Dropdowns.FullscreenDropdown).value = option.Graphic.screenMode switch
        {
            FullScreenMode.FullScreenWindow => 0,
            FullScreenMode.Windowed => 1,
            FullScreenMode.ExclusiveFullScreen => 2,
            _ => 0
        };
        GetDropdown((int)Dropdowns.QualityDropdown).value = (int)option.Graphic.quality;
        GetToggle((int)Toggles.VsyncToggle).isOn = option.Graphic.vSync;
        GetToggle((int)Toggles.AntialiasingToggle).isOn = option.Graphic.antiAliasing;
        GetToggle((int)Toggles.BloomToggle).isOn = option.Graphic.bloom;
        GetToggle((int)Toggles.AOToggle).isOn = option.Graphic.ambientOccusion;
        GetImage((int)Images.VsyncToggleImage).SetVisual(isEnabled: option.Graphic.vSync);
        GetImage((int)Images.AntialiasingToggleImage).SetVisual(isEnabled: option.Graphic.antiAliasing);
        GetImage((int)Images.BloomToggleImage).SetVisual(isEnabled: option.Graphic.bloom);
        GetImage((int)Images.AOToggleImage).SetVisual(isEnabled: option.Graphic.ambientOccusion);
        UpdateCheckmark(GetImage((int)Images.VsyncCheckmarkImage), option.Graphic.vSync);
        UpdateCheckmark(GetImage((int)Images.AntialiasingCheckmarkImage), option.Graphic.antiAliasing);
        UpdateCheckmark(GetImage((int)Images.BloomCheckmarkImage), option.Graphic.bloom);
        UpdateCheckmark(GetImage((int)Images.AOCheckmarkImage), option.Graphic.ambientOccusion);
        // DESC ::: AccessPanel
        var languageLocales = Managers.Localization.GetLanguages();
        string currentLocale = Managers.Config?.Option?.Access?.language ?? Literal.Languages.Korean;

        for (int index = 0; index < languageLocales.Count; index++)
        {
            if (languageLocales[index].Equals(currentLocale, StringComparison.OrdinalIgnoreCase))
            {
                GetDropdown((int)Dropdowns.LanguageDropdown).value = index;
                break;
            }
        }

        foreach (var slot in _keybinds)
            slot.Refresh();
    }

    private void Sync()
    {
        var option = Managers.Config.Option;
        // DESC :: SoundPanel
        option.Sound.vMaster = GetScrollbar((int)Scrollbars.MasterScrollbar).value;
        option.Sound.vBGM = GetScrollbar((int)Scrollbars.BGMScrollbar).value;
        option.Sound.vAmbient = GetScrollbar((int)Scrollbars.AmbientScrollbar).value;
        option.Sound.vSFX = GetScrollbar((int)Scrollbars.SFXScrollbar).value;
        option.Sound.vUI = GetScrollbar((int)Scrollbars.UIScrollbar).value;
        option.Sound.mMaster = GetToggle((int)Toggles.MasterToggle).isOn;
        option.Sound.mBGM = GetToggle((int)Toggles.BGMToggle).isOn;
        option.Sound.mAmbient = GetToggle((int)Toggles.AmbientToggle).isOn;
        option.Sound.mSFX = GetToggle((int)Toggles.SFXToggle).isOn;
        option.Sound.mUI = GetToggle((int)Toggles.UIToggle).isOn;
        option.Sound.mute = GetToggle((int)Toggles.MuteToggle).isOn;
        // DESC :: GraphicPanel
        int resIndex = GetDropdown((int)Dropdowns.ResolutionDropdown).value;

        if (_resolutions != null && resIndex < _resolutions.Length)
        {
            var selectedRes = _resolutions[resIndex];
            option.Graphic.rWidth = _resolutions[resIndex].width;
            option.Graphic.rHeight = _resolutions[resIndex].height;
            option.Graphic.rRefreshRate = Mathf.RoundToInt((float)selectedRes.refreshRateRatio.numerator / selectedRes.refreshRateRatio.denominator);
        }

        option.Graphic.screenMode = GetDropdown((int)Dropdowns.FullscreenDropdown).value switch
        {
            0 => FullScreenMode.FullScreenWindow,
            1 => FullScreenMode.Windowed,
            2 => FullScreenMode.ExclusiveFullScreen,
            _ => FullScreenMode.FullScreenWindow
        };
        option.Graphic.quality = (Quality)GetDropdown((int)Dropdowns.QualityDropdown).value;
        option.Graphic.vSync = GetToggle((int)Toggles.VsyncToggle).isOn;
        option.Graphic.antiAliasing = GetToggle((int)Toggles.AntialiasingToggle).isOn;
        option.Graphic.bloom = GetToggle((int)Toggles.BloomToggle).isOn;
        option.Graphic.ambientOccusion = GetToggle((int)Toggles.AOToggle).isOn;
        // DESC ::: AccessPanel
        int langIndex = GetDropdown((int)Dropdowns.LanguageDropdown).value;
        var languageLocales = Managers.Localization.GetLanguages();

        if (languageLocales != null && langIndex < languageLocales.Count)
            Managers.Config.Option.Access.language = languageLocales[langIndex];

        option.Access.keybind = Managers.Control.Save();
    }

    private async UniTask OnApplyClicked(PointerEventData data)
    {
        Sync();

        await Managers.Config.SaveAsync().Lock();
    }

    private async UniTask OnCompleteClicked(PointerEventData data)
    {
        Sync();

        await Managers.Config.SaveAsync().Lock();

        Release();
    }

    private void OnCancelClicked(PointerEventData data)
    {
        if (!string.IsNullOrEmpty(_initialKeybindJson))
            Managers.Control.LoadBindingFromJson(_initialKeybindJson);

        Managers.Config.Option.Access.modifierDash = _initialModifierDash;

        foreach (var slot in _keybinds)
            slot.Refresh();

        Release();
    }

    private async UniTask OnDefaultClick(PointerEventData data)
    {
        await Managers.Config.ResetAsync().Lock();

        Managers.Control.Reset();
        Managers.Config.Option.Access.modifierDash = AccessOption.Default.modifierDash;
        Refresh();
    }
}
