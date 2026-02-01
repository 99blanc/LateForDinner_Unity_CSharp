using Cysharp.Text;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.InputSystem;

public class ResourceManager
{
    public Dictionary<string, AsyncOperationHandle> handles = new();

    public void Init() => handles.Clear();

    private async UniTask<T> Load<T>(string path) where T : Object
    {
        if (handles.TryGetValue(path, out var handle))
        {
            if (handle.IsDone)
                return handle.Convert<T>().Result;

            await handle.ToUniTask();
            return handle.Convert<T>().Result;
        }

        var asyncHandle = Addressables.LoadAssetAsync<T>(path);
        handles[path] = asyncHandle;
        return await asyncHandle.ToUniTask();
    }

    public async UniTask<Image> LoadSprite(string path) => await Load<Image>(ZString.Concat(Define.Path.SPRITE, path));
    public async UniTask<GameObject> LoadPrefab(string path) => await Load<GameObject>(ZString.Concat(Define.Path.PREFAB, path));
    public async UniTask<InputActionAsset> LoadInputSystem(string path) => await Load<InputActionAsset>(ZString.Concat(Define.Path.SYSTEM, path));
    public async UniTask<TextAsset> LoadTextAsset(string path)  => await Load<TextAsset>(ZString.Concat(Define.Path.TABLE, path));


    public async UniTask<GameObject> Instantiate(string path, Transform parent = null)
    {
        GameObject prefab = await LoadPrefab(path);
        Debug.Assert(prefab);
        return Instantiate(prefab, parent);
    }

    public GameObject Instantiate(GameObject prefab, Transform parent = null)
    {
        GameObject gameObject = Object.Instantiate(prefab, parent);
        gameObject.name = prefab.name;
        return gameObject;
    }

    public void Unload(string path)
    {
        if (handles.TryGetValue(path, out var handle))
        {
            Addressables.Release(handle);
            handles.Remove(path);
        }
    }

    public void Destroy(GameObject gameObject)
    {
        if (gameObject)
            Object.Destroy(gameObject);
    }
}
