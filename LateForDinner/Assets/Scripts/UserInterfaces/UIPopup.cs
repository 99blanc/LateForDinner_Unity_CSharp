using R3;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIPopup : UserInterface, IPointerDownHandler
{
    private IDisposable _cancel;
    private Canvas _canvas;
    private RectTransform _rectTransform;
    public Canvas Canvas
    {
        get 
        { 
            if (_canvas == null)
                _canvas = gameObject.GetComponentAssert<Canvas>();

            return _canvas; 
        }
    }
    public RectTransform RectTransform
    {
        get
        {
            if (_rectTransform == null)
                _rectTransform = gameObject.GetComponentAssert<RectTransform>();

            return _rectTransform;
        }
    }

    public override void Init()
        => base.Init();

    private void OnEnable()
    {
        var action = Managers.Config?.ActionAsset;

        if (action != null)
        {
            var cancel = action.FindAction(Literal.Hotkeys.Cancel);

            if (cancel != null)
            {
                _cancel = Observable.FromEvent<InputAction.CallbackContext>(h => cancel.performed += h, h => cancel.performed -= h).Subscribe(_ =>
                {
                    Close();
                });
            }
        }
    }

    private void OnDisable()
    {
        _cancel?.Dispose();
        _cancel = null;
    }

    public virtual void Close() 
        => Managers.UI.ClosePopup(this);

    public virtual void OnPointerDown(PointerEventData data)
        => Managers.UI.Focus(this);
}
