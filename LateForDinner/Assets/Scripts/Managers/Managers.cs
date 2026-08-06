using Cysharp.Threading.Tasks;
using UnityEngine;

public class Managers : MonoBehaviour
{
    private static Managers _instance;
    public static Managers Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject { name = Literal.Roots.Managers }.AddComponent<Managers>();
                DontDestroyOnLoad(_instance);
            }

            return _instance;
        }
    }
    public static LogManager Log { get; private set; }
    public static ResourceManager Resource { get; private set; }
    public static DataManager Data { get; private set; }
    public static LocalizationManager Localization { get; private set; }
    public static SceneManager Scene { get; private set; }
    public static ConfigManager Config { get; private set; }
    public static ControlManager Control { get; private set; }
    public static PoolManager Pool { get; private set; }
    public static UIManager UI { get; private set; }
    public static ConsoleManager Console { get; private set; }
    public static SaveManager Save { get; private set; }
    public static PreloadManager Preload { get; private set; }

    public async UniTask LoadAsync()
    {
        Log = new LogManager();
        Resource = new ResourceManager();
        Data = new DataManager();
        Localization = new LocalizationManager();
        Scene = new SceneManager();
        Config = new ConfigManager();
        Control = new ControlManager();
        Pool = new PoolManager();
        UI = new UIManager();
        Console = new ConsoleManager();
        Save = new SaveManager();
        Preload = new PreloadManager();

        await Resource.InitAsync();
        await Data.InitAsync();
        await Localization.InitAsync();
        await Config.InitAsync();
        await Control.InitAsync();
        await Console.InitAsync();
        await Save.InitAsync();
    }
}

