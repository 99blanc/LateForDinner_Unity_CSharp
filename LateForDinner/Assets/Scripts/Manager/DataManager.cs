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
        TextAsset chTable = await Managers.Resource.LoadTextAsset(Define.Asset.TABLE_PLAYER);
        players = ParseToDictionary<PlayerID, PlayerData>(chTable.text, data => data.id);
    }

    public async UniTask Localization(LocalizationManager localization)
    {
        TextAsset uiTable = await Managers.Resource.LoadTextAsset(Define.Asset.TABLE_LOCALIZATION_UI);
        localization.UI.Data = ParseToDictionary<string, LocalizationData>(uiTable.text, data => data.ID);
        TextAsset statTable = await Managers.Resource.LoadTextAsset(Define.Asset.TABLE_LOCALIZATION_STAT);
        localization.Stat.Data = ParseToDictionary<string, LocalizationData>(statTable.text, data => data.ID);
        TextAsset dialogTable = await Managers.Resource.LoadTextAsset(Define.Asset.TABLE_LOCALIZATION_DIALOGUE);
        localization.Dialogue.Data = ParseToDictionary<string, LocalizationData>(dialogTable.text, data => data.ID);
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
