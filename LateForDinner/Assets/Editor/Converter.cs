#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class Converter
{
    [MenuItem("Tools/Tables/Convert All Tables")]
    public static void ConvertAll()
    {
        string path = Path.Combine(Application.dataPath, Literal.Paths.Tables);

        if (!Directory.Exists(path))
            return;

        string[] csvFiles = Directory.GetFiles(path, "*.csv");
        List<string> localizationFiles = new List<string>();
        List<string> otherFiles = new List<string>();

        foreach (var file in csvFiles)
        {
            string name = Path.GetFileNameWithoutExtension(file);

            if (name.StartsWith("Localization", StringComparison.OrdinalIgnoreCase))
                localizationFiles.Add(file);
            else
                otherFiles.Add(file);
        }

        int total = otherFiles.Count + (localizationFiles.Count > 0 ? 1 : 0);
        int success = 0;
        int currentProgress = 0;

        foreach (var file in otherFiles)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            EditorUtility.DisplayProgressBar("Converting Tables...", $"Processing: {name}", (float)currentProgress / total);

            if (ConvertTable(name))
                success++;

            currentProgress++;
        }

        if (localizationFiles.Count > 0)
        {
            EditorUtility.DisplayProgressBar("Converting Tables...", "Processing Localization Tables...", (float)currentProgress / total);

            if (ConvertAndEncryptionMergeLocalization(localizationFiles))
                success += localizationFiles.Count;

            currentProgress++;
        }

        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("Table Bake", $"Conversion Complete: {success} converted", "OK");
    }

    private static bool ConvertTable(string name)
    {
        Type dataType = FindType(name);

        if (dataType != null)
        {
            MethodInfo method = typeof(Table).GetMethod("Convert", BindingFlags.Public | BindingFlags.Static);
            MethodInfo genericMethod = method.MakeGenericMethod(dataType);
            genericMethod.Invoke(null, new object[] { name });

            return true;
        }
        else
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Table Conversion Warning", $"Matching data type not found for table:\n'{name}'", "OK");

            return false;
        }
    }

    private static bool ConvertAndEncryptionMergeLocalization(List<string> files)
    {
        MethodInfo method = typeof(Table).GetMethod("ConvertAndMergeLocalization", BindingFlags.Public | BindingFlags.Static);

        if (method != null)
        {
            method.Invoke(null, new object[] { files });
            
            return true;
        }

        return false;
    }

    private static Type FindType(string name)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return type;
            }
        }

        return null;
    }

    [MenuItem("Tools/Tables/Open Table Folder")]
    public static void OpenTableFolder()
    {
        string path = Path.Combine(Application.dataPath, Literal.Paths.Tables);

        if (Directory.Exists(path))
            EditorUtility.RevealInFinder(path);
        else
            EditorUtility.DisplayDialog("Path Error", "Tables folder does not exist.", "OK");
    }

    [MenuItem("Assets/Open Table with Default App")]
    public static void OpenSelectedTable()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);

        if (path.EndsWith(".csv"))
            EditorUtility.OpenWithDefaultApp(path);
        else
            EditorUtility.DisplayDialog("Error", "Only CSV files can be opened.", "OK");
    }
}
#endif
