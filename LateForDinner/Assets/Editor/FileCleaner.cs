#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public class FileCleaner
{
    [MenuItem("Tools/Files/Delete Config File")]
    public static void DeleteConfig()
    {
        if (!EditorUtility.DisplayDialog("Warning", "Are you sure you want to delete the config file?", "Yes", "No"))
            return;

        string configPath = Path.Combine(Application.persistentDataPath, $"{Literal.Files.Config}{Literal.Extensions.Bytes}");
        string tempPath = Path.Combine(Application.persistentDataPath, $"{Literal.Files.Config}{Literal.Extensions.Temp}");
        bool deleted = false;

        if (File.Exists(configPath))
        {
            File.Delete(configPath);
            deleted = true;
        }

        if (File.Exists(tempPath))
        {
            File.Delete(tempPath);
            deleted = true;
        }

        AssetDatabase.Refresh();
        string message = deleted ? "Config file has been deleted successfully." : "Config file does not exist.";
        EditorUtility.DisplayDialog("Config Cleaner", message, "OK");
    }

    [MenuItem("Tools/Files/Delete All Save Files")]
    public static void DeleteAllSaves()
    {
        if (!EditorUtility.DisplayDialog("Warning", "Are you sure you want to delete all save data and metadata?", "Yes", "No"))
            return;

        string savesDir = Literal.Folders.Saves.GetDirectory();

        if (Directory.Exists(savesDir))
        {
            Directory.Delete(savesDir, true);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Save Cleaner", "All save files and backups have been deleted.", "OK");
        }
        else
            EditorUtility.DisplayDialog("Save Cleaner", "Save folder does not exist.", "OK");
    }

    [MenuItem("Tools/Files/Delete All Localization Files")]
    public static void DeleteAllLocalizations()
    {
        if (!EditorUtility.DisplayDialog("Warning", "Are you sure you want to delete all localization files?", "Yes", "No"))
            return;

        string localizationDir = Literal.Folders.Localizations.GetDirectory();

        if (Directory.Exists(localizationDir))
        {
            string[] files = Directory.GetFiles(localizationDir, "*.json");
            bool deleted = false;

            foreach (var file in files)
            {
                File.Delete(file);
                deleted = true;
            }

            AssetDatabase.Refresh();
            string message = deleted ? "All localization files have been deleted successfully." : "No localization files found.";
            EditorUtility.DisplayDialog("Localization Cleaner", message, "OK");
        }
        else
            EditorUtility.DisplayDialog("Localization Cleaner", "Localization folder does not exist.", "OK");
    }

    [MenuItem("Tools/Files/Open Persistent Data Path")]
    public static void OpenPersistentDataPath()
    {
        if (Directory.Exists(Application.persistentDataPath))
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        else
            EditorUtility.DisplayDialog("Path Error", "PersistentDataPath folder does not exist.", "OK");
    }
}
#endif
