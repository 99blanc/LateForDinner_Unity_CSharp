using Cysharp.Threading.Tasks;
using MemoryPack;
using System;
using System.Collections.Generic;
using UnityEngine;
using LateForDinner.Data;

public class DataManager
{
    public Dictionary<AttributeType, AttributeData> Attributes { get; private set; } = new Dictionary<AttributeType, AttributeData>();
    public Dictionary<string, LocalizationData> Localization { get; private set; } = new Dictionary<string, LocalizationData>();
    public Dictionary<int, CharacterData> Characters { get; private set; } = new Dictionary<int, CharacterData>();
    public Dictionary<int, SceneData> Scenes { get; private set; } = new Dictionary<int, SceneData>();
    public Dictionary<int, SceneTransitionData> SceneTransitions { get; private set; } = new Dictionary<int, SceneTransitionData>();
    public Dictionary<int, PlayableCharacterData> PlayableCharacters { get; private set; } = new Dictionary<int, PlayableCharacterData>();
    public Dictionary<int, Dictionary<string, string>> PlayableCharacterTemplates { get; private set; } = new Dictionary<int, Dictionary<string, string>>();

    public async UniTask InitAsync()
    {
        Localization = await LoadDictionaryAsync<string, LocalizationData>(Literal.Tables.Localization, data => data.Key);
        Log.Info(global::LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.Localization);
        var attributes = await LoadListAsync<AttributeData>(Literal.Tables.Attribute);
        attributes.BindTypes();
        Log.Info(global::LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.Attribute);
        Characters = await LoadDictionaryAsync<int, CharacterData>(Literal.Tables.Character, data => data.ID);
        Log.Info(global::LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.Character);
        Scenes = await LoadDictionaryAsync<int, SceneData>(Literal.Tables.Scene, data => data.ID);
        Log.Info(global::LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.Scene);
        SceneTransitions = await LoadDictionaryAsync<int, SceneTransitionData>(Literal.Tables.SceneTransition, data => data.ID);
        Log.Info(global::LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.SceneTransition);
        PlayableCharacters = await LoadDictionaryAsync<int, PlayableCharacterData>(Literal.Tables.PlayableCharacter, data => data.ID);
        Log.Info(global::LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.PlayableCharacter);
        var playableCharacterTemplates = await LoadListAsync<PlayableCharacterTemplateData>(Literal.Tables.PlayableCharacterTemplate);
        PlayableCharacterTemplates = playableCharacterTemplates.ToNestedDictionary(x => x.PlayableCharacterID, x => x.AttributeKey, x => x.Value);
        Log.Info(global::LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.PlayableCharacterTemplate);
    }

    private async UniTask<List<T>> LoadListAsync<T>(string name)
    {
        try
        {
            TextAsset asset = await Managers.Resource.LoadTextAssetAsync(name);

            if (asset == null)
            {
                Log.Error(global::LocalizationKey.Log_Data_AssetNotFound, name);
                return new List<T>();
            }

            byte[] decryptedBytes = DecryptAssetBytes(asset.bytes);
            return MemoryPackSerializer.Deserialize<List<T>>(decryptedBytes) ?? new List<T>();
        }
        catch
        {
            Log.Warning(global::LocalizationKey.Log_Data_DeserializeFailed, name);
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
                Log.Warning(global::LocalizationKey.Log_Data_DuplicateKey, name, key.ToString());
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
