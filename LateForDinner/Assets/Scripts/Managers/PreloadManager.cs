using Cysharp.Threading.Tasks;
using UnityEngine.U2D;

public class PreloadManager
{
    private readonly LoadDriver _driver = new LoadDriver();

    public async UniTask Release_BootAsync()
    {
        await _driver.RunAsync(async load =>
        {
            // TODO ::: 부트 시 필요한 게임 내 리소스 생성
            await load.LoadAsync(0.3f, "Loading Game Data...");
            await UniTask.Delay(300);
            await load.LoadAsync(0.5f, "Preparing Asset Resources...");
            await Managers.Resource.LoadAssetAsync<SpriteAtlas>(Define.Atlas.UI_Common);
            await load.LoadAsync(0.7f, "Preparing Object Pools...");
            await Managers.Pool.PrewarmAsync<UISaveSlot>(Define.Save.Amount);
            await Managers.Pool.PrewarmAsync<UIOptionPopup>(1);
            await Managers.Pool.PrewarmAsync<UITitleScreen>(1);
            await UniTask.Delay(300);
            await load.LoadAsync(1.0f, "Complete!");
            await UniTask.Delay(200);
        });
    }
}
