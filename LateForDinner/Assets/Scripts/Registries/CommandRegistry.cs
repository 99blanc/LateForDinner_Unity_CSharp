using Cysharp.Threading.Tasks;
using UnityEngine;

public class CommandRegistry
{
    private ConsoleManager _console;

    public void RegisterDefaultCommands(ConsoleManager console)
    {
        _console = console;
        _console.RegisterCommand("help", OnCommandHelp, Managers.Localization.Get(Localization.Command_Desc_Help));
        _console.RegisterCommand("debug", OnCommandToggleDebug, Managers.Localization.Get(Localization.Command_Desc_Debug));
        _console.RegisterCommand("clear", OnCommandClear, Managers.Localization.Get(Localization.Command_Desc_Clear));
        _console.RegisterCommand("fps", OnCommandToggleFPS, Managers.Localization.Get(Localization.Command_Desc_FPS));
        _console.RegisterCommand("time", OnCommandTimeScale, Managers.Localization.Get(Localization.Command_Desc_Time));
        _console.RegisterCommand("set", OnCommandSetVariable, Managers.Localization.Get(Localization.Command_Desc_Set));
        _console.RegisterCommand("get", OnCommandGetVariable, Managers.Localization.Get(Localization.Command_Desc_Get));
        _console.RegisterCommand("log_search", OnCommandLogSearch, Managers.Localization.Get(Localization.Command_Desc_LogSearch));
        _console.RegisterCommand("log_filter", OnCommandLogFilter, Managers.Localization.Get(Localization.Command_Desc_LogFilter));
    }

    private void OnCommandHelp(string[] args)
    {
        if (args.Length > 0)
        {
            string cmdName = args[0].ToLower();
            string desc = _console.GetDescription(cmdName);

            if (!string.IsNullOrEmpty(desc))
                Log.Info(Localization.Console_Help_Detail, cmdName, desc);
            else
                Log.Warning(Localization.Command_Help_NotFound, cmdName);

            return;
        }

        Log.Info(Localization.Console_Help_Header);

        foreach (var cmd in _console.GetCommandNames())
        {
            string desc = _console.GetDescription(cmd);
            Log.Info(Localization.Console_Help_Format, cmd, desc);
        }
    }

    private void OnCommandToggleDebug(string[] args)
    {
        var debug = Managers.Config.Option.Debug;
        debug.isDebugMode = !debug.isDebugMode;
        Managers.Config.SaveAsync().Forget();
        Log.Info(Localization.Command_Debug_Toggle, debug.isDebugMode.ToString());
    }

    private void OnCommandClear(string[] args)
    {
        var uiConsole = Managers.UI.GetSystem<UIConsoleSystem>();

        if (uiConsole != null)
        {
            uiConsole.ClearLogs();
            Log.Info(Localization.Command_Clear_Success);
        }
        else
            Log.Warning(Localization.Command_Clear_NotOpen);
    }

    private void OnCommandToggleFPS(string[] args)
    {
        var fpsSystem = Managers.UI.GetSystem<UIFPSSystem>();

        if (fpsSystem != null)
        {
            Managers.UI.Close(fpsSystem);
            Log.Info(Localization.Command_FPS_Disabled);
        }
        else
        {
            Managers.UI.OpenSystem<UIFPSSystem>();
            Log.Info(Localization.Command_FPS_Enabled);
        }
    }

    private void OnCommandTimeScale(string[] args)
    {
        if (args.Length > 0 && float.TryParse(args[0], out float scale))
        {
            Time.timeScale = scale;
            Log.Info(Localization.Command_Time_Set, scale);
        }
        else
            Log.Info(Localization.Command_Time_Current, Time.timeScale);
    }

    private void OnCommandSetVariable(string[] args)
    {
        if (args.Length < 2)
        {
            Log.Warning(Localization.Command_Set_Usage);
            return;
        }

        _console.SetVariable(args[0], args[1]);
        Log.Info(Localization.Command_Set_Success, args[0], args[1]);
    }

    private void OnCommandGetVariable(string[] args)
    {
        if (args.Length < 1)
        {
            Log.Warning(Localization.Command_Get_Usage);
            return;
        }

        string val = _console.GetVariable(args[0]);

        if (val != null)
            Log.Info(Localization.Command_Get_Success, args[0], val);
        else
            Log.Warning(Localization.Command_Get_NotFound, args[0]);
    }

    private void OnCommandLogSearch(string[] args)
    {
        var uiConsole = Managers.UI.GetSystem<UIConsoleSystem>();

        if (uiConsole == null)
            return;

        if (args.Length == 0)
        {
            uiConsole.SetSearchKeyword(string.Empty);
            Log.Info(Localization.Command_LogSearch_Reset);
        }
        else
        {
            string keyword = args[0];
            uiConsole.SetSearchKeyword(keyword);
            Log.Info(Localization.Command_LogSearch_Filtered, keyword);
        }
    }

    private void OnCommandLogFilter(string[] args)
    {
        if (args == null || args.Length < 2)
        {
            Log.Warning(Localization.Command_LogFilter_Usage);
            return;
        }

        var uiConsole = Managers.UI.GetSystem<UIConsoleSystem>();

        if (uiConsole == null)
            return;

        string target = args[0].ToLower();

        if (!bool.TryParse(args[1], out bool value))
        {
            Log.Warning(Localization.Command_LogFilter_InvalidBool);
            return;
        }

        switch (target)
        {
            case "info":
                uiConsole.SetFilterInfo(value);
                break;
            case "warn":
            case "warning":
                uiConsole.SetFilterWarning(value);
                break;
            case "error":
                uiConsole.SetFilterError(value);
                break;
            case "system":
                uiConsole.SetFilterSystem(value);
                break;
            default:
                Log.Warning(Localization.Command_LogFilter_UnknownType, target);
                return;
        }

        Log.Info(Localization.Command_LogFilter_Success, target, value);
    }
}
