using UnityEngine;
using UnityEditor;
using System.IO;

public class ScriptOpener
{
    [MenuItem("Tools/Open All Scripts %&o")]
    public static void OpenAll()
    {
        string[] paths = Directory.GetFiles("Assets", "*.cs", SearchOption.AllDirectories);

        foreach (var path in paths)
        {
            Object script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

            if (script)
                AssetDatabase.OpenAsset(script);
        }
    }
}
