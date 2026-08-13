#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class PoolDebugger : EditorWindow
{
    private Dictionary<string, bool> _outs = new Dictionary<string, bool>();

    [MenuItem("Tools/Pool Debugger")]
    public static void ShowWindow() 
        => GetWindow<PoolDebugger>("Pool Debugger");

    private void OnGUI()
    {
        if (!TryValidatePlayModeAndManagers(out var poolRoot))
            return;

        DrawHeader();
        DrawPoolSystemStatus(poolRoot);
    }

    private bool TryValidatePlayModeAndManagers(out Transform poolRoot)
    {
        poolRoot = null;

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Available only in Play Mode.", MessageType.Info);
            return false;
        }

        if (Managers.Pool == null)
        {
            EditorGUILayout.HelpBox("Managers.Pool has not been initialized yet.", MessageType.Warning);
            return false;
        }

        poolRoot = Managers.Pool.Root.transform;
        return true;
    }

    private void DrawHeader()
    {
        GUILayout.Label("Pool System Status (Grouped by Folders)", EditorStyles.boldLabel);
        EditorGUILayout.Space();
    }

    private void DrawPoolSystemStatus(Transform poolRoot)
    {
        var groupedPools = CollectGroupedPools(poolRoot);

        foreach (var group in groupedPools)
            DrawFolderGroup(group.Key, group.Value);
    }

    private Dictionary<string, List<KeyValuePair<string, int>>> CollectGroupedPools(Transform poolRoot)
    {
        var groupedPools = new Dictionary<string, List<KeyValuePair<string, int>>>();

        for (int index = 0; index < poolRoot.childCount; index++)
        {
            Transform folder = poolRoot.GetChild(index);
            string folderName = folder.name;

            if (!groupedPools.ContainsKey(folderName))
                groupedPools[folderName] = new List<KeyValuePair<string, int>>();

            PopulateFolderItems(folder, groupedPools[folderName]);
        }

        return groupedPools;
    }

    private void PopulateFolderItems(Transform folder, List<KeyValuePair<string, int>> items)
    {
        for (int sub = 0; sub < folder.childCount; sub++)
        {
            Transform pooledObj = folder.GetChild(sub);
            string key = pooledObj.name;
            int index = items.FindIndex(x => x.Key == key);

            if (index != -1)
            {
                var existing = items[index];
                items[index] = new KeyValuePair<string, int>(key, existing.Value + 1);
            }
            else
                items.Add(new KeyValuePair<string, int>(key, 1));
        }
    }

    private void DrawFolderGroup(string folderName, List<KeyValuePair<string, int>> items)
    {
        if (!_outs.ContainsKey(folderName))
            _outs[folderName] = true;

        int totalCount = CalculateTotalCount(items);
        _outs[folderName] = EditorGUILayout.Foldout(_outs[folderName], $"{folderName} (Total Cached: {totalCount})", true);

        if (!_outs[folderName])
            return;

        EditorGUI.indentLevel++;
        DrawFolderItems(items);
        EditorGUI.indentLevel--;
        EditorGUILayout.Space(5);
    }

    private int CalculateTotalCount(List<KeyValuePair<string, int>> items)
    {
        int total = 0;

        for (int index = 0; index < items.Count; index++)
            total += items[index].Value;

        return total;
    }

    private void DrawFolderItems(List<KeyValuePair<string, int>> items)
    {
        if (items.Count == 0)
        {
            EditorGUILayout.LabelField("Empty", EditorStyles.miniLabel);
            return;
        }

        for (int index = 0; index < items.Count; index++)
        {
            var item = items[index];
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField(item.Key, GUILayout.Width(200));
            EditorGUILayout.LabelField($"Count: {item.Value}", GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif
