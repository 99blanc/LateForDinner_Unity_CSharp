using Cysharp.Threading.Tasks;
using UnityEngine;

public class Bootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static async void Execute()
        => await InitAsync();

    private static async UniTask InitAsync()
    {
        await Managers.Instance.LoadAsync();
        Managers.Log.Setup();
        Managers.Console.Setup();
        await Managers.Preload.Release_BootAsync();
        Managers.UI.Setup();
        Managers.Control.Setup();
        await Managers.UI.OpenDisplayAsync<UISplashDisplay>().PlayAsync().Release();
        Managers.UI.OpenDisplay<UITitleDisplay>();
    }
}