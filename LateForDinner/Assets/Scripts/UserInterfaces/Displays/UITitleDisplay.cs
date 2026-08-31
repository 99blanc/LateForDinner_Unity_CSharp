using UnityEngine;
using UnityEngine.EventSystems;

public class UITitleDisplay : UIDisplay
{
    private enum Texts
    {
        PressAnyKeyText
    }

    private enum Buttons
    {
        OptionButton
    }

    private enum ScrollRects
    {
        LoadScrollRect
    }

    private enum Panels
    {
        MainPanel,
        LoadPanel
    }

    private enum UI_TitleState 
    { 
        Main, 
        Load 
    }

    private UI_TitleState _state;
    private UISaveDetailPopup _detailPopup;
    private UISaveSlot[] _slots;
    private int? _currentSelectedIndex;

    public override void OnInit()
    {
        base.OnInit();
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindScrollRect(typeof(ScrollRects));
        BindPanel(typeof(Panels));
        InitButtons();
        InitSaveSlots();
        Managers.Control.Subscribe(Literal.Hotkeys.Cancel, Application.Quit).RegisterToPool(this);
        Managers.Control.Subscribe(Literal.Hotkeys.Any, OnAnyPressed).RegisterToPool(this);
        Switch(UI_TitleState.Main);
    }

    private void InitButtons()
        => GetButton(Buttons.OptionButton).BindView(OnClickOption, ViewEvent.LeftClick, this);

    private void InitSaveSlots()
    {
        Managers.Save.EnsureSlot(Define.Amount.Save);
        _slots = new UISaveSlot[Define.Amount.Save];
        var content = GetScrollRect(ScrollRects.LoadScrollRect).content;

        for (int index = 0; index < Define.Amount.Save; index++)
        {
            var (slot, _) = Managers.Pool.Pop<UISaveSlot>(content);
            _slots[index] = slot;
            _slots[index].SetDisplay(this);
            int slotIndex = index;
            _slots[index].SetIndex(slotIndex, OnClickSlotItem);
        }
    }

    public override void OnGet()
    {
        base.OnGet();
        Switch(_state);
    }

    public override void Refresh()
    {
        base.Refresh();
        SetText(Texts.PressAnyKeyText, LocalizationKey.UI_Title_Display_Text_Press_Any_Key);

        if (_slots == null || _slots.Length == 0)
            return;

        var slotOrder = Managers.Save.MetaData.SlotOrder;

        for (int index = 0; index < _slots.Length; index++)
        {
            if (index >= slotOrder.Count)
                continue;

            int saveSlotIndex = slotOrder[index];
            _slots[index].SetIndex(saveSlotIndex, OnClickSlotItem);
            _slots[index].Refresh();
        }

        UpdateSlotSelection();

        if (_currentSelectedIndex.HasValue && _detailPopup != null)
            _detailPopup.Setup(_currentSelectedIndex.Value);
    }

    private void OnClickOption(PointerEventData data) 
        => Managers.UI.OpenPopup<UIOptionPopup>();

    private void OnAnyPressed() 
        => Switch(UI_TitleState.Load);

    private void OnClickSlotItem(int slotIndex)
    {
        if (_currentSelectedIndex == slotIndex)
        {
            CloseDetailPopup();
            return;
        }

        _currentSelectedIndex = slotIndex;
        UpdateSlotSelection();

        if (_detailPopup == null)
            _detailPopup = Managers.UI.OpenPopup<UISaveDetailPopup>();

        _detailPopup.Setup(slotIndex);
    }

    private void UpdateSlotSelection()
    {
        var slotOrder = Managers.Save.MetaData.SlotOrder;

        for (int index = 0; index < _slots.Length; index++)
        {
            if (index >= slotOrder.Count)
                continue;

            int saveSlotIndex = slotOrder[index];
            bool isSelected = _currentSelectedIndex.HasValue && _currentSelectedIndex.Value == saveSlotIndex;
            _slots[index].SetSelected(isSelected);
        }
    }

    private void CloseDetailPopup()
    {
        if (_detailPopup != null)
        {
            Managers.UI.Close(_detailPopup);
            _detailPopup = null;
        }

        _currentSelectedIndex = null;
        UpdateSlotSelection();
    }

    private void Switch(UI_TitleState state)
    {
        _state = state;
        bool isMain = _state == UI_TitleState.Main;
        GetPanel(Panels.MainPanel).SetActivePanel(isMain);
        GetPanel(Panels.LoadPanel).SetActivePanel(!isMain);

        if (!isMain)
            Refresh();
        else
            CloseDetailPopup();
    }

    private void SetText(Texts textEnum, LocalizationKey key) 
        => GetText(textEnum).text = Managers.Localization.Get(key);
}
