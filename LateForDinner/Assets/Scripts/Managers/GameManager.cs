using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
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

        if (IsPlayerPrefabNull(playerPrefab))
            return null;

        if (!TrySetupCharacterComponents(playerPrefab, characterID, out var playerComponent, out var characterAnimator))
        {
            UnityEngine.Object.Destroy(playerPrefab);
            return null;
        }

        Character = playerComponent;
        await Character.InitAsync();
        UnityEngine.Object.DontDestroyOnLoad(playerPrefab);
        Managers.Camera.SetTarget(Character);
        Log.System(LocalizationKey.Log_Game_PlayerSpawnSuccess, characterID.ToString());
        return Character;
    }

    private async UniTask<GameObject> CreatePlayerPrefabAsync(CharacterID characterID)
    {
        GameObject playerPrefab = await Managers.Resource.InstantiateAsync(Literal.Assets.PlayableCharacterObject);

        if (IsPlayerPrefabNull(playerPrefab))
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

        if (HasAnyTypeMissing(characterType, animatorType))
        {
            Log.Error(LocalizationKey.Log_Game_PlayerSpawnFailed, characterID.ToString());
            return false;
        }

        playerComponent = playerPrefab.AddComponent(characterType) as PlayableCharacter;
        characterAnimator = playerPrefab.AddComponent(animatorType) as CharacterAnimator;

        if (HasAnyComponentMissing(playerComponent, characterAnimator))
        {
            Log.Error(LocalizationKey.Log_Game_PlayerSpawnFailed, characterID.ToString());
            return false;
        }

        return true;
    }

    private void DespawnExistingPlayer()
    {
        if (IsCharacterNull())
            return;

        UnityEngine.Object.Destroy(Character.gameObject);
        Character = null;
    }

    private bool IsPlayerPrefabNull(GameObject playerPrefab)
        => playerPrefab == null;

    private bool HasAnyTypeMissing(Type characterType, Type animatorType)
        => characterType == null || animatorType == null;

    private bool HasAnyComponentMissing(PlayableCharacter playerComponent, CharacterAnimator characterAnimator)
        => playerComponent == null || characterAnimator == null;

    private bool IsCharacterNull()
        => Character == null;
}
