#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class ScriptExplorer : EditorWindow
{
    private Vector2 _position;
    private List<string> _paths = new List<string>();

    [MenuItem("Tools/Scripts/Open Script Explorer")]
    public static void ShowWindow()
    {
        var window = GetWindow<ScriptExplorer>("Script Explorer");
        window.RefreshScriptList();
    }

    private void RefreshScriptList()
    {
        _paths.Clear();
        string[] allFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
        _paths.AddRange(allFiles);
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Refresh Script List")) 
            RefreshScriptList();

        _position = EditorGUILayout.BeginScrollView(_position);

        foreach (var path in _paths)
        {
            string name = Path.GetFileName(path);
            if (GUILayout.Button(name, EditorStyles.label))
            {
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(FileUtil.GetProjectRelativePath(path));
                AssetDatabase.OpenAsset(script);
            }
        }

        EditorGUILayout.EndScrollView();
    }
}
#endif
