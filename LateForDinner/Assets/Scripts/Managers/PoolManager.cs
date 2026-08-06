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
                Setup();
            }

            return _root;
        }
    }
    private readonly Dictionary<string, Queue<GameObject>> _registries = new Dictionary<string, Queue<GameObject>>();
    private readonly Dictionary<string, Transform> _folders = new Dictionary<string, Transform>();
    private readonly Dictionary<GameObject, Transform> _parents = new Dictionary<GameObject, Transform>();
    private readonly Dictionary<string, string> _maps = new Dictionary<string, string>()
    {
        { Literal.Keys.UI, Literal.Roots.UserInterfaces },
    };

    private void Setup()
    {
        foreach (var folderName in _maps.Values)
        {
            if (!_folders.ContainsKey(folderName))
            {
                Transform folder = new GameObject { name = folderName }.transform;
                folder.SetParent(Root.transform, false);
                _folders.Add(folderName, folder);
            }
        }
    }

    public async UniTask<(GameObject instance, IDisposable rentHandle)> PopAsync(string key, Transform parent = null)
    {
        GameObject instance = null;
        bool isNew = false;

        if (_registries.TryGetValue(key, out var queue) && queue.Count > 0)
            instance = queue.Dequeue();

        if (instance == null)
        {
            Transform newParent = parent == null ? Get(key) : parent;
            instance = await Managers.Resource.InstantiateAsync(key, newParent, false);

            if (instance == null)
                return (null, null);

            instance.name = key;
            _parents[instance] = newParent;
            isNew = true;
        }
        else
        {
            Transform original = parent != null ? parent : (_parents.TryGetValue(instance, out var p) ? p : Get(key));
            instance.transform.SetParent(original, false);
            instance.SetActive(true);
        }

        if (instance.TryGetComponent<IPoolable>(out var poolable))
        {
            if (isNew)
                poolable.Init();
            else
                poolable.Get();
        }

        GameObject target = instance;
        IDisposable rentHandle = Disposable.Create(() => Push(target, key));

        return (instance, rentHandle);
    }

    public async UniTask<(T component, IDisposable rentHandle)> PopAsync<T>(Transform parent = null) where T : Component
    {
        string key = typeof(T).Name;
        var (instance, rentHandle) = await PopAsync(key, parent);

        return (instance.GetComponentAssert<T>(), rentHandle);
    }

    public (GameObject instance, IDisposable rentHandle) Pop(string key, Transform parent = null)
    {
        GameObject instance = null;
        bool isNew = false;

        if (_registries.TryGetValue(key, out var queue) && queue.Count > 0)
            instance = queue.Dequeue();

        if (instance == null)
        {
            Transform newParent = parent == null ? Get(key) : parent;
            instance = Managers.Resource.Instantiate(key, newParent, false);

            if (instance == null)
                return (null, null);

            instance.name = key;
            _parents[instance] = newParent;
            isNew = true;
        }
        else
        {
            Transform original = parent != null ? parent : (_parents.TryGetValue(instance, out var p) ? p : Get(key));
            instance.transform.SetParent(original, false);
            instance.SetActive(true);
        }

        if (instance.TryGetComponent<IPoolable>(out var poolable))
        {
            if (isNew)
                poolable.Init();
            else
                poolable.Get();
        }

        GameObject target = instance;
        IDisposable rentHandle = Disposable.Create(() => Push(target, key));

        return (instance, rentHandle);
    }

    public (T component, IDisposable rentHandle) Pop<T>(Transform parent = null) where T : Component
    {
        string key = typeof(T).Name;
        var (instance, rentHandle) = Pop(key, parent);

        if (instance == null)
            return (null, null);

        return (instance.GetComponentAssert<T>(), rentHandle);
    }

    public void Push(GameObject gameObject, string key = null)
    {
        if (gameObject == null) 
            return;

        _parents[gameObject] = gameObject.transform.parent;
        string newKey = string.IsNullOrEmpty(key) ? gameObject.name : key;

        if (!_registries.ContainsKey(newKey))
            _registries.Add(newKey, new Queue<GameObject>());

        gameObject.SetActive(false); 
        gameObject.transform.SetParent(Get(newKey), false);
        _registries[newKey].Enqueue(gameObject);
    }

    public void Push(Component component, string key = null)
    {
        if (component != null) 
            Push(component.gameObject, key);
    }

    public async UniTask PrewarmAsync<T>(int count, Transform parent = null) where T : Component
    {
        string key = typeof(T).Name;

        if (_registries.TryGetValue(key, out var queue) && queue.Count >= count)
            return;

        for (int index = 0; index < count; index++)
        {
            var (instance, rentHandle) = await PopAsync<T>(parent);

            if (instance != null)
                rentHandle.Dispose();
        }
    }

    private Transform Get(string key)
    {
        foreach (var pair in _maps)
        {
            if (key.Contains(pair.Key))
            {
                if (_folders.TryGetValue(pair.Value, out var folder) && folder != null)
                    return folder;
            }
        }

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
        _parents.Clear();
    }
}
