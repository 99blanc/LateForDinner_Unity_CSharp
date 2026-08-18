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
    private readonly ReactiveProperty<ButtonState> _languageArrowButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);

    private enum Images
    {
        SoundButtonImage,
        GraphicButtonImage,
        AccessButtonImage,
        ApplyButtonImage,
        CompleteButtonImage,
        CancelButtonImage,
        DefaultButtonImage,
        MasterInputImage,
        MasterToggleImage,
        MasterCheckmarkImage,
        BGMInputImage,
        BGMToggleImage,
        BGMCheckmarkImage,
        AmbientInputImage,
        AmbientToggleImage,
        AmbientCheckmarkImage,
        SFXInputImage,
        SFXToggleImage,
        SFXCheckmarkImage,
        UIInputImage,
        UIToggleImage,
        UICheckmarkImage,
        MuteToggleImage,
        MuteCheckmarkImage,
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
        AOCheckmarkImage,
        LanguageArrowImage
    }

    private enum Texts
    {
        SoundButtonText,
        GraphicButtonText,
        AccessButtonText,
        ApplyButtonText,
        CompleteButtonText,
        CancelButtonText,
        DefaultButtonText,
        MasterText,
        MasterInputText,
        BGMText,
        BGMInputText,
        AmbientText,
        AmbientInputText,
        SFXText,
        SFXInputText,
        UIText,
        UIInputText,
        MuteText,
        ResolutionText,
        FullscreenText,
        QualityText,
        VsyncText,
        AntialiasingText,
        BloomText,
        AOText
    }

    private enum InputFields
    {
        MasterInputField,
        BGMInputField,
        AmbientInputField,
        SFXInputField,
        UIInputField
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
        QualityButton,
        LanguageButton
    }

    private enum Toggles
    {
        MasterToggle,
        BGMToggle,
        AmbientToggle,
        SFXToggle,
        UIToggle,
        MuteToggle,
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
        ResolutionDropdown,
        FullscreenDropdown,
        QualityDropdown,
        LanguageDropdown
    }

    private enum Panels
    {
        SoundPanel,
        GraphicPanel,
        AccessPanel
    }

    private enum UI_OptionState { Sound, Graphic, Access }

    private UI_OptionState _state;
    private Resolution[] _resolutions;
    private List<UIKeybindSlot> _keybinds = new List<UIKeybindSlot>();
    private bool _isUpdatingVolume;
    private bool _isRebinding;
    private bool _initialModifierDash;
    private string _initialKeybindJson;

    public override void Init()
    {
        base.Init();
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindInputField(typeof(InputFields));
        BindButton(typeof(Buttons));
        BindToggle(typeof(Toggles));
        BindScrollRect(typeof(ScrollRects));
        BindScrollbar(typeof(Scrollbars));
        BindDropdown(typeof(Dropdowns));
        BindPanel(typeof(Panels));
        BindButtonStates();
        BindButtonActions();
        InitStaticTexts();
        Switch(UI_OptionState.Sound);
        InitSoundPanel();
        InitGraphicPanel();
        InitAccessPanel();
    }

    private void BindButtonStates()
    {
        GetImage((int)Images.SoundButtonImage).BindState(_soundButton, Define.Atlas.Common, this);
        GetImage((int)Images.GraphicButtonImage).BindState(_graphicButton, Define.Atlas.Common, this);
        GetImage((int)Images.AccessButtonImage).BindState(_accessButton, Define.Atlas.Common, this);
        GetImage((int)Images.ApplyButtonImage).BindState(_applyButton, Define.Atlas.Common, this);
        GetImage((int)Images.CompleteButtonImage).BindState(_completeButton, Define.Atlas.Common, this);
        GetImage((int)Images.CancelButtonImage).BindState(_cancelButton, Define.Atlas.Common, this);
        GetImage((int)Images.DefaultButtonImage).BindState(_defaultButton, Define.Atlas.Common, this);
    }

    private void BindButtonActions()
    {
        GetButton((int)Buttons.SoundButton).BindViewAsButton(OnClickSound, ViewEvent.LeftClick, this, _soundButton);
        GetButton((int)Buttons.GraphicButton).BindViewAsButton(OnClickGraphic, ViewEvent.LeftClick, this, _graphicButton);
        GetButton((int)Buttons.AccessButton).BindViewAsButton(OnClickAccess, ViewEvent.LeftClick, this, _accessButton);
        GetButton((int)Buttons.ApplyButton).BindViewAsButton(async data => await OnClickApply(data), ViewEvent.LeftClick, this, _applyButton);
        GetButton((int)Buttons.CompleteButton).BindViewAsButton(async data => await OnClickComplete(data), ViewEvent.LeftClick, this, _completeButton);
        GetButton((int)Buttons.CancelButton).BindViewAsButton(OnClickCancel, ViewEvent.LeftClick, this, _cancelButton);
        GetButton((int)Buttons.DefaultButton).BindViewAsButton(async data => await OnClickDefault(data), ViewEvent.LeftClick, this, _defaultButton);
    }

    private void InitStaticTexts()
    {
        SetText(Texts.SoundButtonText, Localization.UI_Option_Popup_Text_Sound);
        SetText(Texts.GraphicButtonText, Localization.UI_Option_Popup_Text_Graphic);
        SetText(Texts.AccessButtonText, Localization.UI_Option_Popup_Text_Access);
        SetText(Texts.ApplyButtonText, Localization.Apply);
        SetText(Texts.CompleteButtonText, Localization.Complete);
        SetText(Texts.CancelButtonText, Localization.Cancel);
        SetText(Texts.DefaultButtonText, Localization.Default);
    }

    private void InitSoundPanel()
    {
        BindToggleAction(Toggles.MasterToggle, Images.MasterCheckmarkImage, Images.MasterInputImage, Toggles.MasterToggle, Scrollbars.MasterScrollbar);
        BindToggleAction(Toggles.BGMToggle, Images.BGMCheckmarkImage, Images.BGMInputImage, Toggles.BGMToggle, Scrollbars.BGMScrollbar);
        BindToggleAction(Toggles.AmbientToggle, Images.AmbientCheckmarkImage, Images.AmbientInputImage, Toggles.AmbientToggle, Scrollbars.AmbientScrollbar);
        BindToggleAction(Toggles.SFXToggle, Images.SFXCheckmarkImage, Images.SFXInputImage, Toggles.SFXToggle, Scrollbars.SFXScrollbar);
        BindToggleAction(Toggles.UIToggle, Images.UICheckmarkImage, Images.UIInputImage, Toggles.UIToggle, Scrollbars.UIScrollbar);
        GetToggle((int)Toggles.MuteToggle).BindView(_ =>
        {
            bool isOn = GetToggle((int)Toggles.MuteToggle).isOn;
            UpdateCheckmark(GetImage((int)Images.MuteCheckmarkImage), isOn);
            GetImage((int)Images.MuteToggleImage).SetVisual(null, null, isOn);
        }, ViewEvent.LeftClick, this);
        BindVolumeControl(Scrollbars.MasterScrollbar, InputFields.MasterInputField);
        BindVolumeControl(Scrollbars.BGMScrollbar, InputFields.BGMInputField);
        BindVolumeControl(Scrollbars.AmbientScrollbar, InputFields.AmbientInputField);
        BindVolumeControl(Scrollbars.SFXScrollbar, InputFields.SFXInputField);
        BindVolumeControl(Scrollbars.UIScrollbar, InputFields.UIInputField);
        SetText(Texts.MasterText, Localization.UI_Option_Popup_Text_Master);
        SetText(Texts.BGMText, Localization.UI_Option_Popup_Text_BGM);
        SetText(Texts.AmbientText, Localization.UI_Option_Popup_Text_Ambient);
        SetText(Texts.SFXText, Localization.UI_Option_Popup_Text_SFX);
        SetText(Texts.UIText, Localization.UI_Option_Popup_Text_UI);
        SetText(Texts.MuteText, Localization.UI_Option_Popup_Text_Mute);
    }

    private void BindToggleAction(Toggles toggleEnum, Images checkmarkEnum, Images inputImageEnum, Toggles toggleImageEnum, Scrollbars scrollbarEnum)
    {
        GetToggle((int)toggleEnum).BindView(_ =>
        {
            bool isOn = GetToggle((int)toggleEnum).isOn;
            UpdateCheckmark(GetImage((int)checkmarkEnum), isOn);
            GetImage((int)inputImageEnum).SetVisual(GetImage((int)toggleImageEnum), GetScrollbar((int)scrollbarEnum), isOn);
        }, ViewEvent.LeftClick, this);
    }

    private void InitGraphicPanel()
    {
        InitResolution();
        GetImage((int)Images.ResolutionArrowImage).BindStateAsArrow(_resolutionArrowButton, Define.Atlas.Common, this);
        GetImage((int)Images.FullscreenArrowImage).BindStateAsArrow(_fullscreenArrowButton, Define.Atlas.Common, this);
        GetImage((int)Images.QualityArrowImage).BindStateAsArrow(_qualityArrowButton, Define.Atlas.Common, this);
        BindArrowDropdownButton(Buttons.ResolutionButton, _resolutionArrowButton);
        BindArrowDropdownButton(Buttons.FullscreenButton, _fullscreenArrowButton);
        BindArrowDropdownButton(Buttons.QualityButton, _qualityArrowButton);
        InitDropdownOptions(Dropdowns.FullscreenDropdown, new[]
        {
            Localization.UI_Option_Popup_Text_Fullscreen_FullscreenWindow,
            Localization.UI_Option_Popup_Text_Fullscreen_Windowed,
            Localization.UI_Option_Popup_Text_Fullscreen_ExclusiveFullscreen
        });
        InitDropdownOptions(Dropdowns.QualityDropdown, new[]
        {
            Localization.UI_Option_Popup_Text_Quality_Low,
            Localization.UI_Option_Popup_Text_Quality_Medium,
            Localization.UI_Option_Popup_Text_Quality_High
        });
        BindGraphicToggle(Toggles.VsyncToggle, Images.VsyncCheckmarkImage, Images.VsyncToggleImage);
        BindGraphicToggle(Toggles.AntialiasingToggle, Images.AntialiasingCheckmarkImage, Images.AntialiasingToggleImage);
        BindGraphicToggle(Toggles.BloomToggle, Images.BloomCheckmarkImage, Images.BloomToggleImage);
        BindGraphicToggle(Toggles.AOToggle, Images.AOCheckmarkImage, Images.AOToggleImage);
        SetText(Texts.ResolutionText, Localization.UI_Option_Popup_Text_Resolution);
        SetText(Texts.FullscreenText, Localization.UI_Option_Popup_Text_Fullscreen);
        SetText(Texts.QualityText, Localization.UI_Option_Popup_Text_Quality);
        SetText(Texts.VsyncText, Localization.UI_Option_Popup_Text_Vsync);
        SetText(Texts.AntialiasingText, Localization.UI_Option_Popup_Text_Antialiasing);
        SetText(Texts.BloomText, Localization.UI_Option_Popup_Text_Bloom);
        SetText(Texts.AOText, Localization.UI_Option_Popup_Text_AO);
    }

    private void BindArrowDropdownButton(Buttons button, ReactiveProperty<ButtonState> state) =>
        GetButton((int)button).BindViewAsButton(_ => { }, ViewEvent.LeftClick, this, state);

    private void InitDropdownOptions(Dropdowns dropdown, Localization[] keys)
    {
        var dd = GetDropdown((int)dropdown);
        dd.ClearOptions();
        var options = keys.Select(k => Managers.Localization.Get(k)).ToList();
        dd.AddOptions(options);
    }

    private void BindGraphicToggle(Toggles toggle, Images checkmark, Images toggleImage)
    {
        GetToggle((int)toggle).BindView(_ =>
        {
            bool isOn = GetToggle((int)toggle).isOn;
            UpdateCheckmark(GetImage((int)checkmark), isOn);
            GetImage((int)toggleImage).SetVisual(isEnabled: isOn);
        }, ViewEvent.LeftClick, this);
    }

    private void InitAccessPanel()
    {
        GetImage((int)Images.LanguageArrowImage).BindStateAsArrow(_languageArrowButton, Define.Atlas.Common, this);
        BindArrowDropdownButton(Buttons.LanguageButton, _languageArrowButton);
        var languageDropdown = GetDropdown((int)Dropdowns.LanguageDropdown);

        if (languageDropdown != null)
        {
            languageDropdown.ClearOptions();
            var displayOptions = Managers.Localization.GetLanguages().Select(l => l.ToNative()).ToList();
            languageDropdown.AddOptions(displayOptions);
        }

        var content = GetScrollRect((int)ScrollRects.KeybindScrollRect).content;
        Func<bool> isRebindingCheck = () => _isRebinding;
        Action<bool> setRebindingLock = isBusy => _isRebinding = isBusy;
        var (dashSlot, _) = Managers.Pool.Pop<UIKeybindSlot>(content);
        _keybinds.Add(dashSlot);
        dashSlot.SetupDashCommand(isRebindingCheck, setRebindingLock, (_, _) => { });

        foreach (var action in Managers.Control.GetBindableActions())
        {
            var (slot, _) = Managers.Pool.Pop<UIKeybindSlot>(content);
            _keybinds.Add(slot);
            slot.Setup(action.name, action, _keybinds, isRebindingCheck, setRebindingLock, (_, _) => { });
        }
    }

    private void InitResolution()
    {
        var resolutionDropdown = GetDropdown((int)Dropdowns.ResolutionDropdown);
        resolutionDropdown.ClearOptions();
        _resolutions = Screen.resolutions
        .Select(r => new Resolution { width = r.width, height = r.height, refreshRateRatio = r.refreshRateRatio })
        .GroupBy(r => new { r.width, r.height, hz = Mathf.RoundToInt((float)r.refreshRateRatio.numerator / r.refreshRateRatio.denominator) })
        .Select(g => g.First())
        .OrderBy(r => r.width)
        .ThenBy(r => r.height)
        .ThenBy(r => (double)r.refreshRateRatio.numerator / r.refreshRateRatio.denominator)
        .ToArray();
        var resolutionOptions = _resolutions
        .Select(res => Managers.Localization.Get(Localization.UI_Option_Popup_Text_Resolution_Dropdown, res.width, res.height, Mathf.RoundToInt((float)res.refreshRateRatio.numerator / res.refreshRateRatio.denominator)))
        .ToList();
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

    private void OnClickSound(PointerEventData data) 
        => SwitchState(UI_OptionState.Sound);

    private void OnClickGraphic(PointerEventData data) 
        => SwitchState(UI_OptionState.Graphic);

    private void OnClickAccess(PointerEventData data) 
        => SwitchState(UI_OptionState.Access);

    private void SwitchState(UI_OptionState targetState)
    {
        if (_state == targetState)
            return;

        Switch(targetState);
    }

    private void Switch(UI_OptionState state)
    {
        _state = state;
        GetPanel((int)Panels.SoundPanel).SetActivePanel(_state == UI_OptionState.Sound);
        GetPanel((int)Panels.GraphicPanel).SetActivePanel(_state == UI_OptionState.Graphic);
        GetPanel((int)Panels.AccessPanel).SetActivePanel(_state == UI_OptionState.Access);
        UpdateTab();
    }

    private void UpdateTab()
    {
        _soundButton.Value = _state == UI_OptionState.Sound ? ButtonState.Disable : ButtonState.Normal;
        _graphicButton.Value = _state == UI_OptionState.Graphic ? ButtonState.Disable : ButtonState.Normal;
        _accessButton.Value = _state == UI_OptionState.Access ? ButtonState.Disable : ButtonState.Normal;
    }

    private void UpdateVolume(TMP_InputField inputField, float value)
    {
        if (inputField == null)
            return;

        _isUpdatingVolume = true;
        inputField.text = Mathf.RoundToInt(value * 100f).ToString();
        _isUpdatingVolume = false;
    }

    private void UpdateCheckmark(Image image, bool isOn)
    {
        if (image == null)
            return;

        string sprite = isOn ? Define.Sprite.Checkmark_Yes : Define.Sprite.Checkmark_No;
        image.sprite = Managers.Resource.GetSprite(Define.Atlas.Common, sprite);
    }

    private void BindVolumeControl(Scrollbars scrollbarEnum, InputFields inputFieldEnum)
    {
        var scrollbar = GetScrollbar((int)scrollbarEnum);
        var inputField = GetInputField((int)inputFieldEnum);

        if (scrollbar == null || inputField == null)
            return;

        scrollbar.BindScrollbar(val =>
        {
            if (_isUpdatingVolume)
                return;

            _isUpdatingVolume = true;
            inputField.text = Mathf.RoundToInt(val * 100f).ToString();
            _isUpdatingVolume = false;
        }, this);
        inputField.BindInputField(text =>
        {
            if (_isUpdatingVolume || string.IsNullOrEmpty(text))
                return;

            if (!int.TryParse(text, out int percent))
                return;

            if (percent > 100)
            {
                percent = 100;
                _isUpdatingVolume = true;
                inputField.text = "100";
                _isUpdatingVolume = false;
            }
            else if (percent < 0)
                percent = 0;

            _isUpdatingVolume = true;
            scrollbar.value = percent / 100f;
            _isUpdatingVolume = false;
        }, this);
        inputField.BindInputEndEdit(text =>
        {
            if (_isUpdatingVolume)
                return;

            if (string.IsNullOrEmpty(text) || !int.TryParse(text, out int percent))
                percent = 0;

            percent = Mathf.Clamp(percent, 0, 100);
            _isUpdatingVolume = true;
            inputField.text = percent.ToString();
            scrollbar.value = percent / 100f;
            _isUpdatingVolume = false;
        }, this);
    }

    private void Refresh()
    {
        RefreshSoundPanel();
        RefreshGraphicPanel();
        RefreshAccessPanel();

        foreach (var slot in _keybinds)
            slot.Refresh();
    }

    private void RefreshSoundPanel()
    {
        var sound = Managers.Config.Option.Sound;

        GetScrollbar((int)Scrollbars.MasterScrollbar).value = sound.vMaster;
        GetScrollbar((int)Scrollbars.BGMScrollbar).value = sound.vBGM;
        GetScrollbar((int)Scrollbars.AmbientScrollbar).value = sound.vAmbient;
        GetScrollbar((int)Scrollbars.SFXScrollbar).value = sound.vSFX;
        GetScrollbar((int)Scrollbars.UIScrollbar).value = sound.vUI;
        UpdateVolume(GetInputField((int)InputFields.MasterInputField), sound.vMaster);
        UpdateVolume(GetInputField((int)InputFields.BGMInputField), sound.vBGM);
        UpdateVolume(GetInputField((int)InputFields.AmbientInputField), sound.vAmbient);
        UpdateVolume(GetInputField((int)InputFields.SFXInputField), sound.vSFX);
        UpdateVolume(GetInputField((int)InputFields.UIInputField), sound.vUI);
        SetToggleAndVisual(Toggles.MasterToggle, Images.MasterInputImage, Images.MasterToggleImage, Images.MasterCheckmarkImage, sound.mMaster, Scrollbars.MasterScrollbar);
        SetToggleAndVisual(Toggles.BGMToggle, Images.BGMInputImage, Images.BGMToggleImage, Images.BGMCheckmarkImage, sound.mBGM, Scrollbars.BGMScrollbar);
        SetToggleAndVisual(Toggles.AmbientToggle, Images.AmbientInputImage, Images.AmbientToggleImage, Images.AmbientCheckmarkImage, sound.mAmbient, Scrollbars.AmbientScrollbar);
        SetToggleAndVisual(Toggles.SFXToggle, Images.SFXInputImage, Images.SFXToggleImage, Images.SFXCheckmarkImage, sound.mSFX, Scrollbars.SFXScrollbar);
        SetToggleAndVisual(Toggles.UIToggle, Images.UIInputImage, Images.UIToggleImage, Images.UICheckmarkImage, sound.mUI, Scrollbars.UIScrollbar);
        GetToggle((int)Toggles.MuteToggle).isOn = sound.mute;
        UpdateCheckmark(GetImage((int)Images.MuteCheckmarkImage), sound.mute);
    }

    private void SetToggleAndVisual(Toggles toggle, Images inputImage, Images toggleImage, Images checkmark, bool isOn, Scrollbars scrollbar)
    {
        GetToggle((int)toggle).isOn = isOn;
        GetImage((int)inputImage).SetVisual(GetImage((int)toggleImage), GetScrollbar((int)scrollbar), isOn);
        UpdateCheckmark(GetImage((int)checkmark), isOn);
    }

    private void RefreshGraphicPanel()
    {
        var graphic = Managers.Config.Option.Graphic;

        for (int index = 0; index < _resolutions.Length; index++)
        {
            int hz = Mathf.RoundToInt((float)_resolutions[index].refreshRateRatio.numerator / _resolutions[index].refreshRateRatio.denominator);
            
            if (_resolutions[index].width == graphic.rWidth && _resolutions[index].height == graphic.rHeight && hz == graphic.rRefreshRate)
            {
                GetDropdown((int)Dropdowns.ResolutionDropdown).value = index;
                break;
            }
        }

        GetDropdown((int)Dropdowns.FullscreenDropdown).value = graphic.screenMode switch
        {
            FullScreenMode.FullScreenWindow => 0,
            FullScreenMode.Windowed => 1,
            FullScreenMode.ExclusiveFullScreen => 2,
            _ => 0
        };
        GetDropdown((int)Dropdowns.QualityDropdown).value = (int)graphic.quality;
        SetGraphicToggleAndVisual(Toggles.VsyncToggle, Images.VsyncToggleImage, Images.VsyncCheckmarkImage, graphic.vSync);
        SetGraphicToggleAndVisual(Toggles.AntialiasingToggle, Images.AntialiasingToggleImage, Images.AntialiasingCheckmarkImage, graphic.antiAliasing);
        SetGraphicToggleAndVisual(Toggles.BloomToggle, Images.BloomToggleImage, Images.BloomCheckmarkImage, graphic.bloom);
        SetGraphicToggleAndVisual(Toggles.AOToggle, Images.AOToggleImage, Images.AOCheckmarkImage, graphic.ambientOccusion);
    }

    private void SetGraphicToggleAndVisual(Toggles toggle, Images toggleImage, Images checkmark, bool isOn)
    {
        GetToggle((int)toggle).isOn = isOn;
        GetImage((int)toggleImage).SetVisual(isEnabled: isOn);
        UpdateCheckmark(GetImage((int)checkmark), isOn);
    }

    private void RefreshAccessPanel()
    {
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
    }

    private void Sync()
    {
        SyncSoundPanel();
        SyncGraphicPanel();
        SyncAccessPanel();
    }

    private void SyncSoundPanel()
    {
        var sound = Managers.Config.Option.Sound;
        sound.vMaster = GetScrollbar((int)Scrollbars.MasterScrollbar).value;
        sound.vBGM = GetScrollbar((int)Scrollbars.BGMScrollbar).value;
        sound.vAmbient = GetScrollbar((int)Scrollbars.AmbientScrollbar).value;
        sound.vSFX = GetScrollbar((int)Scrollbars.SFXScrollbar).value;
        sound.vUI = GetScrollbar((int)Scrollbars.UIScrollbar).value;
        sound.mMaster = GetToggle((int)Toggles.MasterToggle).isOn;
        sound.mBGM = GetToggle((int)Toggles.BGMToggle).isOn;
        sound.mAmbient = GetToggle((int)Toggles.AmbientToggle).isOn;
        sound.mSFX = GetToggle((int)Toggles.SFXToggle).isOn;
        sound.mUI = GetToggle((int)Toggles.UIToggle).isOn;
        sound.mute = GetToggle((int)Toggles.MuteToggle).isOn;
    }

    private void SyncGraphicPanel()
    {
        var graphic = Managers.Config.Option.Graphic;
        int resIndex = GetDropdown((int)Dropdowns.ResolutionDropdown).value;

        if (_resolutions != null && resIndex < _resolutions.Length)
        {
            var res = _resolutions[resIndex];
            graphic.rWidth = res.width;
            graphic.rHeight = res.height;
            graphic.rRefreshRate = Mathf.RoundToInt((float)res.refreshRateRatio.numerator / res.refreshRateRatio.denominator);
        }

        graphic.screenMode = GetDropdown((int)Dropdowns.FullscreenDropdown).value switch
        {
            0 => FullScreenMode.FullScreenWindow,
            1 => FullScreenMode.Windowed,
            2 => FullScreenMode.ExclusiveFullScreen,
            _ => FullScreenMode.FullScreenWindow
        };
        graphic.quality = (Quality)GetDropdown((int)Dropdowns.QualityDropdown).value;
        graphic.vSync = GetToggle((int)Toggles.VsyncToggle).isOn;
        graphic.antiAliasing = GetToggle((int)Toggles.AntialiasingToggle).isOn;
        graphic.bloom = GetToggle((int)Toggles.BloomToggle).isOn;
        graphic.ambientOccusion = GetToggle((int)Toggles.AOToggle).isOn;
    }

    private void SyncAccessPanel()
    {
        int langIndex = GetDropdown((int)Dropdowns.LanguageDropdown).value;
        var languageLocales = Managers.Localization.GetLanguages();

        if (languageLocales != null && langIndex < languageLocales.Count)
            Managers.Config.Option.Access.language = languageLocales[langIndex];

        Managers.Config.Option.Access.keybind = Managers.Control.Save();
    }

    private void CancelAllRebinds()
    {
        foreach (var slot in _keybinds)
        {
            if (slot != null)
                slot.CancelRebind();
        }

        _isRebinding = false;
    }

    private async UniTask OnClickApply(PointerEventData data)
    {
        CancelAllRebinds();
        Sync();
        await Managers.Config.SaveAsync().Lock();
    }

    private async UniTask OnClickComplete(PointerEventData data)
    {
        CancelAllRebinds();
        Sync();
        await Managers.Config.SaveAsync().Lock();
        Release();
    }

    private void OnClickCancel(PointerEventData data)
    {
        CancelAllRebinds();

        if (!string.IsNullOrEmpty(_initialKeybindJson))
            Managers.Control.LoadBindingFromJson(_initialKeybindJson);

        Managers.Config.Option.Access.modifierDash = _initialModifierDash;

        foreach (var slot in _keybinds)
            slot.Refresh();

        Release();
    }

    private async UniTask OnClickDefault(PointerEventData data)
    {
        CancelAllRebinds();

        await Managers.Config.ResetAsync().Lock();
        Managers.Control.Reset();
        Managers.Config.Option.Access.modifierDash = AccessOption.Default.modifierDash;
        Refresh();
    }

    private void SetText(Texts textEnum, Localization key) 
        => GetText((int)textEnum).text = Managers.Localization.Get(key);
}
