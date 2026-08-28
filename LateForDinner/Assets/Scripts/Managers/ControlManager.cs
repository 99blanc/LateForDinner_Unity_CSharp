using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlManager
{
    private readonly Dictionary<string, InputAction> _actionCaches = new Dictionary<string, InputAction>();
    private readonly Dictionary<string, float> _lastFirstTapTimes = new Dictionary<string, float>();
    private readonly HashSet<string> _triggeredCaches = new HashSet<string>();
    private readonly HashSet<string> _pressedCaches = new HashSet<string>();
    private readonly Dictionary<string, float> _doubleTriggeredCaches = new Dictionary<string, float>();
    private readonly Vector2 _cursorHotspot = Define.Cursor.Hotspot;
    private InputActionAsset _actionAsset;
    private Texture2D _normalCursorTexture;
    private Texture2D _pressCursorTexture;
    private string _pendingDoubleActionName;

    public void Setup()
    {
        CacheCursorTextures();
        StartUnifiedUpdateLoop();
        RegisterShortcutHandlers();
    }

    private void CacheCursorTextures()
    {
        var normalSprite = Managers.Resource.GetSprite(Define.Atlas.Common, Define.Sprite.Cursor_Normal);
        var pressSprite = Managers.Resource.GetSprite(Define.Atlas.Common, Define.Sprite.Cursor_Press);

        if (AreCursorSpritesMissing(normalSprite, pressSprite))
            return;

        _normalCursorTexture = Managers.Resource.GetTextureFromSprite(normalSprite);
        _pressCursorTexture = Managers.Resource.GetTextureFromSprite(pressSprite);
    }

    private bool AreCursorSpritesMissing(Sprite normal, Sprite press)
        => normal == null || press == null;

    private void StartUnifiedUpdateLoop()
    {
        Observable.EveryUpdate()
        .Subscribe(_ =>
        {
            CachePressedStates();
            UpdateCursorState();
        });
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

    private void UpdateCursorState()
    {
        if (!CanUpdateCursorVisual())
            return;

        Vector2 mousePosition = GetCurrentMousePosition();

        if (IsMouseUnavailableOrOutOfBounds(mousePosition))
        {
            SetDefaultCursor();
            return;
        }

        SetCursorVisual(IsLeftMouseButtonPressed());
    }

    private bool CanUpdateCursorVisual()
    {
        if (Application.isFocused == false)
            return false;

        if (_normalCursorTexture == null || _pressCursorTexture == null)
            return false;

        return true;
    }

    private Vector2 GetCurrentMousePosition()
        => Mouse.current?.position.ReadValue() ?? Vector2.negativeInfinity;

    private bool IsMouseUnavailableOrOutOfBounds(Vector2 position)
        => Mouse.current == null || IsCursorOutOfBounds(position);

    private bool IsCursorOutOfBounds(Vector2 position)
        => position.x < 0 || position.x > Screen.width || position.y < 0 || position.y > Screen.height;

    private bool IsLeftMouseButtonPressed()
        => Mouse.current.leftButton.isPressed;

    private void SetDefaultCursor()
        => Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);

    private void SetCursorVisual(bool isPressed)
    {
        var targetTexture = isPressed ? _pressCursorTexture : _normalCursorTexture;
        Cursor.SetCursor(targetTexture, _cursorHotspot, CursorMode.ForceSoftware);
    }

    private void RegisterShortcutHandlers()
    {
        BindSystemUIToggleAction<UIConsoleSystem>(Literal.Hotkeys.Console);
        BindPopupUIToggleAction<UIOptionPopup>(Literal.Hotkeys.Option);
        BindAction(Literal.Hotkeys.Cancel, () => Managers.UI.CloseFocusPopup());
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
        _triggeredCaches.Add(actionName);

        if (!IsUserAction(actionName))
            return;

        ResetPendingDoubleTapIfDifferentKey(actionName);

        if (TryEvaluateDoubleTap(actionName, currentTime))
            RegisterSuccessfulDoubleTap(actionName);
        else
            RegisterFirstTap(actionName, currentTime);
    }

    private bool IsUserAction(string actionName)
    {
        var userMap = _actionAsset?.FindActionMap(Literal.Maps.User);
        return userMap != null && userMap.FindAction(actionName) != null;
    }

    private void ResetPendingDoubleTapIfDifferentKey(string currentActionName)
    {
        if (HasPendingDifferentDoubleTap(currentActionName))
            _pendingDoubleActionName = null;
    }

    private bool HasPendingDifferentDoubleTap(string currentActionName)
        => !string.IsNullOrEmpty(_pendingDoubleActionName) && _pendingDoubleActionName != currentActionName;

    private bool TryEvaluateDoubleTap(string actionName, float currentTime)
    {
        if (_pendingDoubleActionName != actionName)
            return false;

        if (_lastFirstTapTimes.TryGetValue(actionName, out float firstTime))
            return IsWithinDoubleTapThreshold(currentTime, firstTime);

        return false;
    }

    private bool IsWithinDoubleTapThreshold(float currentTime, float firstTime)
    {
        float threshold = Define.Scaler.Threshold;
        return (currentTime - firstTime) <= threshold;
    }

    private void RegisterSuccessfulDoubleTap(string actionName)
    {
        _doubleTriggeredCaches[actionName] = Time.unscaledTime;
        _pendingDoubleActionName = null;
        _lastFirstTapTimes.Remove(actionName);
    }

    private void RegisterFirstTap(string actionName, float currentTime)
    {
        _pendingDoubleActionName = actionName;
        _lastFirstTapTimes[actionName] = currentTime;
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

    public bool IsPressed(string actionName)
        => _pressedCaches.Contains(actionName);

    public bool IsTriggered(string actionName)
    {
        if (_triggeredCaches.Contains(actionName))
        {
            _triggeredCaches.Remove(actionName);
            return true;
        }

        return false;
    }

    public bool IsDoubleTriggered(string actionName, float threshold = Define.Scaler.Threshold)
    {
        if (ConsumeDoubleTriggerCache(actionName))
            return true;

        CheckDoubleTapTimeout(actionName, threshold);
        return false;
    }

    private bool ConsumeDoubleTriggerCache(string actionName)
    {
        if (_doubleTriggeredCaches.TryGetValue(actionName, out float timestamp))
        {
            _doubleTriggeredCaches.Remove(actionName);

            if (Time.unscaledTime - timestamp <= Define.Scaler.Threshold)
                return true;
        }

        return false;
    }

    private void CheckDoubleTapTimeout(string actionName, float threshold)
    {
        if (_pendingDoubleActionName != actionName)
            return;

        if (_lastFirstTapTimes.TryGetValue(actionName, out float firstTime))
        {
            if (HasDoubleTapTimedOut(firstTime, threshold))
                ClearPendingDoubleTap(actionName);
        }
    }

    private bool HasDoubleTapTimedOut(float firstTime, float threshold)
        => Time.unscaledTime - firstTime > threshold;

    private void ClearPendingDoubleTap(string actionName)
    {
        _pendingDoubleActionName = null;
        _lastFirstTapTimes.Remove(actionName);
    }

    public bool IsModifierTriggered(string modifierActionName, string mainActionName)
        => IsPressed(modifierActionName) && IsTriggered(mainActionName);

    public Observable<Unit> AsObservable(string actionName)
    {
        if (!_actionCaches.TryGetValue(actionName, out var action) || action == null)
            return Observable.Empty<Unit>();

        return Observable.FromEvent<InputAction.CallbackContext>(h => action.performed += h, h => action.performed -= h).Select(_ => Unit.Default);
    }

    public IDisposable Subscribe(string actionName, Action onPerformed)
        => AsObservable(actionName).Subscribe(_ => onPerformed());

    public IEnumerable<KeyValuePair<string, InputAction>> GetActions()
        => _actionCaches;

    public void ClearInputStates()
    {
        _triggeredCaches.Clear();
        _pressedCaches.Clear();
        _doubleTriggeredCaches.Clear();
        _lastFirstTapTimes.Clear();
        _pendingDoubleActionName = null;

        if (_actionAsset == null)
            return;

        foreach (var map in _actionAsset.actionMaps)
        {
            map.Disable();
            map.Enable();
        }
    }

    public void LoadBindingFromJson(string json)
    {
        if (_actionAsset == null)
            return;

        if (string.IsNullOrEmpty(json))
            _actionAsset.RemoveAllBindingOverrides();
        else
            _actionAsset.LoadBindingOverridesFromJson(json);

        CacheAllActions();
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
