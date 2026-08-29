using Cysharp.Threading.Tasks;
using UnityEngine;

public class Managers : MonoBehaviour
{
    private static Managers _instance;
    public static Managers Instance 
        => _instance ??= InitInstance();
    public static LogManager Log { get; private set; }
    public static ResourceManager Resource { get; private set; }
    public static DataManager Data { get; private set; }
    public static LocalizationManager Localization { get; private set; }
    public static PoolManager Pool { get; private set; }
    public static UIManager UI { get; private set; }
    public static ConfigManager Config { get; private set; }
    public static ControlManager Control { get; private set; }
    public static CursorManager Cursor { get; private set; }
    public static SceneManager Scene { get; private set; }
    public static NotifyManager Notify { get; private set; }
    public static ConsoleManager Console { get; private set; }
    public static SaveManager Save { get; private set; }
    public static PreloadManager Preload { get; private set; }
    public static GameManager Game { get; private set; }
    public static CooldownManager Cooldown { get; private set; }
    public static CameraManager Camera { get; private set; }

    public async UniTask LoadAsync()
    {
        CreateManagers();
        await InitializeManagersAsync();
    }

    private static Managers InitInstance()
    {
        var gameObject = new GameObject { name = Literal.Roots.Managers };
        _instance = gameObject.AddComponent<Managers>();
        DontDestroyOnLoad(_instance);
        return _instance;
    }

    private void CreateManagers()
    {
        Log = new LogManager();
        Resource = new ResourceManager();
        Data = new DataManager();
        Localization = new LocalizationManager();
        Pool = new PoolManager();
        UI = new UIManager();
        Config = new ConfigManager();
        Control = new ControlManager();
        Cursor = new CursorManager();
        Scene = new SceneManager();
        Notify = new NotifyManager();
        Console = new ConsoleManager();
        Save = new SaveManager();
        Preload = new PreloadManager();
        Game = new GameManager();
        Cooldown = new CooldownManager();
        Camera = new CameraManager();
    }

    private async UniTask InitializeManagersAsync()
    {
        await Resource.InitAsync();
        await Data.InitAsync();
        await Localization.InitAsync();
        await Save.InitAsync();
    }
}
