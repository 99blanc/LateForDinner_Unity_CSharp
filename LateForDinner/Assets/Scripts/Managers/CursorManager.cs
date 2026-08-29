using R3;
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorManager
{
    private Texture2D _normalCursorTexture;
    private Texture2D _pressCursorTexture;
    private readonly Vector2 _cursorHotspot = Define.Cursor.Hotspot;

    private Vector2 _lastMousePosition = Vector2.negativeInfinity;
    private float _lastMouseMovedTime;
    private bool _isCursorVisible = true;

    public void Setup()
    {
        CacheCursorTextures();
        StartCursorUpdateLoop();
        _lastMouseMovedTime = Time.unscaledTime;
    }

    private void CacheCursorTextures()
    {
        var normalSprite = Managers.Resource.GetSprite(Define.Atlas.Common, Define.Sprite.Cursor_Normal);
        var pressSprite = Managers.Resource.GetSprite(Define.Atlas.Common, Define.Sprite.Cursor_Press);

        if (normalSprite == null || pressSprite == null)
            return;

        _normalCursorTexture = Managers.Resource.GetTextureFromSprite(normalSprite);
        _pressCursorTexture = Managers.Resource.GetTextureFromSprite(pressSprite);
    }

    private void StartCursorUpdateLoop()
    {
        Observable.EveryUpdate()
        .Subscribe(_ => UpdateCursorState());
    }

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

        HandleCursorVisibility(mousePosition);

        if (!_isCursorVisible)
            return;

        SetCursorVisual(Mouse.current != null && Mouse.current.leftButton.isPressed);
    }

    private bool CanUpdateCursorVisual() 
        => Application.isFocused && _normalCursorTexture != null && _pressCursorTexture != null;

    private Vector2 GetCurrentMousePosition() 
        => Mouse.current?.position.ReadValue() ?? Vector2.negativeInfinity;

    private bool IsMouseUnavailableOrOutOfBounds(Vector2 pos) 
        => Mouse.current == null || pos.x < 0 || pos.x > Screen.width || pos.y < 0 || pos.y > Screen.height;

    private void SetDefaultCursor() 
        => Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);

    private void SetCursorVisual(bool isPressed)
    {
        var targetTexture = isPressed ? _pressCursorTexture : _normalCursorTexture;
        Cursor.SetCursor(targetTexture, _cursorHotspot, CursorMode.ForceSoftware);
    }

    private void HandleCursorVisibility(Vector2 currentMousePosition)
    {
        if (_lastMousePosition == Vector2.negativeInfinity)
        {
            _lastMousePosition = currentMousePosition;
            _lastMouseMovedTime = Time.unscaledTime;
            return;
        }

        if (currentMousePosition != _lastMousePosition)
        {
            _lastMousePosition = currentMousePosition;
            _lastMouseMovedTime = Time.unscaledTime;

            if (!_isCursorVisible)
            {
                _isCursorVisible = true;
                Cursor.visible = true;
            }
        }
        else
        {
            if (_isCursorVisible && (Time.unscaledTime - _lastMouseMovedTime) >= Define.Cursor.Duration)
            {
                _isCursorVisible = false;
                Cursor.visible = false;
            }
        }
    }
}
