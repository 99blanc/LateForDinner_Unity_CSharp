using Cysharp.Threading.Tasks;
using R3;
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
    private UISaveSlot[] _slots;

    public override void Init()
    {
        base.Init();
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindScrollRect(typeof(ScrollRects));
        BindPanel(typeof(Panels));
        GetText((int)Texts.PressAnyKeyText).text = Managers.Localization.Get(Localization.UI_Title_Display_Text_Press_Any_Key);
        GetButton((int)Buttons.OptionButton).BindView(OnOptionClicked, ViewEvent.LeftClick, this);
        _slots = new UISaveSlot[Define.Save.Amount];
        var content = GetScrollRect((int)ScrollRects.LoadScrollRect).content;

        for (int index = 0; index < Define.Save.Amount; index++)
        {
            var (slot, rentHandle) = Managers.Pool.Pop<UISaveSlot>(content);
            _slots[index] = slot;
            _slots[index].SetDisplay(this);
        }

        Managers.Save.EnsureSlot(_slots.Length);
        Managers.Control.Subscribe(Literal.Hotkeys.Cancel, Application.Quit).AddTo(this);
        Managers.Control.Subscribe(Literal.Hotkeys.Any, OnAnyPressed).AddTo(this);
        Switch(UI_TitleState.Main);
    }

    public override void Get()
        => Switch(_state);

    private void OnOptionClicked(PointerEventData data)
        => Managers.UI.OpenPopup<UIOptionPopup>();

    private void OnAnyPressed()
        => Switch(UI_TitleState.Load);

    private void Switch(UI_TitleState state)
    {
        _state = state;
        bool isMain = (_state == UI_TitleState.Main);
        GetPanel((int)Panels.MainPanel).SetActivePanel(isMain);
        GetPanel((int)Panels.LoadPanel).SetActivePanel(!isMain);

        if (!isMain)
            Refresh();
    }

    public void Refresh()
    {
        if (_slots == null || _slots.Length == 0) 
            return;

        var slotOrder = Managers.Save.MetaData.SlotOrder;

        for (int index = 0; index < _slots.Length; index++)
        {
            if (index < slotOrder.Count)
            {
                int saveSlotIndex = slotOrder[index];
                _slots[index].SetIndex(saveSlotIndex);
            }
        }
    }
}
