using Cysharp.Threading.Tasks;
using R3;

public class UISaveSlot : UISlot
{
    private readonly ReactiveProperty<ButtonState> _button = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _upButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _downButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private UITitleScreen _screen;
    private int _index;

    private enum Texts
    {
        DayText,
        TimeText,
        SlotText
    }

    private enum Images
    {
        MealTimeImage,
        SlotImage,
        UpButtonImage,
        DownButtonImage
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
        BindText(typeof(Texts));
        BindImage(typeof(Images));
        BindButton(typeof(Buttons));
        GetImage((int)Images.SlotImage)?.BindButtonState(_button, Define.Atlas.UI_Common, this);
        GetButton((int)Buttons.SlotButton).BindButtonEvent(() => OnSlotClicked().Forget(), this, _button, SwitchButton);
        GetImage((int)Images.UpButtonImage)?.BindButtonArrowState(_upButton, Define.Atlas.UI_Common, this);
        GetButton((int)Buttons.UpButton).BindButtonEvent(() => OnUpButtonClicked().Forget(), this, _upButton);
        GetImage((int)Images.DownButtonImage)?.BindButtonArrowState(_downButton, Define.Atlas.UI_Common, this);
        GetButton((int)Buttons.DownButton).BindButtonEvent(() => OnDownButtonClicked().Forget(), this, _downButton);
    }

    public void SetIndex(int index)
    {
        _index = index;
        SwitchButton();
        RefreshArrow();
        Refresh();
    }

    public void Refresh()
    {
        SlotMeta meta = Managers.Save.MetaData.Slots[_index];
        var slotText = GetText((int)Texts.SlotText);
        var dayText = GetText((int)Texts.DayText);
        var timeText = GetText((int)Texts.TimeText);
        var mealTimeImage = GetImage((int)Images.MealTimeImage);
        SwitchButton();
        RefreshArrow();

        if (meta.IsActive)
        {
            slotText.text = Managers.Localization.Get(Localization.UI_Save_Slot_Text_Slot, meta.Day);
            dayText.text = Managers.Localization.Get(Localization.UI_Save_Slot_Text_Day, meta.Year, meta.Month, meta.Date);
            timeText.text = Managers.Localization.Get(Localization.UI_Save_Slot_Text_Time, meta.Hour, meta.Minute, meta.Second);
            string name = meta.Meal.ToSpriteAsMealTime();

            if (mealTimeImage == null) 
                return;

            mealTimeImage.sprite = Managers.Resource.GetSpriteFromAtlas(Define.Atlas.UI_Common, name);
            mealTimeImage.gameObject.SetActive(true);
        }
        else
        {
            slotText.text = Managers.Localization.Get(Localization.UI_Save_Slot_Text_None);
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

    private void SwitchButton()
    {
        SlotMeta meta = Managers.Save.MetaData.Slots[_index];
        _button.Value = meta.IsActive ? ButtonState.Normal : ButtonState.New;
    }

    private async UniTaskVoid OnSlotClicked()
    {
        // TODO ::: 슬롯 클릭 시 로드 또는 뉴게임 분기 처리
        await Managers.UI.LockAsync(async () =>
        {
            SlotMeta meta = Managers.Save.MetaData.Slots[_index];

            if (meta.IsActive)
                await Managers.Save.LoadAsync(_index);
            else
            {
                Managers.Save.NewGame(_index);
                Refresh();
            }
        });
    }

    private async UniTaskVoid OnUpButtonClicked()
    {
        var slotOrder = Managers.Save.MetaData.SlotOrder;
        int currentPos = slotOrder.IndexOf(_index);

        if (currentPos > 0)
        {
            int targetIndex = slotOrder[currentPos - 1];

            await Managers.Save.SwapSlotOrderAsync(_index, targetIndex);

            _screen.Refresh();
        }
    }

    private async UniTaskVoid OnDownButtonClicked()
    {
        var slotOrder = Managers.Save.MetaData.SlotOrder;
        int currentPos = slotOrder.IndexOf(_index);

        if (currentPos >= 0 && currentPos < slotOrder.Count - 1)
        {
            int targetIndex = slotOrder[currentPos + 1];
            await Managers.Save.SwapSlotOrderAsync(_index, targetIndex);

            _screen?.Refresh();
        }
    }

    public void SetScreen(UITitleScreen screen)
        => _screen = screen;

    public override void Close() 
    { 
        // DESC :: 풀링되지 않는 오브젝트로 관리
    }
}
