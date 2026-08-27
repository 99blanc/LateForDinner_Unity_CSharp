using Cysharp.Threading.Tasks;
using UnityEngine;

public class CommandRegistry
{
    private ConsoleManager _console;

    public void RegisterCommands(ConsoleManager console)
    {
        _console = console;
        // DESC ::: 기본 명령어 등록
        _console.RegisterCommand("help", OnCommandHelp, Managers.Localization.Get(LocalizationKey.Command_Desc_Help));
        _console.RegisterCommand("debug", OnCommandToggleDebug, Managers.Localization.Get(LocalizationKey.Command_Desc_Debug));
        _console.RegisterCommand("clear", OnCommandClear, Managers.Localization.Get(LocalizationKey.Command_Desc_Clear));
        _console.RegisterCommand("fps", OnCommandToggleFPS, Managers.Localization.Get(LocalizationKey.Command_Desc_FPS));
        _console.RegisterCommand("log_search", OnCommandLogSearch, Managers.Localization.Get(LocalizationKey.Command_Desc_LogSearch));
        _console.RegisterCommand("log_filter", OnCommandLogFilter, Managers.Localization.Get(LocalizationKey.Command_Desc_LogFilter));
        UpdateDebugCommands(CheckIsDebugMode());
    }

    private void UpdateDebugCommands(bool isDebugMode)
    {
        if (isDebugMode)
        {
            _console.RegisterCommand("set", OnCommandSetVariable, Managers.Localization.Get(LocalizationKey.Command_Desc_Set));
            _console.RegisterCommand("get", OnCommandGetVariable, Managers.Localization.Get(LocalizationKey.Command_Desc_Get));
            _console.RegisterCommand("time_debug", OnCommandTimeScale, Managers.Localization.Get(LocalizationKey.Command_Desc_Time));
            _console.RegisterCommand("ground_debug", OnCommandToggleGroundDebug, Managers.Localization.Get(LocalizationKey.Command_Desc_Ground));
        }
        else
        {
            _console.UnregisterCommand("set");
            _console.UnregisterCommand("get");
            _console.UnregisterCommand("time_debug");
            _console.UnregisterCommand("ground_debug");
        }
    }

    private void OnCommandHelp(string[] args)
    {
        if (args.Length > 0)
        {
            string cmdName = args[0].ToLower();
            string desc = _console.GetDescription(cmdName);

            if (!string.IsNullOrEmpty(desc))
                Log.Info(LocalizationKey.Console_Help_Detail, cmdName, desc);
            else
                Log.Warning(LocalizationKey.Command_Help_NotFound, cmdName);

            return;
        }

        Log.Info(LocalizationKey.Console_Help_Header);

        foreach (var cmd in _console.GetCommandNames())
        {
            string desc = _console.GetDescription(cmd);
            Log.Info(LocalizationKey.Console_Help_Format, cmd, desc);
        }
    }

    private void OnCommandToggleDebug(string[] args)
    {
        var debug = Managers.Config.Option.Debug;
        debug.isDebugMode = !debug.isDebugMode;
        Managers.Config.SaveAsync().Forget();
        UpdateDebugCommands(debug.isDebugMode);
        Log.Info(LocalizationKey.Command_Debug_Toggle, debug.isDebugMode.ToString());
    }

    private void OnCommandClear(string[] args)
    {
        var uiConsole = Managers.UI.GetSystem<UIConsoleSystem>();

        if (uiConsole != null)
        {
            uiConsole.ClearLogs();
            Log.Info(LocalizationKey.Command_Clear_Success);
        }
        else
            Log.Warning(LocalizationKey.Command_Clear_NotOpen);
    }

    private void OnCommandToggleFPS(string[] args)
    {
        var fpsSystem = Managers.UI.GetSystem<UIFPSSystem>();

        if (fpsSystem != null)
        {
            Managers.UI.Close(fpsSystem);
            Log.Info(LocalizationKey.Command_FPS_Disabled);
        }
        else
        {
            Managers.UI.OpenSystem<UIFPSSystem>();
            Log.Info(LocalizationKey.Command_FPS_Enabled);
        }
    }

    private void OnCommandLogSearch(string[] args)
    {
        var uiConsole = Managers.UI.GetSystem<UIConsoleSystem>();

        if (uiConsole == null)
            return;

        if (args.Length == 0)
        {
            uiConsole.SetSearchKeyword(string.Empty);
            Log.Info(LocalizationKey.Command_LogSearch_Reset);
        }
        else
        {
            string keyword = args[0];
            uiConsole.SetSearchKeyword(keyword);
            Log.Info(LocalizationKey.Command_LogSearch_Filtered, keyword);
        }
    }

    private void OnCommandLogFilter(string[] args)
    {
        if (args == null || args.Length < 2)
        {
            Log.Warning(LocalizationKey.Command_LogFilter_Usage);
            return;
        }

        var uiConsole = Managers.UI.GetSystem<UIConsoleSystem>();

        if (uiConsole == null)
            return;

        string target = args[0].ToLower();

        if (!bool.TryParse(args[1], out bool value))
        {
            Log.Warning(LocalizationKey.Command_LogFilter_InvalidBool);
            return;
        }

        switch (target)
        {
            case Literal.Types.Info:
                uiConsole.SetFilterInfo(value);
                break;
            case Literal.Types.Warn:
            case Literal.Types.Warning:
                uiConsole.SetFilterWarning(value);
                break;
            case Literal.Types.Error:
                uiConsole.SetFilterError(value);
                break;
            case Literal.Types.System:
                uiConsole.SetFilterSystem(value);
                break;
            default:
                Log.Warning(LocalizationKey.Command_LogFilter_UnknownType, target);
                return;
        }

        Log.Info(LocalizationKey.Command_LogFilter_Success, target, value);
    }

    private void OnCommandTimeScale(string[] args)
    {
        if (!CheckIsDebugMode())
            return;

        if (args.Length > 0 && float.TryParse(args[0], out float scale))
        {
            Time.timeScale = scale;
            Log.Info(LocalizationKey.Command_Time_Set, scale);
        }
        else
            Log.Info(LocalizationKey.Command_Time_Current, Time.timeScale);
    }

    private void OnCommandSetVariable(string[] args)
    {
        if (!CheckIsDebugMode())
            return;

        if (args.Length < 2)
        {
            Log.Warning(LocalizationKey.Command_Set_Usage);
            return;
        }

        _console.SetVariable(args[0], args[1]);
        Log.Info(LocalizationKey.Command_Set_Success, args[0], args[1]);
    }

    private void OnCommandGetVariable(string[] args)
    {
        if (!CheckIsDebugMode())
            return;

        if (args.Length < 1)
        {
            Log.Warning(LocalizationKey.Command_Get_Usage);
            return;
        }

        string val = _console.GetVariable(args[0]);

        if (val != null)
            Log.Info(LocalizationKey.Command_Get_Success, args[0], val);
        else
            Log.Warning(LocalizationKey.Command_Get_NotFound, args[0]);
    }

    private void OnCommandToggleGroundDebug(string[] args)
    {
        if (!CheckIsDebugMode())
            return;

        bool currentState = DebugExtensions.ToggleDebugView();
        Log.Info(LocalizationKey.Command_Ground_Toggle, currentState.ToString());
    }

    private bool CheckIsDebugMode()
    {
        if (Managers.Config == null || Managers.Config.Option == null || Managers.Config.Option.Debug == null)
            return false;

        return Managers.Config.Option.Debug.isDebugMode;
    }
}
