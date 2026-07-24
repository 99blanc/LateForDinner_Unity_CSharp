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

        var loadingUI = await Managers.UI.OpenSceneAsync<UIPhaseScene>();
        using var disposable = Observable.CombineLatest(Managers.Instance.Progress, Managers.Instance.Message, (progress, msg) => (progress, msg)).Subscribe(x =>
        {
            loadingUI.Phase(x.progress, x.msg);
        });

        await Managers.Instance.LoadAsync();
        await UniTask.Delay(1000);
    }
}
