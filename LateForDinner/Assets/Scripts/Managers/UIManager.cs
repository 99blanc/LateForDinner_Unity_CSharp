using Cysharp.Text;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager
{
    private Canvas _canvas;
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

    public float ScaleFactor
        => _canvas != null ? _canvas.scaleFactor : 1f;

    private readonly Dictionary<Layer, Transform> _layer = new Dictionary<Layer, Transform>();
    private readonly List<UIPopup> _popups = new List<UIPopup>();
    private readonly Dictionary<UserInterface, IDisposable> _handles = new Dictionary<UserInterface, IDisposable>();
    private UIDisplay _display;

    private void InitRoot()
    {
        _root = new GameObject { name = Literal.Roots.UserInterfaces };
        _root.transform.SetParent(Managers.Instance.transform, false);
        CreateLayer(Layer.Display);
        CreateLayer(Layer.Popup);
        CreateLayer(Layer.System);
        CreateLayer(Layer.Lock);
        Log.System(Localization.Log_UI_RootInitialized);
    }

    private GameObject CreateCanvas(string name, Transform parent, int sortingOrder, out Canvas canvasOut)
    {
        var gameObject = new GameObject { name = name };
        gameObject.transform.SetParent(parent, false);
        canvasOut = gameObject.AddComponent<Canvas>();
        canvasOut.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasOut.overrideSorting = true;
        canvasOut.sortingOrder = sortingOrder;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = Define.Scaler.Resolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Shrink;
        scaler.referencePixelsPerUnit = Define.Scaler.PixelsPerUnit;
        gameObject.AddComponent<GraphicRaycaster>();
        return gameObject;
    }

    private void CreateLayer(Layer layer)
    {
        string name = ZString.Concat(Literal.Roots.Layers, $"{layer}");
        var gameObject = CreateCanvas(name, _root.transform, (int)layer, out var canvas);

        if (layer == Layer.Display)
            _canvas = canvas;

        _layer[layer] = gameObject.transform;
    }

    public void Setup()
    {
        var system = Managers.Resource.Instantiate(Literal.Assets.EventSystem, Managers.Instance.transform, false);
        system.name = Literal.Roots.Events;
    }

    public async UniTask<T> OpenDisplayAsync<T>() where T : UIDisplay
    {
        if (_display is T existingDisplay)
            return existingDisplay;

        if (_display != null)
            Close(_display);

        var _ = Root;
        var (instance, rentHandle) = await Managers.Pool.PopAsync<T>(_layer[Layer.Display]);

        if (instance == null)
        {
            Log.Error(Localization.Log_UI_OpenDisplayFailed, typeof(T).Name);
            return null;
        }

        _display = instance;
        _handles[instance] = rentHandle;
        return instance;
    }

    public T OpenDisplay<T>() where T : UIDisplay
    {
        if (_display is T existingScreen)
            return existingScreen;

        if (_display != null)
            Close(_display);

        var _ = Root;
        var (instance, rentHandle) = Managers.Pool.Pop<T>(_layer[Layer.Display]);

        if (instance == null)
        {
            Log.Error(Localization.Log_UI_OpenDisplayFailed, typeof(T).Name);
            return null;
        }

        _display = instance;
        _handles[instance] = rentHandle;
        return instance;
    }

    public async UniTask<T> OpenPopupAsync<T>() where T : UIPopup
    {
        foreach (var popup in _popups)
        {
            if (popup is T targetPopup)
                return targetPopup;
        }

        var _ = Root;
        var (instance, rentHandle) = await Managers.Pool.PopAsync<T>(_layer[Layer.Popup]);

        if (instance == null)
        {
            Log.Error(Localization.Log_UI_OpenPopupFailed, typeof(T).Name);
            return null;
        }

        _popups.Add(instance);
        _handles[instance] = rentHandle;
        RefreshPopup();
        return instance;
    }

    public T OpenPopup<T>() where T : UIPopup
    {
        foreach (var popup in _popups)
        {
            if (popup is T targetPopup)
                return targetPopup;
        }

        var _ = Root;
        var (instance, rentHandle) = Managers.Pool.Pop<T>(_layer[Layer.Popup]);

        if (instance == null)
        {
            Log.Error(Localization.Log_UI_OpenPopupFailed, typeof(T).Name);
            return null;
        }

        _popups.Add(instance);
        _handles[instance] = rentHandle;
        RefreshPopup();
        return instance;
    }

    public async UniTask<T> OpenSystemAsync<T>(Layer layer = Layer.System) where T : UISystem
    {
        var existingSystem = GetSystem<T>();

        if (existingSystem != null)
            return existingSystem;

        var _ = Root;
        var (instance, rentHandle) = await Managers.Pool.PopAsync<T>(_layer[layer]);

        if (instance == null)
        {
            Log.Error(Localization.Log_UI_OpenSystemFailed, typeof(T).Name);
            return null;
        }

        _handles[instance] = rentHandle;
        return instance;
    }

    public T OpenSystem<T>(Layer layer = Layer.System) where T : UISystem
    {
        var existingSystem = GetSystem<T>();

        if (existingSystem != null)
            return existingSystem;

        var _ = Root;
        var (instance, rentHandle) = Managers.Pool.Pop<T>(_layer[layer]);

        if (instance == null)
        {
            Log.Error(Localization.Log_UI_OpenSystemFailed, typeof(T).Name);
            return null;
        }

        _handles[instance] = rentHandle;
        return instance;
    }

    public T GetScreen<T>() where T : UIDisplay
    => _display as T;

    public T GetSystem<T>() where T : UISystem
    {
        foreach (var pair in _handles)
        {
            if (pair.Key is T targetSystem)
                return targetSystem;
        }

        return null;
    }

    public void Close(UserInterface ui)
    {
        if (ui == null || !_handles.ContainsKey(ui))
            return;

        if (_display == ui)
            _display = null;

        if (ui is UIPopup popup)
        {
            _popups.Remove(popup);
            RefreshPopup();
        }

        if (_handles.TryGetValue(ui, out var handle))
        {
            _handles.Remove(ui);
            handle?.Dispose();
        }
    }

    public void Close<T>() where T : UserInterface
    {
        foreach (var pair in _handles)
        {
            if (pair.Key is T targetUI)
            {
                Close(targetUI);
                break;
            }
        }
    }

    public void CloseAll()
    {
        foreach (var handle in _handles.Values)
            handle?.Dispose();

        _handles.Clear();
        _popups.Clear();
        _display = null;
        RefreshPopup();
    }

    public bool CloseFocusPopup()
    {
        if (_popups.Count <= 0)
            return false;

        var topPopup = _popups[_popups.Count - 1];
        Close(topPopup);
        return true;
    }

    public void FocusPopup(UIPopup popup)
    {
        if (popup == null || !_popups.Contains(popup))
            return;

        _popups.Remove(popup);
        _popups.Add(popup);
        RefreshPopup();
    }

    private void RefreshPopup()
    {
        int siblingIndex = 0;

        for (int index = 0; index < _popups.Count; index++)
            _popups[index].transform.SetSiblingIndex(siblingIndex++);
    }

    public void RefreshAll()
    {
        foreach (var ui in _handles.Keys)
        {
            if (ui != null)
                ui.Refresh();
        }
    }
}
