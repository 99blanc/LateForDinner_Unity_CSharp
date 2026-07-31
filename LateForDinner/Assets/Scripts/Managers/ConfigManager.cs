using Cysharp.Threading.Tasks;
using MemoryPack;
using System.IO;
using UnityEngine;

public class ConfigManager
{
    private string _temp => Path.Combine(Application.persistentDataPath, Literal.Files.Config_Temp);
    private string _save => Path.Combine(Application.persistentDataPath, Literal.Files.Config);
    public Settings Settings { get; private set; } = new Settings();

    public async UniTask InitAsync()
        => await LoadAsync();

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
            Settings = new Settings();
        }

        ApplyToEngine();
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
            // DESC ::: 예외 발생 시 무시
        }

        ApplyToEngine();
    }

    public async UniTask ResetAsync()
    {
        Settings = new Settings();
        Managers.Control.Reset();

        await SaveAsync();
    }

    public async UniTask SaveKeybindAsync()
    {
        Settings.Access.keybind = Managers.Control.Save();

        await SaveAsync();
    }

    public void ApplyToEngine()
    {
        Screen.SetResolution(Settings.Graphic.rWidth, Settings.Graphic.rHeight, Settings.Graphic.screenMode);
        QualitySettings.vSyncCount = Settings.Graphic.vSync ? 1 : 0;
        Application.targetFrameRate = Settings.Graphic.frameRate;
        QualitySettings.SetQualityLevel(Settings.Graphic.quality);
        Application.runInBackground = !Settings.Sound.mBackground;
    }
}
