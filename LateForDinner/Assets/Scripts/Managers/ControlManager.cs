using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlManager
{
    private struct DoubleTapState
    {
        public float FirstTapTime;
        public bool IsPending;
    }
    private readonly Dictionary<string, HashSet<IPoolable>> _subscribers = new Dictionary<string, HashSet<IPoolable>>();
    private readonly Dictionary<string, InputAction> _actionCaches = new Dictionary<string, InputAction>();
    private readonly Dictionary<string, DoubleTapState> _doubleTapStates = new Dictionary<string, DoubleTapState>();
    private readonly Dictionary<string, float> _repeatTimers = new Dictionary<string, float>();
    private readonly Dictionary<string, float> _triggeredCaches = new Dictionary<string, float>();
    private readonly HashSet<string> _pressedCaches = new HashSet<string>();
    private readonly Dictionary<string, float> _doubleTriggeredCaches = new Dictionary<string, float>();
    private readonly Dictionary<string, Subject<Unit>> _actionSubjects = new Dictionary<string, Subject<Unit>>();
    private InputActionAsset _actionAsset;

    public void Setup()
    {
        StartInputUpdateLoop();
        RegisterShortcutHandlers();
    }

    private void StartInputUpdateLoop()
    {
        Observable.EveryUpdate()
        .Subscribe(_ => CachePressedStates());
    }

    private void CachePressedStates()
    {
        _pressedCaches.Clear();

        foreach (var (actionName, action) in _actionCaches)
        {
            if (IsActionValidAndPressed(action))
                _pressedCaches.Add(actionName);
        }
    }

    private bool IsActionValidAndPressed(InputAction action)
        => action != null && action.IsPressed();

    private void RegisterShortcutHandlers()
    {
        BindSystemUIToggleAction<UIConsoleSystem>(Literal.Hotkeys.Console);
        BindAction(Literal.Hotkeys.Cancel, () => Managers.UI.CloseFocusPopup());
        BindPopupUIToggleAction<UIPausePopup>(Literal.Hotkeys.Pause);
    }

    private void BindSystemUIToggleAction<T>(string hotkeyName) where T : UISystem
    {
        BindAction(hotkeyName, () =>
        {
            var targetUI = Managers.UI.GetSystem<T>();

            if (targetUI != null)
                Managers.UI.Close(targetUI);
            else
                Managers.UI.OpenSystem<T>();
        });
    }

    private void BindPopupUIToggleAction<T>(string hotkeyName) where T : UIPopup
    {
        BindAction(hotkeyName, () =>
        {
            if (Managers.UI.GetDisplay<UITitleDisplay>())
                return;

            var targetUI = Managers.UI.GetPopup<T>();

            if (targetUI != null)
                Managers.UI.Close(targetUI);
            else
                Managers.UI.OpenPopup<T>();
        });
    }

    private void BindAction(string actionName, Action onPerformed)
        => AsObservable(actionName).Subscribe(_ => onPerformed());

    public async UniTask LoadAsync()
    {
        var originalAsset = await Managers.Resource.LoadAssetAsync<InputActionAsset>(Literal.Assets.InputActionAsset);

        if (originalAsset == null)
        {
            Log.Error(LocalizationKey.Log_Control_AssetLoadFailed, Literal.Assets.InputActionAsset);
            return;
        }

        _actionAsset = UnityEngine.Object.Instantiate(originalAsset);
        ApplySavedKeyBindings();
        CacheAllActions();
        EnableActionMap(Literal.Maps.User);
        EnableActionMap(Literal.Maps.UI);
        Log.Info(LocalizationKey.Log_Control_LoadedSuccessfully);
    }

    private void ApplySavedKeyBindings()
    {
        var savedKeybindJson = Managers.Config?.Option?.Access?.keybind;

        if (!string.IsNullOrEmpty(savedKeybindJson))
            _actionAsset.LoadBindingOverridesFromJson(savedKeybindJson);
    }

    private void CacheAllActions()
    {
        if (_actionAsset == null)
            return;

        _actionCaches.Clear();
        CacheActionMap(Literal.Maps.User);
        CacheActionMap(Literal.Maps.UI);
    }

    private void CacheActionMap(string mapName)
    {
        var map = _actionAsset.FindActionMap(mapName);

        if (map == null)
            return;

        foreach (var action in map.actions)
        {
            if (action.name == Literal.Hotkeys.Point || action.name == Literal.Hotkeys.Look || action.type == InputActionType.PassThrough)
                continue;

            _actionCaches[action.name] = action;
            RegisterActionEvents(action.name, action);
        }
    }

    private void RegisterActionEvents(string actionName, InputAction action)
    {
        action.performed -= OnActionPerformed;
        action.performed += OnActionPerformed;
    }

    private void OnActionPerformed(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        foreach (var (actionName, action) in _actionCaches)
        {
            if (action == context.action)
            {
                ProcessActionPerformed(actionName);
                break;
            }
        }
    }

    private void ProcessActionPerformed(string actionName)
    {
        float currentTime = Time.unscaledTime;
        _triggeredCaches[actionName] = currentTime;

        if (_actionSubjects.TryGetValue(actionName, out var subject))
            subject.OnNext(Unit.Default);

        if (!IsUserAction(actionName))
            return;

        foreach (var key in new List<string>(_doubleTapStates.Keys))
        {
            if (key != actionName)
            {
                var otherState = _doubleTapStates[key];

                if (otherState.IsPending)
                    _doubleTapStates[key] = new DoubleTapState { IsPending = false, FirstTapTime = 0f };
            }
        }

        EvaluateDoubleTap(actionName, currentTime);
    }

    private bool IsUserAction(string actionName)
    {
        var userMap = _actionAsset?.FindActionMap(Literal.Maps.User);
        return userMap != null && userMap.FindAction(actionName) != null;
    }

    private void EvaluateDoubleTap(string actionName, float currentTime)
    {
        if (!_doubleTapStates.TryGetValue(actionName, out var state))
            state = new DoubleTapState { IsPending = false, FirstTapTime = 0f };

        if (state.IsPending)
        {
            if ((currentTime - state.FirstTapTime) <= Define.Scaler.Threshold)
            {
                _doubleTriggeredCaches[actionName] = currentTime;
                _doubleTapStates[actionName] = new DoubleTapState { IsPending = false, FirstTapTime = 0f };
                return;
            }
        }

        _doubleTapStates[actionName] = new DoubleTapState { IsPending = true, FirstTapTime = currentTime };
    }

    public void EnableActionMap(string mapName)
    {
        if (_actionAsset == null)
            return;

        var map = _actionAsset?.FindActionMap(mapName);

        if (map == null)
        {
            Log.Warning(LocalizationKey.Log_Control_MapNotFound, mapName);
            return;
        }

        map.Enable();
    }

    public void DisableActionMap(string mapName)
    {
        if (_actionAsset == null)
            return;

        _actionAsset?.FindActionMap(mapName)?.Disable();
    }
    private bool IsAuthorized(IPoolable owner, string actionName)
    {
        if (owner == null) 
            return false;

        return _subscribers.TryGetValue(actionName, out var set) && set.Contains(owner);
    }

    public bool IsPressed(IPoolable owner, string actionName)
    {
        if (!IsAuthorized(owner, actionName))
            return false;

        return _pressedCaches.Contains(actionName);
    }

    public bool IsTriggered(IPoolable owner, string actionName, float bufferWindow = Define.Scaler.Threshold)
    {
        if (!IsAuthorized(owner, actionName))
            return false;

        if (_triggeredCaches.TryGetValue(actionName, out float timestamp))
        {
            _triggeredCaches.Remove(actionName);

            if (Time.unscaledTime - timestamp <= bufferWindow)
                return true;
        }

        return false;
    }

    public bool IsDoubleTriggered(IPoolable owner, string actionName, float threshold = Define.Scaler.Threshold)
    {
        if (!IsAuthorized(owner, actionName))
            return false;

        if (ConsumeDoubleTriggerCache(actionName, threshold))
            return true;

        CheckDoubleTapTimeout(actionName, threshold);
        return false;
    }

    private bool ConsumeDoubleTriggerCache(string actionName, float threshold)
    {
        if (_doubleTriggeredCaches.TryGetValue(actionName, out float timestamp))
        {
            _doubleTriggeredCaches.Remove(actionName);

            if (Time.unscaledTime - timestamp <= threshold)
                return true;
        }

        return false;
    }

    private void CheckDoubleTapTimeout(string actionName, float threshold)
    {
        if (_doubleTapStates.TryGetValue(actionName, out var state))
        {
            if (state.IsPending && (Time.unscaledTime - state.FirstTapTime > threshold))
                _doubleTapStates[actionName] = new DoubleTapState { IsPending = false, FirstTapTime = 0f };
        }
    }

    public bool IsHoldRepeated(IPoolable owner, string actionName, float interval = Define.Scaler.Threshold)
    {
        if (!IsAuthorized(owner, actionName))
        {
            _repeatTimers.Remove(actionName);
            return false;
        }

        if (!IsPressed(owner, actionName))
        {
            _repeatTimers.Remove(actionName);
            return false;
        }

        float currentTime = Time.unscaledTime;

        if (!_repeatTimers.TryGetValue(actionName, out float nextTriggerTime))
        {
            _repeatTimers[actionName] = currentTime + interval;
            return true;
        }

        if (currentTime < nextTriggerTime)
            return false;

        _repeatTimers[actionName] = currentTime + interval;
        return true;
    }

    public bool IsModifierTriggered(IPoolable owner, string modifierActionName, string mainActionName)
        => IsPressed(owner, modifierActionName) && IsTriggered(owner, mainActionName);

    public Observable<Unit> AsObservable(string actionName)
    {
        if (!_actionCaches.ContainsKey(actionName))
            return Observable.Empty<Unit>();

        if (!_actionSubjects.TryGetValue(actionName, out var subject))
        {
            subject = new Subject<Unit>();
            _actionSubjects[actionName] = subject;
        }

        return subject;
    }

    public IDisposable Subscribe(IPoolable owner, string actionName, InputEventType inputType = InputEventType.Triggered, Action onPerformed = null, float optionValue = Define.Scaler.Threshold)
    {
        RegisterSubscriber(owner, actionName);
        IDisposable innerDisposable = inputType switch
        {
            InputEventType.Triggered => AsObservable(actionName).Subscribe(_ => onPerformed?.Invoke()),
            InputEventType.Pressed => Observable.EveryUpdate().Where(_ => IsPressed(owner, actionName)).Subscribe(_ => onPerformed?.Invoke()),
            InputEventType.DoubleTriggered => Observable.EveryUpdate().Where(_ => IsDoubleTriggered(owner, actionName, optionValue)).Subscribe(_ => onPerformed?.Invoke()),
            InputEventType.HoldRepeated => Observable.EveryUpdate().Where(_ => IsHoldRepeated(owner, actionName, optionValue)).Subscribe(_ => onPerformed?.Invoke()),
            _ => Disposable.Empty
        };
        return Disposable.Create(() =>
        {
            innerDisposable?.Dispose();
            UnregisterSubscriber(owner, actionName);
        });
    }

    private void RegisterSubscriber(IPoolable owner, string actionName)
    {
        if (owner == null) 
            return;

        if (!_subscribers.TryGetValue(actionName, out var set))
        {
            set = new HashSet<IPoolable>();
            _subscribers[actionName] = set;
        }

        set.Add(owner);
    }

    private void UnregisterSubscriber(IPoolable owner, string actionName)
    {
        if (owner == null) 
            return;

        if (_subscribers.TryGetValue(actionName, out var set))
            set.Remove(owner);
    }

    public IEnumerable<KeyValuePair<string, InputAction>> GetActions()
        => _actionCaches;

    public void ClearInputStates()
    {
        _triggeredCaches.Clear();
        _pressedCaches.Clear();
        _doubleTriggeredCaches.Clear();
        _doubleTapStates.Clear();
        _repeatTimers.Clear();
    }

    public void LoadBindingFromJson(string json)
    {
        if (_actionAsset == null)
            return;

        if (string.IsNullOrEmpty(json))
            _actionAsset.RemoveAllBindingOverrides();
        else
            _actionAsset.LoadBindingOverridesFromJson(json);
    }

    public List<InputAction> GetBindableActions()
    {
        var bindableActions = new List<InputAction>();
        var userMap = _actionAsset?.FindActionMap(Literal.Maps.User);

        if (userMap == null)
            return bindableActions;

        foreach (var action in userMap.actions)
            bindableActions.Add(action);

        return bindableActions;
    }

    public string CreateBindingSnapshot()
        => SaveBindingsToJson();

    public void RestoreBindingSnapshot(string snapshotJson)
        => LoadBindingFromJson(snapshotJson);

    public string SaveBindingsToJson()
        => _actionAsset?.SaveBindingOverridesAsJson() ?? string.Empty;

    public void ResetBindings()
        => _actionAsset?.RemoveAllBindingOverrides();
}
