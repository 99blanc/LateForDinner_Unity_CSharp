using Cysharp.Threading.Tasks;
using R3;
using UnityEngine.EventSystems;
using LateForDinner.Data;
using UnityEngine;

public class UISaveDetailPopup : UIPopup, IFocusablePopup
{
    private enum Images
    {
        MealTimeImage,
        CharacterImage,
        TrashButtonImage,
        PlayButtonImage,
        TrashImage
    }

    private enum Texts
    {
        DayText,
        DayTimeText,
        SaveTimeText,
        PlayButtonText
    }

    private enum Buttons
    {
        TrashButton,
        PlayButton
    }

    private readonly ReactiveProperty<ButtonState> _playButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private readonly ReactiveProperty<ButtonState> _trashButton = new ReactiveProperty<ButtonState>(ButtonState.Normal);
    private int? _selectedSlotIndex;

    public override void OnInit()
    {
        base.OnInit();
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindButtonStates();
        BindButtonActions();
    }

    public override void Refresh()
    {
        base.Refresh();
        SetText(Texts.PlayButtonText, LocalizationKey.Play);
    }

    public override void OnRelease()
    {
        base.OnRelease();
        var display = Managers.UI.GetDisplay<UITitleDisplay>();

        if (display != null && _selectedSlotIndex.HasValue)
            display.ClearSlotSelection();

        _selectedSlotIndex = null;
    }

    private void BindButtonStates()
    {
        GetImage(Images.PlayButtonImage).BindState(_playButton, Define.Atlas.Common, this);
        GetImage(Images.TrashButtonImage).BindState(_trashButton, Define.Atlas.Common, this);
    }

    private void BindButtonActions()
    {
        GetButton(Buttons.PlayButton).BindViewAsButton(async data => await OnClickPlay(data), ViewEvent.LeftClick, this, _playButton);
        GetButton(Buttons.TrashButton).BindViewAsButton(async data => await OnClickTrash(data), ViewEvent.LeftClick, this, _trashButton);
    }

    public void Setup(int slotIndex)
    {
        _selectedSlotIndex = slotIndex;
        SlotMeta meta = Managers.Save.MetaData.Slots[slotIndex];
        bool isAutoSlot = (slotIndex == 0);

        if (meta.IsActive)
        {
            string year = (meta.Year % 100).ToString("D2");
            string month = meta.Month.ToString("D2");
            string date = meta.Date.ToString("D2");
            SetText(Texts.DayText, LocalizationKey.Slot_Day_Format, meta.Day);
            SetText(Texts.DayTimeText, LocalizationKey.Slot_DayTime_Format, meta.Hour, meta.Minute, meta.Second);
            SetText(Texts.SaveTimeText, LocalizationKey.Slot_SaveTime_Format, year, month, date);
            SetCharacterImageActive(true);
            SetMealImageActive(true);
            SetMealImageSprite(meta.Meal.ToSpriteAsMealTime());
            GetImage(Images.TrashImage).SetVisual(isEnabled: true);
            _trashButton.Value = ButtonState.Normal;
            _playButton.Value = isAutoSlot ? ButtonState.Disable : ButtonState.Normal;
        }
        else
        {
            SetText(Texts.DayText, LocalizationKey.Slot_Day_Format, Define.Day.Start);
            SetText(Texts.SaveTimeText, string.Empty);
            SetText(Texts.DayTimeText, string.Empty);
            SetCharacterImageActive(false);
            SetMealImageActive(false);
            GetImage(Images.TrashImage).SetVisual(isEnabled: false);
            _trashButton.Value = ButtonState.Disable;
            _playButton.Value = isAutoSlot ? ButtonState.Disable : ButtonState.Normal;
        }
    }

    private async UniTask OnClickPlay(PointerEventData data)
    {
        if (!_selectedSlotIndex.HasValue)
            return;

        try
        {
            int index = _selectedSlotIndex.Value;
            SlotMeta meta = Managers.Save.MetaData.Slots[index];

            if (meta.IsActive)
            {
                await Managers.Game.OldgameAsync(index);
            }
            else
            {
                await Managers.Game.NewgameAsync(index);
                Setup(index);
                Managers.UI.GetDisplay<UITitleDisplay>()?.Refresh();
            }
        }
        catch
        {
            Log.Error(LocalizationKey.Log_Save_Slot_SlotClickFailed, _selectedSlotIndex.Value);
        }
    }

    private async UniTask OnClickTrash(PointerEventData data)
    {
        if (!_selectedSlotIndex.HasValue)
            return;

        int index = _selectedSlotIndex.Value;
        SlotMeta meta = Managers.Save.MetaData.Slots[index];
        
        if (!meta.IsActive) 
            return;

        bool isConfirmed = await Managers.Notify.ConfirmAsync(this, LocalizationKey.UI_SaveDetail_Popup_Delete_Confirm_Title, LocalizationKey.UI_SaveDetail_Popup_Delete_Confirm_Message);
        
        if (!isConfirmed) 
            return;

        await Managers.Save.ClearAsync(index).Lock();
        Setup(index);
        Managers.UI.GetDisplay<UITitleDisplay>()?.Refresh();
    }

    private void SetText(Texts textEnum, string text)
        => GetText(textEnum).text = text;
    private void SetText(Texts textEnum, LocalizationKey key)
        => GetText(textEnum).text = Managers.Localization.Get(key);
    private void SetText<T1>(Texts textEnum, LocalizationKey key, T1 arg1)
        => GetText(textEnum).text = Managers.Localization.Get(key, arg1);
    private void SetText<T1, T2, T3>(Texts textEnum, LocalizationKey key, T1 arg1, T2 arg2, T3 arg3)
        => GetText(textEnum).text = Managers.Localization.Get(key, arg1, arg2, arg3);

    private void SetMealImageActive(bool isActive)
    {
        var image = GetImage(Images.MealTimeImage);

        if (image != null) 
            image.gameObject.SetActive(isActive);
    }

    private void SetMealImageSprite(string spriteName)
    {
        var image = GetImage(Images.MealTimeImage);

        if (image != null) 
            image.sprite = Managers.Resource.GetSprite(Define.Atlas.Common, spriteName);
    }

    private void SetCharacterImageActive(bool isActive)
    {
        var image = GetImage(Images.CharacterImage);

        if (image != null)
            image.gameObject.SetActive(isActive);
    }
}
