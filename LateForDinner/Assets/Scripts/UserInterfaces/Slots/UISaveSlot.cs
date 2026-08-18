using Cysharp.Threading.Tasks;
using R3;
using UnityEngine.EventSystems;

public class UISaveSlot : UISlot
{
    private readonly ReactiveProperty<ButtonState> _button = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _upButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _downButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private UITitleDisplay _display;
    private int _index;

    private enum Images
    {
        MealTimeImage,
        SlotImage,
        UpButtonImage,
        DownButtonImage
    }

    private enum Texts
    {
        DayText,
        TimeText,
        SlotText
    }

    private enum Buttons
    {
        SlotButton,
        UpButton,
        DownButton
    }

    public override void Init()
    {
        base.Init();
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        GetImage((int)Images.SlotImage).BindState(_button, Define.Atlas.Common, this);
        GetImage((int)Images.UpButtonImage).BindStateAsArrow(_upButton, Define.Atlas.Common, this);
        GetImage((int)Images.DownButtonImage).BindStateAsArrow(_downButton, Define.Atlas.Common, this);
        GetButton((int)Buttons.SlotButton).BindViewAsButton(async data => await OnClickSlot(data), ViewEvent.LeftClick, this, _button);
        GetButton((int)Buttons.UpButton).BindViewAsButton(data => OnClickUp(data).Forget(), ViewEvent.LeftClick, this, _upButton);
        GetButton((int)Buttons.DownButton).BindViewAsButton(data => OnClickDown(data).Forget(), ViewEvent.LeftClick, this, _downButton);
    }

    public void SetIndex(int index)
    {
        _index = index;
        Refresh();
    }

    public void Refresh()
    {
        SlotMeta meta = Managers.Save.MetaData.Slots[_index];
        RefreshArrow();

        if (meta.IsActive)
        {
            SetText(Texts.SlotText, Localization.UI_Save_Slot_Text_Slot, meta.Day);
            SetText(Texts.DayText, Localization.UI_Save_Slot_Text_Day, meta.Year, meta.Month, meta.Date);
            SetText(Texts.TimeText, Localization.UI_Save_Slot_Text_Time, meta.Hour, meta.Minute, meta.Second);
            SetMealImageActive(true);
            SetMealImageSprite(meta.Meal.ToSpriteAsMealTime());
            return;
        }

        SetText(Texts.SlotText, Localization.None);
        SetText(Texts.DayText, string.Empty);
        SetText(Texts.TimeText, string.Empty);
        SetMealImageActive(false);
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

    private async UniTask OnClickSlot(PointerEventData data)
    {
        SlotMeta meta = Managers.Save.MetaData.Slots[_index];

        if (meta.IsActive)
        {
            await Managers.Save.LoadAsync(_index).Lock();
            return;
        }

        Managers.Save.NewGame(_index);
        Refresh();
    }

    private async UniTask OnClickUp(PointerEventData data)
    {
        var slotOrder = Managers.Save.MetaData.SlotOrder;
        int currentPos = slotOrder.IndexOf(_index);

        if (currentPos <= 0)
            return;

        int targetIndex = slotOrder[currentPos - 1];
        await Managers.Save.SwapSlotOrderAsync(_index, targetIndex);
        _display.Refresh();
    }

    private async UniTask OnClickDown(PointerEventData data)
    {
        var slotOrder = Managers.Save.MetaData.SlotOrder;
        int currentPos = slotOrder.IndexOf(_index);

        if (currentPos < 0 || currentPos >= slotOrder.Count - 1)
            return;

        int targetIndex = slotOrder[currentPos + 1];
        await Managers.Save.SwapSlotOrderAsync(_index, targetIndex);
        _display.Refresh();
    }

    public void SetDisplay(UITitleDisplay display) 
        => _display = display;

    private void SetText(Texts textEnum, Localization key) 
        => GetText((int)textEnum).text = Managers.Localization.Get(key);

    private void SetText<T1>(Texts textEnum, Localization key, T1 arg1) 
        => GetText((int)textEnum).text = Managers.Localization.Get(key, arg1);

    private void SetText<T1, T2, T3>(Texts textEnum, Localization key, T1 arg1, T2 arg2, T3 arg3) 
        => GetText((int)textEnum).text = Managers.Localization.Get(key, arg1, arg2, arg3);

    private void SetText(Texts textEnum, string text) 
        => GetText((int)textEnum).text = text;

    private void SetMealImageActive(bool isActive)
    {
        var image = GetImage((int)Images.MealTimeImage);

        if (image != null)
            image.gameObject.SetActive(isActive);
    }

    private void SetMealImageSprite(string spriteName)
    {
        var image = GetImage((int)Images.MealTimeImage);

        if (image != null)
            image.sprite = Managers.Resource.GetSprite(Define.Atlas.Common, spriteName);
    }
}
