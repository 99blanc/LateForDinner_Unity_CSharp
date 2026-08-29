#if UNITY_EDITOR
using CsvHelper;
using CsvHelper.Configuration;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using LateForDinner.Data;

public static class Table
{
    public static bool ConvertTableByName(string name)
    {
        switch (name)
        {
            case "Attribute":
                ConvertGeneric<AttributeData>(name, GenerateAttributeType);
                break;
            case "Character":
                ConvertGeneric<CharacterData>(name, GenerateCharacterID);
                break;
            case "Scene":
                ConvertGeneric<SceneData>(name, GenerateSceneID);
                break;
            case "SceneTransition":
                ConvertGeneric<SceneTransitionData>(name, GenerateSceneTransitionID);
                break;
            default:
                return false;
        }
        return true;
    }

    public static void ConvertGeneric<T>(string name, Action<List<T>> generateEnumAction = null)
    {
        string csvPath = Path.Combine(Application.dataPath, Literal.Paths.Tables, $"{name}.csv");

        if (!File.Exists(csvPath))
        {
            EditorUtility.DisplayDialog("Conversion Failed", $"File does not exist:\n{csvPath}", "OK");
            return;
        }

        var config = CreateCsvConfiguration();

        try
        {
            using var reader = new StreamReader(csvPath);
            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<T>().ToList();
            generateEnumAction?.Invoke(records);
            Bake(name, records);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Conversion Error] Table: {name}, Exception: {ex}");
            EditorUtility.DisplayDialog("Conversion Error", $"An error occurred while converting {name}", "OK");
        }
    }

    public static void GenerateAttributeType(List<AttributeData> records)
    {
        string filePath = Path.Combine(Application.dataPath, "Scripts/Enums/AttributeType.cs");
        WriteEnumFile(filePath, "AttributeType", records, data => data.Key, data => $"    {data.Key.Trim().PadRight(GetMaxKeyLength(records, d => d.Key))}, // {data.DataType}");
    }

    public static void GenerateCharacterID(List<CharacterData> records)
    {
        string filePath = Path.Combine(Application.dataPath, "Scripts/Enums/CharacterID.cs");
        WriteEnumFile(filePath, "CharacterID", records, data => data.Name, data => $"    {data.Name.Trim().PadRight(GetMaxKeyLength(records, d => d.Name))} = {data.ID},");
    }

    public static void GenerateSceneID(List<SceneData> records)
    {
        string filePath = Path.Combine(Application.dataPath, "Scripts/Enums/SceneID.cs");
        WriteEnumFile(filePath, "SceneID", records, data => data.Tag, data => $"    {data.Tag.Trim().PadRight(GetMaxKeyLength(records, d => d.Tag))} = {data.ID},");
    }

    public static void GenerateSceneTransitionID(List<SceneTransitionData> records)
    {
        string filePath = Path.Combine(Application.dataPath, "Scripts/Enums/SceneTransitionType.cs");
        WriteEnumFile(filePath, "SceneTransitionType", records, data => data.Type, data => $"    {data.Type.Trim().PadRight(GetMaxKeyLength(records, d => d.Type))},");
    }

    private static void GenerateLocalizationKey(List<LocalizationData> records)
    {
        string filePath = Path.Combine(Application.dataPath, "Scripts/Enums/LocalizationKey.cs");
        WriteEnumFile(filePath, "LocalizationKey", records, data => data.Key, data =>
        {
            string key = data.Key.Trim();
            string textComment = !string.IsNullOrWhiteSpace(data.Text) ? $" // {data.Text.Replace("\n", " ")}" : string.Empty;
            return $"    {key.PadRight(GetMaxKeyLength(records, d => d.Key))},{textComment}";
        });
    }

    private static void WriteEnumFile<T>(string filePath, string enumName, List<T> records, Func<T, string> keySelector, Func<T, string> lineFormatter)
    {
        string dir = Path.GetDirectoryName(filePath);

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var validRecords = records.Where(data => !string.IsNullOrWhiteSpace(keySelector(data))).ToList();
        var uniqueRecords = validRecords.GroupBy(data => keySelector(data).Trim()).Select(group => group.First()).ToList();
        using var writer = new StreamWriter(filePath);
        writer.WriteLine($"public enum {enumName}");
        writer.WriteLine("{");

        foreach (var data in uniqueRecords)
            writer.WriteLine(lineFormatter(data));

        writer.WriteLine("}");
    }

    private static int GetMaxKeyLength<T>(List<T> records, Func<T, string> keySelector)
    {
        int maxLength = 0;

        foreach (var data in records)
        {
            string key = keySelector(data);

            if (!string.IsNullOrEmpty(key))
            {
                int len = key.Trim().Length;

                if (len > maxLength)
                    maxLength = len;
            }
        }

        return maxLength;
    }

    public static void Bake<T>(string name, List<T> data)
    {
        byte[] bytes = MemoryPackSerializer.Serialize(data);

        for (int index = 0; index < bytes.Length; index++)
            bytes[index] ^= Key.Values[index % Key.Values.Length];

        string folderPath = Path.Combine(Application.dataPath, Literal.Paths.Binaries);
        Directory.CreateDirectory(folderPath);
        string filePath = Path.Combine(folderPath, $"{name}.bytes");
        File.WriteAllBytes(filePath, bytes);
        AssetDatabase.Refresh();
    }

    public static void ConvertAndMergeLocalization(List<string> filePaths)
    {
        var mergedRecords = new List<LocalizationData>();
        var config = CreateCsvConfiguration();

        for (int index = 0; index < filePaths.Count; index++)
        {
            string filePath = filePaths[index];

            if (!TryReadLocalizationFile(filePath, config, mergedRecords))
                return;
        }

        GenerateLocalizationKey(mergedRecords);
        Bake("Localization", mergedRecords);
    }

    private static CsvConfiguration CreateCsvConfiguration() => new(CultureInfo.InvariantCulture)
    {
        Comment = '#',
        AllowComments = true,
        IgnoreBlankLines = true,
        HeaderValidated = null,
        MissingFieldFound = null,
    };

    private static bool TryReadLocalizationFile(string filePath, CsvConfiguration config, List<LocalizationData> mergedRecords)
    {
        try
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<LocalizationData>().ToList();

            for (int index = 0; index < records.Count; index++)
            {
                var record = records[index];

                if (record.Key != null)
                    record.Key = record.Key.Trim();
            }

            mergedRecords.AddRange(records);
            return true;
        }
        catch
        {
            EditorUtility.DisplayDialog("Localization Conversion Error", $"Failed to parse localization file:\n{Path.GetFileName(filePath)}", "OK");
            return false;
        }
    }
}
#endif
