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

    private readonly Dictionary<string, string> _maps = new Dictionary<string, string>()
    {
        { Literal.Keys.UI, Literal.Roots.UserInterfaces },
    };

    [Serializable]
    private struct Debug
    {
        public string key;
        public int count;

        public Debug(string key, int count)
        {
            this.key = key;
            this.count = count;
        }
    }

    [SerializeField]
    private List<Debug> _debugs = new List<Debug>();

    private void SyncDebug()
    {
        _debugs.Clear();

        foreach (var pair in _registries)
            _debugs.Add(new Debug(pair.Key, pair.Value.Count));
    }

    public async UniTask InitAsync()
    {
        // TODO ::: UI 프리팹이나 기본 이펙트 등 리소스 메모리 적재
        Setup();

        await UniTask.CompletedTask;
    }

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

        if (_registries.TryGetValue(key, out var queue) && queue.Count > 0)
            instance = queue.Dequeue();

        if (instance == null)
        {
            Transform newParent = parent == null ? Get(key) : parent;
            instance = await Managers.Resource.InstantiateAsync(key, newParent, false);

            if (instance == null)
                return (null, null);

            instance.name = key;
        }
        else
        {
            instance.transform.SetParent(parent, false);
            instance.SetActive(true);
        }

        SyncDebug();
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

    public void Push(GameObject gameObject, string key = null)
    {
        if (gameObject == null) 
            return;

        string newKey = string.IsNullOrEmpty(key) ? gameObject.name : key;

        if (!_registries.ContainsKey(newKey))
            _registries.Add(newKey, new Queue<GameObject>());

        gameObject.SetActive(false); 
        gameObject.transform.SetParent(Get(newKey), false);
        _registries[newKey].Enqueue(gameObject);
        SyncDebug();
    }

    public void Push(Component component, string key = null)
    {
        if (component != null) 
            Push(component.gameObject, key);
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
        SyncDebug();
    }
}
