using Cysharp.Threading.Tasks;
using MemoryPack;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager
{
    public Dictionary<string, LocalizationData> Localization { get; private set; } = new Dictionary<string, LocalizationData>();

    public async UniTask InitAsync()
    {
        // TODO ::: 데이터 테이블 추가 입력
        Localization = await LoadDictionaryAsync<string, LocalizationData>(Literal.Tables.Localization, data => data.Key);
    }

    private async UniTask<List<T>> LoadListAsync<T>(string name)
    {
        try
        {
            TextAsset asset = await Managers.Resource.LoadTextAssetAsync(name);

            if (asset == null)
                return new List<T>();

            byte[] encryptedBytes = asset.bytes;
            byte[] decryptedBytes = new byte[encryptedBytes.Length];

            for (int index = 0; index < encryptedBytes.Length; index++)
                decryptedBytes[index] = (byte)(encryptedBytes[index] ^ Key.Values[index % Key.Values.Length]);
            
            return MemoryPackSerializer.Deserialize<List<T>>(decryptedBytes);
        }
        catch
        {
            return new List<T>();
        }
    }

    private async UniTask<Dictionary<TKey, TValue>> LoadDictionaryAsync<TKey, TValue>(string name, Func<TValue, TKey> keySelector)
    {  
        var list = await LoadListAsync<TValue>(name);

        if (list == null) 
            return new Dictionary<TKey, TValue>();

        var dictionary = new Dictionary<TKey, TValue>();

        foreach (var item in list)
        {
            TKey key = keySelector(item);

            if (dictionary.ContainsKey(key))
                continue;

            dictionary.Add(key, item);
        }

        return dictionary;
    }
}
