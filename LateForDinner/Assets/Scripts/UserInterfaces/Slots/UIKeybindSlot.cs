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
    private readonly ReactiveProperty<ButtonState> _resetButtonState = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    
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

    private enum SlotMode { ActionRebind, DashCommandToggle }

    private SlotMode _slotMode;
    private InputAction _targetAction;
    private string _actionName;
    private Action<string, string> _onRebindCompleted;
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
        GetImage((int)Images.ResetButtonImage).BindState(_resetButtonState, Define.Atlas.Common, this);
        GetButton((int)Buttons.KeybindButton).BindView(OnClickKeybind, ViewEvent.LeftClick, this, _resetButtonState);
        GetButton((int)Buttons.ResetButton).BindViewAsButton(OnClickReset, ViewEvent.LeftClick, this, _resetButtonState);
    }

    public void Setup(string action, InputAction target, List<UIKeybindSlot> slots, Func<bool> locked, Action<bool> lockAction, Action<string, string> complete)
    {
        _actionName = action;
        _targetAction = target;
        _keybinds = slots;
        _isLocked = locked;
        _setLock = lockAction;
        _onRebindCompleted = complete;
        SetActionText(_actionName);
        SetResetText(Localization.Reset);
        Refresh();
    }

    public void SetupDashCommand(Func<bool> locked, Action<bool> lockAction, Action<string, string> complete)
    {
        _slotMode = SlotMode.DashCommandToggle;
        _targetAction = null;
        _isLocked = locked;
        _setLock = lockAction;
        _onRebindCompleted = complete;
        SetActionText(Localization.Action_DashCommand);
        SetResetText(Localization.Switch);
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
    {
        if (IsAnySlotWaiting())
            return true;

        if (_isLocked != null && _isLocked())
            return true;

        return false;
    }

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
        if (_slotMode != SlotMode.DashCommandToggle) 
            return false;

        Managers.Config.Option.Access.modifierDash = !Managers.Config.Option.Access.modifierDash;
        Refresh();
        return true;
    }

    private void StartRebind()
    {
        if (IsPopupLocked() || _targetAction == null) 
            return;

        _isWaitingForInput = true;
        _setLock?.Invoke(true);
        _targetAction.Disable();
        int index = GetIndex();

        if (index == -1)
        {
            CleanUp(null);
            return;
        }

        SetKeybindText(Localization.UI_Option_Popup_Text_Bind);
        BeginOperation(index);
    }

    public void CancelRebind()
    {
        _currentOperation?.Cancel();
        CleanUp(_currentOperation);
        Refresh();
    }

    private int GetIndex()
        => _targetAction.GetBindingIndex(InputBinding.MaskByGroup(Literal.Schemes.KeyboardAndMouse));

    private void BeginOperation(int index)
    {
        _currentOperation?.Dispose();
        _currentOperation = _targetAction.PerformInteractiveRebinding(index)
        .WithControlsExcluding(Literal.Schemes.Mouse)
        .OnComplete(op => OnComplete(op, index))
        .OnCancel(op => OnCancel(op));
        _currentOperation.Start();
    }

    private void OnComplete(InputActionRebindingExtensions.RebindingOperation op, int index)
    {
        if (IsDuplicate(index))
        {
            Retry(op, index);
            return;
        }

        Success(op);
    }

    private bool IsDuplicate(int index)
    {
        if (_keybinds == null)
            return false;

        string path = _targetAction.bindings[index].effectivePath;
        return _keybinds
        .Where(s => s._targetAction != null)
        .Any(s => s != this && s.MatchPath(path));
    }

    private bool MatchPath(string path)
        => GetIndex() != -1 && _targetAction.bindings[GetIndex()].effectivePath == path;


    private void Retry(InputActionRebindingExtensions.RebindingOperation op, int index)
    {
        CleanUp(op);
        Refresh();
        StartRebind();
    }

    private void Success(InputActionRebindingExtensions.RebindingOperation op)
    {
        CleanUp(op);
        _onRebindCompleted?.Invoke(_actionName, _targetAction.SaveBindingOverridesAsJson());
        Refresh();
    }

    private void OnCancel(InputActionRebindingExtensions.RebindingOperation op)
    {
        CleanUp(op);
        Refresh();
    }

    private void CleanUp(InputActionRebindingExtensions.RebindingOperation op)
    {
        _isWaitingForInput = false;

        if (_currentOperation == op)
            _currentOperation = null;

        if (_targetAction != null)
            _targetAction.Enable();

        op?.Dispose();
        _setLock?.Invoke(false);
    }

    private void ResetBinding()
    {
        if (_targetAction == null) 
            return;

        _targetAction.RemoveAllBindingOverrides();
        _onRebindCompleted?.Invoke(_actionName, _targetAction.SaveBindingOverridesAsJson());
        Refresh();
    }

    public void Refresh()
    {
        if (_slotMode == SlotMode.DashCommandToggle)
        {
            bool isModifier = Managers.Config.Option.Access.modifierDash;
            SetKeybindText(isModifier ? Localization.UI_Option_Popup_Text_Modifier : Localization.UI_Option_Popup_Text_Tap);
            return;
        }

        int index = GetIndex();

        if (_targetAction != null && index != -1)
            SetRawKeybindText(_targetAction.GetBindingDisplayString(index));
    }

    private void SetActionText(string actionName)
        => GetText((int)Texts.ActionNameText).text = Managers.Localization.Get(ZString.Concat(Literal.Localizations.Action, actionName));

    private void SetActionText(Localization localizationKey)
        => GetText((int)Texts.ActionNameText).text = Managers.Localization.Get(localizationKey);

    private void SetResetText(Localization localizationKey)
        => GetText((int)Texts.ResetText).text = Managers.Localization.Get(localizationKey);

    private void SetKeybindText(Localization localizationKey)
        => GetText((int)Texts.KeybindButtonText).text = Managers.Localization.Get(localizationKey);

    private void SetRawKeybindText(string displayText)
        => GetText((int)Texts.KeybindButtonText).text = displayText;
}
