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
        Transform poolRoot = Managers.Pool.Root.transform;
        Dictionary<string, List<KeyValuePair<string, int>>> groupedPools = new Dictionary<string, List<KeyValuePair<string, int>>>();
        List<KeyValuePair<string, int>> rootPools = new List<KeyValuePair<string, int>>();

        for (int index = 0; index < poolRoot.childCount; index++)
        {
            Transform folder = poolRoot.GetChild(index);
            string folderName = folder.name;

            if (!groupedPools.ContainsKey(folderName))
                groupedPools[folderName] = new List<KeyValuePair<string, int>>();

            for (int sub = 0; sub < folder.childCount; sub++)
            {
                Transform pooledObj = folder.GetChild(sub);
                string key = pooledObj.name;
                int count = groupedPools[folderName].FindIndex(x => x.Key == key);

                if (count != -1)
                    groupedPools[folderName][count] = new KeyValuePair<string, int>(key, groupedPools[folderName][count].Value + 1);
                else
                    groupedPools[folderName].Add(new KeyValuePair<string, int>(key, 1));
            }
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
    }
}
#endif
