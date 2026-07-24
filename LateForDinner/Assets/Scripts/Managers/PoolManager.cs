using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager
{
    private GameObject _root;
    public GameObject Root
    {
        get
        {
            if (_root == null)
            {
                _root = new GameObject { name = Literal.Roots.Pools };
                _root.transform.SetParent(Managers.Instance.transform, false);
            }

            return _root;
        }
    }
    private readonly Dictionary<string, Queue<GameObject>> _registries = new Dictionary<string, Queue<GameObject>>();
    private readonly Dictionary<string, Transform> _folders = new Dictionary<string, Transform>();

    public async UniTask InitAsync()
        => await Setup();

    private async UniTask Setup()
    {
        string[] folders = {
            Literal.Roots.UserInterfaces
        };

        foreach (string name in folders)
        {
            Transform folder = new GameObject { name = name }.transform;
            folder.SetParent(Root.transform, false);
            _folders.Add(name, folder);
        }

        await UniTask.CompletedTask;
        Log.System(Localization.Log_Pool_SetupComplete, true, _folders.Count);
    }

    public async UniTask<(GameObject instance, IDisposable rentHandle)> PopAsync(string key, Transform parent = null)
    {
        GameObject instance = null;

        if (_registries.TryGetValue(key, out var queue) && queue.Count > 0)
        {
            instance = queue.Dequeue();

            if (instance != null)
            {
                instance.transform.SetParent(parent, false);
                instance.SetActive(true);
            }
        }

        if (instance == null)
        {
            GameObject prefab = await Managers.Resource.LoadPrefabAsync(key);
            bool isNotFound = prefab == null;
            Log.Error(Localization.Log_Pool_PopFailed, isNotFound, key);

            if (isNotFound) 
                return (null, null);

            Transform newParent = parent == null ? Get(key) : parent;
            instance = UnityEngine.Object.Instantiate(prefab, newParent, false);
            instance.name = key;
        }

        GameObject target = instance;
        IDisposable rentHandle = Disposable.Create(() => Push(target));

        return (instance, rentHandle);
    }

    public async UniTask<(T component, IDisposable rentHandle)> PopAsync<T>(Transform parent = null) where T : Component
    {
        string key = typeof(T).Name;
        var (instance, rentHandle) = await PopAsync(key, parent);

        return (instance.GetComponent<T>(), rentHandle);
    }

    public void Push(GameObject gameObject)
    {
        bool isNull = gameObject == null;
        Log.Error(Localization.Log_Pool_PushNull, isNull);

        if (isNull) 
            return;

        string key = gameObject.name;

        if (!_registries.ContainsKey(key))
            _registries.Add(key, new Queue<GameObject>());

        gameObject.SetActive(false); 
        gameObject.transform.SetParent(Get(key), false);
        _registries[key].Enqueue(gameObject);
    }

    public void Push(Component component)
    {
        if (component != null) 
            Push(component.gameObject);
    }

    private Transform Get(string key)
    {
        if (key.Contains(Literal.Keys.UI) && _folders.TryGetValue(Literal.Roots.UserInterfaces, out var user)) 
            return user;

        return Root.transform;
    }

    public void Clear()
    {
        foreach (var queue in _registries.Values)
        {
            while (queue.Count > 0)
            {
                GameObject instance = queue.Dequeue();

                if (instance != null) 
                    UnityEngine.Object.Destroy(instance);
            }
        }

        _registries.Clear();
        Log.System(Localization.Log_Pool_Cleared);
    }
}
