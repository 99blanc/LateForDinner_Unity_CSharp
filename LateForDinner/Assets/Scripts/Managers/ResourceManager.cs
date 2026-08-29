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
    {
        _handles.Clear();
        await Addressables.InitializeAsync().ToUniTask();
    }

    private async UniTask<T> LoadAsync<T>(string path) where T : Object
    {
        CleanupInvalidHandleIfNeeded(path);

        if (_handles.TryGetValue(path, out var handle))
        {
            if (IsHandleNotDone(handle))
                await handle.ToUniTask();

            return handle.Result as T;
        }

        return await LoadAndTrackAssetAsync<T>(path);
    }

    private void CleanupInvalidHandleIfNeeded(string path)
    {
        if (_handles.TryGetValue(path, out var handle) && IsHandleInvalid(handle))
            _handles.Remove(path);
    }

    private async UniTask<T> LoadAndTrackAssetAsync<T>(string path) where T : Object
    {
        try
        {
            AsyncOperationHandle<T> asyncHandle = Addressables.LoadAssetAsync<T>(path);
            _handles[path] = asyncHandle;
            T asset = await asyncHandle.ToUniTask();

            if (IsAssetNotNull(asset))
                return asset;

            RemoveHandle(path);
            Log.Error(LocalizationKey.Log_Resource_LoadFailed_Null, path);
            return null;
        }
        catch
        {
            RemoveHandle(path);
            Log.Error(LocalizationKey.Log_Resource_LoadFailed_Exception, path);
            return null;
        }
    }

    public async UniTask<T> LoadAssetAsync<T>(string path) where T : Object
        => await LoadAsync<T>(path);

    public async UniTask<Sprite> LoadSpriteAsync(string path)
        => await LoadAsync<Sprite>(path);

    public async UniTask<Sprite> LoadSpriteAsync(string atlas, string sprite)
    {
        SpriteAtlas sprites = await LoadAssetAsync<SpriteAtlas>(atlas);
        return IsSpriteAtlasNull(sprites) ? null : sprites.GetSprite(sprite);
    }

    public async UniTask<GameObject> LoadPrefabAsync(string path)
        => await LoadAsync<GameObject>(path);

    public async UniTask<TextAsset> LoadTextAssetAsync(string path)
        => await LoadAsync<TextAsset>(path);

    public async UniTask<RuntimeAnimatorController> LoadAnimatorControllerAsync(string path)
        => await LoadAssetAsync<RuntimeAnimatorController>(path);

    public async UniTask<AnimatorOverrideController> LoadAnimatorOverrideControllerAsync(string path)
    {
        RuntimeAnimatorController controller = await LoadAssetAsync<RuntimeAnimatorController>(path);
        return controller is AnimatorOverrideController overrideController ? overrideController : null;
    }

    public T Get<T>(string path) where T : Object
    {
        if (_handles.TryGetValue(path, out var handle) && IsHandleValidAndDone(handle))
            return handle.Result as T;

        return null;
    }

    public T GetAsset<T>(string path) where T : Object
        => Get<T>(path);

    public Sprite GetSprite(string atlas, string sprite)
        => Get<SpriteAtlas>(atlas)?.GetSprite(sprite);

    public GameObject GetPrefab(string path)
        => Get<GameObject>(path);

    public TextAsset GetTextAsset(string path)
        => Get<TextAsset>(path);

    public RuntimeAnimatorController GetAnimatorController(string path)
        => Get<RuntimeAnimatorController>(path);

    public AnimatorOverrideController GetAnimatorOverrideController(string path)
        => Get<AnimatorOverrideController>(path);

    public Texture2D GetTextureFromSprite(Sprite sprite)
    {
        if (IsSpriteNull(sprite))
            return null;

        var rect = sprite.textureRect;
        var original = sprite.texture;
        Color[] pixels = original.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
        Texture2D result = new Texture2D((int)rect.width, (int)rect.height, original.format, false);
        result.SetPixels(pixels);
        result.Apply();
        return result;
    }

    public async UniTask<GameObject> InstantiateAsync(string path, Transform parent = null, bool hasWorldPosition = false)
    {
        GameObject prefab = await LoadPrefabAsync(path);
        return Instantiate(prefab, parent, hasWorldPosition);
    }

    public GameObject Instantiate(GameObject prefab, Transform parent = null, bool hasWorldPosition = false)
    {
        if (IsPrefabNull(prefab))
            return null;

        return Object.Instantiate(prefab, parent, hasWorldPosition);
    }

    public GameObject Instantiate(string path, Transform parent = null, bool hasWorldPosition = false)
    {
        var prefab = Get<GameObject>(path);
        return Instantiate(prefab, parent, hasWorldPosition);
    }

    public void Unload(string path)
    {
        if (!_handles.TryGetValue(path, out var handle))
            return;

        ReleaseHandleIfValid(handle);
        _handles.Remove(path);
    }

    public void UnloadAll()
    {
        foreach (var handle in _handles.Values)
            ReleaseHandleIfValid(handle);

        _handles.Clear();
    }

    private bool IsHandleNotDone(AsyncOperationHandle handle)
        => !handle.IsDone;

    private bool IsHandleInvalid(AsyncOperationHandle handle)
        => !handle.IsValid();

    private bool IsAssetNotNull(Object asset)
        => asset != null;

    private void RemoveHandle(string path)
        => _handles.Remove(path);

    private bool IsSpriteAtlasNull(SpriteAtlas sprites)
        => sprites == null;

    private bool IsHandleValidAndDone(AsyncOperationHandle handle)
        => handle.IsValid() && handle.IsDone;

    private bool IsSpriteNull(Sprite sprite)
        => sprite == null;

    private bool IsPrefabNull(GameObject prefab)
        => prefab == null;

    private void ReleaseHandleIfValid(AsyncOperationHandle handle)
    {
        if (handle.IsValid())
            Addressables.Release(handle);
    }
}
