using R3;
using System;
using UnityEngine;

public class UIItemDropPopup : UIPopup, IDraggablePopup, IFocusablePopup
{
    private enum Images
    {
        ConfirmButtonImage,
        CancelButtonImage
    }

    private enum Texts
    {
        AlertText,
        MessageText,
        ConfirmButtonText,
        CancelButtonText
    }

    private enum InputFields
    {
        ItemInputField
    }

    private enum Buttons
    {
        ConfirmButton,
        CancelButton
    }

    private enum Scrollbars
    {
        ItemScrollbar
    }

    private readonly ReactiveProperty<ButtonState> _confirmButtonState = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _cancelButtonState = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private Action<int> _onConfirm;
    private Action _onCancel;
    private Func<string> _titleProvider;
    private Func<string> _messageProvider;
    private bool _isUpdatingItemCount;
    private int _maxCount = 1;

    public override void OnInit()
    {
        base.OnInit();
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindInputField(typeof(InputFields));
        BindButton(typeof(Buttons));
        BindScrollbar(typeof(Scrollbars));
    }

    public override void OnGet()
    {
        base.OnGet();
        GetImage(Images.ConfirmButtonImage).BindState(_confirmButtonState, Define.Atlas.Common, this);
        GetImage(Images.CancelButtonImage).BindState(_cancelButtonState, Define.Atlas.Common, this);
        GetButton(Buttons.ConfirmButton).BindViewAsButton(_ => OnClickConfirm(), ViewEvent.LeftClick, this, _confirmButtonState);
        GetButton(Buttons.CancelButton).BindViewAsButton(_ => OnClickCancel(), ViewEvent.LeftClick, this, _cancelButtonState);
        BindItemCountControl(Scrollbars.ItemScrollbar, InputFields.ItemInputField, initialValue: _maxCount);
        this.BindKey(Literal.Hotkeys.Cancel, InputEventType.Triggered, OnClickCancel).RegisterToPool(this);
    }

    public override void Refresh()
    {
        base.Refresh();
        GetText(Texts.AlertText).text = _titleProvider?.Invoke() ?? string.Empty;
        GetText(Texts.MessageText).text = _messageProvider?.Invoke() ?? string.Empty;
    }

    public override void OnRelease()
    {
        base.OnRelease();
        _titleProvider = null;
        _messageProvider = null;
        _onConfirm = null;
        _onCancel = null;
    }

    public void Setup(LocalizationKey titleKey, LocalizationKey messageKey, int quantity, Action<int> onConfirm, Action onCancel = null)
        => SetupCommon(quantity, onConfirm, onCancel, () => Managers.Localization.Get(titleKey), () => Managers.Localization.Get(messageKey));

    public void Setup<T1>(LocalizationKey titleKey, LocalizationKey messageKey, int quantity, Action<int> onConfirm, Action onCancel, T1 arg1)
        => SetupCommon(quantity, onConfirm, onCancel, () => Managers.Localization.Get(titleKey), () => Managers.Localization.Get(messageKey, arg1));

    public void Setup<T1, T2>(LocalizationKey titleKey, LocalizationKey messageKey, int quantity, Action<int> onConfirm, Action onCancel, T1 arg1, T2 arg2)
        => SetupCommon(quantity, onConfirm, onCancel, () => Managers.Localization.Get(titleKey), () => Managers.Localization.Get(messageKey, arg1, arg2));

    public void Setup<T1, T2, T3>(LocalizationKey titleKey, LocalizationKey messageKey, int quantity, Action<int> onConfirm, Action onCancel, T1 arg1, T2 arg2, T3 arg3)
        => SetupCommon(quantity, onConfirm, onCancel, () => Managers.Localization.Get(titleKey), () => Managers.Localization.Get(messageKey, arg1, arg2, arg3));

    public void Setup(LocalizationKey titleKey, LocalizationKey messageKey, int quantity, Action<int> onConfirm, Action onCancel = null, params object[] args)
        => SetupCommon(quantity, onConfirm, onCancel, () => Managers.Localization.Get(titleKey), () => (args != null && args.Length > 0) ? Managers.Localization.Get(messageKey, args) : Managers.Localization.Get(messageKey));

    private void SetupCommon(int quantity, Action<int> onConfirm, Action onCancel, Func<string> titleProvider, Func<string> messageProvider)
    {
        _maxCount = Mathf.Max(1, quantity);
        _onConfirm = onConfirm;
        _onCancel = onCancel;
        _titleProvider = titleProvider;
        _messageProvider = messageProvider;
        Refresh();
        BindItemCountControl(Scrollbars.ItemScrollbar, InputFields.ItemInputField, initialValue: 1);
    }

    private void BindItemCountControl(Scrollbars scrollbarEnum, InputFields inputFieldEnum, int initialValue = 1)
    {
        var scrollbar = GetScrollbar(scrollbarEnum);
        var inputField = GetInputField(inputFieldEnum);

        if (scrollbar == null || inputField == null)
            return;

        scrollbar.interactable = _maxCount > 1;
        int range = Mathf.Max(1, _maxCount - 1);
        int clampedInitial = Mathf.Clamp(initialValue, 1, _maxCount);
        _isUpdatingItemCount = true;
        scrollbar.value = _maxCount == 1 ? 0f : (float)(clampedInitial - 1) / range;
        inputField.text = clampedInitial.ToString();
        _isUpdatingItemCount = false;
        scrollbar.BindScrollbar(val =>
        {
            if (_isUpdatingItemCount)
                return;

            if (inputField.isFocused)
                inputField.DeactivateInputField();

            _isUpdatingItemCount = true;
            int count = _maxCount == 1 ? 1 : Mathf.RoundToInt(val * range) + 1;
            inputField.text = count.ToString();
            _isUpdatingItemCount = false;
        }, this);
        inputField.BindInputField(text =>
        {
            if (_isUpdatingItemCount || string.IsNullOrEmpty(text))
                return;

            if (!int.TryParse(text, out int count))
                return;

            int clampedCount = Mathf.Clamp(count, 1, _maxCount);

            if (count != clampedCount)
            {
                _isUpdatingItemCount = true;
                inputField.text = clampedCount.ToString();
                inputField.MoveTextEnd(false);
                _isUpdatingItemCount = false;
            }

            _isUpdatingItemCount = true;
            scrollbar.value = _maxCount == 1 ? 0f : (float)(clampedCount - 1) / range;
            _isUpdatingItemCount = false;
        }, this);
        inputField.BindInputEndEdit(text =>
        {
            if (_isUpdatingItemCount)
                return;

            int count = string.IsNullOrEmpty(text) || !int.TryParse(text, out int parsedValue) ? 1 : parsedValue;
            count = Mathf.Clamp(count, 1, _maxCount);
            _isUpdatingItemCount = true;
            inputField.text = count.ToString();
            scrollbar.value = _maxCount == 1 ? 0f : (float)(count - 1) / range;
            _isUpdatingItemCount = false;
        }, this);
    }

    public int GetSelectedCount()
    {
        var inputField = GetInputField(InputFields.ItemInputField);

        if (inputField != null && int.TryParse(inputField.text, out int count))
            return Mathf.Clamp(count, 1, _maxCount);

        return 1;
    }

    private void OnClickConfirm()
    {
        int selectedCount = GetSelectedCount();
        _onConfirm?.Invoke(selectedCount);
        Managers.UI.Close(this);
    }

    private void OnClickCancel()
    {
        _onCancel?.Invoke();
        Managers.UI.Close(this);
    }
}
