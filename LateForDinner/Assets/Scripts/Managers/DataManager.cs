using Cysharp.Threading.Tasks;
using LateForDinner.Data;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataManager
{
    public Dictionary<AttributeType, AttributeData> Attributes { get; private set; } = new Dictionary<AttributeType, AttributeData>();
    public Dictionary<string, LocalizationData> Localization { get; private set; } = new Dictionary<string, LocalizationData>();
    public Dictionary<int, CharacterData> Characters { get; private set; } = new Dictionary<int, CharacterData>();
    public Dictionary<int, SceneData> Scenes { get; private set; } = new Dictionary<int, SceneData>();
    public ILookup<int, SceneTransitionData> SceneTransitions { get; private set; } = Enumerable.Empty<SceneTransitionData>().ToLookup(x => x.SceneID);
    public Dictionary<int, PlayableCharacterData> PlayableCharacters { get; private set; } = new Dictionary<int, PlayableCharacterData>();
    public ILookup<int, PlayableCharacterTemplateData> PlayableCharacterTemplates { get; private set; } = Enumerable.Empty<PlayableCharacterTemplateData>().ToLookup(x => x.PlayableCharacterID);

    public async UniTask InitAsync()
    {
        Localization = await LoadDictionaryAsync<string, LocalizationData>(Literal.Tables.Localization, data => data.Key);
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.Localization);
        var attributes = await LoadListAsync<AttributeData>(Literal.Tables.Attribute);
        attributes.BindTypes();
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.Attribute);
        Characters = await LoadDictionaryAsync<int, CharacterData>(Literal.Tables.Character, data => data.ID);
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.Character);
        Scenes = await LoadDictionaryAsync<int, SceneData>(Literal.Tables.Scene, data => data.ID);
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.Scene);
        var sceneTransitions = await LoadListAsync<SceneTransitionData>(Literal.Tables.SceneTransition);
        SceneTransitions = sceneTransitions.ToLookup(x => x.SceneID);
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.SceneTransition);
        PlayableCharacters = await LoadDictionaryAsync<int, PlayableCharacterData>(Literal.Tables.PlayableCharacter, data => data.ID);
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.PlayableCharacter);
        var playableCharacterTemplateList = await LoadListAsync<PlayableCharacterTemplateData>(Literal.Tables.PlayableCharacterTemplate);
        PlayableCharacterTemplates = playableCharacterTemplateList.ToLookup(x => x.PlayableCharacterID);
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.PlayableCharacterTemplate);
    }

    private async UniTask<List<T>> LoadListAsync<T>(string name)
    {
        try
        {
            TextAsset asset = await Managers.Resource.LoadTextAssetAsync(name);

            if (asset == null)
            {
                Log.Error(LocalizationKey.Log_Data_AssetNotFound, name);
                return new List<T>();
            }

            byte[] decryptedBytes = DecryptAssetBytes(asset.bytes);
            return MemoryPackSerializer.Deserialize<List<T>>(decryptedBytes) ?? new List<T>();
        }
        catch
        {
            Log.Warning(LocalizationKey.Log_Data_DeserializeFailed, name);
            return new List<T>();
        }
    }

    private async UniTask<Dictionary<TKey, TValue>> LoadDictionaryAsync<TKey, TValue>(string name, Func<TValue, TKey> keySelector)
    {
        var list = await LoadListAsync<TValue>(name);

        if (list == null || list.Count == 0)
            return new Dictionary<TKey, TValue>();

        var dictionary = new Dictionary<TKey, TValue>(list.Count);

        for (int index = 0; index < list.Count; index++)
        {
            var item = list[index];

            if (item == null)
                continue;

            TKey key = keySelector(item);

            if (key == null)
                continue;

            if (dictionary.ContainsKey(key))
            {
                Log.Warning(LocalizationKey.Log_Data_DuplicateKey, name, key.ToString());
                continue;
            }

            dictionary.Add(key, item);
        }

        return dictionary;
    }

    private byte[] DecryptAssetBytes(byte[] encryptedBytes)
    {
        if (encryptedBytes == null || encryptedBytes.Length == 0)
            return Array.Empty<byte>();

        byte[] decryptedBytes = new byte[encryptedBytes.Length];
        byte[] keyValues = Key.Values;
        int keyLength = keyValues.Length;

        for (int index = 0; index < encryptedBytes.Length; index++)
            decryptedBytes[index] = (byte)(encryptedBytes[index] ^ keyValues[index % keyLength]);

        return decryptedBytes;
    }
}
