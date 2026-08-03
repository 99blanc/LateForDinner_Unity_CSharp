using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlManager
{
    private InputActionAsset _action;
    private readonly Dictionary<string, InputAction> _caches = new Dictionary<string, InputAction>();

    public async UniTask InitAsync()
        => await LoadAsync();

    private async UniTask LoadAsync()
    {
        var original = await Managers.Resource.LoadAssetAsync<InputActionAsset>(Literal.Assets.InputActionAsset);
        
        if (original == null)
            return;

        _action = UnityEngine.Object.Instantiate(original);

        if (Managers.Config?.Settings != null && !string.IsNullOrEmpty(Managers.Config.Settings.Access.keybind))
            _action.LoadBindingOverridesFromJson(Managers.Config.Settings.Access.keybind);

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

    public string Save()
        => _action?.SaveBindingOverridesAsJson() ?? string.Empty;

    public void Reset()
        => _action?.RemoveAllBindingOverrides();
}
