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
        var localizationFiles = new List<string>();
        var otherFiles = new List<string>();
        CategorizeFiles(csvFiles, localizationFiles, otherFiles);
        int total = otherFiles.Count + (localizationFiles.Count > 0 ? 1 : 0);
        int success = 0;
        int currentProgress = 0;

        for (int index = 0; index < otherFiles.Count; index++)
        {
            string file = otherFiles[index];
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

    private static void CategorizeFiles(string[] csvFiles, List<string> localizationFiles, List<string> otherFiles)
    {
        for (int index = 0; index < csvFiles.Length; index++)
        {
            string file = csvFiles[index];
            string name = Path.GetFileNameWithoutExtension(file);

            if (name.StartsWith("Localization", StringComparison.OrdinalIgnoreCase))
                localizationFiles.Add(file);
            else
                otherFiles.Add(file);
        }
    }

    private static bool ConvertTable(string name)
    {
        Type dataType = FindType(name);

        if (dataType == null)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Table Conversion Warning", $"Matching data type not found for table:\n'{name}'", "OK");
            return false;
        }

        MethodInfo method = typeof(Table).GetMethod("Convert", BindingFlags.Public | BindingFlags.Static);
        
        if (method == null)
            return false;

        MethodInfo genericMethod = method.MakeGenericMethod(dataType);
        genericMethod.Invoke(null, new object[] { name });
        return true;
    }

    private static bool ConvertAndEncryptionMergeLocalization(List<string> files)
    {
        MethodInfo method = typeof(Table).GetMethod("ConvertAndMergeLocalization", BindingFlags.Public | BindingFlags.Static);
        
        if (method == null)
            return false;

        method.Invoke(null, new object[] { files });
        return true;
    }

    private static Type FindType(string name)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        for (int index = 0; index < assemblies.Length; index++)
        {
            var types = assemblies[index].GetTypes();

            for (int sub = 0; sub < types.Length; sub++)
            {
                var type = types[sub];
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
