using Cysharp.Threading.Tasks;
using UnityEngine;

public class Managers : MonoBehaviour
{
    private static Managers Instance;
    public static ResourceManager Resource { get; private set; } = new();
    public static ConfigManager Config { get; private set; } = new();
    public static DataManager Data { get; private set; } = new();
    public static LocalizationManager Localization { get; private set; } = new();
    public static LoadManager Load { get; private set; } = new();
    public static UIManager UI { get; private set; } = new();
    public static GameManager Game { get; private set; } = new();

    private void Awake() => Init();

    private async void Init()
    {
        if (Instance)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
        await TaskInit();
    }

    private async UniTask TaskInit()
    {
        Resource.Init();
        Load.SetProgress(0.1f);
        await Config.Init();
        Load.SetProgress(0.3f);
        await UniTask.WhenAll(Data.Init(), Localization.Init());
        Load.SetStatus(Localization.UI.GetText("UI.LOAD.GAME"));
        await Game.Init();
        Load.SetProgress(0.8f);
        Load.SetStatus(Localization.UI.GetText("UI.LOAD.UI"));
        UI.Init();
        Load.SetProgress(1.0f);
        Load.SetStatus(Localization.UI.GetText("UI.LOAD.COMPLETE"));
    }
}