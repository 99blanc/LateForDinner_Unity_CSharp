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
            {
                _root = new GameObject { name = Literal.Roots.UserInterfaces };
                _root.transform.SetParent(Managers.Instance.transform, false);
                CreateLayer(Layer.Background, 0);
                CreateLayer(Layer.Screen, 100);
                CreateLayer(Layer.Popup, 200);
                CreateLayer(Layer.System, 300);
            }

            return _root;
        }
    }
    public float ScaleFactor
    {
        get
        {
            var _ = Root;

            return _canvas != null ? _canvas.scaleFactor : 1f;
        }
    }
    private readonly Dictionary<Layer, Transform> _layer = new Dictionary<Layer, Transform>();
    private readonly List<UIPopup> _popups = new List<UIPopup>();
    private readonly Dictionary<UserInterface, IDisposable> _handles = new Dictionary<UserInterface, IDisposable>();
    private UIScreen _screen;

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
        scaler.referenceResolution = new Vector2(3840f, 2160f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Shrink;
        gameObject.AddComponent<GraphicRaycaster>();

        return gameObject;
    }

    private void CreateLayer(Layer layer, int sortingOrder)
    {
        string name = ZString.Concat(Literal.Roots.Layer, $"{layer}");
        var gameObject = CreateCanvas(name, _root.transform, sortingOrder, out var canvas);

        if (layer == Layer.Screen)
            _canvas = canvas;

        _layer[layer] = gameObject.transform;
    }

    public async UniTask<T> OpenScreenAsync<T>() where T : UIScreen
    {
        if (_screen is T existingScreen)
            return existingScreen;

        if (_screen != null)
            Close(_screen);

        var (instance, rentHandle) = await Managers.Pool.PopAsync<T>(_layer[Layer.Screen]);

        if (instance == null)
            return null;

        instance.Init();
        _screen = instance;
        _handles[instance] = rentHandle;

        return instance;
    }

    public async UniTask<T> OpenPopupAsync<T>() where T : UIPopup
    {
        foreach (var popup in _popups)
        {
            if (popup is T)
                return popup as T;
        }

        var (instance, rentHandle) = await Managers.Pool.PopAsync<T>(_layer[Layer.Popup]);

        if (instance == null)
            return null;

        instance.Init();
        _popups.Add(instance);
        _handles[instance] = rentHandle;
        Refresh();

        return instance;
    }

    public async UniTask<T> OpenSystemAsync<T>() where T : UISystem
    {
        var (instance, rentHandle) = await Managers.Pool.PopAsync<T>(_layer[Layer.System]);

        if (instance == null)
            return null;

        instance.Init();
        _handles[instance] = rentHandle;

        return instance;
    }

    public void Close(UserInterface ui)
    {
        if (ui == null || !_handles.ContainsKey(ui))
            return;

        if (_screen == ui)
            _screen = null;

        if (ui is UIPopup popup)
        {
            _popups.Remove(popup);
            Refresh();
        }

        _handles[ui].Dispose();
        _handles.Remove(ui);
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
        _screen = null;
        Refresh();
    }

    public bool CloseTop()
    {
        if (_popups.Count > 0)
        {
            var topPopup = _popups[_popups.Count - 1];
            Close(topPopup);

            return true;
        }

        return false;
    }

    public void Focus(UIPopup popup)
    {
        if (popup == null || !_popups.Contains(popup))
            return;

        _popups.Remove(popup);
        _popups.Add(popup);
        Refresh();
    }

    private void Refresh()
    {
        int siblingIndex = 0;

        for (int index = 0; index < _popups.Count; index++)
            _popups[index].transform.SetSiblingIndex(siblingIndex++);
    }
}
