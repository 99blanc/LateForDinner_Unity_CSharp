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
    private Vector2 _hotspot = Define.Cursor.Hotspot;
    private IDisposable _handle;

    public void GetCursor()
    {
        var normal = Managers.Resource.GetSprite(Define.Atlas.UI_Common, Define.Sprite.Cursor_Normal);
        var press = Managers.Resource.GetSprite(Define.Atlas.UI_Common, Define.Sprite.Cursor_Press);

        if (normal == null || press == null)
            return;

        Texture2D first = Managers.Resource.GetTextureFromSprite(normal);
        Texture2D last = Managers.Resource.GetTextureFromSprite(press);
        _handle?.Dispose();
        _handle = Observable.EveryUpdate()
        .Where(_ => Mouse.current != null)
        .Subscribe(_ =>
        {
            var mouse = Mouse.current;

            if (mouse.leftButton.wasPressedThisFrame)
                Cursor.SetCursor(last, _hotspot, CursorMode.Auto);
            else
                Cursor.SetCursor(first, _hotspot, CursorMode.Auto);
        });
    }

    public async UniTask LoadAsync()
    {
        var original = await Managers.Resource.LoadAssetAsync<InputActionAsset>(Literal.Assets.InputActionAsset);
        
        if (original == null)
            return;

        _action = UnityEngine.Object.Instantiate(original);

        if (Managers.Config?.Option != null && !string.IsNullOrEmpty(Managers.Config.Option.Access.keybind))
            _action.LoadBindingOverridesFromJson(Managers.Config.Option.Access.keybind);

        CacheActions();
        _action.Enable();
    }

    private void CacheActions()
    {
        if (_action == null)
            return;

        _caches.Clear();

        foreach (var map in _action.actionMaps)
        {
            foreach (var action in map.actions)
                _caches[action.name] = action;
        }
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

        return Observable.FromEvent<InputAction.CallbackContext>(h => output.performed += h, h => output.performed -= h).Select(_ => Unit.Default);
    }

    public IDisposable Subscribe(string actionName, Action onPerformed)
        => AsObservable(actionName).Subscribe(_ => onPerformed());

    public IEnumerable<KeyValuePair<string, InputAction>> GetActions()
        => _caches;

    public List<InputAction> GetBindableActions()
    {
        var list = new List<InputAction>();

        if (_action == null)
            return list;

        var userMap = _action.FindActionMap(Literal.Maps.User);

        if (userMap != null)
        {
            foreach (var action in userMap.actions)
                list.Add(action);
        }
        return list;
    }

    public string Save()
        => _action?.SaveBindingOverridesAsJson() ?? string.Empty;

    public void Reset()
        => _action?.RemoveAllBindingOverrides();
}
