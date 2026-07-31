using Cysharp.Threading.Tasks;
using R3;
using System;
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
    private readonly ReactiveProperty<float> _progress = new ReactiveProperty<float>(0f);
    private readonly ReactiveProperty<string> _message = new ReactiveProperty<string>(string.Empty);
    public ReadOnlyReactiveProperty<float> Progress => _progress;
    public ReadOnlyReactiveProperty<string> Message => _message;
    public static LogManager Log { get; private set; }
    public static ResourceManager Resource { get; private set; }
    public static DataManager Data { get; private set; }
    public static LocalizationManager Localization { get; private set; }
    public static UIManager UI { get; private set; }
    public static SceneManager Scene { get; private set; }
    public static ConfigManager Config { get; private set; }
    public static ControlManager Control { get; private set; }
    public static PoolManager Pool { get; private set; }
    public static ConsoleManager Console { get; private set; }

    public async UniTask CoreAsync()
    {
        Log = new LogManager();
        Resource = new ResourceManager();
        Data = new DataManager();
        Localization = new LocalizationManager();
        UI = new UIManager();
        Scene = new SceneManager();
        Config = new ConfigManager();
        Control = new ControlManager();
        Pool = new PoolManager();
        Console = new ConsoleManager();

        await Data.InitAsync();
        await Localization.InitAsync();
    }

    public async UniTask LoadAsync()
    {
        var steps = new (float progress, Localization key, Func<UniTask> action)[]
        {
            (0.2f, global::Localization.Log_Managers_Load_Config, () => Config.InitAsync()),
            (0.4f, global::Localization.Log_Managers_Load_Control, () => Control.InitAsync()),
            (0.6f, global::Localization.Log_Managers_Load_Pool, () => Pool.InitAsync()),
            (0.8f, global::Localization.Log_Managers_Load_Console, () => Console.InitAsync()),
            (1.0f, global::Localization.Log_Managers_Load_Framework, () => UniTask.CompletedTask)
        };

        foreach (var step in steps)
        {
            _message.Value = Localization.Get(step.key);
            _progress.Value = step.progress;

            await UniTask.WhenAll(step.action(), UniTask.Delay(TimeSpan.FromSeconds(1.0f))
        );
        }
    }
}

