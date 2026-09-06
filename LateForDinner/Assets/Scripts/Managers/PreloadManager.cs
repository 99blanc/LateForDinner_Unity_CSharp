using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.U2D;

public class PreloadManager
{
    private readonly Dictionary<int, bool> _initializedGames = new Dictionary<int, bool>();

    public async UniTask Release_BootAsync()
    {
        _initializedGames.Clear();
        Log.System(LocalizationKey.Log_Preload_BootStarted);
        Log.System(LocalizationKey.Log_Preload_Boot_Data);
        await Managers.Config.LoadAsync();
        await Managers.Control.LoadAsync();
        Log.System(LocalizationKey.Log_Preload_Boot_Asset);
        await Managers.Resource.LoadAssetAsync<SpriteAtlas>(Define.Atlas.Common);
        await Managers.Resource.LoadAssetAsync<SpriteAtlas>(Define.Atlas.Title);
        await Managers.Resource.LoadAssetAsync<SpriteAtlas>(Define.Atlas.Load);
        Log.System(LocalizationKey.Log_Preload_Boot_Object);
        await Managers.Resource.LoadPrefabAsync(Literal.Assets.EventSystem);
        await Managers.Resource.LoadPrefabAsync(Literal.Assets.GlobalVolume);
        Log.System(LocalizationKey.Log_Preload_Boot_UI);
        await Managers.Pool.PrewarmAsync<UILockSystem>(1);
        await Managers.Pool.PrewarmAsync<UILoadDisplay>(1);
        await Managers.Pool.PrewarmAsync<UIToastSlot>(Define.Toast.Count);
        await Managers.Pool.PrewarmAsync<UIPausePopup>(1);
        await Managers.Pool.PrewarmAsync<UIKeybindSlot>(Managers.Control.GetBindableActions().Count + 1);
        await Managers.Pool.PrewarmAsync<UISaveDetailPopup>(1);
        await Managers.Pool.PrewarmAsync<UISaveSlot>(Define.Amount.MaxSaveSlot);
        await Managers.Pool.PrewarmAsync<UIToastSystem>(1);
        await Managers.Pool.PrewarmAsync<UIOptionPopup>(1);
        await Managers.Pool.PrewarmAsync<UITitleDisplay>(1);
        await Managers.Pool.PrewarmAsync<UIConsoleSystem>(1);
        await Managers.Pool.PrewarmAsync<UIFPSSystem>(1);
        Log.System(LocalizationKey.Log_Preload_BootFinished);
    }

    public async UniTask Release_GameAsync(int dayCount = 1)
    {
        if (_initializedGames.TryGetValue(dayCount, out bool isInit) && isInit)
            return;

        switch (dayCount)
        {
            case 1:
                await Release_Game1Async();
                break;
            default:
                await Release_Game1Async();
                break;
        }

        _initializedGames[dayCount] = true;
    }

    public async UniTask Release_Game1Async()
    {
        Log.System(LocalizationKey.Log_Preload_BootStarted);
        var attributes = Managers.Game.Player.Attributes;
        Log.System(LocalizationKey.Log_Preload_Boot_Data);
        Log.System(LocalizationKey.Log_Preload_Boot_Asset);
        await Managers.Resource.LoadAssetAsync<SpriteAtlas>(Define.Atlas.PlayableCharacter);
        await Managers.Resource.LoadAssetAsync<SpriteAtlas>(Define.Atlas.HeadUp);
        await Managers.Resource.LoadAnimatorControllerAsync<UIDashCountSlot>();
        await Managers.Resource.LoadAnimatorControllerAsync<UIRemainHealthSlot>();
        Log.System(LocalizationKey.Log_Preload_Boot_Object);
        Log.System(LocalizationKey.Log_Preload_Boot_UI);
        Managers.Pool.DestroyByKey<UISplashDisplay>();
        await Managers.Pool.PrewarmAsync<UIQuickSlot>(Define.Amount.MaxQuickSlot);
        await Managers.Pool.PrewarmAsync<UIDashCountSlot>(Define.Amount.MaxDashCount);
        await Managers.Pool.PrewarmAsync<UIRemainHealthSlot>(Define.Amount.MaxHealthCount);
        await Managers.Pool.PrewarmAsync<UIHeadUpDisplay>(1);
        await Managers.Pool.PrewarmAsync<UIQuickSlot>(Define.Amount.MaxQuickSlot);
        await Managers.Pool.PrewarmAsync<UIInventorySlot>(Define.Amount.MaxInventorySlot);
        await Managers.Pool.PrewarmAsync<UIQuestInventoryPopup>(1);
        Log.System(LocalizationKey.Log_Preload_BootFinished);
    }
}
