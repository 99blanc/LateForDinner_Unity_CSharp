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
        => _root ??= InitRoot();
    public float ScaleFactor
        => _canvas != null ? _canvas.scaleFactor : 1f;
    private readonly Dictionary<LayerType, Transform> _layer = new Dictionary<LayerType, Transform>();
    private readonly List<UIPopup> _popups = new List<UIPopup>();
    private readonly Dictionary<UserInterface, IDisposable> _handles = new Dictionary<UserInterface, IDisposable>();
    private UIDisplay _display;

    private GameObject InitRoot()
    {
        _root = new GameObject { name = Literal.Roots.UserInterfaces };
        _root.transform.SetParent(Managers.Instance.transform, false);
        CreateLayer(LayerType.Display);
        CreateLayer(LayerType.Popup);
        CreateLayer(LayerType.System);
        CreateLayer(LayerType.Lock);
        Log.System(LocalizationKey.Log_UI_RootInitialized);
        return _root;
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

    private void CreateLayer(LayerType layer)
    {
        string name = ZString.Concat(Literal.Roots.Layers, $"{layer}");
        var gameObject = CreateCanvas(name, _root.transform, (int)layer, out var canvas);

        if (layer == LayerType.Display)
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
        if (HasExistingDisplay<T>(out var existingDisplay))
            return existingDisplay;

        CloseCurrentDisplayIfExist();
        var _ = Root;
        var (instance, rentHandle) = await Managers.Pool.PopAsync<T>(_layer[LayerType.Display]);

        if (IsInstanceInvalid(instance, out var logKey, typeof(T).Name))
        {
            Log.Error(logKey, typeof(T).Name);
            return null;
        }

        RegisterDisplay(instance, rentHandle);
        return instance;
    }

    public T OpenDisplay<T>() where T : UIDisplay
    {
        if (HasExistingDisplay<T>(out var existingDisplay))
            return existingDisplay;

        CloseCurrentDisplayIfExist();
        var _ = Root;
        var (instance, rentHandle) = Managers.Pool.Pop<T>(_layer[LayerType.Display]);

        if (IsInstanceInvalid(instance, out var logKey, typeof(T).Name))
        {
            Log.Error(logKey, typeof(T).Name);
            return null;
        }

        RegisterDisplay(instance, rentHandle);
        return instance;
    }

    public async UniTask<T> OpenPopupAsync<T>(bool allowMultiple = false) where T : UIPopup
    {
        if (!allowMultiple && HasExistingPopup<T>(out var targetPopup))
            return targetPopup;

        var _ = Root;
        var (instance, rentHandle) = await Managers.Pool.PopAsync<T>(_layer[LayerType.Popup]);

        if (IsInstanceInvalid(instance, out var logKey, typeof(T).Name))
        {
            Log.Error(logKey, typeof(T).Name);
            return null;
        }

        RegisterPopup(instance, rentHandle);
        return instance;
    }

    public T OpenPopup<T>() where T : UIPopup
    {
        if (HasExistingPopup<T>(out var targetPopup))
            return targetPopup;

        var _ = Root;
        var (instance, rentHandle) = Managers.Pool.Pop<T>(_layer[LayerType.Popup]);

        if (IsInstanceInvalid(instance, out var logKey, typeof(T).Name))
        {
            Log.Error(logKey, typeof(T).Name);
            return null;
        }

        RegisterPopup(instance, rentHandle);
        return instance;
    }

    public async UniTask<T> OpenSystemAsync<T>(LayerType layer = LayerType.System) where T : UISystem
    {
        if (HasExistingSystem<T>(out var existingSystem))
            return existingSystem;

        var _ = Root;
        var (instance, rentHandle) = await Managers.Pool.PopAsync<T>(_layer[layer]);

        if (IsInstanceInvalid(instance, out var logKey, typeof(T).Name))
        {
            Log.Error(logKey, typeof(T).Name);
            return null;
        }

        RegisterSystem(instance, rentHandle);
        return instance;
    }

    public T OpenSystem<T>(LayerType layer = LayerType.System) where T : UISystem
    {
        if (HasExistingSystem<T>(out var existingSystem))
            return existingSystem;

        var _ = Root;
        var (instance, rentHandle) = Managers.Pool.Pop<T>(_layer[layer]);

        if (IsInstanceInvalid(instance, out var logKey, typeof(T).Name))
        {
            Log.Error(logKey, typeof(T).Name);
            return null;
        }

        RegisterSystem(instance, rentHandle);
        return instance;
    }

    public T GetScreen<T>() where T : UIDisplay
        => _display as T;

    public T GetPopup<T>() where T : UIPopup
    {
        foreach (var popup in _popups)
        {
            if (popup is T targetPopup)
                return targetPopup;
        }

        return null;
    }

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
        if (!IsUIValidAndManaged(ui))
            return;

        HandleUIDisconnect(ui);
        HandleUIPopupDisconnect(ui);
        HandleUIHandleDispose(ui);
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

    public void CloseAll(params LayerType[] excludeLayers)
    {
        if (HasNoExcludeLayers(excludeLayers))
        {
            ClearAllUIStates();
            return;
        }

        CloseFilteredUI(excludeLayers);
    }

    public void CloseAllExcept<T>() where T : UserInterface
    {
        var targetsToClose = CollectUIExceptType<T>();

        foreach (var ui in targetsToClose)
            Close(ui);
    }

    public bool CloseFocusPopup()
    {
        if (HasNoPopups())
            return false;

        var topPopup = GetTopPopup();
        Close(topPopup);
        return true;
    }

    public void FocusPopup(UIPopup popup)
    {
        if (!IsPopupValidAndOpened(popup))
            return;

        ReorderPopupToTop(popup);
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

    private bool HasExistingDisplay<T>(out T display) where T : UIDisplay
    {
        display = _display as T;
        return display != null;
    }

    private void CloseCurrentDisplayIfExist()
    {
        if (_display != null)
            Close(_display);
    }

    private bool IsInstanceInvalid<T>(T instance, out LocalizationKey logKey, string typeName) where T : class
    {
        logKey = LocalizationKey.Log_UI_OpenDisplayFailed;
        return instance == null;
    }

    private void RegisterDisplay(UIDisplay instance, IDisposable rentHandle)
    {
        _display = instance;
        _handles[instance] = rentHandle;
    }

    private bool HasExistingPopup<T>(out T popup) where T : UIPopup
    {
        popup = GetPopup<T>();
        return popup != null;
    }

    private void RegisterPopup(UIPopup instance, IDisposable rentHandle)
    {
        _popups.Add(instance);
        _handles[instance] = rentHandle;
        RefreshPopup();
    }

    private bool HasExistingSystem<T>(out T system) where T : UISystem
    {
        system = GetSystem<T>();
        return system != null;
    }

    private void RegisterSystem(UISystem instance, IDisposable rentHandle)
        => _handles[instance] = rentHandle;

    private bool IsUIValidAndManaged(UserInterface ui)
        => ui != null && _handles.ContainsKey(ui);

    private void HandleUIDisconnect(UserInterface ui)
    {
        if (_display == ui)
            _display = null;
    }

    private void HandleUIPopupDisconnect(UserInterface ui)
    {
        if (ui is UIPopup popup)
        {
            _popups.Remove(popup);
            RefreshPopup();
        }
    }

    private void HandleUIHandleDispose(UserInterface ui)
    {
        if (_handles.TryGetValue(ui, out var handle))
        {
            _handles.Remove(ui);
            handle?.Dispose();
        }
    }

    private bool HasNoExcludeLayers(LayerType[] excludeLayers)
        => excludeLayers == null || excludeLayers.Length == 0;

    private void ClearAllUIStates()
    {
        foreach (var handle in _handles.Values)
            handle?.Dispose();

        _handles.Clear();
        _popups.Clear();
        _display = null;
        RefreshPopup();
    }

    private void CloseFilteredUI(LayerType[] excludeLayers)
    {
        var targetsToClose = new List<UserInterface>();

        foreach (var pair in _handles)
        {
            var ui = pair.Key;

            if (ui == null)
                continue;

            if (!IsBelongsToAnyLayer(ui, excludeLayers))
                targetsToClose.Add(ui);
        }

        foreach (var ui in targetsToClose)
            Close(ui);
    }

    private bool IsBelongsToAnyLayer(UserInterface ui, LayerType[] layers)
    {
        foreach (var layer in layers)
        {
            if (_layer.TryGetValue(layer, out var layerTransform) && ui.transform.IsChildOf(layerTransform))
                return true;
        }

        return false;
    }

    private List<UserInterface> CollectUIExceptType<T>() where T : UserInterface
    {
        var targets = new List<UserInterface>();

        foreach (var pair in _handles)
        {
            if (pair.Key != null && !(pair.Key is T))
                targets.Add(pair.Key);
        }

        return targets;
    }

    private bool HasNoPopups()
        => _popups.Count <= 0;

    private UIPopup GetTopPopup()
        => _popups[_popups.Count - 1];

    private bool IsPopupValidAndOpened(UIPopup popup)
        => popup != null && _popups.Contains(popup);

    private void ReorderPopupToTop(UIPopup popup)
    {
        _popups.Remove(popup);
        _popups.Add(popup);
        RefreshPopup();
    }
}
