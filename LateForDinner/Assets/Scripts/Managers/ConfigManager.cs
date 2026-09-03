using Cysharp.Text;
using Cysharp.Threading.Tasks;
using MemoryPack;
using System;
using System.IO;
using UnityEngine;

public class ConfigManager
{
    private string _tempPath;
    private string _savePath;
    private string TempPath
        => _tempPath ??= Path.Combine(Application.persistentDataPath, ZString.Concat(Literal.Files.Config, Literal.Extensions.Temp));
    private string SavePath
        => _savePath ??= Path.Combine(Application.persistentDataPath, ZString.Concat(Literal.Files.Config, Literal.Extensions.Bytes));
    public Option Option { get; private set; } = new Option();

    public async UniTask LoadAsync()
    {
        if (!File.Exists(SavePath))
        {
            Option = Option.Default;
            await SaveAsync();
            Log.System(LocalizationKey.Log_Config_CreatedNew);
        }
        else
        {
            try
            {
                byte[] bytes = await File.ReadAllBytesAsync(SavePath);
                Option = MemoryPackSerializer.Deserialize<Option>(bytes) ?? Option.Default;
                Log.Info(LocalizationKey.Log_Config_LoadedSuccessfully);
            }
            catch
            {
                Log.Warning(LocalizationKey.Log_Config_LoadFailed);
                Option = Option.Default;
            }
        }

        CheckCommandLineArguments();
        ApplyToEngine();
    }

    public async UniTask SaveAsync()
    {
        try
        {
            byte[] bytes = MemoryPackSerializer.Serialize(Option);
            await File.WriteAllBytesAsync(TempPath, bytes);

            if (File.Exists(SavePath))
                File.Replace(TempPath, SavePath, null);
            else
                File.Move(TempPath, SavePath);
        }
        catch
        {
            Log.Error(LocalizationKey.Log_Config_SaveFailed);
        }

        ApplyToEngine();
    }

    public async UniTask ResetAsync()
    {
        Option = Option.Default;
        Managers.Control?.ResetBindings();
        await SaveAsync();
        Log.System(LocalizationKey.Log_Config_Reset);
    }

    public async UniTask SaveKeybindAsync()
    {
        Option.Access.keybind = Managers.Control?.SaveBindingsToJson() ?? string.Empty;
        await SaveAsync();
    }

    private void CheckCommandLineArguments()
    {
        string[] args = Environment.GetCommandLineArgs();

        if (HasNoArguments(args))
            return;

        Option.Debug.enableConsole = false;
        Option.Debug.isDebugMode = false;

        foreach (string arg in args)
        {
            if (arg.Equals(Define.Execute.Console, StringComparison.OrdinalIgnoreCase))
            {
                Option.Debug.enableConsole = true;
                Log.System(LocalizationKey.Log_Config_ConsoleEnabled);
            }

            if (arg.Equals(Define.Execute.Debug, StringComparison.OrdinalIgnoreCase))
            {
                Option.Debug.isDebugMode = true;
                Log.System(LocalizationKey.Log_Config_DebugEnabled);
            }
        }
    }

    public void ApplyToEngine()
    {
        if (IsOptionInvalid())
            return;

        var graphic = Option.Graphic;
        var sound = Option.Sound;
        Managers.Cursor.UpdateCursorLockState(graphic.screenMode);
        Managers.Graphic.ApplyGraphicOptions(graphic);
        Application.runInBackground = !sound.mute;
    }

    private bool HasNoArguments(string[] args)
        => args == null;

    private bool IsOptionInvalid()
        => Option == null;
}
