using Cysharp.Text;
using Cysharp.Threading.Tasks;
using MemoryPack;
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
            Option = new Option();
            await SaveAsync();
            return;
        }

        try
        {
            byte[] bytes = await File.ReadAllBytesAsync(SavePath);
            Option = MemoryPackSerializer.Deserialize<Option>(bytes) ?? new Option();
        }
        catch
        {
            Option = new Option();
        }

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
            // DESC ::: 예외 발생 시 무시
        }

        ApplyToEngine();
    }

    public async UniTask ResetAsync()
    {
        Option = new Option();
        Managers.Control?.Reset();
        await SaveAsync();
    }

    public async UniTask SaveKeybindAsync()
    {
        Option.Access.keybind = Managers.Control?.Save() ?? string.Empty;
        await SaveAsync();
    }

    public void ApplyToEngine()
    {
        if (Option == null)
            return;

        var graphic = Option.Graphic;
        var sound = Option.Sound;
        Screen.SetResolution(graphic.rWidth, graphic.rHeight, graphic.screenMode, graphic.Resolution.refreshRateRatio);
        QualitySettings.vSyncCount = graphic.vSync ? 1 : 0;
        Application.targetFrameRate = graphic.rRefreshRate;
        QualitySettings.SetQualityLevel((int)graphic.quality);
        Application.runInBackground = !sound.mute;
    }
}
