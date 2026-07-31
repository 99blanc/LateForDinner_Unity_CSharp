using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlManager
{
    private InputActionAsset _action;
    private readonly Dictionary<string, Subject<Unit>> _subjects = new Dictionary<string, Subject<Unit>>();
    private readonly Dictionary<string, ReactiveProperty<Vector2>> _inputs = new Dictionary<string, ReactiveProperty<Vector2>>();
    private readonly ReactiveProperty<string> _map = new ReactiveProperty<string>(string.Empty);
    public ReadOnlyReactiveProperty<string> Map => _map;

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

        Bind();
        Switch(Literal.Maps.User);
    }

    private void Bind()
    {
        if (_action == null)
            return;

        foreach (var map in _action.actionMaps)
        {
            foreach (var action in map.actions)
            {
                if (action.expectedControlType == Literal.Maps.Vector2)
                {
                    var prop = new ReactiveProperty<Vector2>(Vector2.zero);
                    _inputs[action.name] = prop;
                    action.performed += ctx => prop.Value = ctx.ReadValue<Vector2>();
                    action.canceled += _ => prop.Value = Vector2.zero;
                }
                else
                {
                    var subject = new Subject<Unit>();
                    _subjects[action.name] = subject;
                    action.performed += _ => subject.OnNext(Unit.Default);
                }
            }
        }
    }

    public Observable<Unit> GetSubject(string action)
    {
        if (!_subjects.TryGetValue(action, out var subject))
        {
            subject = new Subject<Unit>();
            _subjects[action] = subject;
        }

        return subject;
    }

    public ReadOnlyReactiveProperty<Vector2> GetInput(string action)
    {
        if (!_inputs.TryGetValue(action, out var prop))
        {
            prop = new ReactiveProperty<Vector2>(Vector2.zero);
            _inputs[action] = prop;
        }

        return prop;
    }

    public void Switch(string map)
    {
        if (_action == null)
            return;

        _action.Disable();
        var targetMap = _action.FindActionMap(map);

        if (targetMap != null)
        {
            targetMap.Enable();
            _map.Value = map;
        }
    }

    public IDisposable Subscribe(string action, Action onPerformed)
        => GetSubject(action).Subscribe(_ => onPerformed());

    public IDisposable Subscribe(string action, Action<Vector2> onValueChanged)
        => GetInput(action).Subscribe(onValueChanged);
    
    public string Save()
        => _action?.SaveBindingOverridesAsJson() ?? string.Empty;

    public void Reset()
        => _action?.RemoveAllBindingOverrides();
}
