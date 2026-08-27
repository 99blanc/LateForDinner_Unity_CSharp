#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.IO;

public class Addressable
{
    [MenuItem("Tools/Addressables/Auto Setup All")]
    public static void SetupAll()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

        if (settings == null)
            return;

        int binariesCount = RegisterFolderToGroup(settings, "Assets/" + Literal.Paths.Binaries, Literal.Groups.Binaries);
        int systemsCount = RegisterFolderToGroup(settings, "Assets/" + Literal.Paths.Systems, Literal.Groups.Systems);
        int systemPrefabsCount = RegisterFolderToGroup(settings, "Assets/" + Literal.Paths.SystemPrefabs, Literal.Groups.Systems);
        int uiCount = RegisterFolderToGroup(settings, "Assets/" + Literal.Paths.UIPrefabs, Literal.Groups.UserInterfaces);
        int atlasCount = RegisterFolderToGroup(settings, "Assets/" + Literal.Paths.Atlases, Literal.Groups.Atlases);
        int animatorsCount = RegisterFolderToGroup(settings, "Assets/" + Literal.Paths.Animators, Literal.Groups.Animators);
        int clipsCount = RegisterFolderToGroup(settings, "Assets/" + Literal.Paths.Clips, Literal.Groups.Clips);
        int prefabsCount = RegisterFolderToGroup(settings, "Assets/" + Literal.Paths.Prefabs, Literal.Groups.Prefabs, Literal.Paths.UIPrefabs, Literal.Paths.SystemPrefabs);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Addressables Setup", $"Animators: {animatorsCount}\nClips: {clipsCount}\nAtlases: {atlasCount}\nBinaries: {binariesCount}\nSystems: {systemsCount + systemPrefabsCount}\nUserInterfaces: {uiCount}\nPrefabs: {prefabsCount}\nTotal processed", "OK");
    }

    [MenuItem("Tools/Addressables/Auto Setup Atlases")]
    public static void SetupAtlases()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

        if (settings == null)
            return;

        int atlasCount = RegisterFolderToGroup(settings, "Assets/" + Literal.Paths.Atlases, Literal.Groups.Atlases);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Addressables Atlas Setup", $"Atlases: {atlasCount} assets processed", "OK");
    }

    [MenuItem("Tools/Addressables/Auto Setup Animators")]
    public static void SetupAnimators()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

        if (settings == null)
            return;

        int animatorsCount = RegisterFolderToGroup(settings, "Assets/" + Literal.Paths.Animators, Literal.Groups.Animators);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Addressables Animators Setup", $"Animators: {animatorsCount} assets processed", "OK");
    }

    [MenuItem("Tools/Addressables/Auto Setup Clips")]
    public static void SetupClips()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

        if (settings == null)
            return;

        int clipsCount = RegisterFolderToGroup(settings, "Assets/" + Literal.Paths.Clips, Literal.Groups.Clips);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Addressables Clips Setup", $"Clips: {clipsCount} assets processed", "OK");
    }

    [MenuItem("Tools/Addressables/Auto Setup Prefabs")]
    public static void SetupPrefabs()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

        if (settings == null)
            return;

        int prefabsCount = RegisterFolderToGroup(settings, "Assets/" + Literal.Paths.Prefabs, Literal.Groups.Prefabs);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Addressables Prefabs Setup", $"Prefabs: {prefabsCount} assets processed", "OK");
    }

    [MenuItem("Tools/Addressables/Auto Setup Binaries and Systems")]
    public static void SetupSystems()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

        if (settings == null) 
            return;

        int binariesCount = RegisterFolderToGroup(settings, "Assets/" + Literal.Paths.Binaries, Literal.Groups.Binaries);
        int systemsCount = RegisterFolderToGroup(settings, "Assets/" + Literal.Paths.Systems, Literal.Groups.Systems);
        int systemPrefabsCount = RegisterFolderToGroup(settings, "Assets/" + Literal.Paths.SystemPrefabs, Literal.Groups.Systems);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Addressables Setup", $"Binaries: {binariesCount} registered\nSystems: {systemsCount + systemPrefabsCount} registered", "OK");
    }

    [MenuItem("Tools/Addressables/Auto Setup UserInterfaces")]
    public static void SetupUserInterfaces()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

        if (settings == null) 
            return;

        int uiCount = RegisterFolderToGroup(settings, "Assets/" + Literal.Paths.UIPrefabs, Literal.Groups.UserInterfaces);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Addressables UI Setup", $"UserInterfaces: {uiCount} assets processed", "OK");
    }

    private static int RegisterFolderToGroup(AddressableAssetSettings settings, string path, string name, params string[] excludePaths)
    {
        if (!Directory.Exists(path))
            return 0;

        AddressableAssetGroup group = settings.FindGroup(name);

        if (group == null)
            group = settings.CreateGroup(name, false, false, true, settings.DefaultGroup.Schemas);

        string[] guids = AssetDatabase.FindAssets("", new[] { path });
        int count = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            if (Directory.Exists(assetPath))
                continue;

            bool shouldExclude = false;

            if (excludePaths != null)
            {
                foreach (var exclude in excludePaths)
                {
                    if (!string.IsNullOrEmpty(exclude) && assetPath.Contains(exclude))
                    {
                        shouldExclude = true;
                        break;
                    }
                }
            }

            if (shouldExclude)
                continue;

            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);

            if (entry != null)
            {
                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                entry.address = fileName;
                count++;
            }
        }

        EditorUtility.SetDirty(settings);
        return count;
    }

    [MenuItem("Tools/Addressables/Clean Addresses to File Names")]
    public static void CleanAddresses()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

        if (settings == null) 
            return;

        int count = 0;

        foreach (var group in settings.groups)
        {
            if (group == null) 
                continue;

            foreach (var entry in group.entries)
            {
                string assetPath = entry.AssetPath;
                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                entry.address = fileName;
                count++;
            }
        }

        EditorUtility.SetDirty(settings);
        EditorUtility.DisplayDialog("Addressables Clean", $"Successfully cleaned addresses for {count} assets.", "OK");
    }
}
#endif
