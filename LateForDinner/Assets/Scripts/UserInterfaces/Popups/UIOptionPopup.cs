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

public class UIOptionPopup : UIPopup, IDraggablePopup, IFocusablePopup
{
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
        VignetteToggleImage,
        VignetteCheckmarkImage,
        MotionBlurToggleImage,
        MotionBlurCheckmarkImage,
        ContrastToggleImage,
        ContrastCheckmarkImage,
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
        VignetteText,
        MotionBlurText,
        ContrastText
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
        VignetteToggle,
        MotionBlurToggle,
        ContrastToggle
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

    private enum UI_OptionState 
    { 
        Sound, 
        Graphic, 
        Access 
    }

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
    private UI_OptionState _state;
    private Resolution[] _resolutions;
    private List<UIKeybindSlot> _keybinds = new List<UIKeybindSlot>();
    private bool _isUpdatingVolume;
    private bool _isRebinding;
    private bool _initialModifierDash;
    private string _initialLanguage;
    private string _initialKeybindJson;
    private string _initialBindingSnapshot;

    public override void OnInit()
    {
        base.OnInit();
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
        Switch(UI_OptionState.Sound);
        InitSoundPanel();
        InitGraphicPanel();
        InitAccessPanel();
    }

    private void BindButtonStates()
    {
        GetImage(Images.SoundButtonImage).BindState(_soundButton, Define.Atlas.Common, this);
        GetImage(Images.GraphicButtonImage).BindState(_graphicButton, Define.Atlas.Common, this);
        GetImage(Images.AccessButtonImage).BindState(_accessButton, Define.Atlas.Common, this);
        GetImage(Images.ApplyButtonImage).BindState(_applyButton, Define.Atlas.Common, this);
        GetImage(Images.CompleteButtonImage).BindState(_completeButton, Define.Atlas.Common, this);
        GetImage(Images.CancelButtonImage).BindState(_cancelButton, Define.Atlas.Common, this);
        GetImage(Images.DefaultButtonImage).BindState(_defaultButton, Define.Atlas.Common, this);
    }

    private void BindButtonActions()
    {
        GetButton(Buttons.SoundButton).BindViewAsButton(OnClickSound, ViewEvent.LeftClick, this, _soundButton);
        GetButton(Buttons.GraphicButton).BindViewAsButton(OnClickGraphic, ViewEvent.LeftClick, this, _graphicButton);
        GetButton(Buttons.AccessButton).BindViewAsButton(OnClickAccess, ViewEvent.LeftClick, this, _accessButton);
        GetButton(Buttons.ApplyButton).BindViewAsButton(async data => await OnClickApply(data), ViewEvent.LeftClick, this, _applyButton);
        GetButton(Buttons.CompleteButton).BindViewAsButton(async data => await OnClickComplete(data), ViewEvent.LeftClick, this, _completeButton);
        GetButton(Buttons.CancelButton).BindViewAsButton(async data => await OnClickCancel(data), ViewEvent.LeftClick, this, _cancelButton);
        GetButton(Buttons.DefaultButton).BindViewAsButton(async data => await OnClickDefault(data), ViewEvent.LeftClick, this, _defaultButton);
    }

    private void InitStaticTexts()
    {
        SetText(Texts.SoundButtonText, LocalizationKey.UI_Option_Popup_Text_Sound);
        SetText(Texts.GraphicButtonText, LocalizationKey.UI_Option_Popup_Text_Graphic);
        SetText(Texts.AccessButtonText, LocalizationKey.UI_Option_Popup_Text_Access);
        SetText(Texts.ApplyButtonText, LocalizationKey.Apply);
        SetText(Texts.CompleteButtonText, LocalizationKey.Complete);
        SetText(Texts.CancelButtonText, LocalizationKey.Cancel);
        SetText(Texts.DefaultButtonText, LocalizationKey.Default);
    }

    private void InitSoundPanel()
    {
        BindToggleAction(Toggles.MasterToggle, Images.MasterCheckmarkImage, Images.MasterInputImage, Toggles.MasterToggle, Scrollbars.MasterScrollbar);
        BindToggleAction(Toggles.BGMToggle, Images.BGMCheckmarkImage, Images.BGMInputImage, Toggles.BGMToggle, Scrollbars.BGMScrollbar);
        BindToggleAction(Toggles.AmbientToggle, Images.AmbientCheckmarkImage, Images.AmbientInputImage, Toggles.AmbientToggle, Scrollbars.AmbientScrollbar);
        BindToggleAction(Toggles.SFXToggle, Images.SFXCheckmarkImage, Images.SFXInputImage, Toggles.SFXToggle, Scrollbars.SFXScrollbar);
        BindToggleAction(Toggles.UIToggle, Images.UICheckmarkImage, Images.UIInputImage, Toggles.UIToggle, Scrollbars.UIScrollbar);
        GetToggle(Toggles.MuteToggle).BindView(_ =>
        {
            bool isOn = GetToggle(Toggles.MuteToggle).isOn;
            UpdateCheckmark(GetImage(Images.MuteCheckmarkImage), isOn);
            GetImage(Images.MuteToggleImage).SetVisual(null, null, isOn);
        }, ViewEvent.LeftClick, this);
        BindVolumeControl(Scrollbars.MasterScrollbar, InputFields.MasterInputField);
        BindVolumeControl(Scrollbars.BGMScrollbar, InputFields.BGMInputField);
        BindVolumeControl(Scrollbars.AmbientScrollbar, InputFields.AmbientInputField);
        BindVolumeControl(Scrollbars.SFXScrollbar, InputFields.SFXInputField);
        BindVolumeControl(Scrollbars.UIScrollbar, InputFields.UIInputField);
        SetText(Texts.MasterText, LocalizationKey.UI_Option_Popup_Text_Master);
        SetText(Texts.BGMText, LocalizationKey.UI_Option_Popup_Text_BGM);
        SetText(Texts.AmbientText, LocalizationKey.UI_Option_Popup_Text_Ambient);
        SetText(Texts.SFXText, LocalizationKey.UI_Option_Popup_Text_SFX);
        SetText(Texts.UIText, LocalizationKey.UI_Option_Popup_Text_UI);
        SetText(Texts.MuteText, LocalizationKey.UI_Option_Popup_Text_Mute);
    }

    private void BindToggleAction(Toggles toggleEnum, Images checkmarkEnum, Images inputImageEnum, Toggles toggleImageEnum, Scrollbars scrollbarEnum)
    {
        GetToggle(toggleEnum).BindView(_ =>
        {
            bool isOn = GetToggle(toggleEnum).isOn;
            UpdateCheckmark(GetImage(checkmarkEnum), isOn);
            GetImage(inputImageEnum).SetVisual(GetImage(toggleImageEnum), GetScrollbar(scrollbarEnum), isOn);
        }, ViewEvent.LeftClick, this);
    }

    private void InitGraphicPanel()
    {
        InitResolution();
        GetImage(Images.ResolutionArrowImage).BindStateAsArrow(_resolutionArrowButton, Define.Atlas.Common, this);
        GetImage(Images.FullscreenArrowImage).BindStateAsArrow(_fullscreenArrowButton, Define.Atlas.Common, this);
        GetImage(Images.QualityArrowImage).BindStateAsArrow(_qualityArrowButton, Define.Atlas.Common, this);
        BindArrowDropdownButton(Buttons.ResolutionButton, _resolutionArrowButton);
        BindArrowDropdownButton(Buttons.FullscreenButton, _fullscreenArrowButton);
        BindArrowDropdownButton(Buttons.QualityButton, _qualityArrowButton);
        InitDropdownOptions(Dropdowns.FullscreenDropdown, new[]
        {
            LocalizationKey.UI_Option_Popup_Text_Fullscreen_FullscreenWindow,
            LocalizationKey.UI_Option_Popup_Text_Fullscreen_Windowed,
            LocalizationKey.UI_Option_Popup_Text_Fullscreen_ExclusiveFullscreen
        });
        InitDropdownOptions(Dropdowns.QualityDropdown, new[]
        {
            LocalizationKey.UI_Option_Popup_Text_Quality_Low,
            LocalizationKey.UI_Option_Popup_Text_Quality_Medium,
            LocalizationKey.UI_Option_Popup_Text_Quality_High
        });
        BindGraphicToggle(Toggles.VsyncToggle, Images.VsyncCheckmarkImage, Images.VsyncToggleImage);
        BindGraphicToggle(Toggles.AntialiasingToggle, Images.AntialiasingCheckmarkImage, Images.AntialiasingToggleImage);
        BindGraphicToggle(Toggles.BloomToggle, Images.BloomCheckmarkImage, Images.BloomToggleImage);
        BindGraphicToggle(Toggles.VignetteToggle, Images.VignetteCheckmarkImage, Images.VignetteToggleImage);
        BindGraphicToggle(Toggles.MotionBlurToggle, Images.MotionBlurCheckmarkImage, Images.MotionBlurToggleImage);
        BindGraphicToggle(Toggles.ContrastToggle, Images.ContrastCheckmarkImage, Images.ContrastToggleImage);
        SetText(Texts.ResolutionText, LocalizationKey.UI_Option_Popup_Text_Resolution);
        SetText(Texts.FullscreenText, LocalizationKey.UI_Option_Popup_Text_Fullscreen);
        SetText(Texts.QualityText, LocalizationKey.UI_Option_Popup_Text_Quality);
        SetText(Texts.VsyncText, LocalizationKey.UI_Option_Popup_Text_Vsync);
        SetText(Texts.AntialiasingText, LocalizationKey.UI_Option_Popup_Text_Antialiasing);
        SetText(Texts.BloomText, LocalizationKey.UI_Option_Popup_Text_Bloom);
        SetText(Texts.VignetteText, LocalizationKey.UI_Option_Popup_Text_Vignette);
        SetText(Texts.MotionBlurText, LocalizationKey.UI_Option_Popup_Text_MotionBlur);
        SetText(Texts.ContrastText, LocalizationKey.UI_Option_Popup_Text_Contrast);
    }

    private void BindArrowDropdownButton(Buttons button, ReactiveProperty<ButtonState> state) =>
        GetButton(button).BindViewAsButton(_ => { }, ViewEvent.LeftClick, this, state);

    private void InitDropdownOptions(Dropdowns dropdown, LocalizationKey[] keys)
    {
        var dd = GetDropdown(dropdown);
        dd.ClearOptions();
        var options = keys.Select(k => Managers.Localization.Get(k)).ToList();
        dd.AddOptions(options);
    }

    private void BindGraphicToggle(Toggles toggle, Images checkmark, Images toggleImage)
    {
        GetToggle(toggle).BindView(_ =>
        {
            bool isOn = GetToggle(toggle).isOn;
            UpdateCheckmark(GetImage(checkmark), isOn);
            GetImage(toggleImage).SetVisual(isEnabled: isOn);
        }, ViewEvent.LeftClick, this);
    }

    private void InitAccessPanel()
    {
        GetImage(Images.LanguageArrowImage).BindStateAsArrow(_languageArrowButton, Define.Atlas.Common, this);
        BindArrowDropdownButton(Buttons.LanguageButton, _languageArrowButton);
        var languageDropdown = GetDropdown(Dropdowns.LanguageDropdown);

        if (languageDropdown != null)
        {
            languageDropdown.ClearOptions();
            var displayOptions = Managers.Localization.GetLanguages().Select(l => l.ToNative()).ToList();
            languageDropdown.AddOptions(displayOptions);
        }

        var content = GetScrollRect(ScrollRects.KeybindScrollRect).content;
        Func<bool> isRebindingCheck = () => _isRebinding;
        Action<bool> setRebindingLock = isBusy => _isRebinding = isBusy;
        var (dashSlot, _) = Managers.Pool.Pop<UIKeybindSlot>(content);
        _keybinds.Add(dashSlot);
        dashSlot.SetupDashCommand(isRebindingCheck, setRebindingLock);

        foreach (var action in Managers.Control.GetBindableActions())
        {
            var (slot, _) = Managers.Pool.Pop<UIKeybindSlot>(content);
            _keybinds.Add(slot);
            slot.Setup(action.name, action, _keybinds, isRebindingCheck, setRebindingLock,
            (duplicateActionName, duplicateKeyName) =>
            {
                // DESC ::: 중복된 키 입력 시 토스트 출력
                Managers.Notify.ToastAsync(LocalizationKey.Log_Option_Popup_Keybind_Duplicate, duplicateActionName, duplicateKeyName).Forget();
            });
        }
    }

    private void InitResolution()
    {
        var resolutionDropdown = GetDropdown(Dropdowns.ResolutionDropdown);
        resolutionDropdown.ClearOptions();
        _resolutions = Screen.resolutions
        .Select(r => new Resolution { width = r.width, height = r.height, refreshRateRatio = r.refreshRateRatio })
        .GroupBy(r => new { r.width, r.height, hz = Math.Round((double)r.refreshRateRatio.numerator / r.refreshRateRatio.denominator, 1) })
        .Select(g => g.First())
        .OrderBy(r => r.width)
        .ThenBy(r => r.height)
        .ThenBy(r => (double)r.refreshRateRatio.numerator / r.refreshRateRatio.denominator)
        .ToArray();
        var resolutionOptions = _resolutions
        .Select(res => Managers.Localization.Get(LocalizationKey.UI_Option_Popup_Text_Resolution_Dropdown, res.width, res.height, Mathf.RoundToInt((float)res.refreshRateRatio.numerator / res.refreshRateRatio.denominator)))
        .ToList();
        resolutionDropdown.AddOptions(resolutionOptions);
    }

    public override void OnGet()
    {
        base.OnGet();
        Refresh();
        _initialModifierDash = Managers.Config.Option.Access.modifierDash;
        _initialLanguage = Managers.Config.Option.Access.language;
        _initialKeybindJson = Managers.Config.Option.Access.keybind;

        if (!string.IsNullOrEmpty(_initialKeybindJson))
            Managers.Control.LoadBindingFromJson(_initialKeybindJson);

        _initialBindingSnapshot = Managers.Control.CreateBindingSnapshot();
        Switch(UI_OptionState.Sound);
    }

    public override void Refresh()
    {
        base.Refresh();
        InitStaticTexts();
        RefreshSoundPanel();
        RefreshGraphicPanel();
        RefreshAccessPanel();

        foreach (var slot in _keybinds)
            slot.Refresh();
    }

    private void RefreshSoundPanel()
    {
        var sound = Managers.Config.Option.Sound;
        GetScrollbar(Scrollbars.MasterScrollbar).value = sound.vMaster;
        GetScrollbar(Scrollbars.BGMScrollbar).value = sound.vBGM;
        GetScrollbar(Scrollbars.AmbientScrollbar).value = sound.vAmbient;
        GetScrollbar(Scrollbars.SFXScrollbar).value = sound.vSFX;
        GetScrollbar(Scrollbars.UIScrollbar).value = sound.vUI;
        UpdateVolume(GetInputField(InputFields.MasterInputField), sound.vMaster);
        UpdateVolume(GetInputField(InputFields.BGMInputField), sound.vBGM);
        UpdateVolume(GetInputField(InputFields.AmbientInputField), sound.vAmbient);
        UpdateVolume(GetInputField(InputFields.SFXInputField), sound.vSFX);
        UpdateVolume(GetInputField(InputFields.UIInputField), sound.vUI);
        SetToggleAndVisual(Toggles.MasterToggle, Images.MasterInputImage, Images.MasterToggleImage, Images.MasterCheckmarkImage, sound.mMaster, Scrollbars.MasterScrollbar);
        SetToggleAndVisual(Toggles.BGMToggle, Images.BGMInputImage, Images.BGMToggleImage, Images.BGMCheckmarkImage, sound.mBGM, Scrollbars.BGMScrollbar);
        SetToggleAndVisual(Toggles.AmbientToggle, Images.AmbientInputImage, Images.AmbientToggleImage, Images.AmbientCheckmarkImage, sound.mAmbient, Scrollbars.AmbientScrollbar);
        SetToggleAndVisual(Toggles.SFXToggle, Images.SFXInputImage, Images.SFXToggleImage, Images.SFXCheckmarkImage, sound.mSFX, Scrollbars.SFXScrollbar);
        SetToggleAndVisual(Toggles.UIToggle, Images.UIInputImage, Images.UIToggleImage, Images.UICheckmarkImage, sound.mUI, Scrollbars.UIScrollbar);
        GetToggle(Toggles.MuteToggle).isOn = sound.mute;
        UpdateCheckmark(GetImage(Images.MuteCheckmarkImage), sound.mute);
    }

    private void SetToggleAndVisual(Toggles toggle, Images inputImage, Images toggleImage, Images checkmark, bool isOn, Scrollbars scrollbar)
    {
        GetToggle(toggle).isOn = isOn;
        GetImage(inputImage).SetVisual(GetImage(toggleImage), GetScrollbar(scrollbar), isOn);
        UpdateCheckmark(GetImage(checkmark), isOn);
    }

    private void RefreshGraphicPanel()
    {
        var graphic = Managers.Config.Option.Graphic;

        for (int index = 0; index < _resolutions.Length; index++)
        {
            float currentHz = (float)_resolutions[index].refreshRateRatio.numerator / _resolutions[index].refreshRateRatio.denominator;

            if (_resolutions[index].width != graphic.rWidth || _resolutions[index].height != graphic.rHeight || Mathf.Abs(currentHz - graphic.rRefreshRate) > 1f)
                continue;

            GetDropdown(Dropdowns.ResolutionDropdown).value = index;
            break;
        }

        GetDropdown(Dropdowns.FullscreenDropdown).value = graphic.screenMode switch
        {
            FullScreenMode.FullScreenWindow => 0,
            FullScreenMode.Windowed => 1,
            FullScreenMode.ExclusiveFullScreen => 2,
            _ => 0
        };
        GetDropdown(Dropdowns.QualityDropdown).value = (int)graphic.quality;
        SetGraphicToggleAndVisual(Toggles.VsyncToggle, Images.VsyncToggleImage, Images.VsyncCheckmarkImage, graphic.vSync);
        SetGraphicToggleAndVisual(Toggles.AntialiasingToggle, Images.AntialiasingToggleImage, Images.AntialiasingCheckmarkImage, graphic.antiAliasing);
        SetGraphicToggleAndVisual(Toggles.BloomToggle, Images.BloomToggleImage, Images.BloomCheckmarkImage, graphic.bloom);
        SetGraphicToggleAndVisual(Toggles.VignetteToggle, Images.VignetteToggleImage, Images.VignetteCheckmarkImage, graphic.vignette);
        SetGraphicToggleAndVisual(Toggles.MotionBlurToggle, Images.MotionBlurToggleImage, Images.MotionBlurCheckmarkImage, graphic.mblur);
        SetGraphicToggleAndVisual(Toggles.ContrastToggle, Images.ContrastToggleImage, Images.ContrastCheckmarkImage, graphic.contrast);
    }

    private void SetGraphicToggleAndVisual(Toggles toggle, Images toggleImage, Images checkmark, bool isOn)
    {
        GetToggle(toggle).isOn = isOn;
        GetImage(toggleImage).SetVisual(isEnabled: isOn);
        UpdateCheckmark(GetImage(checkmark), isOn);
    }

    private void RefreshAccessPanel()
    {
        var languageLocales = Managers.Localization.GetLanguages();
        string currentLocale = Managers.Config.Option.Access?.language ?? Literal.Languages.Korean;

        for (int index = 0; index < languageLocales.Count; index++)
        {
            if (!languageLocales[index].Equals(currentLocale, StringComparison.OrdinalIgnoreCase))
                continue;

            GetDropdown(Dropdowns.LanguageDropdown).value = index;
            break;
        }
    }

    public override void OnRelease()
    {
        base.OnRelease();
        CloseAllDropdowns();
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
        CloseAllDropdowns();
        _state = state;
        GetPanel(Panels.SoundPanel).SetActivePanel(_state == UI_OptionState.Sound);
        GetPanel(Panels.GraphicPanel).SetActivePanel(_state == UI_OptionState.Graphic);
        GetPanel(Panels.AccessPanel).SetActivePanel(_state == UI_OptionState.Access);
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
        string newText = Mathf.RoundToInt(value * 100f).ToString();

        if (inputField.text != newText)
        {
            inputField.text = newText;

            if (!inputField.isFocused)
                inputField.MoveTextEnd(false);
        }

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
        var scrollbar = GetScrollbar(scrollbarEnum);
        var inputField = GetInputField(inputFieldEnum);

        if (scrollbar == null || inputField == null)
            return;

        scrollbar.BindScrollbar(val =>
        {
            if (_isUpdatingVolume)
                return;

            if (inputField.isFocused)
                inputField.DeactivateInputField();

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

            int clampedPercent = Mathf.Clamp(percent, 0, 100);

            if (percent != clampedPercent)
            {
                _isUpdatingVolume = true;
                inputField.text = clampedPercent.ToString();
                inputField.MoveTextEnd(false);
                _isUpdatingVolume = false;
            }

            _isUpdatingVolume = true;
            scrollbar.value = clampedPercent / 100f;
            _isUpdatingVolume = false;
        }, this);
        inputField.BindInputEndEdit(text =>
        {
            if (_isUpdatingVolume)
                return;

            int percent = string.IsNullOrEmpty(text) || !int.TryParse(text, out int parsedValue) ? 0 : parsedValue;
            percent = Mathf.Clamp(percent, 0, 100);
            _isUpdatingVolume = true;
            inputField.text = percent.ToString();
            scrollbar.value = percent / 100f;
            _isUpdatingVolume = false;
        }, this);
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
        sound.vMaster = GetScrollbar(Scrollbars.MasterScrollbar).value;
        sound.vBGM = GetScrollbar(Scrollbars.BGMScrollbar).value;
        sound.vAmbient = GetScrollbar(Scrollbars.AmbientScrollbar).value;
        sound.vSFX = GetScrollbar(Scrollbars.SFXScrollbar).value;
        sound.vUI = GetScrollbar(Scrollbars.UIScrollbar).value;
        sound.mMaster = GetToggle(Toggles.MasterToggle).isOn;
        sound.mBGM = GetToggle(Toggles.BGMToggle).isOn;
        sound.mAmbient = GetToggle(Toggles.AmbientToggle).isOn;
        sound.mSFX = GetToggle(Toggles.SFXToggle).isOn;
        sound.mUI = GetToggle(Toggles.UIToggle).isOn;
        sound.mute = GetToggle(Toggles.MuteToggle).isOn;
    }

    private void SyncGraphicPanel()
    {
        var graphic = Managers.Config.Option.Graphic;
        int resIndex = GetDropdown(Dropdowns.ResolutionDropdown).value;

        if (_resolutions != null && resIndex < _resolutions.Length)
        {
            var res = _resolutions[resIndex];
            graphic.rWidth = res.width;
            graphic.rHeight = res.height;
            graphic.rRefreshRate = Mathf.RoundToInt((float)res.refreshRateRatio.numerator / res.refreshRateRatio.denominator);
        }

        graphic.screenMode = GetDropdown(Dropdowns.FullscreenDropdown).value switch
        {
            0 => FullScreenMode.FullScreenWindow,
            1 => FullScreenMode.Windowed,
            2 => FullScreenMode.ExclusiveFullScreen,
            _ => FullScreenMode.FullScreenWindow
        };
        graphic.quality = (TextureQuality)GetDropdown(Dropdowns.QualityDropdown).value;
        graphic.vSync = GetToggle(Toggles.VsyncToggle).isOn;
        graphic.antiAliasing = GetToggle(Toggles.AntialiasingToggle).isOn;
        graphic.bloom = GetToggle(Toggles.BloomToggle).isOn;
        graphic.vignette = GetToggle(Toggles.VignetteToggle).isOn;
        graphic.mblur = GetToggle(Toggles.MotionBlurToggle).isOn;
        graphic.contrast = GetToggle(Toggles.ContrastToggle).isOn;
    }

    private void SyncAccessPanel()
    {
        int langIndex = GetDropdown(Dropdowns.LanguageDropdown).value;
        var languageLocales = Managers.Localization.GetLanguages();

        if (languageLocales != null && langIndex < languageLocales.Count)
            Managers.Config.Option.Access.language = languageLocales[langIndex];

        Managers.Config.Option.Access.keybind = Managers.Control.SaveBindingsToJson();
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

    private void CloseAllDropdowns()
    {
        GetDropdown(Dropdowns.ResolutionDropdown)?.Close();
        GetDropdown(Dropdowns.FullscreenDropdown)?.Close();
        GetDropdown(Dropdowns.QualityDropdown)?.Close();
        GetDropdown(Dropdowns.LanguageDropdown)?.Close();
    }

    private async UniTask OnClickApply(PointerEventData data)
    {
        try
        {
            await ((Func<UniTask>)(async () =>
            {
                CancelAllRebinds();
                Sync();
                string newLanguage = Managers.Config.Option.Access.language;
                bool isLanguageChanged = !string.Equals(_initialLanguage, newLanguage, StringComparison.OrdinalIgnoreCase);

                if (isLanguageChanged)
                    await Managers.Localization.ChangeLanguageAsync(newLanguage);

                await Managers.Config.SaveAsync();
                _initialKeybindJson = Managers.Config.Option.Access.keybind;
                _initialModifierDash = Managers.Config.Option.Access.modifierDash;
                _initialBindingSnapshot = Managers.Control.CreateBindingSnapshot();
                _initialLanguage = newLanguage;

                if (isLanguageChanged)
                    Managers.UI.RefreshAll();
            })).Lock();
        }
        catch
        {
            Log.Error(LocalizationKey.Log_Option_Popup_ApplyFailed);
        }
    }

    private async UniTask OnClickComplete(PointerEventData data)
    {
        try
        {
            await ((Func<UniTask>)(async () =>
            {
                CancelAllRebinds();
                Sync();
                string newLanguage = Managers.Config.Option.Access.language;
                bool isLanguageChanged = !string.Equals(_initialLanguage, newLanguage, StringComparison.OrdinalIgnoreCase);

                if (isLanguageChanged)
                    await Managers.Localization.ChangeLanguageAsync(newLanguage);

                await Managers.Config.SaveAsync();
                Close();

                if (isLanguageChanged)
                    Managers.UI.RefreshAll();
            })).Lock();
        }
        catch
        {
            Log.Error(LocalizationKey.Log_Option_Popup_CompleteFailed);
        }
    }

    private async UniTask OnClickCancel(PointerEventData data)
    {
        CancelAllRebinds();

        if (!string.IsNullOrEmpty(_initialBindingSnapshot))
            Managers.Control.RestoreBindingSnapshot(_initialBindingSnapshot);

        if (!string.IsNullOrEmpty(_initialKeybindJson))
            Managers.Control.LoadBindingFromJson(_initialKeybindJson);

        Managers.Config.Option.Access.modifierDash = _initialModifierDash;

        if (Managers.Config.Option.Access.language != _initialLanguage)
        {
            Managers.Config.Option.Access.language = _initialLanguage;
            await Managers.Localization.ChangeLanguageAsync(_initialLanguage).Lock();
            Managers.UI.RefreshAll();
        }

        foreach (var slot in _keybinds)
            slot.Refresh();

        Close();
    }

    private async UniTask OnClickDefault(PointerEventData data)
    {
        try
        {
            bool isConfirmed = await Managers.Notify.ConfirmAsync(this, LocalizationKey.UI_Option_Popup_Default_Confirm_Title, LocalizationKey.UI_Option_Popup_Default_Confirm_Message);

            if (!isConfirmed)
                return;

            CancelAllRebinds();
            await ((Func<UniTask>)(async () =>
            {
                await Managers.Config.ResetAsync();
                Managers.Control.ResetBindings();
                Managers.Config.Option.Access.modifierDash = AccessOption.Default.modifierDash;
                Managers.Config.Option.Access.keybind = Managers.Control.SaveBindingsToJson();
                string defaultLang = Managers.Config.Option.Access.language;
                await Managers.Localization.ChangeLanguageAsync(defaultLang);
                await Managers.Config.SaveAsync();
                _initialKeybindJson = Managers.Config.Option.Access.keybind;
                _initialModifierDash = Managers.Config.Option.Access.modifierDash;
                _initialBindingSnapshot = Managers.Control.CreateBindingSnapshot();
                _initialLanguage = defaultLang;
                Refresh();
                Managers.UI.RefreshAll();
            })).Lock();
        }
        catch
        {
            Log.Error(LocalizationKey.Log_Option_Popup_DefaultFailed);
        }
    }

    private void SetText(Texts textEnum, LocalizationKey key) 
        => GetText(textEnum).text = Managers.Localization.Get(key);
}
