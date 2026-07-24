#if UNITY_EDITOR
using CsvHelper;
using CsvHelper.Configuration;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ZLinq;
using UnityEditor;
using UnityEngine;

public static class Table
{
    private static void GenerateLocalizationKey(List<LocalizationData> records)
    {
        string filePath = Path.Combine(Application.dataPath, "Scripts/Enums/Localization.cs");
        string dir = Path.GetDirectoryName(filePath);

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var uniqueKeys = records.Where(data => !string.IsNullOrWhiteSpace(data.Key)).Select(data => data.Key.Trim()).Distinct().ToList();

        using (var writer = new StreamWriter(filePath))
        {
            writer.WriteLine("public enum Localization");
            writer.WriteLine("{");

            foreach (var key in uniqueKeys)
                writer.WriteLine($"    {key},");

            writer.WriteLine("}");
        }
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

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Comment = '#',
            AllowComments = true,
            IgnoreBlankLines = true,
            HeaderValidated = null,
            MissingFieldFound = null,
        };

        try
        {
            using (var reader = new StreamReader(csvPath))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<T>().ToList();
                Bake(name, records);
            }
        }
        catch
        {
            EditorUtility.DisplayDialog("Conversion Error", $"An error occurred while converting {name}", "OK");
        }
    }

    public static void ConvertAndMergeLocalization(List<string> filePaths)
    {
        var mergedRecords = new List<LocalizationData>();
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Comment = '#',
            AllowComments = true,
            IgnoreBlankLines = true,
            HeaderValidated = null,
            MissingFieldFound = null,
        };

        foreach (var filePath in filePaths)
        {
            try
            {
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, config))
                {
                    var records = csv.GetRecords<LocalizationData>().ToList();

                    foreach (var record in records)
                    {
                        if (record.Key != null)
                            record.Key = record.Key.Trim();
                    }

                    mergedRecords.AddRange(records);
                }
            }
            catch
            {
                EditorUtility.DisplayDialog("Localization Conversion Error", $"Failed to parse localization file:\n{Path.GetFileName(filePath)}", "OK");
                
                return;
            }
        }

        GenerateLocalizationKey(mergedRecords);
        Bake("Localization", mergedRecords);
    }
}
#endif
