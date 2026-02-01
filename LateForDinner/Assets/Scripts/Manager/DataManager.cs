using CsvHelper;
using CsvHelper.Configuration;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ZLinq;
using Token.ID;
using UnityEngine;

public class DataManager
{
    private readonly CsvConfiguration csvConfig = new(CultureInfo.InvariantCulture)
    {
        ShouldSkipRecord = args => args.Row.Parser.Record.All(string.IsNullOrWhiteSpace),
        HasHeaderRecord = true,
        AllowComments = true,
        Comment = '#',
    };

    public Dictionary<PlayerID, PlayerData> players { get; private set; }

    public async UniTask Init()
    {
        TextAsset cTable = await Managers.Resource.LoadTextAsset(Define.Asset.FILE_PLAYER);
        await UniTask.Yield(PlayerLoopTiming.Update);
        players = ParseToDictionary<PlayerID, PlayerData>(cTable.text, data => data.id);
    }

    private List<T> ParseToList<T>(string text)
    {
        using StringReader reader = new(text);
        using CsvReader csv = new(reader, csvConfig);
        return csv.GetRecords<T>().AsValueEnumerable().ToList();
    }

    private Dictionary<TKey, TItem> ParseToDictionary<TKey, TItem>(string text, Func<TItem, TKey> key)
    {
        using StringReader reader = new(text);
        using CsvReader csv = new(reader, csvConfig);
        return csv.GetRecords<TItem>().AsValueEnumerable().ToDictionary(key);
    }
}
