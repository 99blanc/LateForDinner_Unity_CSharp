using Cysharp.Text;
using Cysharp.Threading.Tasks;
using MemoryPack;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;

public class ConfigManager
{
    private readonly string SAVE_PATH = Path.Combine(Application.persistentDataPath, ZString.Concat(Define.USER, Define.CONFIG));
    private readonly string TEMP_PATH = Path.Combine(Application.persistentDataPath, ZString.Concat(Define.USER, Define.TEMP));
    public Config config { get; private set; }
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
                return config = new Config();
        }

        try
        {
            byte[] data = await File.ReadAllBytesAsync(SAVE_PATH);
            config = MemoryPackSerializer.Deserialize<Config>(data);
        }
        catch (System.Exception)
        {
            if (File.Exists(SAVE_PATH))
                File.Delete(SAVE_PATH);

            config = new Config();
        }

        return config;
    }

    public async UniTask Set(Config newConfig)
    {
        if (newConfig == null) 
            return;

        config = newConfig;

        try
        {
            byte[] data = MemoryPackSerializer.Serialize(config);
            await File.WriteAllBytesAsync(TEMP_PATH, data);

            if (File.Exists(SAVE_PATH))
                File.Replace(TEMP_PATH, SAVE_PATH, null);
            else
                File.Move(TEMP_PATH, SAVE_PATH);

        }
        catch (System.Exception)
        {
            if (File.Exists(TEMP_PATH))
                File.Delete(TEMP_PATH);
        }
    }

    private async UniTask SetupKeybind()
    {
        var original = await Managers.Resource.LoadInputSystem(Define.Asset.FILE_INPUT_SYSTEM);
        
        if (original == null) 
            return;

        actAsset = Object.Instantiate(original);
        bool hasSavedData = !string.IsNullOrEmpty(config.control.keybind);
        string bindJson = hasSavedData ? config.control.keybind : actAsset.SaveBindingOverridesAsJson();
        actAsset.LoadBindingOverridesFromJson(bindJson);

        if (!hasSavedData)
        {
            config.control = new ControlConfig { keybind = bindJson };
            await Set(config);
        }

        Sync(actAsset);
        actAsset.Enable();
        actMap = actAsset.FindActionMap(Define.Input.MAP_USER);
    }

    private void Sync(InputActionAsset asset)
    {
        var map = asset.FindActionMap(Define.Input.MAP_USER);
        var moveAction = map?.FindAction(Define.Input.ACTION_MOVE);
        var dashAction = map?.FindAction(Define.Input.ACTION_DASH);

        if (moveAction == null || dashAction == null) 
            return;

        foreach (var moveBinding in moveAction.bindings)
        {
            if (moveBinding.isComposite) 
                continue;

            dashAction.ApplyBindingOverride(new InputBinding
            {
                name = moveBinding.name,
                overridePath = moveBinding.effectivePath
            });
        }
    }

    public void OnDestroy(InputActionAsset asset = null)
    {
        if (!asset)
            asset = actAsset;

        if (asset)
        {
            asset.Disable();
            Object.Destroy(asset);
        }
    }
}