using Cysharp.Text;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

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

    public async UniTask<SpriteAtlas> LoadAtlas(string path) => await Load<SpriteAtlas>(ZString.Concat(Define.Path.ATLAS, path));
    public async UniTask<Sprite> LoadSprite(string path) => await Load<Sprite>(ZString.Concat(Define.Path.SPRITE, path));
    public async UniTask<GameObject> LoadPrefab(string path) => await Load<GameObject>(ZString.Concat(Define.Path.PREFAB, path));
    public async UniTask<InputActionAsset> LoadSystem(string path) => await Load<InputActionAsset>(ZString.Concat(Define.Path.SYSTEM, path));
    public async UniTask<TextAsset> LoadTextAsset(string path) => await Load<TextAsset>(ZString.Concat(Define.Path.TABLE, path));
    public async UniTask<RuntimeAnimatorController> LoadAnimator(string path) => await Load<RuntimeAnimatorController>(ZString.Concat(Define.Path.ANIMATOR, path));

    public async UniTask<Sprite> GetSpriteInAtlas(string atlas, string sprite)
    {
        SpriteAtlas take = await LoadAtlas(atlas);

        if (take is null) 
            return null;

        return take.GetSprite(sprite);
    }

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
