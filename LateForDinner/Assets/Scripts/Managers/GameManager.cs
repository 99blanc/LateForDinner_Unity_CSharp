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
            await load.LoadAsync(1.0f, Managers.Localization.Get(LocalizationKey.Log_Game_Loading_ResourcePackaging));
            await Managers.Preload.Release_GameAsync(Managers.Save.CurrentData.Day);
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
            await load.LoadAsync(1.0f, Managers.Localization.Get(LocalizationKey.Log_Game_Loading_ResourcePackaging));
            await Managers.Preload.Release_GameAsync(Managers.Save.CurrentData.Day);
        })).Load();
    }

    public async UniTask DebugGameAsync(SceneID targetSceneID)
    {
        Managers.UI.CloseAll();

        await ((Func<UILoadDisplay, UniTask>)(async load =>
        {
            await load.LoadAsync(0.3f, Managers.Localization.Get(LocalizationKey.Log_Game_Loading_DebugData));
            Managers.Save.SetDebugDefaultData();
            Managers.Save.CurrentData.CurrentSceneID = targetSceneID;
            await load.LoadAsync(0.7f, Managers.Localization.Get(LocalizationKey.Log_Game_Loading_PlayerSpawn));
            await PrepareAndSpawnPlayerAsync();
            await load.LoadAsync(1.0f, Managers.Localization.Get(LocalizationKey.Log_Game_Loading_ResourcePackaging));
            await Managers.Preload.Release_GameAsync();
        })).Load();
    }

    private async UniTask PrepareAndSpawnPlayerAsync()
    {
        var saveData = Managers.Save.CurrentData;
        await Managers.Scene.LoadSceneAsync(saveData.CurrentSceneID);
        await SpawnPlayerAsync(saveData.SelectedPlayerID);
        Managers.Scene.RelocateCharacterToSpawnpoint();
    }

    public async UniTask<T> SpawnPlayerAsync<T>(CharacterID characterID) where T : PlayableCharacter
    {
        DespawnExistingPlayer();
        var character = await SpawnCharacterAsync<T>(characterID, Vector3.zero);

        if (character != null)
        {
            Character = character;
            UnityEngine.Object.DontDestroyOnLoad(character.gameObject);
            Managers.Camera.SetTarget(Character);
        }

        return character;
    }

    public async UniTask<PlayableCharacter> SpawnPlayerAsync(CharacterID characterID)
        => await SpawnPlayerAsync<PlayableCharacter>(characterID);

    public async UniTask<T> SpawnCharacterAsync<T>(CharacterID characterID, Vector3 position, Quaternion rotation = default) where T : Character
    {
        GameObject characterPrefab = await CreateCharacterPrefabAsync(characterID);

        if (characterPrefab == null)
            return default;

        characterPrefab.transform.position = position;
        characterPrefab.transform.rotation = rotation == default ? Quaternion.identity : rotation;
        var characterComponent = characterPrefab.GetComponentAssert<Character>();

        if (characterComponent is not T typedCharacter)
        {
            Log.Error(LocalizationKey.Log_Game_CharacterSpawnFailed, characterID.ToString());
            Managers.Pool.Destroy(characterPrefab);
            return default;
        }

        await typedCharacter.InitAsync();
        Log.System(LocalizationKey.Log_Game_CharacterSpawnSuccess, characterID.ToString());
        return typedCharacter;
    }

    private async UniTask<GameObject> CreateCharacterPrefabAsync(CharacterID characterID)
    {
        if (!Managers.Data.Characters.TryGetValue((int)characterID, out var characterData) || string.IsNullOrEmpty(characterData.AddressableKey))
        {
            Log.Error(LocalizationKey.Log_Game_CharacterSpawnFailed, characterID.ToString());
            return null;
        }

        GameObject prefab = await Managers.Resource.InstantiateAsync(characterData.AddressableKey);

        if (prefab == null)
        {
            Log.Error(LocalizationKey.Log_Game_CharacterSpawnFailed, characterID.ToString());
            return null;
        }

        prefab.name = characterID.ToString();
        return prefab;
    }

    private void DespawnExistingPlayer()
    {
        if (IsCharacterNull())
            return;

        Managers.Pool.Destroy(Character.gameObject);
        Character = null;
    }

    private bool IsCharacterNull()
        => Character == null;
}
