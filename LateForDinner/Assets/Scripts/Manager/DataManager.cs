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

public class DataManager
{
    public Dictionary<PlayerID, PlayerData> players { get; private set; }

    private readonly CsvConfiguration csvConfig = new(CultureInfo.InvariantCulture) 
    {
        HasHeaderRecord = true,
        AllowComments = true,
        Comment = '#',
    };

    public async UniTask Init()
    {
        var cTable = await Managers.Resource.LoadTextAsset(Define.Asset.FILE_PLAYER);
        await UniTask.Yield(PlayerLoopTiming.Update);
        players = ParseToDictionary<PlayerID, PlayerData>(cTable.text, data => data.id);
    }

    private List<T> ParseToList<T>(string text)
    {
        using var reader = new StringReader(text);
        using var csv = new CsvReader(reader, csvConfig);
        return csv.GetRecords<T>().AsValueEnumerable().ToList();
    }

    private Dictionary<TKey, TItem> ParseToDictionary<TKey, TItem>(string text, Func<TItem, TKey> key)
    {
        using var reader = new StringReader(text);
        using var csv = new CsvReader(reader, csvConfig);
        return csv.GetRecords<TItem>().AsValueEnumerable().ToDictionary(key);
    }
}
