using Cysharp.Text;
using R3;
using System;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIKeybindSlot : UISlot
{
    private readonly ReactiveProperty<ButtonState> _resetButtonState = new ReactiveProperty<ButtonState>(ButtonState.Normal);

    private enum Texts
    {
        ActionNameText,
        KeybindButtonText,
        ResetText
    }

    private enum Images
    {
        KeybindButtonImage,
        ResetButtonImage
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

    public override void Init()
    {
        base.Init();
        BindText(typeof(Texts));
        BindImage(typeof(Images));
        BindButton(typeof(Buttons));
        GetImage((int)Images.ResetButtonImage).BindState(_resetButtonState, Define.Atlas.UI_Common, this);
        GetButton((int)Buttons.KeybindButton).BindView(OnKeybindButtonClicked, ViewEvent.LeftClick, this, _resetButtonState);
        GetButton((int)Buttons.ResetButton).BindViewAsButton(OnResetOrSwitchButtonClicked, ViewEvent.LeftClick, this, _resetButtonState);
    }

    public void Setup(string actionName, InputAction action, Action<string, string> onRebindCompleted)
    {
        _actionName = actionName;
        _targetAction = action;
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
        if (_targetAction == null) 
            return;

        _targetAction.Disable();
        int bindingIndex = _targetAction.GetBindingIndex(InputBinding.MaskByGroup(Literal.Schemes.KeyboardAndMouse));

        if (bindingIndex == -1) 
            return;

        GetText((int)Texts.KeybindButtonText).text = Managers.Localization.Get(Localization.UI_Option_Popup_Text_Bind);
        var rebindOperation = _targetAction.PerformInteractiveRebinding(bindingIndex)
        .WithControlsExcluding(Literal.Schemes.Mouse)
        .OnComplete(operation =>
        {
            operation.Dispose();
            _targetAction.Enable();
            Refresh();
        })
        .OnCancel(operation =>
        {
            operation.Dispose();
            _targetAction.Enable();
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
