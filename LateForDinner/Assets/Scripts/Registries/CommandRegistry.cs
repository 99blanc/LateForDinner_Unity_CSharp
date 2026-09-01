using Cysharp.Threading.Tasks;
using LateForDinner.Data;
using System;
using UnityEngine;

public class CommandRegistry
{
    private ConsoleManager _console;

    public void RegisterCommands(ConsoleManager console)
    {
        _console = console;
        // DESC ::: 기본 명령어 등록
        _console.RegisterCommand("help", OnCommandHelp, Managers.Localization.Get(LocalizationKey.Console_Desc_Help));
        _console.RegisterCommand("debug", OnCommandToggleDebug, Managers.Localization.Get(LocalizationKey.Console_Desc_Debug));
        _console.RegisterCommand("clear", OnCommandClear, Managers.Localization.Get(LocalizationKey.Console_Desc_Clear));
        _console.RegisterCommand("log_search", OnCommandLogSearch, Managers.Localization.Get(LocalizationKey.Console_Desc_LogSearch));
        _console.RegisterCommand("log_filter", OnCommandLogFilter, Managers.Localization.Get(LocalizationKey.Console_Desc_LogFilter));
        UpdateDebugCommands(CheckIsDebugMode());
    }

    private void UpdateDebugCommands(bool isDebugMode)
    {
        // DESC ::: 디버그 명령어 등록
        if (isDebugMode)
        {
            _console.RegisterCommand("set", OnCommandSetVariable, Managers.Localization.Get(LocalizationKey.Console_Desc_Set));
            _console.RegisterCommand("get", OnCommandGetVariable, Managers.Localization.Get(LocalizationKey.Console_Desc_Get));
            _console.RegisterCommand("fps", OnCommandToggleFPS, Managers.Localization.Get(LocalizationKey.Console_Desc_FPS));
            _console.RegisterCommand("time_debug", OnCommandTimeScale, Managers.Localization.Get(LocalizationKey.Console_Desc_Time));
            _console.RegisterCommand("ground_debug", OnCommandToggleGroundDebug, Managers.Localization.Get(LocalizationKey.Console_Desc_Ground));
            _console.RegisterCommand("scene", async (args) => await OnCommandScene(args), Managers.Localization.Get(LocalizationKey.Console_Desc_Scene));
            _console.RegisterCommand("spawn", async (args) => await OnCommandSpawnCharacter(args), Managers.Localization.Get(LocalizationKey.Console_Desc_Spawn));
        }
        else
        {
            _console.UnregisterCommand("set");
            _console.UnregisterCommand("get");
            _console.UnregisterCommand("fps");
            _console.UnregisterCommand("time_debug");
            _console.UnregisterCommand("ground_debug");
            _console.UnregisterCommand("scene");
            _console.UnregisterCommand("spawn");
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
                Log.Warning(LocalizationKey.Console_Help_NotFound, cmdName);

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
        Log.Info(LocalizationKey.Console_Debug_Toggle, debug.isDebugMode.ToString());
    }

    private void OnCommandClear(string[] args)
    {
        var uiConsole = Managers.UI.GetSystem<UIConsoleSystem>();

        if (uiConsole != null)
        {
            uiConsole.ClearLogs();
            Log.Info(LocalizationKey.Console_Clear_Success);
        }
        else
            Log.Warning(LocalizationKey.Console_Clear_NotOpen);
    }

    private void OnCommandToggleFPS(string[] args)
    {
        var fpsSystem = Managers.UI.GetSystem<UIFPSSystem>();

        if (fpsSystem != null)
        {
            Managers.UI.Close(fpsSystem);
            Log.Info(LocalizationKey.Console_FPS_Disabled);
        }
        else
        {
            Managers.UI.OpenSystem<UIFPSSystem>();
            Log.Info(LocalizationKey.Console_FPS_Enabled);
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
            Log.Info(LocalizationKey.Console_LogSearch_Reset);
        }
        else
        {
            string keyword = args[0];
            uiConsole.SetSearchKeyword(keyword);
            Log.Info(LocalizationKey.Console_LogSearch_Filtered, keyword);
        }
    }

    private void OnCommandLogFilter(string[] args)
    {
        if (args == null || args.Length < 2)
        {
            Log.Warning(LocalizationKey.Console_LogFilter_Usage);
            return;
        }

        var uiConsole = Managers.UI.GetSystem<UIConsoleSystem>();

        if (uiConsole == null)
            return;

        string target = args[0].ToLower();

        if (!bool.TryParse(args[1], out bool value))
        {
            Log.Warning(LocalizationKey.Console_LogFilter_InvalidBool);
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
                Log.Warning(LocalizationKey.Console_LogFilter_UnknownType, target);
                return;
        }

        Log.Info(LocalizationKey.Console_LogFilter_Success, target, value);
    }

    private void OnCommandTimeScale(string[] args)
    {
        if (!CheckIsDebugMode())
            return;

        if (args.Length > 0 && float.TryParse(args[0], out float scale))
        {
            Time.timeScale = scale;
            Log.Info(LocalizationKey.Console_Time_Set, scale);
        }
        else
            Log.Info(LocalizationKey.Console_Time_Current, Time.timeScale);
    }

    private void OnCommandSetVariable(string[] args)
    {
        if (!CheckIsDebugMode())
            return;

        if (args.Length < 2 || args[0].Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            Log.Warning(LocalizationKey.Console_Set_Usage);
            return;
        }

        _console.SetVariable(args[0], args[1]);
        Log.Info(LocalizationKey.Console_Set_Success, args[0], args[1]);
    }

    private void OnCommandGetVariable(string[] args)
    {
        if (!CheckIsDebugMode())
            return;

        if (args.Length < 1 || args[0].Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            Log.Warning(LocalizationKey.Console_Get_Usage);
            return;
        }

        string val = _console.GetVariable(args[0]);

        if (val != null)
            Log.Info(LocalizationKey.Console_Get_Success, args[0], val);
        else
            Log.Warning(LocalizationKey.Console_Get_NotFound, args[0]);
    }

    private void OnCommandToggleGroundDebug(string[] args)
    {
        if (!CheckIsDebugMode())
            return;

        bool currentState = DebugExtensions.ToggleDebugView();
        Log.Info(LocalizationKey.Console_Ground_Toggle, currentState.ToString());
    }

    private async UniTask OnCommandScene(string[] args)
    {
        if (!CheckIsDebugMode())
            return;

        if (args.Length < 1 || args[0].Equals("help", StringComparison.OrdinalIgnoreCase) || args[0].Equals("list", StringComparison.OrdinalIgnoreCase))
        {
            Log.Warning(LocalizationKey.Console_Scene_Usage);
            Log.Info(LocalizationKey.Console_Scene_Available);

            foreach (var sceneName in Enum.GetNames(typeof(SceneID)))
                Log.Info(LocalizationKey.Console_Scene_Format, sceneName);
            return;
        }

        if (Enum.TryParse<SceneID>(args[0], true, out var targetSceneID))
        {
            Log.Info(Managers.Localization.Get(LocalizationKey.Console_Scene_MovingProcess, targetSceneID));

            if (Managers.Game.Character == null)
                await Managers.Game.DebugGameAsync(targetSceneID);
            else
                await Managers.Scene.LoadSceneAsync(targetSceneID, forceTransition: true);
        }
        else
            Log.Warning(Managers.Localization.Get(LocalizationKey.Console_Scene_Invalid, args[0]));
    }

    private async UniTask OnCommandSpawnCharacter(string[] args)
    {
        if (!CheckIsDebugMode())
            return;

        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (!currentSceneName.Equals("Demo", StringComparison.OrdinalIgnoreCase))
        {
            Log.Warning(LocalizationKey.Console_Spawn_NotDemo);
            return;
        }

        if (args.Length < 1 || args[0].Equals("help", StringComparison.OrdinalIgnoreCase) || args[0].Equals("list", StringComparison.OrdinalIgnoreCase))
        {
            Log.Warning(LocalizationKey.Console_Spawn_Usage);
            Log.Info(LocalizationKey.Console_Spawn_Available);

            foreach (var characterName in Enum.GetNames(typeof(CharacterID)))
                Log.Info(LocalizationKey.Console_Spawn_Format, characterName);

            return;
        }

        if (!Enum.TryParse<CharacterID>(args[0], true, out var targetCharacterID))
        {
            Log.Warning(LocalizationKey.Console_Spawn_Invalid, args[0]);
            return;
        }

        bool isPlayable = Managers.Data.PlayableCharacters.ContainsKey((int)targetCharacterID);

        if (isPlayable)
        {
            if (Managers.Game.Character == null)
                Log.Info(LocalizationKey.Console_Spawn_NotFoundPlayableCharacter, targetCharacterID.ToString());

            await Managers.Game.SpawnPlayerAsync(targetCharacterID);
            return;
        }

        if (Managers.Game.Character == null)
        {
            Log.Info(LocalizationKey.Console_Spawn_NotFoundPlayableCharacter, CharacterID.Protagonist.ToString());
            await Managers.Game.SpawnPlayerAsync(CharacterID.Protagonist);
        }

        SpawnGeneralCharacter(targetCharacterID);
    }

    private void SpawnGeneralCharacter(CharacterID characterID)
    {
        Vector2 lookDir = Managers.Game.Character.GetLookDirection();
        Vector3 spawnPosition = Managers.Game.Character.transform.position + (Vector3)(lookDir * 2f);
        Managers.Game.SpawnCharacterAsync<Character>(characterID, spawnPosition).Forget();
        Log.Info(LocalizationKey.Console_Spawn_Success, characterID);
    }

    private bool CheckIsDebugMode()
    {
        if (Managers.Config == null || Managers.Config.Option == null || Managers.Config.Option.Debug == null)
            return false;

        return Managers.Config.Option.Debug.isDebugMode;
    }
}
