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
                InitRoot();

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

    private void InitRoot()
    {
        _root = new GameObject { name = Literal.Roots.Pools };
        _root.transform.SetParent(Managers.Instance.transform, false);
        SetupFolders();
    }

    private void SetupFolders()
    {
        foreach (var folderName in _maps.Values)
        {
            if (_folders.ContainsKey(folderName))
                continue;

            Transform folder = new GameObject { name = folderName }.transform;
            folder.SetParent(Root.transform, false);
            _folders.Add(folderName, folder);
        }
    }

    public async UniTask<(GameObject instance, IDisposable rentHandle)> PopAsync(string key, Transform parent = null)
    {
        var (instance, isNew) = await GetOrCreateInstanceAsync(key, parent);

        if (instance == null)
            return (null, null);

        InitializePoolable(instance, isNew);
        IDisposable rentHandle = Disposable.Create(() => Push(instance, key));
        return (instance, rentHandle);
    }

    public async UniTask<(T component, IDisposable rentHandle)> PopAsync<T>(Transform parent = null) where T : Component
    {
        string key = typeof(T).Name;
        var (instance, rentHandle) = await PopAsync(key, parent);

        if (instance == null)
            return (null, null);

        return (instance.GetComponentAssert<T>(), rentHandle);
    }

    public (GameObject instance, IDisposable rentHandle) Pop(string key, Transform parent = null)
    {
        var (instance, isNew) = GetOrCreateInstance(key, parent);

        if (instance == null)
            return (null, null);

        InitializePoolable(instance, isNew);
        IDisposable rentHandle = Disposable.Create(() => Push(instance, key));
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

        if (gameObject.TryGetComponent<IPoolable>(out var poolable))
        {
            poolable.Release();
            poolable.SetPooled(true);
        }

        _parents[gameObject] = gameObject.transform.parent;
        string newKey = string.IsNullOrEmpty(key) ? gameObject.name : key;

        if (!_registries.ContainsKey(newKey))
            _registries.Add(newKey, new Queue<GameObject>());

        gameObject.SetActive(false);
        gameObject.transform.SetParent(GetFolder(newKey), false);
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

        int needed = count - (queue != null ? queue.Count : 0);

        for (int index = 0; index < needed; index++)
        {
            Transform targetParent = parent == null ? GetFolder(key) : parent;
            var instance = await Managers.Resource.InstantiateAsync(key, targetParent, false);

            if (instance == null)
                continue;

            instance.name = key;
            _parents[instance] = targetParent;

            if (instance.TryGetComponent<IPoolable>(out var poolable))
            {
                poolable.Init();
                poolable.SetPooled(true);
            }

            if (!_registries.ContainsKey(key))
                _registries.Add(key, new Queue<GameObject>());

            instance.SetActive(false);
            instance.transform.SetParent(GetFolder(key), false);
            _registries[key].Enqueue(instance);
        }
    }

    private async UniTask<(GameObject instance, bool isNew)> GetOrCreateInstanceAsync(string key, Transform parent)
    {
        if (_registries.TryGetValue(key, out var queue) && queue.Count > 0)
        {
            var cachedInstance = queue.Dequeue();
            PrepareCachedInstance(cachedInstance, parent, key);
            return (cachedInstance, false);
        }

        Transform newParent = parent == null ? GetFolder(key) : parent;
        var newInstance = await Managers.Resource.InstantiateAsync(key, newParent, false);

        if (newInstance == null)
        {
            Log.Error(Localization.Log_Pool_InstantiateFailed, key);
            return (null, false);
        }

        newInstance.name = key;
        _parents[newInstance] = newParent;
        return (newInstance, true);
    }

    private (GameObject instance, bool isNew) GetOrCreateInstance(string key, Transform parent)
    {
        if (_registries.TryGetValue(key, out var queue) && queue.Count > 0)
        {
            var cachedInstance = queue.Dequeue();
            PrepareCachedInstance(cachedInstance, parent, key);
            return (cachedInstance, false);
        }

        Transform newParent = parent == null ? GetFolder(key) : parent;
        var newInstance = Managers.Resource.Instantiate(key, newParent, false);

        if (newInstance == null)
        {
            Log.Error(Localization.Log_Pool_InstantiateFailed, key);
            return (null, false);
        }

        newInstance.name = key;
        _parents[newInstance] = newParent;
        return (newInstance, true);
    }

    private void PrepareCachedInstance(GameObject instance, Transform parent, string key)
    {
        Transform original = parent != null ? parent : (_parents.TryGetValue(instance, out var p) ? p : GetFolder(key));
        instance.transform.SetParent(original, false);
        instance.SetActive(true);
    }

    private void InitializePoolable(GameObject instance, bool isNew)
    {
        if (!instance.TryGetComponent<IPoolable>(out var poolable))
            return;

        if (isNew)
        {
            poolable.Init();
            poolable.SetPooled(false);
            return;
        }

        poolable.Get();
        poolable.SetPooled(false);
    }

    private Transform GetFolder(string key)
    {
        foreach (var pair in _maps)
        {
            if (!key.Contains(pair.Key))
                continue;

            if (_folders.TryGetValue(pair.Value, out var folder) && folder != null)
                return folder;
        }

        return Root.transform;
    }

    public void Clear()
    {
        int totalDestroyed = 0;

        foreach (var queue in _registries.Values)
        {
            while (queue.Count > 0)
            {
                GameObject instance = queue.Dequeue();

                if (instance != null)
                {
                    UnityEngine.Object.Destroy(instance);
                    totalDestroyed++;
                }
            }
        }

        _registries.Clear();
        _parents.Clear();
        Log.System(Localization.Log_Pool_Cleared, totalDestroyed);
    }
}
