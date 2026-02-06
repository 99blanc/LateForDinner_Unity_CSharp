using Cysharp.Text;
using Cysharp.Threading.Tasks;
using MemoryPack;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;

public class ConfigManager
{
    private string SAVE_PATH => Path.Combine(Application.persistentDataPath, ZString.Concat(Define.USER, Define.CONFIG));
    private string TEMP_PATH => Path.Combine(Application.persistentDataPath, ZString.Concat(Define.USER, Define.TEMP));
    public Config value { get; private set; }
    public InputActionAsset actAsset { get; private set; }
    public InputActionMap actMap { get; private set; }

    public async UniTask Init()
    {
        await Get();
        await SetupKeybind();
    }

    public async UniTask<Config> Get()
    {
        if (!File.Exists(SAVE_PATH))
        {
            if (File.Exists(TEMP_PATH)) 
                File.Move(TEMP_PATH, SAVE_PATH);
            else 
                return value = new();
        }

        try
        {
            byte[] data = await File.ReadAllBytesAsync(SAVE_PATH);
            value = MemoryPackSerializer.Deserialize<Config>(data);
        }
        catch
        {
            if (File.Exists(SAVE_PATH))
                File.Delete(SAVE_PATH);

            value = new();
        }

        return value;
    }

    public async UniTask Set(Config newConfig)
    {
        if (newConfig is null) 
            return;

        value = newConfig;

        try
        {
            byte[] data = MemoryPackSerializer.Serialize(value);
            await File.WriteAllBytesAsync(TEMP_PATH, data);

            if (File.Exists(SAVE_PATH))
                File.Replace(TEMP_PATH, SAVE_PATH, null);
            else
                File.Move(TEMP_PATH, SAVE_PATH);

        }
        catch
        {
            if (File.Exists(TEMP_PATH))
                File.Delete(TEMP_PATH);
        }
    }

    private async UniTask SetupKeybind()
    {
        InputActionAsset original = await Managers.Resource.LoadSystem(Define.Asset.INPUT_SYSTEM);
        
        if (original is null) 
            return;

        actAsset = Object.Instantiate(original);
        bool hasSavedData = !string.IsNullOrEmpty(value.control.keybind);
        string bindJson = hasSavedData ? value.control.keybind : actAsset.SaveBindingOverridesAsJson();
        actAsset.LoadBindingOverridesFromJson(bindJson);

        if (!hasSavedData)
        {
            value.control = new(){ keybind = bindJson, useModifierDash = value.control.useModifierDash };
            await Set(value);
        }

        actAsset.Enable();
        actMap = actAsset.FindActionMap(Define.Input.MAP_USER);
    }
}