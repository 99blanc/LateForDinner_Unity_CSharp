using Cysharp.Text;
using Cysharp.Threading.Tasks;
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

    public override void Init()
    {
        base.Init();
        BindText(typeof(Texts));
        BindInputField(typeof(InputFields));
        BindScrollRect(typeof(ScrollRects));
        _inputField = GetInputField((int)InputFields.CommandInputField);
        _inputField.BindInputSubmit(OnPressSubmit, this);
        Managers.Control.Subscribe(Literal.Hotkeys.Up, OnPressUp).AddTo(this);
        Managers.Control.Subscribe(Literal.Hotkeys.Down, OnPressDown).AddTo(this);
        Managers.Control.Subscribe(Literal.Hotkeys.Tab, OnPressTab).AddTo(this);
    }

    public override void Get()
    {
        base.Get();
        _logSubscription?.Dispose();
        _logSubscription = Managers.Log.OnLogAdded.Subscribe(_ => RefreshLogUI());
        ResetInputField();
        RefreshLogUI();
    }

    public override void Release()
    {
        _logSubscription?.Dispose();
        _logSubscription = null;
        base.Release();
    }

    private void RefreshLogUI()
    {
        var contentText = GetText((int)Texts.LogContentText);
        var scrollRect = GetScrollRect((int)ScrollRects.LogScrollRect);

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
        RefreshLogUI();
    }

    public void SetFilter(bool info, bool warning, bool error, bool system)
    {
        _showInfo = info;
        _showWarning = warning;
        _showError = error;
        _showSystem = system;
        RefreshLogUI();
    }

    public void SetSearchKeyword(string keyword)
    {
        _searchKeyword = keyword?.Trim() ?? string.Empty;
        RefreshLogUI();
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
            Log.Error(Localization.UI_Console_System_ProcessFailed, input);
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
            Log.Info(Localization.UI_Console_System_AutoComplete_Candidates, string.Join(", ", candidates));
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
        RefreshLogUI();
    }

    public void SetFilterWarning(bool value)
    {
        _showWarning = value;
        RefreshLogUI();
    }

    public void SetFilterError(bool value)
    {
        _showError = value;
        RefreshLogUI();
    }

    public void SetFilterSystem(bool value)
    {
        _showSystem = value;
        RefreshLogUI();
    }
}
