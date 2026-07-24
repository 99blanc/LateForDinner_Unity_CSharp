using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    private GameObject _root;
    public GameObject Root
    {
        get
        {
            if (_root == null)
            {
                _root = new GameObject { name = Literal.Roots.UserInterfaces };
                _root.transform.SetParent(Managers.Instance.transform, false);
            }

            return _root;
        }
    }
    private const int _baseOrder = 100;
    private readonly List<UIPopup> _popups = new List<UIPopup>();
    private readonly Dictionary<UIPopup, IDisposable> _handles = new Dictionary<UIPopup, IDisposable>();
    private UIScene _scene;
    private IDisposable _handle;

    public async UniTask InitAsync()
    {
        CloseAll();

        await UniTask.CompletedTask;
        Log.System(Localization.Log_UI_InitComplete);
    }

    public async UniTask<T> OpenSceneAsync<T>() where T : UIScene
    {
        if (_scene is T existingScene)
            return existingScene as T;

        if (_scene != null)
            CloseScene(_scene);

        var (scene, rentHandle) = await Managers.Pool.PopAsync<T>();

        if (scene == null)
        {
            Log.Error(Localization.Log_UI_OpenFailed, true, typeof(T).Name);
            return null;
        }

        scene.transform.SetParent(Root.transform, false);
        _scene = scene;
        _handle = rentHandle;
        Refresh();
        Log.System(Localization.Log_UI_Opened, true, typeof(T).Name, 1);

        return scene;
    }

    public void CloseScene(UIScene scene)
    {
        if (scene == null || _scene != scene)
            return;

        _handle?.Dispose();
        _handle = null;
        _scene = null;
        Refresh();
        Log.System(Localization.Log_UI_Closed, true, _popups.Count);
    }

    public async UniTask<T> OpenPopupAsync<T>() where T : UIPopup
    {
        foreach (var popup in _popups)
        {
            if (popup is T)
            {
                Log.Warning(Localization.Log_UI_AlreadyOpened, true, typeof(T).Name);
                return popup as T;
            }
        }

        var (instance, rentHandle) = await Managers.Pool.PopAsync<T>();

        if (instance == null) 
            return null;

        instance.transform.SetParent(Root.transform, false);
        _popups.Add(instance);
        _handles.Add(instance, rentHandle);
        Refresh();
        Log.System(Localization.Log_UI_Opened, true, typeof(T).Name, _popups.Count);

        return instance;
    }

    public bool ClosePopup()
    {
        if (_popups.Count == 0)
            return false;

        ClosePopup(_popups[_popups.Count - 1]);
        return true;
    }

    public void ClosePopup(UIPopup popup)
    {
        if (popup == null || !_handles.ContainsKey(popup)) 
            return;

        _popups.Remove(popup);
        _handles[popup].Dispose();
        _handles.Remove(popup);
        Refresh();
        Log.System(Localization.Log_UI_Closed, true, _popups.Count);
    }

    public void CloseAll()
    {
        for (int index = _popups.Count - 1; index >= 0; index--)
            _handles[_popups[index]].Dispose();

        _popups.Clear();
        _handles.Clear();

        if (_scene != null)
        {
            _handle?.Dispose();
            _handle = null;
            _scene = null;
        }

        Log.System(Localization.Log_UI_ClosedAll);
    }

    public void Focus(UIPopup popup)
    {
        if (popup == null || !_popups.Contains(popup)) 
            return;

        _popups.Remove(popup);
        _popups.Add(popup);
        Refresh();
        Log.System(Localization.Log_UI_Focused);
    }

    private void Refresh()
    {
        if (_scene != null)
        {
            Canvas canvas = _scene.Canvas;

            if (canvas != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.overrideSorting = true;
                canvas.sortingOrder = _baseOrder - 50;
            }
        }

        for (int index = 0; index < _popups.Count; index++)
        {
            Canvas canvas = _popups[index].Canvas;

            if (canvas != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.overrideSorting = true;
                canvas.sortingOrder = _baseOrder + index;
                _popups[index].transform.SetSiblingIndex(index);
            }
        }
    }
}
