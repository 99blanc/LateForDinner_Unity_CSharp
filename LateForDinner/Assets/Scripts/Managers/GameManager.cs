using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class GameManager
{
    public PlayableCharacter Character { get; private set; }

    public async UniTask OldgameAsync(int slotIndex)
    {
        Managers.UI.CloseAll();

        await ((Func<UILoadDisplay, UniTask>)(async load =>
        {
            await load.LoadAsync(0.3f, Managers.Localization.Get(LocalizationKey.Log_Game_Loading_SaveData));
            await Managers.Save.LoadAsync(slotIndex);
            await load.LoadAsync(0.7f, Managers.Localization.Get(LocalizationKey.Log_Game_Loading_PlayerSpawn));
            await PrepareAndSpawnPlayerAsync();
            await load.LoadAsync(1.0f, Managers.Localization.Get(LocalizationKey.Log_Game_Loading_UI));
            await Managers.UI.OpenDisplayAsync<UIHeadUpDisplay>();
        })).Load();
    }

    public async UniTask NewgameAsync(int slotIndex)
    {
        Managers.UI.CloseAll();

        await ((Func<UILoadDisplay, UniTask>)(async load =>
        {
            await load.LoadAsync(0.3f, Managers.Localization.Get(LocalizationKey.Log_Game_Loading_NewData));
            Managers.Save.Newgame(slotIndex);
            await Managers.Save.SaveAsync();
            await load.LoadAsync(0.7f, Managers.Localization.Get(LocalizationKey.Log_Game_Loading_PlayerSpawn));
            await PrepareAndSpawnPlayerAsync();
            await load.LoadAsync(1.0f, Managers.Localization.Get(LocalizationKey.Log_Game_Loading_UI));
            await Managers.UI.OpenDisplayAsync<UIHeadUpDisplay>();
        })).Load();
    }

    private async UniTask PrepareAndSpawnPlayerAsync()
    {
        var saveData = Managers.Save.CurrentData;
        await Managers.Scene.LoadSceneAsync(saveData.CurrentSceneID);
        await SpawnPlayerAsync(saveData.SelectedCharacterID);
        Managers.Scene.RelocateCharacterToSpawnpoint();
    }

    public async UniTask<PlayableCharacter> SpawnPlayerAsync(CharacterID characterID)
    {
        DespawnExistingPlayer();
        GameObject playerPrefab = await CreatePlayerPrefabAsync(characterID);

        if (playerPrefab == null)
            return null;

        if (!TrySetupCharacterComponents(playerPrefab, characterID, out var playerComponent, out var characterAnimator))
        {
            UnityEngine.Object.Destroy(playerPrefab);
            return null;
        }

        FinalizePlayerSpawn(playerComponent, playerPrefab, characterID);
        await ApplyAnimatorOverrideControllerAsync(characterAnimator, characterID);
        return Character;
    }

    private async UniTask<GameObject> CreatePlayerPrefabAsync(CharacterID characterID)
    {
        GameObject playerPrefab = await Managers.Resource.InstantiateAsync(Literal.Assets.PlayableCharacterObject);

        if (playerPrefab == null)
        {
            Log.Error(LocalizationKey.Log_Game_PlayerSpawnFailed, characterID.ToString());
            return null;
        }

        playerPrefab.name = characterID.ToString();
        return playerPrefab;
    }

    private bool TrySetupCharacterComponents(GameObject playerPrefab, CharacterID characterID, out PlayableCharacter playerComponent, out CharacterAnimator characterAnimator)
    {
        playerComponent = null;
        characterAnimator = null;
        var (characterType, animatorType) = characterID.GetCharacterTypes();

        if (characterType == null || animatorType == null)
        {
            Log.Error(LocalizationKey.Log_Game_PlayerSpawnFailed, characterID.ToString());
            return false;
        }

        playerComponent = playerPrefab.AddComponent(characterType) as PlayableCharacter;
        characterAnimator = playerPrefab.AddComponent(animatorType) as CharacterAnimator;

        if (playerComponent == null || characterAnimator == null)
        {
            Log.Error(LocalizationKey.Log_Game_PlayerSpawnFailed, characterID.ToString());
            return false;
        }

        return true;
    }

    private async UniTask ApplyAnimatorOverrideControllerAsync(CharacterAnimator characterAnimator, CharacterID characterID)
    {
        string overrideControllerPath = characterID.GetAnimatorOverrideControllerPath();
        AnimatorOverrideController overrideController = await Managers.Resource.LoadAnimatorOverrideControllerAsync(overrideControllerPath);

        if (overrideController != null)
            characterAnimator.SetOverrideController(overrideController);
        else
            Log.Error(LocalizationKey.Log_Resource_LoadFailed_Null, overrideControllerPath);
    }

    private void FinalizePlayerSpawn(PlayableCharacter playerComponent, GameObject playerPrefab, CharacterID characterID)
    {
        Character = playerComponent;
        Character.Init();
        UnityEngine.Object.DontDestroyOnLoad(playerPrefab);
        Log.System(LocalizationKey.Log_Game_PlayerSpawnSuccess, characterID.ToString());
    }

    private void DespawnExistingPlayer()
    {
        if (Character == null)
            return;

        UnityEngine.Object.Destroy(Character.gameObject);
        Character = null;
    }
}
