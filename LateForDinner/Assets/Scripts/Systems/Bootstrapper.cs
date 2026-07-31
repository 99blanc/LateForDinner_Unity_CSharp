using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class Bootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Execute()
        => InitAsync().Forget();

    private static async UniTask InitAsync()
    {
        await Managers.Instance.CoreAsync();

        var splashScene = await Managers.UI.OpenScreenAsync<UISplashScreen>();

        await splashScene.PlayAsync();
        await LoadAsync();
    }

    private static async UniTask LoadAsync()
    {
        var loadScene = await Managers.UI.OpenScreenAsync<UILoadScreen>();
        loadScene.PlayAsync().Forget();
        using var disposable = Observable.CombineLatest(Managers.Instance.Progress, Managers.Instance.Message, (progress, msg) => (progress, msg)).Subscribe(x =>
        {
            if (loadScene != null)
                loadScene.LoadAsync(x.progress, x.msg).Forget();
        });

        await Managers.Instance.LoadAsync();
        await UniTask.Delay(1000);

        loadScene.Close();
    }
}