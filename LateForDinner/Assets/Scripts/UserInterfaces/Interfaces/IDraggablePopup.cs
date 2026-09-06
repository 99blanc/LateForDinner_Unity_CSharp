using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IDraggablePopup : IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private static readonly ConditionalWeakTable<IDraggablePopup, DragStateValue> _dragValues = new ConditionalWeakTable<IDraggablePopup, DragStateValue>();
    private class DragStateValue
    {
        public bool CanDrag = true;
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData data) 
    {
        if (this is not UIPopup popup || popup.RectTransform == null)
            return;

        var val = _dragValues.GetOrCreateValue(this);
        bool isOverInteractive = IsPointerOverInteractiveElement(popup, data);
        val.CanDrag = !isOverInteractive;
    }

    void IDragHandler.OnDrag(PointerEventData data)
    {
        if (this is not UIPopup popup || popup.RectTransform == null)
            return;

        var val = _dragValues.GetOrCreateValue(this);

        if (!val.CanDrag)
            return;

        float scaleFactor = Managers.UI.ScaleFactor;
        Vector2 nextPosition = popup.RectTransform.anchoredPosition + (data.delta / scaleFactor);
        nextPosition = ClampWithMargin(popup.RectTransform, nextPosition);
        popup.RectTransform.anchoredPosition = nextPosition;
    }

    void IEndDragHandler.OnEndDrag(PointerEventData data) 
    {
        var val = _dragValues.GetOrCreateValue(this);
        val.CanDrag = true;
    }

    private bool IsPointerOverInteractiveElement(UIPopup popup, PointerEventData data)
    {
        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, raycastResults);

        foreach (var result in raycastResults)
        {
            if (!result.gameObject.transform.IsChildOf(popup.transform))
                continue;

            if (result.gameObject == popup.gameObject || result.gameObject.name == Literal.Objects.BackgroundImage)
                continue;

            if (result.gameObject.GetComponentInParent<Selectable>() != null)
                return true;

            if (result.gameObject.GetComponentInParent<ScrollRect>() != null)
                return true;
        }
        return false;
    }

    private Vector2 ClampWithMargin(RectTransform rectTransform, Vector2 targetPosition)
    {
        Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
        RectTransform canvasRect = canvas.GetComponentAssert<RectTransform>();

        if (canvas == null || canvasRect == null)
            return targetPosition;

        Rect canvasRectArea = canvasRect.rect;
        Vector2 popupSize = rectTransform.rect.size;
        Vector2 pivot = rectTransform.pivot;
        float allowedOverflowX = popupSize.x * Define.Scaler.Margin;
        float allowedOverflowY = popupSize.y * Define.Scaler.Margin;
        float minX = canvasRectArea.xMin - allowedOverflowX + (popupSize.x * pivot.x);
        float maxX = canvasRectArea.xMax + allowedOverflowX - (popupSize.x * (1f - pivot.x));
        float minY = canvasRectArea.yMin - allowedOverflowY + (popupSize.y * pivot.y);
        float maxY = canvasRectArea.yMax + allowedOverflowY - (popupSize.y * (1f - pivot.y));
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        return targetPosition;
    }
}
