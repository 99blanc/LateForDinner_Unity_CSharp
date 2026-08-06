using Cysharp.Threading.Tasks;
using System;

public class LoadDriver
{
    public async UniTask RunAsync(Func<UILoadScreen, UniTask> task)
    {
        Managers.UI.CloseAll();
        var load = await Managers.UI.OpenScreenAsync<UILoadScreen>();

        if (load == null || task == null)
            return;

        load.PlayAsync().Forget();

        try
        {
            await task(load);
        }
        finally
        {
            load.Release();
        }
    }
}
