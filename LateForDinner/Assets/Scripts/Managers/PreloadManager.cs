using Cysharp.Threading.Tasks;
using UnityEngine.U2D;

public class PreloadManager
{
    public async UniTask Release_BootAsync()
    {
        Log.System(LocalizationKey.Log_Preload_BootStarted);
        Log.System(LocalizationKey.Log_Preload_Boot_Data);
        await Managers.Config.LoadAsync();
        await Managers.Control.LoadAsync();
        Log.System(LocalizationKey.Log_Preload_Boot_Asset);
        await Managers.Resource.LoadAnimatorControllerAsync(Define.Animator.UIAnimator);
        await Managers.Resource.LoadAssetAsync<SpriteAtlas>(Define.Atlas.Common);
        await Managers.Resource.LoadAssetAsync<SpriteAtlas>(Define.Atlas.Splash);
        await Managers.Resource.LoadAssetAsync<SpriteAtlas>(Define.Atlas.Common);
        await Managers.Resource.LoadAssetAsync<SpriteAtlas>(Define.Atlas.Title);
        await Managers.Resource.LoadAssetAsync<SpriteAtlas>(Define.Atlas.Load);
        Log.System(LocalizationKey.Log_Preload_Boot_Object);
        await Managers.Resource.LoadPrefabAsync(Literal.Assets.EventSystem);
        Log.System(LocalizationKey.Log_Preload_Boot_UI);
        await Managers.Pool.PrewarmAsync<UILockSystem>(1);
        await Managers.Pool.PrewarmAsync<UIToastSlot>(Define.Toast.Count);
        // DESC ::: 대시 조합키 추가를 위한 1 덧셈
        await Managers.Pool.PrewarmAsync<UIKeybindSlot>(Managers.Control.GetBindableActions().Count + 1);
        await Managers.Pool.PrewarmAsync<UISaveDetailPopup>(1);
        await Managers.Pool.PrewarmAsync<UISaveSlot>(Define.Amount.Save);
        await Managers.Pool.PrewarmAsync<UIToastSystem>(1);
        await Managers.Pool.PrewarmAsync<UIOptionPopup>(1);
        await Managers.Pool.PrewarmAsync<UITitleDisplay>(1);
        await Managers.Pool.PrewarmAsync<UIConsoleSystem>(1);
        await Managers.Pool.PrewarmAsync<UIFPSSystem>(1);
        Log.System(LocalizationKey.Log_Preload_BootFinished);
    }

    public async UniTask Release_GameAsync(int dayCount = 1)
    {
        switch (dayCount)
        {
            case 1:
                await Release_Game1Async();
                break;
            default:
                await Release_Game1Async();
                break;
        }
    }

    public async UniTask Release_Game1Async()
    {
        Log.System(LocalizationKey.Log_Preload_BootStarted);
        var attributes = Managers.Game.Character.Attributes;
        Log.System(LocalizationKey.Log_Preload_Boot_Data);
        Log.System(LocalizationKey.Log_Preload_Boot_Asset);
        await Managers.Resource.LoadAssetAsync<SpriteAtlas>(Define.Atlas.PlayableCharacter);
        await Managers.Resource.LoadAssetAsync<SpriteAtlas>(Define.Atlas.HeadUp);
        Log.System(LocalizationKey.Log_Preload_Boot_Object);
        Log.System(LocalizationKey.Log_Preload_Boot_UI);
        Managers.Pool.DestroyByKey<UISplashDisplay>();
        await Managers.Pool.PrewarmAsync<UIQuickSlot>(Define.Amount.QuickSlot);
        await Managers.Pool.PrewarmAsync<UIDashCountSlot>(attributes.GetBase<int>(AttributeType.DashCount).CurrentValue);
        await Managers.Pool.PrewarmAsync<UIRemainHealthSlot>(attributes.GetBase<int>(AttributeType.Health).CurrentValue);
        await Managers.UI.OpenDisplayAsync<UIHeadUpDisplay>();
        Log.System(LocalizationKey.Log_Preload_BootFinished);
    }
}
