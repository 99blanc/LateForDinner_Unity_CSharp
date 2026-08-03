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

        var splash = await Managers.UI.OpenScreenAsync<UISplashScreen>();

        await splash.PlayAsync();

        splash.Close();

        await Managers.UI.OpenScreenAsync<UITitleScreen>();
    }
}