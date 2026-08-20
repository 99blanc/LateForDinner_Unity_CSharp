#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public class SaveDataCleaner
{
    [MenuItem("Tools/Data/Delete Config File")]
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

    [MenuItem("Tools/Data/Delete All Save Files")]
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

    [MenuItem("Tools/Data/Open Persistent Data Path")]
    public static void OpenPersistentDataPath()
    {
        if (Directory.Exists(Application.persistentDataPath))
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        else
            EditorUtility.DisplayDialog("Path Error", "PersistentDataPath folder does not exist.", "OK");
    }
}
#endif
