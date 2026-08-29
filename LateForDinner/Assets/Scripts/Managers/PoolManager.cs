using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager
{
    private GameObject _root;
    public GameObject Root 
        => _root ??= InitRoot();
    private readonly Dictionary<string, Queue<GameObject>> _registries = new Dictionary<string, Queue<GameObject>>();
    private readonly Dictionary<string, Transform> _folders = new Dictionary<string, Transform>();
    private readonly Dictionary<GameObject, Transform> _parents = new Dictionary<GameObject, Transform>();
    private readonly Dictionary<string, string> _maps = new Dictionary<string, string>()
    {
        { Literal.Keys.UI, Literal.Roots.UserInterfaces },
    };

    private GameObject InitRoot()
    {
        _root = new GameObject { name = Literal.Roots.Pools };
        _root.transform.SetParent(Managers.Instance.transform, false);
        SetupFolders();
        return _root;
    }

    private void SetupFolders()
    {
        foreach (var folderName in _maps.Values)
        {
            if (HasFolder(folderName))
                continue;

            Transform folder = new GameObject { name = folderName }.transform;
            folder.SetParent(Root.transform, false);
            _folders.Add(folderName, folder);
        }
    }

    public async UniTask<(GameObject instance, IDisposable rentHandle)> PopAsync(string key, Transform parent = null)
    {
        var (instance, isNew) = await GetOrCreateInstanceAsync(key, parent);

        if (IsInstanceNull(instance))
            return (null, null);

        InitializePoolable(instance, isNew);
        IDisposable rentHandle = Disposable.Create(() => Push(instance, key));
        return (instance, rentHandle);
    }

    public async UniTask<(T component, IDisposable rentHandle)> PopAsync<T>(Transform parent = null) where T : Component
    {
        string key = typeof(T).Name;
        var (instance, rentHandle) = await PopAsync(key, parent);

        if (IsInstanceNull(instance))
            return (null, null);

        return (instance.GetComponentAssert<T>(), rentHandle);
    }

    public (GameObject instance, IDisposable rentHandle) Pop(string key, Transform parent = null)
    {
        var (instance, isNew) = GetOrCreateInstance(key, parent);

        if (IsInstanceNull(instance))
            return (null, null);

        InitializePoolable(instance, isNew);
        IDisposable rentHandle = Disposable.Create(() => Push(instance, key));
        return (instance, rentHandle);
    }

    public (T component, IDisposable rentHandle) Pop<T>(Transform parent = null) where T : Component
    {
        string key = typeof(T).Name;
        var (instance, rentHandle) = Pop(key, parent);

        if (IsInstanceNull(instance))
            return (null, null);

        return (instance.GetComponentAssert<T>(), rentHandle);
    }

    public void Push(GameObject gameObject, string key = null)
    {
        if (IsGameObjectNull(gameObject))
            return;

        if (gameObject.TryGetComponent<IPoolable>(out var poolable))
            poolable.ProtectedRelease();

        _parents[gameObject] = gameObject.transform.parent;
        string newKey = string.IsNullOrEmpty(key) ? gameObject.name : key;

        if (!HasRegistry(newKey))
            _registries.Add(newKey, new Queue<GameObject>());

        gameObject.SetActive(false);
        gameObject.transform.SetParent(GetFolder(newKey), false);
        _registries[newKey].Enqueue(gameObject);
    }

    public void Push(Component component, string key = null)
    {
        if (IsComponentNotNull(component))
            Push(component.gameObject, key);
    }

    public async UniTask PrewarmAsync<T>(int count, Transform parent = null) where T : Component
    {
        string key = typeof(T).Name;

        if (HasEnoughCachedInstances(key, count))
            return;

        int needed = GetNeededPrewarmCount(key, count);

        for (int index = 0; index < needed; index++)
        {
            Transform targetParent = parent == null ? GetFolder(key) : parent;
            var instance = await Managers.Resource.InstantiateAsync(key, targetParent, false);

            if (IsInstanceNull(instance))
                continue;

            instance.name = key;
            _parents[instance] = targetParent;

            if (instance.TryGetComponent<IPoolable>(out var poolable))
                poolable.ProtectedInit();

            if (!HasRegistry(key))
                _registries.Add(key, new Queue<GameObject>());

            instance.SetActive(false);
            instance.transform.SetParent(GetFolder(key), false);
            _registries[key].Enqueue(instance);
        }
    }

    private async UniTask<(GameObject instance, bool isNew)> GetOrCreateInstanceAsync(string key, Transform parent)
    {
        if (TryGetCachedInstance(key, out var cachedInstance))
        {
            PrepareCachedInstance(cachedInstance, parent, key);
            return (cachedInstance, false);
        }

        Transform newParent = parent == null ? GetFolder(key) : parent;
        var newInstance = await Managers.Resource.InstantiateAsync(key, newParent, false);

        if (IsInstanceNull(newInstance))
        {
            Log.Error(LocalizationKey.Log_Pool_InstantiateFailed, key);
            return (null, false);
        }

        newInstance.name = key;
        _parents[newInstance] = newParent;
        return (newInstance, true);
    }

    private (GameObject instance, bool isNew) GetOrCreateInstance(string key, Transform parent)
    {
        if (TryGetCachedInstance(key, out var cachedInstance))
        {
            PrepareCachedInstance(cachedInstance, parent, key);
            return (cachedInstance, false);
        }

        Transform newParent = parent == null ? GetFolder(key) : parent;
        var newInstance = Managers.Resource.Instantiate(key, newParent, false);

        if (IsInstanceNull(newInstance))
        {
            Log.Error(LocalizationKey.Log_Pool_InstantiateFailed, key);
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
            instance.OnDestroyAsObservable()
            .Subscribe(_ => poolable.ProtectedRelease())
            .RegisterTo(instance.GetCancellationTokenOnDestroy());
            poolable.ProtectedInit();
            return;
        }

        poolable.ProtectedGet();
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

                if (IsInstanceNotNull(instance))
                {
                    UnityEngine.Object.Destroy(instance);
                    totalDestroyed++;
                }
            }
        }

        _registries.Clear();
        _parents.Clear();
        Log.System(LocalizationKey.Log_Pool_Cleared, totalDestroyed);
    }

    private bool HasFolder(string folderName)
        => _folders.ContainsKey(folderName);

    private bool IsInstanceNull(GameObject instance)
        => instance == null;

    private bool IsInstanceNotNull(GameObject instance)
        => instance != null;

    private bool IsGameObjectNull(GameObject gameObject)
        => gameObject == null;

    private bool IsComponentNotNull(Component component)
        => component != null;

    private bool HasRegistry(string key)
        => _registries.ContainsKey(key);

    private bool HasEnoughCachedInstances(string key, int count)
        => _registries.TryGetValue(key, out var queue) && queue.Count >= count;

    private int GetNeededPrewarmCount(string key, int count)
        => count - (_registries.TryGetValue(key, out var queue) && queue != null ? queue.Count : 0);

    private bool TryGetCachedInstance(string key, out GameObject cachedInstance)
    {
        cachedInstance = null;

        if (_registries.TryGetValue(key, out var queue) && queue.Count > 0)
        {
            cachedInstance = queue.Dequeue();
            return true;
        }

        return false;
    }
}
