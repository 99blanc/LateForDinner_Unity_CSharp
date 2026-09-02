using Cysharp.Threading.Tasks;
using LateForDinner.Data;
using System;
using UnityEngine;

public class GameManager
{
    public PlayableCharacter Player { get; private set; }

    public async UniTask OldgameAsync(int slotIndex)
    {
        Managers.UI.CloseAll();

        await ((Func<UILoadDisplay, UniTask>)(async load =>
        {
            await load.LoadAsync(0.3f, LocalizationKey.Log_Game_Loading_SaveData);
            await Managers.Save.LoadAsync(slotIndex);
            await load.LoadAsync(0.7f, LocalizationKey.Log_Game_Loading_PlayerSpawn);
            await PrepareAndSpawnPlayerAsync();
            await load.LoadAsync(1.0f, LocalizationKey.Log_Game_Loading_ResourcePackaging);
            await Managers.Preload.Release_GameAsync(Managers.Save.CurrentData.Day);
        })).Load();

        Managers.UI.OpenDisplay<UIHeadUpDisplay>();
    }

    public async UniTask NewgameAsync(int slotIndex)
    {
        Managers.UI.CloseAll();

        await ((Func<UILoadDisplay, UniTask>)(async load =>
        {
            await load.LoadAsync(0.3f, LocalizationKey.Log_Game_Loading_NewData);
            Managers.Save.Newgame(slotIndex);
            await Managers.Save.SaveAsync();
            await load.LoadAsync(0.7f, LocalizationKey.Log_Game_Loading_PlayerSpawn);
            await PrepareAndSpawnPlayerAsync();
            await load.LoadAsync(1.0f, LocalizationKey.Log_Game_Loading_ResourcePackaging);
            await Managers.Preload.Release_GameAsync(Managers.Save.CurrentData.Day);
        })).Load();

        Managers.UI.OpenDisplay<UIHeadUpDisplay>();
    }

    public async UniTask DebugGameAsync(SceneID targetSceneID)
    {
        Managers.UI.CloseAll();

        await ((Func<UILoadDisplay, UniTask>)(async load =>
        {
            await load.LoadAsync(0.3f, LocalizationKey.Log_Game_Loading_DebugData);

            if (Managers.Game.Player == null)
                Managers.Save.SetDebugDefaultData();

            Managers.Save.CurrentData.CurrentSceneID = targetSceneID;
            await load.LoadAsync(0.7f, LocalizationKey.Log_Game_Loading_PlayerSpawn);
            await PrepareAndSpawnPlayerAsync(forceTransition: true);
            await load.LoadAsync(1.0f, LocalizationKey.Log_Game_Loading_ResourcePackaging);
            await Managers.Preload.Release_GameAsync();
        })).Load();

        Managers.UI.OpenDisplay<UIHeadUpDisplay>();
    }

    private async UniTask PrepareAndSpawnPlayerAsync(bool forceTransition = false)
    {
        var saveData = Managers.Save.CurrentData;
        await Managers.Scene.LoadSceneAsync(saveData.CurrentSceneID, forceTransition);
        await SpawnPlayerAsync(saveData.SelectedPlayerID);
        Managers.Scene.RelocateCharacterToSpawnpoint();
    }

    public async UniTask<T> SpawnPlayerAsync<T>(CharacterID characterID) where T : PlayableCharacter
    {
        DespawnExistingPlayer();
        var character = await SpawnCharacterAsync<T>(characterID, Vector3.zero);

        if (character != null)
        {
            Player = character;
            UnityEngine.Object.DontDestroyOnLoad(character.gameObject);
            Managers.Camera.SetTarget(Player);
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

        Managers.Pool.Destroy(Player.gameObject);
        Player = null;
    }

    public async UniTask TitleGameAsync()
    {
        if (Managers.Scene.CurrentSceneID == SceneID.Bootstrap)
            return;

        Managers.UI.CloseAll();
        Resume();
        await ((Func<UILoadDisplay, UniTask>)(async load =>
        {
            await load.LoadAsync(0.5f, LocalizationKey.Log_Game_Loading_Title);
            DespawnExistingPlayer();
            await Managers.Scene.LoadSceneAsync(SceneID.Bootstrap, forceTransition: true);
            await load.LoadAsync(1.0f, LocalizationKey.Log_Game_Loading_ResourcePackaging);
        })).Load();
        Managers.UI.OpenDisplay<UITitleDisplay>();
    }

    private bool IsCharacterNull()
        => Player == null;

    public void Pause()
        => Time.timeScale = 0f;

    public void Resume()
        => Time.timeScale = 1f;
}
