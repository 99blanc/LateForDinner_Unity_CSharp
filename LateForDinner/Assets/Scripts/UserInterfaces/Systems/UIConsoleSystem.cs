using Cysharp.Text;
using R3;
using System;
using TMPro;

public class UIConsoleSystem : UISystem
{
    private enum Texts
    {
        LogContentText
    }

    private enum InputFields
    {
        CommandInputField
    }

    private enum ScrollRects
    {
        LogScrollRect
    }

    private TMP_InputField _inputField;
    private bool _showInfo = true;
    private bool _showWarning = true;
    private bool _showError = true;
    private bool _showSystem = true;
    private string _searchKeyword = string.Empty;
    private int _clearThresholdIndex = 0;
    private IDisposable _logSubscription;

    public override void OnInit()
    {
        base.OnInit();
        BindText(typeof(Texts));
        BindInputField(typeof(InputFields));
        BindScrollRect(typeof(ScrollRects));
        _inputField = GetInputField(InputFields.CommandInputField);
        _inputField.BindInputSubmit(OnPressSubmit, this);
        Managers.Control.Subscribe(Literal.Hotkeys.Up, OnPressUp).RegisterToPool(this);
        Managers.Control.Subscribe(Literal.Hotkeys.Down, OnPressDown).RegisterToPool(this);
        Managers.Control.Subscribe(Literal.Hotkeys.Tab, OnPressTab).RegisterToPool(this);
    }

    public override void OnGet()
    {
        base.OnGet();
        _logSubscription?.Dispose();
        _logSubscription = Managers.Log.OnLogAdded.Subscribe(_ => Refresh());
        Managers.Control.DisableActionMap(Literal.Maps.User);
        ResetInputField();
        Refresh();
    }

    public override void OnRelease()
    {
        base.OnRelease();
        _logSubscription?.Dispose();
        _logSubscription = null;
        Managers.Control.EnableActionMap(Literal.Maps.User);
    }

    public override void Refresh()
    {
        var contentText = GetText(Texts.LogContentText);
        var scrollRect = GetScrollRect(ScrollRects.LogScrollRect);

        if (contentText == null || scrollRect == null)
            return;

        using var sb = ZString.CreateStringBuilder();
        var allLogs = Managers.Log.Logs;

        for (int index = _clearThresholdIndex; index < allLogs.Count; index++)
        {
            var log = allLogs[index];
            bool isVisible = log.Type switch
            {
                LogType.Warning => _showWarning,
                LogType.Error => _showError,
                LogType.System => _showSystem,
                _ => _showInfo
            };

            if (!isVisible)
                continue;

            if (!string.IsNullOrEmpty(_searchKeyword) && !log.Message.Contains(_searchKeyword, System.StringComparison.OrdinalIgnoreCase))
                continue;

            sb.AppendLine(log.Message);
        }

        contentText.text = sb.ToString();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void ClearLogs()
    {
        _clearThresholdIndex = Managers.Log.Logs.Count;
        Refresh();
    }

    public void SetFilter(bool info, bool warning, bool error, bool system)
    {
        _showInfo = info;
        _showWarning = warning;
        _showError = error;
        _showSystem = system;
        Refresh();
    }

    public void SetSearchKeyword(string keyword)
    {
        _searchKeyword = keyword?.Trim() ?? string.Empty;
        Refresh();
    }

    private void OnPressSubmit(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return;

        try
        {
            Managers.Console.ProcessCommand(input);
        }
        catch
        {
            Log.Error(LocalizationKey.Log_Console_System_ProcessFailed, input);
        }
    }

    private void OnPressUp()
    {
        if (!IsInputFocused())
            return;

        SetTextAndMoveEnd(Managers.Console.GetPreviousCommand());
    }

    private void OnPressDown()
    {
        if (!IsInputFocused())
            return;

        SetTextAndMoveEnd(Managers.Console.GetNextCommand());
    }

    private void OnPressTab()
    {
        if (!IsInputFocused())
            return;

        string currentInput = _inputField.text.Trim();
        var candidates = Managers.Console.GetAutoCompleteCandidates(currentInput);

        if (candidates.Count == 1)
            SetTextAndMoveEnd(candidates[0]);
        if (candidates.Count > 1)
            Log.Info(LocalizationKey.UI_Console_System_AutoComplete_Candidates, string.Join(", ", candidates));
    }

    private void ResetInputField()
    {
        if (_inputField == null)
            return;

        _inputField.text = string.Empty;
        _inputField.ActivateInputField();
    }

    private void SetTextAndMoveEnd(string text)
    {
        if (_inputField == null)
            return;

        _inputField.text = text ?? string.Empty;
        _inputField.Select();
        _inputField.MoveTextEnd(false);
        _inputField.ActivateInputField();
    }

    private bool IsInputFocused()
        => _inputField != null && _inputField.isFocused;

    public void SetFilterInfo(bool value)
    {
        _showInfo = value;
        Refresh();
    }

    public void SetFilterWarning(bool value)
    {
        _showWarning = value;
        Refresh();
    }

    public void SetFilterError(bool value)
    {
        _showError = value;
        Refresh();
    }

    public void SetFilterSystem(bool value)
    {
        _showSystem = value;
        Refresh();
    }
}
