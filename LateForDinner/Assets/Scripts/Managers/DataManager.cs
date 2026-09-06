using Cysharp.Threading.Tasks;
using LateForDinner.Data;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZLinq;

public class DataManager
{
    public Dictionary<string, AttributeData> Attributes { get; private set; } = new Dictionary<string, AttributeData>();
    public Dictionary<string, LocalizationData> Localization { get; private set; } = new Dictionary<string, LocalizationData>();
    public Dictionary<int, CharacterData> Characters { get; private set; } = new Dictionary<int, CharacterData>();
    public Dictionary<int, PlayableCharacterData> PlayableCharacters { get; private set; } = new Dictionary<int, PlayableCharacterData>();
    public ILookup<int, PlayableCharacterTemplateData> PlayableCharacterTemplates { get; private set; } = Enumerable.Empty<PlayableCharacterTemplateData>().ToLookup(x => x.PlayableCharacterID);
    public Dictionary<int, ItemData> Items { get; private set; } = new Dictionary<int, ItemData>();
    public Dictionary<int, ArmorItemData> ArmorItems { get; private set; } = new Dictionary<int, ArmorItemData>();
    public Dictionary<int, WeaponItemData> WeaponItems { get; private set; } = new Dictionary<int, WeaponItemData>();
    public Dictionary<int, ConsumptionItemData> ConsumptionItems { get; private set; } = new Dictionary<int, ConsumptionItemData>();
    public Dictionary<int, EtcItemData> EtcItems { get; private set; } = new Dictionary<int, EtcItemData>();
    public ILookup<int, ItemTemplateData> ItemTemplates { get; private set; } = Enumerable.Empty<ItemTemplateData>().ToLookup(x => x.ItemID);
    public Dictionary<string, ArmorCategoryData> ArmorCategories { get; private set; } = new Dictionary<string, ArmorCategoryData>();
    public Dictionary<string, WeaponCategoryData> WeaponCategories { get; private set; } = new Dictionary<string, WeaponCategoryData>();
    public Dictionary<int, ShopData> Shops { get; private set; } = new Dictionary<int, ShopData>();
    public ILookup<int, ShopItemData> ShopItems { get; private set; } = Enumerable.Empty<ShopItemData>().ToLookup(x => x.ShopID);
    public Dictionary<int, SceneData> Scenes { get; private set; } = new Dictionary<int, SceneData>();
    public ILookup<int, SceneTransitionData> SceneTransitions { get; private set; } = Enumerable.Empty<SceneTransitionData>().ToLookup(x => x.SceneID);
    public Dictionary<string, PropData> Props { get; private set; } = new Dictionary<string, PropData>();

    public async UniTask InitAsync()
    {
        Localization = await LoadDictionaryAsync<string, LocalizationData>(Literal.Tables.Localization, data => data.Key);
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.Localization);
        Attributes = (await LoadDictionaryAsync<string, AttributeData>(Literal.Tables.Attribute, data => data.Key)).BindTypes();
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.Attribute);
        Characters = await LoadDictionaryAsync<int, CharacterData>(Literal.Tables.Character, data => data.ID);
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.Character);
        PlayableCharacters = await LoadDictionaryAsync<int, PlayableCharacterData>(Literal.Tables.PlayableCharacter, data => data.ID);
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.PlayableCharacter);
        PlayableCharacterTemplates = (await LoadListAsync<PlayableCharacterTemplateData>(Literal.Tables.PlayableCharacterTemplate)).ToLookup(x => x.PlayableCharacterID);
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.PlayableCharacterTemplate);
        Items = await LoadDictionaryAsync<int, ItemData>(Literal.Tables.Item, data => data.ID);
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.Item);
        ArmorItems = await LoadDictionaryAsync<int, ArmorItemData>(Literal.Tables.ArmorItem, data => data.ID);
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.ArmorItem);
        WeaponItems = await LoadDictionaryAsync<int, WeaponItemData>(Literal.Tables.WeaponItem, data => data.ID);
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.WeaponItem);
        ConsumptionItems = await LoadDictionaryAsync<int, ConsumptionItemData>(Literal.Tables.ConsumptionItem, data => data.ID);
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.ConsumptionItem);
        EtcItems = await LoadDictionaryAsync<int, EtcItemData>(Literal.Tables.EtcItem, data => data.ID);
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.EtcItem);
        ItemTemplates = (await LoadListAsync<ItemTemplateData>(Literal.Tables.ItemTemplate)).ToLookup(x => x.ItemID);
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.ItemTemplate);
        ArmorCategories = await LoadDictionaryAsync<string, ArmorCategoryData>(Literal.Tables.ArmorCategory, data => data.ArmorCategory);
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.ArmorCategory);
        WeaponCategories = await LoadDictionaryAsync<string, WeaponCategoryData>(Literal.Tables.WeaponCategory, data => data.WeaponCategory);
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.WeaponCategory);
        Shops = await LoadDictionaryAsync<int, ShopData>(Literal.Tables.Shop, data => data.ID);
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.Shop);
        ShopItems = (await LoadListAsync<ShopItemData>(Literal.Tables.ShopItem)).ToLookup(x => x.ShopID);
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.ShopItem);
        Scenes = await LoadDictionaryAsync<int, SceneData>(Literal.Tables.Scene, data => data.ID);
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.Scene);
        SceneTransitions = (await LoadListAsync<SceneTransitionData>(Literal.Tables.SceneTransition)).ToLookup(x => x.SceneID);
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.SceneTransition);
        Props = await LoadDictionaryAsync<string, PropData>(Literal.Tables.Prop, data => data.Key);
        Log.Info(LocalizationKey.Log_Data_LoadedSuccessfully, Literal.Tables.Prop);
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

        if (HasNoItems(list))
            return new Dictionary<TKey, TValue>();

        var dictionary = new Dictionary<TKey, TValue>(list.Count);

        for (int index = 0; index < list.Count; index++)
        {
            var item = list[index];

            if (item == null)
                continue;

            TKey key = keySelector(item);

            if (EqualityComparer<TKey>.Default.Equals(key, default))
                continue;

            if (dictionary.ContainsKey(key))
            {
                Log.Warning(LocalizationKey.Log_Data_DuplicateKey, name, key);
                continue;
            }

            dictionary.Add(key, item);
        }

        return dictionary;
    }

    private byte[] DecryptAssetBytes(byte[] encryptedBytes)
    {
        if (IsBytesEmpty(encryptedBytes))
            return Array.Empty<byte>();

        byte[] decryptedBytes = new byte[encryptedBytes.Length];
        byte[] keyValues = Key.Values;
        int keyLength = keyValues.Length;

        for (int index = 0; index < encryptedBytes.Length; index++)
            decryptedBytes[index] = (byte)(encryptedBytes[index] ^ keyValues[index % keyLength]);

        return decryptedBytes;
    }

    private bool HasNoItems<T>(List<T> list)
        => list == null || list.Count == 0;

    private bool IsBytesEmpty(byte[] bytes)
        => bytes == null || bytes.Length == 0;
}
