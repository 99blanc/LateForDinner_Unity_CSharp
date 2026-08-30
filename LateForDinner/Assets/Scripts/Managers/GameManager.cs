using Cysharp.Threading.Tasks;
using System;
using System.Runtime.InteropServices;
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

        if (!TrySetupGeneralCharacterComponents(characterPrefab, characterID, out var characterComponent, out var animatorComponent))
        {
            UnityEngine.Object.Destroy(characterPrefab);
            return default;
        }

        if (characterComponent is not T typedCharacter)
        {
            Log.Error(LocalizationKey.Log_Game_CharacterSpawnFailed, characterID.ToString());
            UnityEngine.Object.Destroy(characterPrefab);
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

    private bool TrySetupGeneralCharacterComponents(GameObject prefab, CharacterID characterID, out Character characterComponent, out CharacterAnimator characterAnimator)
    {
        characterComponent = null;
        characterAnimator = null;
        var (characterType, animatorType) = characterID.GetCharacterTypes();

        if (characterType == null)
        {
            Log.Error(LocalizationKey.Log_Game_CharacterSpawnFailed, characterID.ToString());
            return false;
        }

        characterComponent = prefab.AddComponent(characterType) as Character;

        if (animatorType != null)
            characterAnimator = prefab.AddComponent(animatorType) as CharacterAnimator;

        if (characterComponent == null)
        {
            Log.Error(LocalizationKey.Log_Game_CharacterSpawnFailed, characterID.ToString());
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

    private bool IsCharacterNull()
        => Character == null;
}
