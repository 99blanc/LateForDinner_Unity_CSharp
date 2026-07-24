using Cysharp.Threading.Tasks;
using MemoryPack;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class ConfigManager
{
    private string _temp => Path.Combine(Application.persistentDataPath, Literal.Files.Config_Temp);
    private string _save => Path.Combine(Application.persistentDataPath, Literal.Files.Config);
    private Settings _tempSettings = new Settings();
    public Settings Settings { get; private set; } = new Settings();
    public InputActionAsset ActionAsset { get; private set; }

    public async UniTask InitAsync()
    {
        await LoadAsync();
        await SetupKeybindAsync();
        await ApplyToEngine();
    }

    public async UniTask LoadAsync()
    {
        if (!File.Exists(_save))
        {
            Settings = new Settings();

            await SaveAsync();
            return;
        }

        try
        {
            byte[] bytes = await File.ReadAllBytesAsync(_save);
            Settings = MemoryPackSerializer.Deserialize<Settings>(bytes);
        }
        catch
        {
            Log.Error(Localization.Log_Config_LoadFailed);
            Settings = new Settings();
        }
    }

    public async UniTask SaveAsync()
    {
        try
        {
            byte[] bytes = MemoryPackSerializer.Serialize(Settings);

            await File.WriteAllBytesAsync(_temp, bytes);

            if (File.Exists(_save)) 
                File.Replace(_temp, _save, null);
            else 
                File.Move(_temp, _save);
        }
        catch
        {
            Log.Error(Localization.Log_Config_SaveFailed);
        }
    }

    public async UniTask ResetAsync()
    {
        Settings = new Settings();
        ActionAsset.RemoveAllBindingOverrides();

        await SaveAsync();
        await ApplyToEngine();
        Log.System(Localization.Log_Config_ResetComplete);
    }

    public void PrepareTemp()
    {
        byte[] bytes = MemoryPackSerializer.Serialize(Settings);
        _tempSettings = MemoryPackSerializer.Deserialize<Settings>(bytes);
    }

    public async UniTask ApplyAsync()
    {
        Settings = _tempSettings;

        await SaveAsync();
        await ApplyToEngine();
    }

    private async UniTask SetupKeybindAsync()
    {
        InputActionAsset original = await Managers.Resource.LoadAssetAsync<InputActionAsset>(Literal.Assets.InputActionAsset);
        
        if (original == null)
        {
            Log.Error(Localization.Log_Config_KeybindLoadFailed);
            return;
        }

        ActionAsset = Object.Instantiate(original);

        if (!string.IsNullOrEmpty(Settings.Access.keybind))
            ActionAsset.LoadBindingOverridesFromJson(Settings.Access.keybind);

        ActionAsset.Enable();
    }

    public async UniTask SaveKeybindAsync()
    {
        Settings.Access.keybind = ActionAsset.SaveBindingOverridesAsJson();

        await SaveAsync();
    }

    public async UniTask ApplyToEngine()
    {
        Screen.SetResolution(Settings.Graphic.rWidth, Settings.Graphic.rHeight, Settings.Graphic.screenMode);
        QualitySettings.vSyncCount = Settings.Graphic.vSync ? 1 : 0;
        Application.targetFrameRate = Settings.Graphic.frameRate;
        QualitySettings.SetQualityLevel(Settings.Graphic.quality);
        Application.runInBackground = !Settings.Sound.mBackground;

        await UniTask.CompletedTask;
        Log.System(Localization.Log_Config_EngineApplied);
    }
}
