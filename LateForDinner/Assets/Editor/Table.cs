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
using UnityEditor.U2D.Animation;
using UnityEngine;
using ZLinq;

public static class Table
{
    private static void GenerateAttributeType(List<AttributeData> records)
    {
        string filePath = Path.Combine(Application.dataPath, "Scripts/Enums/AttributeType.cs");
        string dir = Path.GetDirectoryName(filePath);

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var validRecords = records
        .Where(data => !string.IsNullOrWhiteSpace(data.Key))
        .ToList();
        var uniqueRecords = validRecords
        .GroupBy(data => data.Key.Trim())
        .Select(group => group.First())
        .ToList();
        int maxKeyLength = 0;

        foreach (var data in uniqueRecords)
        {
            string key = data.Key.Trim();
            if (key.Length > maxKeyLength)
                maxKeyLength = key.Length;
        }

        using var writer = new StreamWriter(filePath);
        writer.WriteLine("public enum AttributeType");
        writer.WriteLine("{");

        foreach (var data in uniqueRecords)
        {
            string key = data.Key.Trim();
            string paddedKey = key.PadRight(maxKeyLength);
            writer.WriteLine($"    {paddedKey}, // {data.DataType}");
        }

        writer.WriteLine("}");
    }

    private static void GenerateCharacterID(List<CharacterData> records)
    {
        string filePath = Path.Combine(Application.dataPath, "Scripts/Enums/CharacterID.cs");
        string dir = Path.GetDirectoryName(filePath);

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var validRecords = records
            .Where(data => !string.IsNullOrWhiteSpace(data.Name))
            .ToList();

        var uniqueRecords = validRecords
            .GroupBy(data => data.Name.Trim())
            .Select(group => group.First())
            .ToList();

        int maxKeyLength = 0;

        foreach (var data in uniqueRecords)
        {
            string key = data.Name.Trim();
            if (key.Length > maxKeyLength)
                maxKeyLength = key.Length;
        }

        using var writer = new StreamWriter(filePath);
        writer.WriteLine("public enum CharacterID");
        writer.WriteLine("{");

        foreach (var data in uniqueRecords)
        {
            string key = data.Name.Trim();
            string paddedKey = key.PadRight(maxKeyLength);
            writer.WriteLine($"    {paddedKey} = {data.ID},");
        }

        writer.WriteLine("}");
    }

    private static void GenerateLocalizationKey(List<LocalizationData> records)
    {
        string filePath = Path.Combine(Application.dataPath, "Scripts/Enums/LocalizationKey.cs");
        string dir = Path.GetDirectoryName(filePath);

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var validRecords = records
        .Where(data => !string.IsNullOrWhiteSpace(data.Key))
        .ToList();
        var uniqueRecords = validRecords
        .GroupBy(data => data.Key.Trim())
        .Select(group => group.First())
        .ToList();
        int maxKeyLength = 0;

        foreach (var data in uniqueRecords)
        {
            string key = data.Key.Trim();
            if (key.Length > maxKeyLength)
                maxKeyLength = key.Length;
        }

        using var writer = new StreamWriter(filePath);
        writer.WriteLine("public enum LocalizationKey");
        writer.WriteLine("{");

        foreach (var data in uniqueRecords)
        {
            string key = data.Key.Trim();
            string paddedKey = key.PadRight(maxKeyLength);
            string textComment = !string.IsNullOrWhiteSpace(data.Text) ? $" // {data.Text.Replace("\n", " ")}" : string.Empty;
            writer.WriteLine($"    {paddedKey},{textComment}");
        }

        writer.WriteLine("}");
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

    public static void Convert<T>(string name)
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
            Bake(name, records);
        }
        catch
        {
            EditorUtility.DisplayDialog("Conversion Error", $"An error occurred while converting {name}", "OK");
        }
    }

    public static void ConvertAttribute(string name)
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
            var records = csv.GetRecords<AttributeData>().ToList();
            GenerateAttributeType(records);
            Bake(name, records);
        }
        catch
        {
            EditorUtility.DisplayDialog("Conversion Error", $"An error occurred while converting {name}", "OK");
        }
    }

    public static void ConvertCharacter(string name)
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
            var records = csv.GetRecords<CharacterData>().ToList();
            GenerateCharacterID(records);
            Bake(name, records);
        }
        catch
        {
            EditorUtility.DisplayDialog("Conversion Error", $"An error occurred while converting {name}", "OK");
        }
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
