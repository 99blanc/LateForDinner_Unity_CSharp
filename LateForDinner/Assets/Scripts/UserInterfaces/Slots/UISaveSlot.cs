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
        GetImage((int)Images.SlotImage).BindState(_button, Define.Atlas.UI_Common, this);
        GetImage((int)Images.UpButtonImage).BindStateAsArrow(_upButton, Define.Atlas.UI_Common, this);
        GetImage((int)Images.DownButtonImage).BindStateAsArrow(_downButton, Define.Atlas.UI_Common, this);
        GetButton((int)Buttons.SlotButton).BindViewAsButton(async (data) => await OnSlotClicked(data), ViewEvent.LeftClick, this, _button);
        GetButton((int)Buttons.UpButton).BindViewAsButton(data => OnUpButtonClicked(data).Forget(), ViewEvent.LeftClick, this, _upButton);
        GetButton((int)Buttons.DownButton).BindViewAsButton(data => OnDownButtonClicked(data).Forget(), ViewEvent.LeftClick, this, _downButton);
    }

    public void SetIndex(int index)
    {
        _index = index;
        Refresh();
    }

    public void Refresh()
    {
        SlotMeta meta = Managers.Save.MetaData.Slots[_index];
        var slotText = GetText((int)Texts.SlotText);
        var dayText = GetText((int)Texts.DayText);
        var timeText = GetText((int)Texts.TimeText);
        var mealTimeImage = GetImage((int)Images.MealTimeImage);
        RefreshArrow();

        if (meta.IsActive)
        {
            slotText.text = Managers.Localization.Get(Localization.UI_Save_Slot_Text_Slot, meta.Day);
            dayText.text = Managers.Localization.Get(Localization.UI_Save_Slot_Text_Day, meta.Year, meta.Month, meta.Date);
            timeText.text = Managers.Localization.Get(Localization.UI_Save_Slot_Text_Time, meta.Hour, meta.Minute, meta.Second);
            string name = meta.Meal.ToSpriteAsMealTime();
            mealTimeImage?.gameObject.SetActive(true);

            if (mealTimeImage != null)
                mealTimeImage.sprite = Managers.Resource.GetSprite(Define.Atlas.UI_Common, name);
        }
        else
        {
            slotText.text = Managers.Localization.Get(Localization.None);
            dayText.text = string.Empty;
            timeText.text = string.Empty;
            mealTimeImage?.gameObject.SetActive(false);
        }
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

    private async UniTask OnSlotClicked(PointerEventData data)
    {
        // TODO ::: 슬롯 클릭 시 로드 또는 뉴게임 분기 처리
        SlotMeta meta = Managers.Save.MetaData.Slots[_index];

        if (meta.IsActive)
            await Managers.Save.LoadAsync(_index).Lock();
        else
        {
            Managers.Save.NewGame(_index);
            Refresh();
        }
    }

    private async UniTask OnUpButtonClicked(PointerEventData data)
    {
        var slotOrder = Managers.Save.MetaData.SlotOrder;
        int currentPos = slotOrder.IndexOf(_index);

        if (currentPos > 0)
        {
            int targetIndex = slotOrder[currentPos - 1];

            await Managers.Save.SwapSlotOrderAsync(_index, targetIndex);

            _display.Refresh();
        }
    }

    private async UniTask OnDownButtonClicked(PointerEventData data)
    {
        var slotOrder = Managers.Save.MetaData.SlotOrder;
        int currentPos = slotOrder.IndexOf(_index);

        if (currentPos >= 0 && currentPos < slotOrder.Count - 1)
        {
            int targetIndex = slotOrder[currentPos + 1];

            await Managers.Save.SwapSlotOrderAsync(_index, targetIndex);

            _display.Refresh();
        }
    }

    public void SetDisplay(UITitleDisplay display)
        => _display = display;
}
