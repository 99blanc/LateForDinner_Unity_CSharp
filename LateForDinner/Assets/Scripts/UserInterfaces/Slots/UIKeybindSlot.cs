using Cysharp.Text;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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

    private enum SlotMode 
    { 
        ActionRebind, 
        DashCommandToggle 
    }

    private SlotMode _slotMode;
    private InputAction _targetAction;
    private string _actionName;
    private Action<string, string> _onRebindCompleted;
    private List<UIKeybindSlot> _keybinds;
    private Func<bool> _checkIsRebinding;
    private Action<bool> _setIsRebinding;

    public override void Init()
    {
        base.Init();
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        GetImage((int)Images.ResetButtonImage).BindState(_resetButtonState, Define.Atlas.UI_Common, this);
        GetButton((int)Buttons.KeybindButton).BindView(OnKeybindButtonClicked, ViewEvent.LeftClick, this, _resetButtonState);
        GetButton((int)Buttons.ResetButton).BindViewAsButton(OnResetOrSwitchButtonClicked, ViewEvent.LeftClick, this, _resetButtonState);
    }

    public void Setup(string actionName, InputAction action, List<UIKeybindSlot> popupKeybinds, Func<bool> checkIsRebinding, Action<bool> setIsRebinding, Action<string, string> onRebindCompleted)
    {
        _actionName = actionName;
        _targetAction = action;
        _keybinds = popupKeybinds;
        _checkIsRebinding = checkIsRebinding;
        _setIsRebinding = setIsRebinding;
        _onRebindCompleted = onRebindCompleted;
        GetText((int)Texts.ActionNameText).text = Managers.Localization.Get(ZString.Concat(Literal.Localizations.Action, _actionName));
        GetText((int)Texts.ResetText).text = Managers.Localization.Get(Localization.Reset);
        Refresh();
    }

    public void SetupDashCommand(Action<string, string> onRebindCompleted)
    {
        _slotMode = SlotMode.DashCommandToggle;
        _targetAction = null;
        _onRebindCompleted = onRebindCompleted;
        GetText((int)Texts.ActionNameText).text = Managers.Localization.Get(Localization.Action_DashCommand);
        GetText((int)Texts.ResetText).text = Managers.Localization.Get(Localization.Switch);
        Refresh();
    }

    private void OnKeybindButtonClicked(PointerEventData data)
    {
        if (_slotMode == SlotMode.DashCommandToggle)
        {
            ToggleDashMode();
            return;
        }

        StartInteractiveRebind();
    }

    private void OnResetOrSwitchButtonClicked(PointerEventData data)
    {
        if (_slotMode == SlotMode.DashCommandToggle)
        {
            ToggleDashMode();
            return;
        }

        ResetBinding();
    }

    private void ToggleDashMode()
    {
        bool currentMode = Managers.Config.Option.Access.modifierDash;
        Managers.Config.Option.Access.modifierDash = !currentMode;
        Refresh();
    }

    private void StartInteractiveRebind()
    {
        if (_checkIsRebinding != null && _checkIsRebinding())
            return;

        if (_targetAction == null) 
            return;

        _setIsRebinding?.Invoke(true);
        _targetAction.Disable();
        int bindingIndex = _targetAction.GetBindingIndex(InputBinding.MaskByGroup(Literal.Schemes.KeyboardAndMouse));
        
        if (bindingIndex == -1) 
            return;

        var rebindOperation = _targetAction.PerformInteractiveRebinding(bindingIndex).WithControlsExcluding(Literal.Schemes.Mouse)
        .OnComplete(operation =>
        {
            string newPath = _targetAction.bindings[bindingIndex].effectivePath;
            bool isDuplicate = false;

            if (_keybinds != null)
            {
                foreach (var slot in _keybinds)
                {
                    if (slot == this || slot._targetAction == null) 
                        continue;

                    int otherIndex = slot._targetAction.GetBindingIndex(InputBinding.MaskByGroup(Literal.Schemes.KeyboardAndMouse));
                    
                    if (otherIndex != -1 && slot._targetAction.bindings[otherIndex].effectivePath == newPath)
                    {
                        isDuplicate = true;
                        break;
                    }
                }
            }

            if (isDuplicate)
            {
                _targetAction.RemoveBindingOverride(bindingIndex);
                operation.Dispose();
                StartInteractiveRebind();
                return;
            }

            operation.Dispose();
            _targetAction.Enable();
            _setIsRebinding?.Invoke(false);
            _onRebindCompleted?.Invoke(_actionName, _targetAction.SaveBindingOverridesAsJson());
            Refresh();
        })
        .OnCancel(operation =>
        {
            operation.Dispose();
            _targetAction.Enable();
            _setIsRebinding?.Invoke(false);
            Refresh();
        });

        rebindOperation.Start();
    }

    private void ResetBinding()
    {
        if (_targetAction == null) 
            return;

        _targetAction.RemoveAllBindingOverrides();
        Refresh();
    }

    public void Refresh()
    {
        if (_slotMode == SlotMode.DashCommandToggle)
        {
            bool isModifier = Managers.Config.Option.Access.modifierDash;
            GetText((int)Texts.KeybindButtonText).text = isModifier ? Managers.Localization.Get(Localization.UI_Option_Popup_Text_Modifier) : Managers.Localization.Get(Localization.UI_Option_Popup_Text_Tap);
            return;
        }

        if (_targetAction != null)
        {
            int bindingIndex = _targetAction.GetBindingIndex(InputBinding.MaskByGroup(Literal.Schemes.KeyboardAndMouse));

            if (bindingIndex != -1)
                GetText((int)Texts.KeybindButtonText).text = _targetAction.GetBindingDisplayString(bindingIndex);
        }
    }
}
