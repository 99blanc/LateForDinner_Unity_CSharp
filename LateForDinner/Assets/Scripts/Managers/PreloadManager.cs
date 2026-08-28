using Cysharp.Threading.Tasks;
using System;
using UnityEngine.U2D;

public class PreloadManager
{
    public async UniTask Release_BootAsync()
    {
        Log.System(LocalizationKey.Log_Preload_BootStarted);
        await ((Func<UILoadDisplay, UniTask>)(async load =>
        {
            // DESC ::: 부트 시 필요한 게임 내 리소스 생성
            await load.LoadAsync(0.2f, Managers.Localization.Get(LocalizationKey.Log_Preload_Boot_Data));
            await Managers.Config.LoadAsync();
            await Managers.Control.LoadAsync();
            await load.LoadAsync(0.4f, Managers.Localization.Get(LocalizationKey.Log_Preload_Boot_Asset));
            await Managers.Resource.LoadAssetAsync<SpriteAtlas>(Define.Atlas.Common);
            await load.LoadAsync(0.6f, Managers.Localization.Get(LocalizationKey.Log_Preload_Boot_Object));
            await Managers.Resource.LoadPrefabAsync(Literal.Assets.EventSystem);
            await load.LoadAsync(0.8f, Managers.Localization.Get(LocalizationKey.Log_Preload_Boot_UI));
            await Managers.Pool.PrewarmAsync<UILockSystem>(1);
            await Managers.Pool.PrewarmAsync<UIToastSlot>(Define.Toast.Count);
            await Managers.Pool.PrewarmAsync<UIKeybindSlot>(Managers.Control.GetBindableActions().Count + 1 /* DESC ::: 대시 조합키 추가를 위한 1 덧셈 */);
            await Managers.Pool.PrewarmAsync<UISaveDetailPopup>(1);
            await Managers.Pool.PrewarmAsync<UISaveSlot>(Define.Save.Amount);
            await Managers.Pool.PrewarmAsync<UIToastSystem>(1);
            await Managers.Pool.PrewarmAsync<UIOptionPopup>(1);
            await Managers.Pool.PrewarmAsync<UITitleDisplay>(1);
            await Managers.Pool.PrewarmAsync<UIConsoleSystem>(1);
            await Managers.Pool.PrewarmAsync<UIFPSSystem>(1);
            await UniTask.Delay(200);
            await load.LoadAsync(1.0f, Managers.Localization.Get(LocalizationKey.Log_Preload_Boot_Complete));
            await UniTask.Delay(100);
        })).Load();
        Log.System(LocalizationKey.Log_Preload_BootFinished);
    }
}
