using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

public class ResourceManager
{
    private readonly Dictionary<string, AsyncOperationHandle> _handles = new Dictionary<string, AsyncOperationHandle>();

    public async UniTask InitAsync()
        => await UnloadAll();

    private async UniTask<T> LoadAsync<T>(string path) where T : Object
    {
        if (_handles.TryGetValue(path, out var handle))
        {
            if (handle.IsValid())
            {
                await handle.ToUniTask();
                return handle.Result as T;
            }
            else
                _handles.Remove(path);
        }

        try
        {
            AsyncOperationHandle<T> asyncHandle = Addressables.LoadAssetAsync<T>(path);
            _handles[path] = asyncHandle;
            T asset = await asyncHandle.ToUniTask();
            bool isFailed = asset == null;
            Log.Error(Localization.Log_Resource_LoadFailed, isFailed, path);

            if (isFailed)
            {
                Addressables.Release(asyncHandle);
                _handles.Remove(path);
            }

            return asset;
        }
        catch
        {
            Log.Error(Localization.Log_Resource_LoadException, true, path);
            _handles.Remove(path);

            return null;
        }
    }

    public async UniTask<T> LoadAssetAsync<T>(string path) where T : Object
        => await LoadAsync<T>(path);

    public async UniTask<SpriteAtlas> LoadAtlasAsync(string path) 
        => await LoadAsync<SpriteAtlas>(path);

    public async UniTask<Sprite> LoadSpriteAsync(string path) 
        => await LoadAsync<Sprite>(path);

    public async UniTask<Sprite> LoadSpriteAsync(string atlas, string sprite)
    {
        SpriteAtlas sprites = await LoadAtlasAsync(atlas);
        bool isNotFound = sprites == null || sprites.GetSprite(sprite) == null;
        Log.Error(Localization.Log_Resource_SpriteNotFound, isNotFound, atlas, sprite);

        return isNotFound ? null : sprites.GetSprite(sprite);
    }

    public async UniTask<GameObject> LoadPrefabAsync(string path) 
        => await LoadAsync<GameObject>(path);

    public async UniTask<TextAsset> LoadTextAssetAsync(string path) 
        => await LoadAsync<TextAsset>(path);

    public async UniTask Unload(string path)
    {
        if (_handles.TryGetValue(path, out var handle))
        {
            if (handle.IsValid())
                Addressables.Release(handle);

            _handles.Remove(path);
            Log.System(Localization.Log_Resource_UnloadedSingle, true, path);
        }

        await UniTask.CompletedTask;
    }

    public async UniTask UnloadAll()
    {
        foreach (var handle in _handles.Values)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }
        _handles.Clear();

        await UniTask.CompletedTask;
    }

}
