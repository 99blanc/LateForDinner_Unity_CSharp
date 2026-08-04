using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class UITitleScreen : UIScreen
{
    private enum Texts
    {
        PressAnyKeyText
    }

    private enum Images
    {
        OptionImage
    }

    private enum Buttons
    {
        OptionButton
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

    private UI_TitleState _state = UI_TitleState.Main;
    private UISaveSlot[] _slots;

    public override void Init()
    {
        base.Init();
        BindText(typeof(Texts));
        GetText((int)Texts.PressAnyKeyText).text = Managers.Localization.Get(Localization.UI_Title_Screen_Text_Press_Any_Key);
        BindCanvasGroup(typeof(Panels));
        _slots = GetCanvasGroup((int)Panels.LoadPanel)?.GetComponentsInChildren<UISaveSlot>(true);
        Switch(UI_TitleState.Main);

        if (_slots != null && _slots.Length > 0)
        {
            Managers.Save.EnsureSlot(_slots.Length);

            for (int index = 0; index < _slots.Length; index++)
            {
                _slots[index].SetScreen(this);
                _slots[index].SetIndex(index);
            }
        }

        Managers.Control.Subscribe(Literal.Hotkeys.Cancel, () =>
        {
            Application.Quit();
        }).AddTo(this);
        Managers.Control.Subscribe(Literal.Hotkeys.Back, () =>
        {
            if (_state == UI_TitleState.Load)
                Switch(UI_TitleState.Main);
        }).AddTo(this);
        Managers.Control.Subscribe(Literal.Hotkeys.Any, () =>
        {
            if (_state == UI_TitleState.Main)
                Switch(UI_TitleState.Load);
        }).AddTo(this);
    }

    private void Switch(UI_TitleState state)
    {
        _state = state;
        bool isMain = (_state == UI_TitleState.Main);
        var mainGroup = GetCanvasGroup((int)Panels.MainPanel);
        var loadGroup = GetCanvasGroup((int)Panels.LoadPanel);
        mainGroup?.SetActivePanel(isMain);
        loadGroup?.SetActivePanel(!isMain);

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
