using Cysharp.Threading.Tasks;
using System;

public class LoadDriver
{
    public async UniTask RunAsync(Func<UILoadDisplay, UniTask> task)
    {
        Managers.UI.CloseAll();
        var load = await Managers.UI.OpenDisplayAsync<UILoadDisplay>();
        load?.PlayAsync().Forget();

        try
        {
            await task(load);
        }
        finally
        {
            load?.Release();
        }
    }
}
