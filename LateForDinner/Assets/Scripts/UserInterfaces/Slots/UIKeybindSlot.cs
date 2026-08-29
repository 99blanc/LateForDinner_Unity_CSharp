using Cysharp.Text;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using ZLinq;

public class UIKeybindSlot : UISlot
{
    private enum Images
    {
        KeybindButtonImage,
        ResetButtonImage
    }

    private enum Texts
    {
        ActionNameText,
        KeybindButtonText,
        ResetText
    }

    private enum Buttons
    {
        KeybindButton,
        ResetButton
    }

    private enum UI_KeybindMode 
    { 
        ActionRebind, 
        DashCommandToggle 
    }

    private readonly ReactiveProperty<ButtonState> _resetButtonState = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private UI_KeybindMode _slotMode;
    private InputAction _targetAction;
    private string _cachedActionLocalizationKey;
    private LocalizationKey _cachedResetLocalizationKey;
    private string _previousPath;
    private Action<string, string> _onDuplicated;
    private InputActionRebindingExtensions.RebindingOperation _currentOperation;
    private List<UIKeybindSlot> _keybinds;
    private Func<bool> _isLocked;
    private Action<bool> _setLock;
    private bool _isWaitingForInput;

    public override void Init()
    {
        base.Init();
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        GetImage(Images.ResetButtonImage).BindState(_resetButtonState, Define.Atlas.Common, this);
        GetButton(Buttons.KeybindButton).BindView(OnClickKeybind, ViewEvent.LeftClick, this, _resetButtonState);
        GetButton(Buttons.ResetButton).BindViewAsButton(OnClickReset, ViewEvent.LeftClick, this, _resetButtonState);
    }

    public override void Refresh()
    {
        base.Refresh();
        GetText(Texts.ActionNameText).text = Managers.Localization.Get(_cachedActionLocalizationKey);

        if (_slotMode == UI_KeybindMode.DashCommandToggle)
        {
            bool isModifier = Managers.Config.Option.Access.modifierDash;
            SetKeybindText(isModifier ? LocalizationKey.UI_Option_Popup_Text_Modifier : LocalizationKey.UI_Option_Popup_Text_Tap);
        }
        else
        {
            int index = GetIndex();

            if (_targetAction != null && index != -1)
                SetRawKeybindText(_targetAction.GetBindingDisplayString(index));
        }

        SetResetText(_cachedResetLocalizationKey);
    }

    public void Setup(string action, InputAction target, List<UIKeybindSlot> slots, Func<bool> locked, Action<bool> lockAction, Action<string, string> onDuplicated)
    {
        _slotMode = UI_KeybindMode.ActionRebind;
        _targetAction = target;
        _keybinds = slots;
        _isLocked = locked;
        _setLock = lockAction;
        _onDuplicated = onDuplicated;
        _cachedActionLocalizationKey = ZString.Concat(Literal.Localizations.Action, action);
        _cachedResetLocalizationKey = LocalizationKey.Reset;

        Refresh();
    }

    public void SetupDashCommand(Func<bool> locked, Action<bool> lockAction)
    {
        _slotMode = UI_KeybindMode.DashCommandToggle;
        _targetAction = null;
        _isLocked = locked;
        _setLock = lockAction;
        _cachedActionLocalizationKey = Managers.Localization.Get(LocalizationKey.Action_DashCommand);
        _cachedResetLocalizationKey = LocalizationKey.Switch;

        Refresh();
    }

    private bool IsAnySlotWaiting()
    {
        if (_keybinds == null)
            return _isWaitingForInput;

        if (_isWaitingForInput)
            return true;

        return _keybinds.Any(slot => slot != null && slot._isWaitingForInput);
    }

    private bool IsPopupLocked()
        => IsAnySlotWaiting() || (_isLocked?.Invoke() ?? false);

    private void OnClickKeybind(PointerEventData data)
    {
        if (IsPopupLocked())
            return;

        if (HandleDash())
            return;

        StartRebind();
    }

    private void OnClickReset(PointerEventData data)
    {
        if (_isWaitingForInput)
        {
            _currentOperation?.Cancel();
            return;
        }

        if (IsPopupLocked())
            return;

        if (HandleDash())
            return;

        ResetBinding();
    }

    private bool HandleDash()
    {
        if (_slotMode != UI_KeybindMode.DashCommandToggle)
            return false;

        Managers.Config.Option.Access.modifierDash = !Managers.Config.Option.Access.modifierDash;
        Refresh();
        return true;
    }

    private void StartRebind()
    {
        if (IsPopupLocked() || _targetAction == null)
            return;

        int index = GetIndex();

        if (index == -1)
            return;

        _previousPath = _targetAction.bindings[index].effectivePath;
        _isWaitingForInput = true;
        _setLock?.Invoke(true);
        _targetAction.Disable();
        SetKeybindText(LocalizationKey.UI_Option_Popup_Text_Bind);
        BeginOperation(index);
    }

    public void CancelRebind()
    {
        _currentOperation?.Cancel();
        EndRebind(_currentOperation);
    }

    private int GetIndex()
        => _targetAction.GetBindingIndex(InputBinding.MaskByGroup(Literal.Schemes.KeyboardAndMouse));

    private void BeginOperation(int index)
    {
        try
        {
            _currentOperation?.Dispose();
            _currentOperation = _targetAction.PerformInteractiveRebinding(index)
            .WithControlsExcluding(Literal.Schemes.Mouse)
            .OnComplete(op => OnComplete(op, index))
            .OnCancel(op => EndRebind(op));
            _currentOperation.Start();
        }
        catch
        {
            Log.Error(LocalizationKey.Log_Keybind_Slot_RebindFailed, _cachedActionLocalizationKey);
            EndRebind(_currentOperation);
        }
    }

    private void OnComplete(InputActionRebindingExtensions.RebindingOperation op, int index)
    {
        if (IsDuplicate(index, out string duplicateActionName, out string duplicateKeyName))
        {
            if (!string.IsNullOrEmpty(_previousPath))
                _targetAction.ApplyBindingOverride(index, _previousPath);

            _onDuplicated?.Invoke(duplicateActionName, duplicateKeyName);
            Retry(op, index);
            return;
        }

        EndRebind(op);
    }

    private bool IsDuplicate(int index, out string duplicateActionName, out string duplicateKeyName)
    {
        duplicateActionName = string.Empty;
        duplicateKeyName = string.Empty;

        if (_keybinds == null)
            return false;

        string path = _targetAction.bindings[index].effectivePath;
        var duplicatedSlot = _keybinds
        .Where(s => s._targetAction != null)
        .FirstOrDefault(s => s != this && s.MatchPath(path));

        if (duplicatedSlot != null)
        {
            int targetIndex = duplicatedSlot.GetIndex();

            if (targetIndex != -1)
            {
                duplicateActionName = Managers.Localization.Get(duplicatedSlot._cachedActionLocalizationKey);
                duplicateKeyName = duplicatedSlot._targetAction.GetBindingDisplayString(targetIndex);
            }

            return true;
        }

        return false;
    }

    private bool MatchPath(string path)
        => GetIndex() != -1 && _targetAction.bindings[GetIndex()].effectivePath == path;

    private void Retry(InputActionRebindingExtensions.RebindingOperation op, int index)
    {
        EndRebind(op);
        StartRebind();
    }

    private void EndRebind(InputActionRebindingExtensions.RebindingOperation op)
    {
        _isWaitingForInput = false;

        if (_currentOperation == op)
            _currentOperation = null;

        if (_targetAction != null)
            _targetAction.Enable();

        op?.Dispose();
        _setLock?.Invoke(false);
        Refresh();
    }

    private void ResetBinding()
    {
        if (_targetAction == null)
            return;

        _targetAction.RemoveAllBindingOverrides();
        Refresh();
    }

    private void SetResetText(LocalizationKey localizationKey)
        => GetText(Texts.ResetText).text = Managers.Localization.Get(localizationKey);

    private void SetKeybindText(LocalizationKey localizationKey)
        => GetText(Texts.KeybindButtonText).text = Managers.Localization.Get(localizationKey);

    private void SetRawKeybindText(string displayText)
        => GetText(Texts.KeybindButtonText).text = displayText;
}
