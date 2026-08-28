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
        {
            EditorUtility.DisplayDialog("Path Error", $"Tables folder does not exist at:\n{path}", "OK");
            return;
        }

        string[] csvFiles = Directory.GetFiles(path, "*.csv");

        if (csvFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("Path Error", "No CSV files found in the tables folder.", "OK");
            return;
        }

        CleanupObsoleteBinaries(csvFiles);
        var localizationFiles = new List<string>();
        var otherFiles = new List<string>();
        CategorizeFiles(csvFiles, localizationFiles, otherFiles);
        int localizationUnitCount = localizationFiles.Count > 0 ? 1 : 0;
        int total = otherFiles.Count + localizationUnitCount;
        int successGeneral = 0;
        bool successLocalization = false;
        int currentProgress = 0;

        for (int index = 0; index < otherFiles.Count; index++)
        {
            string file = otherFiles[index];
            string name = Path.GetFileNameWithoutExtension(file);
            EditorUtility.DisplayProgressBar("Converting Tables...", $"Processing: {name}", (float)currentProgress / total);

            if (ConvertTable(name))
                successGeneral++;

            currentProgress++;
        }

        if (localizationFiles.Count > 0)
        {
            EditorUtility.DisplayProgressBar("Converting Tables...", "Processing Localization Tables...", (float)currentProgress / total);

            if (ConvertAndEncryptionMergeLocalization(localizationFiles))
                successLocalization = true;

            currentProgress++;
        }

        EditorUtility.ClearProgressBar();
        string locStatus = localizationFiles.Count > 0 ? (successLocalization ? $"Merged ({localizationFiles.Count} files)" : "Failed") : "None";
        string resultMessage = "Table Conversion Complete!\n\n" + $"General Tables: {successGeneral} / {otherFiles.Count} processed\n" + $"Localization: {locStatus}";
        EditorUtility.DisplayDialog("Table Bake Result", resultMessage, "OK");
    }

    private static void CleanupObsoleteBinaries(string[] csvFiles)
    {
        string binariesPath = Path.Combine(Application.dataPath, Literal.Paths.Binaries);

        if (!Directory.Exists(binariesPath)) 
            return;

        var validNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in csvFiles)
        {
            string name = Path.GetFileNameWithoutExtension(file);

            if (name.StartsWith("Localization", StringComparison.OrdinalIgnoreCase))
                validNames.Add("Localization");
            else
                validNames.Add(name);
        }

        string[] existingBytesFiles = Directory.GetFiles(binariesPath, "*.bytes");

        foreach (var bytesFile in existingBytesFiles)
        {
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(bytesFile);

            if (!validNames.Contains(fileNameWithoutExt))
            {
                File.Delete(bytesFile);
                string metaFile = bytesFile + ".meta";

                if (File.Exists(metaFile))
                    File.Delete(metaFile);

                Debug.Log($"[Converter] Removed obsolete binary file: {fileNameWithoutExt}.bytes");
            }
        }

        AssetDatabase.Refresh();
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
        MethodInfo method = typeof(Table).GetMethod("ConvertTableByName", BindingFlags.Public | BindingFlags.Static);

        if (method != null)
        {
            var result = method.Invoke(null, new object[] { name });
            if (result is bool handled && handled)
                return true;
        }

        string className = name + "Data";
        Type dataType = FindType(className);

        if (dataType == null)
        {
            Debug.LogError($"[Converter] Matching data type not found for table: '{name}' (Looking for class: '{className}')");
            return false;
        }

        MethodInfo genericMethod = typeof(Table).GetMethod("ConvertGeneric", BindingFlags.Public | BindingFlags.Static)?.MakeGenericMethod(dataType);

        if (genericMethod == null)
        {
            Debug.LogError("[Converter] ConvertGeneric method not found in Table class.");
            return false;
        }

        genericMethod.Invoke(null, new object[] { name, null });
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
        Type targetType = Type.GetType($"LateForDinner.Data.{name}, Assembly-CSharp");

        if (targetType != null)
            return targetType;

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        for (int index = 0; index < assemblies.Length; index++)
        {
            var types = assemblies[index].GetTypes();

            for (int sub = 0; sub < types.Length; sub++)
            {
                var type = types[sub];

                if (type.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    string ns = type.Namespace ?? string.Empty;

                    if (ns.Equals("LateForDinner.Data", StringComparison.Ordinal))
                        return type;

                    if (ns.StartsWith("System") || ns.StartsWith("UnityEngine") || ns.StartsWith("UnityEditor") || ns.StartsWith("Unity.UI"))
                        continue;

                    return type;
                }
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
