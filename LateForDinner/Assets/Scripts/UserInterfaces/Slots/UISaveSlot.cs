using Cysharp.Threading.Tasks;
using R3;
using System;
using UnityEngine.EventSystems;
using LateForDinner.Data;

public class UISaveSlot : UISlot
{
    private readonly ReactiveProperty<ButtonState> _button = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _upButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _downButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);

    private enum Images
    {
        SlotImage,
        UpButtonImage,
        DownButtonImage
    }

    private enum Texts
    {
        DayText,
        TagText,
        SaveTimeText
    }

    private enum Buttons
    {
        SlotButton,
        UpButton,
        DownButton
    }

    private UITitleDisplay _display;
    private Action<int> _onSlotSelected;
    private int _index;

    public override void Init()
    {
        base.Init();
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        GetImage(Images.SlotImage).BindState(_button, Define.Atlas.Common, this);
        GetImage(Images.UpButtonImage).BindStateAsArrow(_upButton, Define.Atlas.Common, this);
        GetImage(Images.DownButtonImage).BindStateAsArrow(_downButton, Define.Atlas.Common, this);
        GetButton(Buttons.SlotButton).BindViewAsToggle(data => OnClickSlot(data), ViewEvent.LeftClick, this, _button);
        GetButton(Buttons.UpButton).BindViewAsButton(data => OnClickUp(data).Forget(), ViewEvent.LeftClick, this, _upButton);
        GetButton(Buttons.DownButton).BindViewAsButton(data => OnClickDown(data).Forget(), ViewEvent.LeftClick, this, _downButton);
    }

    public override void Get()
    {
        base.Get();
        SetSelected(false);
    }

    public override void Refresh()
    {
        SlotMeta meta = Managers.Save.MetaData.Slots[_index];
        RefreshArrow();
        RefreshTag();

        if (meta.IsActive)
        {
            string year = (meta.Year % 100).ToString("D2");
            string month = meta.Month.ToString("D2");
            string date = meta.Date.ToString("D2");
            SetText(Texts.DayText, LocalizationKey.Slot_Day_Format, meta.Day);
            SetText(Texts.SaveTimeText, LocalizationKey.Slot_SaveTime_Format, year, month, date);
            return;
        }

        SetText(Texts.DayText, LocalizationKey.None);
        SetText(Texts.SaveTimeText, string.Empty);
    }

    private void RefreshArrow()
    {
        var slotOrder = Managers.Save.MetaData.SlotOrder;
        int currentPos = slotOrder.IndexOf(_index);
        bool canMoveUp = currentPos > 0;
        bool canMoveDown = currentPos >= 0 && currentPos < slotOrder.Count - 1;
        _upButton.Value = canMoveUp ? ButtonState.Normal : ButtonState.Disable;
        _downButton.Value = canMoveDown ? ButtonState.Normal : ButtonState.Disable;
    }

    public void RefreshTag()
    {
        if (_index == 0)
            SetText(Texts.TagText, LocalizationKey.Slot_Auto);
        else
            SetText(Texts.TagText, _index.ToString());
    }

    public void SetIndex(int index, Action<int> onSlotSelected)
    {
        _index = index;
        _onSlotSelected = onSlotSelected;
        Refresh();
    }

    private void OnClickSlot(PointerEventData data)
        => _onSlotSelected?.Invoke(_index);

    private async UniTask OnClickUp(PointerEventData data)
    {
        try
        {
            var slotOrder = Managers.Save.MetaData.SlotOrder;
            int currentPos = slotOrder.IndexOf(_index);

            if (currentPos <= 0)
                return;

            int targetIndex = slotOrder[currentPos - 1];
            await Managers.Save.SwapSlotOrderAsync(_index, targetIndex);
            _display?.Refresh();
        }
        catch
        {
            Log.Error(LocalizationKey.Log_Save_Slot_MoveUpFailed, _index);
        }
    }

    private async UniTask OnClickDown(PointerEventData data)
    {
        try
        {
            var slotOrder = Managers.Save.MetaData.SlotOrder;
            int currentPos = slotOrder.IndexOf(_index);

            if (currentPos < 0 || currentPos >= slotOrder.Count - 1)
                return;

            int targetIndex = slotOrder[currentPos + 1];
            await Managers.Save.SwapSlotOrderAsync(_index, targetIndex);
            _display?.Refresh();
        }
        catch
        {
            Log.Error(LocalizationKey.Log_Save_Slot_MoveDownFailed, _index);
        }
    }

    public void SetSelected(bool isSelected)
        => _button.Value = isSelected ? ButtonState.Disable : ButtonState.Normal;

    public void SetDisplay(UITitleDisplay display) 
        => _display = display;

    private void SetText(Texts textEnum, LocalizationKey key) 
        => GetText(textEnum).text = Managers.Localization.Get(key);

    private void SetText<T1>(Texts textEnum, LocalizationKey key, T1 arg1) 
        => GetText(textEnum).text = Managers.Localization.Get(key, arg1);

    private void SetText<T1, T2, T3>(Texts textEnum, LocalizationKey key, T1 arg1, T2 arg2, T3 arg3) 
        => GetText(textEnum).text = Managers.Localization.Get(key, arg1, arg2, arg3);

    private void SetText(Texts textEnum, string text) 
        => GetText(textEnum).text = text;
}
