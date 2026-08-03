using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

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
        GetImage((int)Images.SlotImage)?.BindButton(_button, Define.Atlas.UI_Common, this);
        GetButton((int)Buttons.SlotButton).BindState(_button, OnSlotClicked, this, SwitchButton);
        GetImage((int)Images.UpButtonImage)?.BindArrowButton(_upButton, Define.Atlas.UI_Common, this);
        GetButton((int)Buttons.UpButton).BindState(_upButton, OnUpButtonClicked, this);
        GetImage((int)Images.DownButtonImage)?.BindArrowButton(_downButton, Define.Atlas.UI_Common, this);
        GetButton((int)Buttons.DownButton).BindState(_downButton, OnDownButtonClicked, this);
    }

    public void SetIndex(int index)
    {
        _index = index;
        SwitchButton();
        RefreshArrowButton();
        Refresh().Forget();
    }

    public async UniTask Refresh()
    {
        SlotMeta meta = meta = Managers.Save.MetaData.Slots[_index];
        var slotText = GetText((int)Texts.SlotText);
        var dayText = GetText((int)Texts.DayText);
        var timeText = GetText((int)Texts.TimeText);
        var mealTimeImage = GetImage((int)Images.MealTimeImage);
        SwitchButton();
        RefreshArrowButton();

        if (meta.IsActive)
        {
            slotText.text = Managers.Localization.Get(Localization.UI_Save_Slot_Text_Slot, meta.Day);
            dayText.text = Managers.Localization.Get(Localization.UI_Save_Slot_Text_Day, meta.Year, meta.Month, meta.Date);
            timeText.text = Managers.Localization.Get(Localization.UI_Save_Slot_Text_Time, meta.Hour, meta.Minute, meta.Second);
            string name = meta.Meal.ToSpriteAsMealTime();
            Sprite sprite = await Managers.Resource.LoadSpriteAsync(Define.Atlas.UI_Common, name);

            if (sprite != null)
            {
                mealTimeImage.sprite = sprite;
                mealTimeImage.gameObject.SetActive(true);
            }
        }
        else
        {
            slotText.text = Managers.Localization.Get(Localization.UI_Save_Slot_Text_None);
            dayText.text = string.Empty;
            timeText.text = string.Empty;
            mealTimeImage?.gameObject.SetActive(false);
        }
    }

    private void RefreshArrowButton()
    {
        var slotOrder = Managers.Save.MetaData.SlotOrder;
        int currentPos = slotOrder.IndexOf(_index);
        bool canMoveUp = currentPos > 0;
        _upButton.Value = canMoveUp ? ButtonState.Normal : ButtonState.Disable;
        bool canMoveDown = currentPos >= 0 && currentPos < slotOrder.Count - 1;
        _downButton.Value = canMoveDown ? ButtonState.Normal : ButtonState.Disable;
    }

    private void SwitchButton()
    {
        SlotMeta meta = Managers.Save.MetaData.Slots[_index];
        _button.Value = meta.IsActive ? ButtonState.Normal : ButtonState.New;
    }

    private async void OnSlotClicked()
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

                await Refresh();
            }
        });
    }

    private async void OnUpButtonClicked()
    {
        var slotOrder = Managers.Save.MetaData.SlotOrder;
        int currentPos = slotOrder.IndexOf(_index);

        if (currentPos > 0)
        {
            int targetIndex = slotOrder[currentPos - 1];

            await Managers.Save.SwapSlotOrderAsync(_index, targetIndex);

            _screen.RefreshSlots();
        }
    }

    private async void OnDownButtonClicked()
    {
        var slotOrder = Managers.Save.MetaData.SlotOrder;
        int currentPos = slotOrder.IndexOf(_index);

        if (currentPos >= 0 && currentPos < slotOrder.Count - 1)
        {
            int targetIndex = slotOrder[currentPos + 1];
            await Managers.Save.SwapSlotOrderAsync(_index, targetIndex);

            _screen?.RefreshSlots();
        }
    }

    public void SetScreen(UITitleScreen screen)
        => _screen = screen;

    public override void Close() 
    { 
        // DESC :: 풀링되지 않는 오브젝트로 관리
    }
}
