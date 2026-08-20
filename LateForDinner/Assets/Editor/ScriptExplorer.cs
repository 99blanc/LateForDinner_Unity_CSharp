#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class ScriptExplorer : EditorWindow
{
    private Vector2 _scrollPosition;
    private List<string> _paths = new List<string>();

    [MenuItem("Tools/Script/Open Script Explorer")]
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
        DrawRefreshButton();
        DrawScriptList();
    }

    private void DrawRefreshButton()
    {
        if (GUILayout.Button("Refresh Script List"))
            RefreshScriptList();
    }

    private void DrawScriptList()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        for (int index = 0; index < _paths.Count; index++)
        {
            string path = _paths[index];
            string scriptName = Path.GetFileName(path);

            if (!GUILayout.Button(scriptName, EditorStyles.label))
                continue;

            OpenScript(path);
        }

        EditorGUILayout.EndScrollView();
    }

    private void OpenScript(string absolutePath)
    {
        string relativePath = FileUtil.GetProjectRelativePath(absolutePath);
        var script = AssetDatabase.LoadAssetAtPath<MonoScript>(relativePath);

        if (script != null)
            AssetDatabase.OpenAsset(script);
    }
}
#endif
