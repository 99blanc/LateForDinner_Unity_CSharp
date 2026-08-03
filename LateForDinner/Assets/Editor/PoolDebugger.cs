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
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Available only in Play Mode.", MessageType.Info);
            
            return;
        }

        if (Managers.Pool == null)
        {
            EditorGUILayout.HelpBox("Managers.Pool has not been initialized yet.", MessageType.Warning);
            
            return;
        }

        GUILayout.Label("Pool System Status (Grouped by Folders)", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        var registries = Managers.Pool.GetRegistrySnapshot();
        var maps = Managers.Pool.GetMapsSnapshot();
        Dictionary<string, List<KeyValuePair<string, int>>> groupedPools = new Dictionary<string, List<KeyValuePair<string, int>>>();
        List<KeyValuePair<string, int>> rootPools = new List<KeyValuePair<string, int>>();

        foreach (string folderName in maps.Values)
        {
            if (!groupedPools.ContainsKey(folderName))
                groupedPools[folderName] = new List<KeyValuePair<string, int>>();
        }

        foreach (var pair in registries)
        {
            string key = pair.Key;
            int count = pair.Value;
            var itemPair = new KeyValuePair<string, int>(key, count);
            bool matched = false;

            foreach (var mapPair in maps)
            {
                string mapKey = mapPair.Key;
                string folderName = mapPair.Value;

                if (key.Contains(mapKey))
                {
                    if (groupedPools.ContainsKey(folderName))
                    {
                        groupedPools[folderName].Add(itemPair);
                        matched = true;

                        break;
                    }
                }
            }

            if (!matched)
                rootPools.Add(itemPair);
        }

        foreach (var group in groupedPools)
        {
            string folderName = group.Key;
            var items = group.Value;

            if (!_outs.ContainsKey(folderName))
                _outs[folderName] = true;

            int totalCount = 0;

            foreach (var item in items) 
                totalCount += item.Value;

            _outs[folderName] = EditorGUILayout.Foldout(_outs[folderName], $"{folderName} (Total Cached: {totalCount})", true);

            if (_outs[folderName])
            {
                EditorGUI.indentLevel++;

                if (items.Count == 0)
                    EditorGUILayout.LabelField("Empty", EditorStyles.miniLabel);
                else
                {
                    foreach (var item in items)
                    {
                        EditorGUILayout.BeginHorizontal("box");
                        EditorGUILayout.LabelField(item.Key, GUILayout.Width(200));
                        EditorGUILayout.LabelField($"Count: {item.Value}", GUILayout.Width(100));
                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space(5);
            }
        }

        if (rootPools.Count > 0)
        {
            if (!_outs.ContainsKey("Root"))
                _outs["Root"] = true;

            _outs["Root"] = EditorGUILayout.Foldout(_outs["Root"], $"Root / Others", true);
            
            if (_outs["Root"])
            {
                EditorGUI.indentLevel++;

                foreach (var item in rootPools)
                {
                    EditorGUILayout.BeginHorizontal("box");
                    EditorGUILayout.LabelField(item.Key, GUILayout.Width(200));
                    EditorGUILayout.LabelField($"Count: {item.Value}", GUILayout.Width(100));
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;
            }
        }

        Repaint();
    }
}
#endif
