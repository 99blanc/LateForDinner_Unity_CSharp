using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

public class ResourceManager
{
    private readonly Dictionary<string, AsyncOperationHandle> _handles = new Dictionary<string, AsyncOperationHandle>();
    private readonly Dictionary<string, SpriteAtlas> _atlases = new Dictionary<string, SpriteAtlas>();

    public async UniTask InitAsync()
    {
        _handles.Clear();

        await Addressables.InitializeAsync().ToUniTask();
        await LoadAtlasAsync(Define.Atlas.UI_Common);
    }

    private async UniTask<T> LoadAsync<T>(string path) where T : Object
    {
        if (_handles.TryGetValue(path, out var handle) && !handle.IsValid())
            _handles.Remove(path);

        if (_handles.TryGetValue(path, out handle))
        {
            if (!handle.IsDone)
                await handle.ToUniTask();

            return handle.Result as T;
        }

        try
        {
            AsyncOperationHandle<T> asyncHandle = Addressables.LoadAssetAsync<T>(path);
            _handles[path] = asyncHandle;
            T asset = await asyncHandle.ToUniTask();

            if (asset != null)
                return asset;

            _handles.Remove(path);

            return null;
        }
        catch
        {
            _handles.Remove(path);

            if (_handles.ContainsKey(path))
                _handles.Remove(path);

            return null;
        }
    }

    public async UniTask<T> LoadAssetAsync<T>(string path) where T : Object
        => await LoadAsync<T>(path);

    public async UniTask<SpriteAtlas> LoadAtlasAsync(string path)
    {
        if (_atlases.TryGetValue(path, out var atlas) && atlas != null)
            return atlas;

        atlas = await LoadAsync<SpriteAtlas>(path);

        if (atlas != null)
            _atlases[path] = atlas;

        return atlas;
    }

    public async UniTask<Sprite> LoadSpriteAsync(string path)
        => await LoadAsync<Sprite>(path);

    public async UniTask<Sprite> LoadSpriteAsync(string atlas, string sprite)
    {
        SpriteAtlas sprites = await LoadAtlasAsync(atlas);

        return sprites == null || sprites.GetSprite(sprite) == null ? null : sprites.GetSprite(sprite);
    }

    public async UniTask<GameObject> LoadPrefabAsync(string path)
        => await LoadAsync<GameObject>(path);

    public async UniTask<TextAsset> LoadTextAssetAsync(string path)
        => await LoadAsync<TextAsset>(path);

    public async UniTask<GameObject> InstantiateAsync(string path, Transform parent = null, bool hasWorldPosition = false)
    {
        GameObject prefab = await LoadPrefabAsync(path);

        if (prefab == null)
            return null;

        return Object.Instantiate(prefab, parent, hasWorldPosition);
    }

    public GameObject InstantiateAsync(GameObject prefab, Transform parent = null, bool hasWorldPosition = false)
    {
        if (prefab == null)
            return null;

        return Object.Instantiate(prefab, parent, hasWorldPosition);
    }

    public Sprite GetSpriteFromAtlas(string atlas, string sprite)
    {
        if (_atlases.TryGetValue(atlas, out var output) && output != null)
            return output.GetSprite(sprite);

        return null;
    }

    public Texture2D GetTextureFromSprite(Sprite sprite)
    {
        if (sprite == null) 
            return null;

        var rect = sprite.textureRect;
        var original = sprite.texture;
        Color[] pixels = original.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
        Texture2D result = new Texture2D((int)rect.width, (int)rect.height, original.format, false);
        result.SetPixels(pixels);
        result.Apply();

        return result;
    }

    public void Unload(string path)
    {
        if (_handles.TryGetValue(path, out var handle))
        {
            if (handle.IsValid())
                Addressables.Release(handle);

            _handles.Remove(path);
        }
    }

    public void UnloadAll()
    {
        foreach (var handle in _handles.Values)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }
        _handles.Clear();
    }
}
