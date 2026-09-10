using Cysharp.Threading.Tasks;
using LateForDinner.Data;
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
        await Managers.Pool.PrewarmAsync<UISplashDisplay>(1);
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

    public async UniTask Release_GameAsync(SaveData data)
    {
        int dayCount = (data != null) ? data.Day : 1;

        if (_initializedGames.TryGetValue(dayCount, out bool isInit) && isInit)
            return;

        switch (dayCount)
        {
            case 1:
                await Release_Game1Async(data);
                break;
            default:
                await Release_Game1Async(data);
                break;
        }

        _initializedGames[dayCount] = true;
    }

    public async UniTask Release_Game1Async(SaveData data)
    {
        Log.System(LocalizationKey.Log_Preload_BootStarted);
        Log.System(LocalizationKey.Log_Preload_Boot_Data);
        Log.System(LocalizationKey.Log_Preload_Boot_Asset);
        await Managers.Resource.LoadAssetAsync<SpriteAtlas>(Define.Atlas.PlayableCharacter);
        await Managers.Resource.LoadAssetAsync<SpriteAtlas>(Define.Atlas.HeadUp);
        await Managers.Resource.LoadAssetAsync<SpriteAtlas>(Define.Atlas.Item);
        await Managers.Resource.LoadAnimatorControllerAsync<UIDashCountSlot>();
        await Managers.Resource.LoadAnimatorControllerAsync<UIRemainHealthSlot>();
        Log.System(LocalizationKey.Log_Preload_Boot_Object);
        await PrewarmCharacterAsync(data.SelectedPlayerID, 1);
        Log.System(LocalizationKey.Log_Preload_Boot_UI);
        Managers.Pool.DestroyByKey<UISplashDisplay>();
        await Managers.Pool.PrewarmAsync<UIQuickSlot>(Define.Amount.MaxQuickSlot);
        await Managers.Pool.PrewarmAsync<UIDashCountSlot>(Define.Amount.MaxDashCount);
        await Managers.Pool.PrewarmAsync<UIRemainHealthSlot>(Define.Amount.MaxHealthCount);
        await Managers.Pool.PrewarmAsync<UIInventorySlot>(Define.Amount.MaxInventorySlot + Define.Amount.MaxEquipmentSlot);
        await Managers.Pool.PrewarmAsync<UIHeadUpDisplay>(1);
        await Managers.Pool.PrewarmAsync<UIGhostImagePopup>(1);
        await Managers.Pool.PrewarmAsync<UIItemDropPopup>(1);
        await Managers.Pool.PrewarmAsync<UIItemDetailPopup>(1);
        await Managers.Pool.PrewarmAsync<UIQuestInventoryPopup>(1);
        Log.System(LocalizationKey.Log_Preload_BootFinished);
    }

    private async UniTask PrewarmCharacterAsync(CharacterID characterID, int count)
    {
        if (Managers.Data.Characters.TryGetValue((int)characterID, out var characterData) && !string.IsNullOrEmpty(characterData.AddressableKey))
        {
            await Managers.Resource.LoadAnimatorOverrideControllerAsync(characterID.GetAnimatorOverrideControllerPath());
            await Managers.Pool.PrewarmAsync(characterData.AddressableKey, count);
        }
    }
}
