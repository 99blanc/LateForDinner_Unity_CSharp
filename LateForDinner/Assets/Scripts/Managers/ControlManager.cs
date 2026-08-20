using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlManager
{
    private readonly Dictionary<string, InputAction> _caches = new Dictionary<string, InputAction>();
    private InputActionAsset _action;
    private readonly Vector2 _hotspot = Define.Cursor.Hotspot;
    private IDisposable _handle;

    public void Setup()
    {
        SetupCursor();
        SetupConsoleToggle();
    }

    private void SetupCursor()
    {
        var normal = Managers.Resource.GetSprite(Define.Atlas.Common, Define.Sprite.Cursor_Normal);
        var press = Managers.Resource.GetSprite(Define.Atlas.Common, Define.Sprite.Cursor_Press);

        if (normal == null || press == null)
            return;

        Texture2D first = Managers.Resource.GetTextureFromSprite(normal);
        Texture2D last = Managers.Resource.GetTextureFromSprite(press);
        _handle?.Dispose();
        _handle = Observable.EveryUpdate()
        .Where(_ => Mouse.current != null)
        .Subscribe(_ => UpdateCursorState(first, last));
    }

    public IDisposable SetupConsoleToggle()
    {
        return AsObservable(Literal.Hotkeys.Console).Subscribe(_ =>
        {
            var console = Managers.UI.GetSystem<UIConsoleSystem>();

            if (console != null)
                Managers.UI.Close(console);
            else
                Managers.UI.OpenSystem<UIConsoleSystem>();
        });
    }

    public async UniTask LoadAsync()
    {
        var original = await Managers.Resource.LoadAssetAsync<InputActionAsset>(Literal.Assets.InputActionAsset);
        
        if (original == null)
        {
            Log.Error(Localization.Log_Control_AssetLoadFailed, Literal.Assets.InputActionAsset);
            return;
        }

        _action = UnityEngine.Object.Instantiate(original);

        if (Managers.Config?.Option != null && !string.IsNullOrEmpty(Managers.Config.Option.Access.keybind))
            _action.LoadBindingOverridesFromJson(Managers.Config.Option.Access.keybind);

        CacheActions();
        EnableMap(Literal.Maps.User);
        EnableMap(Literal.Maps.UI);
        Log.Info(Localization.Log_Control_LoadedSuccessfully);
    }

    private void CacheActions()
    {
        if (_action == null)
            return;

        _caches.Clear();
        var actionMaps = _action.actionMaps;

        for (int index = 0; index < actionMaps.Count; index++)
        {
            var map = actionMaps[index];
            var actions = map.actions;

            for (int sub = 0; sub < actions.Count; sub++)
            {
                var action = actions[sub];
                _caches[action.name] = action;
            }
        }
    }

    private void UpdateCursorState(Texture2D normalCursor, Texture2D pressCursor)
    {
        if (!Application.isFocused)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
            return;
        }

        var mouse = Mouse.current;
        Vector2 mousePos = mouse.position.ReadValue();

        if (mousePos.x < 0 || mousePos.x > Screen.width || mousePos.y < 0 || mousePos.y > Screen.height)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
            return;
        }

        if (mouse.leftButton.isPressed)
            Cursor.SetCursor(pressCursor, _hotspot, CursorMode.ForceSoftware);
        else
            Cursor.SetCursor(normalCursor, _hotspot, CursorMode.ForceSoftware);
    }

    public void EnableMap(string mapName)
    {
        var map = _action?.FindActionMap(mapName);

        if (map == null)
        {
            Log.Warning(Localization.Log_Control_MapNotFound, mapName);
            return;
        }

        map.Enable();
    }

    public void DisableMap(string mapName)
    {
        var map = _action?.FindActionMap(mapName);
        map?.Disable();
    }

    public bool IsPressed(string actionName) 
        => _caches.TryGetValue(actionName, out var action) && action.IsPressed();

    public bool IsTriggered(string actionName) 
        => _caches.TryGetValue(actionName, out var action) && action.triggered;

    public Vector2 GetVector2(string actionName)
    {
        if (_caches.TryGetValue(actionName, out var action))
            return action.ReadValue<Vector2>();

        return Vector2.zero;
    }

    public Observable<Unit> AsObservable(string action)
    {
        if (!_caches.TryGetValue(action, out var output) || output == null)
            return Observable.Empty<Unit>();

        return Observable.FromEvent<InputAction.CallbackContext>(h => output.performed += h, h => output.performed -= h)
        .Select(_ => Unit.Default);
    }

    public IDisposable Subscribe(string actionName, Action onPerformed) 
        => AsObservable(actionName).Subscribe(_ => onPerformed());

    public IEnumerable<KeyValuePair<string, InputAction>> GetActions() 
        => _caches;

    public void LoadBindingFromJson(string json)
    {
        if (_action == null)
            return;

        if (string.IsNullOrEmpty(json))
            _action.RemoveAllBindingOverrides();
        else
            _action.LoadBindingOverridesFromJson(json);

        CacheActions();
    }

    public List<InputAction> GetBindableActions()
    {
        var list = new List<InputAction>();

        if (_action == null)
            return list;

        var userMap = _action.FindActionMap(Literal.Maps.User);

        if (userMap == null)
            return list;

        var actions = userMap.actions;

        for (int index = 0; index < actions.Count; index++)
            list.Add(actions[index]);

        return list;
    }

    public string Save() 
        => _action?.SaveBindingOverridesAsJson() ?? string.Empty;

    public void Reset() 
        => _action?.RemoveAllBindingOverrides();
}
