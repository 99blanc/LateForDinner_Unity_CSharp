#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class ScriptExplorer : EditorWindow
{
    private Vector2 _scrollPosition;
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
        DrawActionButtons();
        DrawScriptList();
    }

    private void DrawActionButtons()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Refresh Script List"))
            RefreshScriptList();

        GUI.backgroundColor = new Color(0.8f, 0.95f, 1f);

        if (GUILayout.Button($"Open All Scripts ({_paths.Count})"))
            OpenAllScripts();

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5);
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

    private void OpenAllScripts()
    {
        if (_paths.Count == 0)
            return;

        if (_paths.Count > 50)
        {
            bool confirm = EditorUtility.DisplayDialog("Warning", $"There are {_paths.Count} scripts. Do you want to open all of them? (This may freeze your IDE)", "Yes", "No");

            if (!confirm)
                return;
        }

        foreach (var path in _paths)
            OpenScript(path);
    }
}
#endif
