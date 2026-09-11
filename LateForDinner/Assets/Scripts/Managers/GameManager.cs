using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class GameManager
{
    private PlayableCharacter _player;
    public PlayableCharacter Player 
        => _player;

    public async UniTask OldgameAsync(int slotIndex)
    {
        Managers.UI.CloseAll();

        await ((Func<UILoadDisplay, UniTask>)(async load =>
        {
            await load.LoadAsync(0.2f, LocalizationKey.Log_Game_Loading_SaveData);
            await Managers.Save.LoadAsync(slotIndex);
            var data = Managers.Save.CurrentData;
            Managers.Inventory.InitInventory(data);
            await load.LoadAsync(0.5f, LocalizationKey.Log_Game_Loading_ResourcePackaging);
            await Managers.Preload.Release_GameAsync(data);
            await load.LoadAsync(0.7f, LocalizationKey.Log_Game_Loading_PlayerSpawn);
            await PrepareAndSpawnPlayerAsync(false, forceTransition: true);
            await load.LoadAsync(1.0f, LocalizationKey.Log_Game_Loading_SaveData);
        })).Load();

        Managers.UI.OpenDisplay<UIHeadUpDisplay>();
    }

    public async UniTask NewgameAsync(int slotIndex)
    {
        Managers.UI.CloseAll();

        await ((Func<UILoadDisplay, UniTask>)(async load =>
        {
            await load.LoadAsync(0.2f, LocalizationKey.Log_Game_Loading_NewData);
            Managers.Save.Newgame(slotIndex);
            var data = Managers.Save.CurrentData;
            Managers.Inventory.InitInventory(data);
            await load.LoadAsync(0.5f, LocalizationKey.Log_Game_Loading_ResourcePackaging);
            await Managers.Preload.Release_GameAsync(data);
            await load.LoadAsync(0.7f, LocalizationKey.Log_Game_Loading_PlayerSpawn);
            await PrepareAndSpawnPlayerAsync(true);
            await load.LoadAsync(1.0f, LocalizationKey.Log_Game_Loading_NewData);
            await Managers.Save.SaveAsync();
        })).Load();

        Managers.UI.OpenDisplay<UIHeadUpDisplay>();
    }

    public async UniTask DebugGameAsync(SceneID targetSceneID)
    {
        Managers.UI.CloseAll();

        await ((Func<UILoadDisplay, UniTask>)(async load =>
        {
            await load.LoadAsync(0.2f, LocalizationKey.Log_Game_Loading_DebugData);

            if (Managers.Game.Player == null || Managers.Save.CurrentSlot < 0)
                Managers.Save.SetDebugDefaultData();

            var data = Managers.Save.CurrentData;
            Managers.Inventory.InitInventory(data);
            Managers.Save.CurrentData.CurrentSceneID = targetSceneID;
            await load.LoadAsync(0.5f, LocalizationKey.Log_Game_Loading_ResourcePackaging);
            await Managers.Preload.Release_GameAsync(data);
            await load.LoadAsync(0.7f, LocalizationKey.Log_Game_Loading_PlayerSpawn);
            await PrepareAndSpawnPlayerAsync(true, forceTransition: true);
            await load.LoadAsync(1.0f, LocalizationKey.Log_Game_Loading_DebugData);
        })).Load();

        Managers.UI.OpenDisplay<UIHeadUpDisplay>();
    }

    private async UniTask PrepareAndSpawnPlayerAsync(bool isNewGame = false, bool forceTransition = false)
    {
        var data = Managers.Save.CurrentData;
        await Managers.Scene.LoadSceneAsync(data.CurrentSceneID, forceTransition);
        await SpawnPlayerAsync(data.SelectedPlayerID);

        if (isNewGame)
            Managers.Scene.RelocateCharacterToSpawnpoint();
    }

    public async UniTask<T> SpawnPlayerAsync<T>(CharacterID characterID) where T : PlayableCharacter
    {
        var data = Managers.Save.CurrentData;
        DespawnCharacter(ref _player);
        var character = await SpawnCharacterAsync<T>(characterID, data.PlayerFlipX, data.PlayerPosition);

        if (character != null)
        {
            _player = character;
            Managers.Camera.SetTarget(Player);
        }

        return character;
    }

    public async UniTask<PlayableCharacter> SpawnPlayerAsync(CharacterID characterID)
        => await SpawnPlayerAsync<PlayableCharacter>(characterID);

    public async UniTask<T> SpawnCharacterAsync<T>(CharacterID characterID, bool flipX, Vector3 position) where T : Character
    {
        var (characterPrefab, rentHandle) = await CreateCharacterPrefabAsync(characterID);

        if (characterPrefab == null)
            return default;

        characterPrefab.transform.position = position;
        var characterComponent = characterPrefab.GetComponentAssert<Character>();
        characterComponent.RentHandle = rentHandle;
        characterComponent.Renderer.flipX = flipX;

        if (characterComponent is not T typedCharacter)
        {
            Log.Error(LocalizationKey.Log_Game_CharacterSpawnFailed, characterID.ToString());
            rentHandle?.Dispose();
            return default;
        }

        Log.System(LocalizationKey.Log_Game_CharacterSpawnSuccess, characterID.ToString());
        return typedCharacter;
    }

    private async UniTask<(GameObject prefab, IDisposable rentHandle)> CreateCharacterPrefabAsync(CharacterID characterID)
    {
        if (!Managers.Data.Characters.TryGetValue((int)characterID, out var characterData) || string.IsNullOrEmpty(characterData.AddressableKey))
        {
            Log.Error(LocalizationKey.Log_Game_CharacterSpawnFailed, characterID.ToString());
            return (null, null);
        }

        var (prefab, rentHandle) = await Managers.Pool.PopAsync(characterData.AddressableKey);

        if (prefab == null)
        {
            Log.Error(LocalizationKey.Log_Game_CharacterSpawnFailed, characterID.ToString());
            return (null, null);
        }

        prefab.name = characterID.ToString();
        return (prefab, rentHandle);
    }

    private void DespawnCharacter<T>(ref T character) where T : Character
    {
        if (character == null)
            return;

        character.RentHandle?.Dispose();
        character = null;
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
            DespawnCharacter(ref _player);
            await Managers.Scene.LoadSceneAsync(SceneID.Bootstrap, forceTransition: true);
            await load.LoadAsync(1.0f, LocalizationKey.Log_Game_Loading_ResourcePackaging);
        })).Load();
        Managers.UI.OpenDisplay<UITitleDisplay>();
    }

    public void Pause()
        => Time.timeScale = 0f;

    public void Resume()
        => Time.timeScale = 1f;
}
