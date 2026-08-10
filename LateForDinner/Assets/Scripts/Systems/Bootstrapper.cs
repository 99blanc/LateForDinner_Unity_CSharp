using Cysharp.Threading.Tasks;
using UnityEngine;

public class Bootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Execute()
        => InitAsync().Forget();

    private static async UniTask InitAsync()
    {
        await Managers.Instance.LoadAsync();
        await Managers.Preload.Release_BootAsync();
        await Managers.UI.OpenDisplayAsync<UISplashDisplay>().PlayAsync().Release();

        Managers.UI.OpenDisplay<UITitleDisplay>();
    }
}