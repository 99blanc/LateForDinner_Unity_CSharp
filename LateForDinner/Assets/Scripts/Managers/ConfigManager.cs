using Cysharp.Text;
using Cysharp.Threading.Tasks;
using MemoryPack;
using System.IO;
using UnityEngine;

public class ConfigManager
{
    private string _temp => Path.Combine(Application.persistentDataPath, ZString.Concat(Literal.Files.Config, Literal.Extensions.Temp));
    private string _save => Path.Combine(Application.persistentDataPath, ZString.Concat(Literal.Files.Config, Literal.Extensions.Bytes));
    public Option Option { get; private set; } = new Option();

    public async UniTask InitAsync()
        => await LoadAsync();

    public async UniTask LoadAsync()
    {
        if (!File.Exists(_save))
        {
            Option = new Option();
            await SaveAsync();
            return;
        }

        try
        {
            byte[] bytes = await File.ReadAllBytesAsync(_save);
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
        Option = new Option();
        Managers.Control.Reset();

        await SaveAsync();
    }

    public async UniTask SaveKeybindAsync()
    {
        Option.Access.keybind = Managers.Control.Save();

        await SaveAsync();
    }

    public void ApplyToEngine()
    {
        Screen.SetResolution(Option.Graphic.rWidth, Option.Graphic.rHeight, Option.Graphic.screenMode);
        QualitySettings.vSyncCount = Option.Graphic.vSync ? 1 : 0;
        Application.targetFrameRate = Option.Graphic.frameRate;
        QualitySettings.SetQualityLevel(Option.Graphic.quality);
        Application.runInBackground = !Option.Sound.mBackground;
    }
}
