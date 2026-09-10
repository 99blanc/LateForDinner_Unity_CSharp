using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CursorManager
{
    private RectTransform _rectTransform;
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
    private Image _cursorImage;
    private Canvas _canvas;
    private Sprite _normalCursorSprite;
    private Sprite _pressCursorSprite;
    private bool _isCursorVisible = true;

    public GameObject InitRoot()
    {
        _root = new GameObject(Literal.Roots.Cursor, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        _root.transform.SetParent(Managers.Instance.transform, false);
        return _root;
    }

    public void Setup()
    {
        var _ = Root;
        CacheCursorSprites();
        CreateCursorUI();
        StartCursorUpdateLoop();
    }

    private void CacheCursorSprites()
    {
        _normalCursorSprite = Managers.Resource.GetSprite(Define.Atlas.Common, Define.Sprite.Cursor_Normal);
        _pressCursorSprite = Managers.Resource.GetSprite(Define.Atlas.Common, Define.Sprite.Cursor_Press);
    }

    private void CreateCursorUI()
    {
        _canvas = _root.GetComponentAssert<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.overrideSorting = true;
        _canvas.sortingOrder = (int)LayerType.Cursor;
        var scaler = _root.GetComponentAssert<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = Define.Scaler.Resolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Shrink;
        scaler.referencePixelsPerUnit = Define.Scaler.PixelsPerUnit;
        var cursor = new GameObject(Literal.Objects.Cursor, typeof(Image));
        cursor.transform.SetParent(_root.transform, false);
        _rectTransform = cursor.GetComponent<RectTransform>();
        _rectTransform.sizeDelta = Define.Cursor.Size;
        Vector2 size = Define.Cursor.Size;
        Vector2 hotspot = Define.Cursor.Hotspot;
        _rectTransform.pivot = new Vector2(hotspot.x / size.x, 1f - (hotspot.y / size.y));
        _cursorImage = cursor.GetComponent<Image>();
        _cursorImage.sprite = _normalCursorSprite;
        _cursorImage.raycastTarget = false;
        Cursor.visible = false;
    }

    public void UpdateCursorLockState(FullScreenMode screenMode)
    {
        switch (screenMode)
        {
            case FullScreenMode.ExclusiveFullScreen:
                Cursor.lockState = CursorLockMode.Confined;
                break;

            case FullScreenMode.FullScreenWindow:
            case FullScreenMode.Windowed:
                Cursor.lockState = CursorLockMode.None;
                break;
        }
    }

    private void StartCursorUpdateLoop()
    {
        Observable.EveryUpdate()
        .Subscribe(_ => UpdateCursorState());
    }

    private void UpdateCursorState()
    {
        if (!Application.isFocused || Mouse.current == null || _rectTransform == null)
        {
            SetCursorVisibility(false);
            return;
        }

        Vector2 mousePosition = GetCurrentMousePosition();

        if (IsMouseOutOfBounds(mousePosition))
        {
            SetCursorVisibility(false);
            return;
        }

        if (!_isCursorVisible)
            SetCursorVisibility(true);

        bool isClicked = Mouse.current.leftButton.isPressed;
        UpdateCursorPosition(mousePosition);
        SetCursorVisual(isClicked);
    }

    private void UpdateCursorPosition(Vector2 mousePosition)
    {
        if (_canvas == null)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.transform as RectTransform, mousePosition, _canvas.worldCamera, out Vector2 localPoint);
        _rectTransform.anchoredPosition = localPoint;
    }

    private Vector2 GetCurrentMousePosition()
        => Mouse.current?.position.ReadValue() ?? Vector2.negativeInfinity;

    private bool IsMouseOutOfBounds(Vector2 position)
    {
        if (position.x < 0 || position.y < 0)
            return true;

        if (position.x >= Screen.width || position.y >= Screen.height)
            return true;

        return false;
    }

    private void SetCursorVisual(bool isPressed)
    {
        if (_cursorImage == null)
            return;

        _cursorImage.sprite = isPressed ? _pressCursorSprite : _normalCursorSprite;
    }

    private void SetCursorVisibility(bool isVisible)
    {
        _isCursorVisible = isVisible;

        if (_cursorImage != null)
            _cursorImage.gameObject.SetActive(isVisible);

        Cursor.visible = !isVisible;
    }
}
