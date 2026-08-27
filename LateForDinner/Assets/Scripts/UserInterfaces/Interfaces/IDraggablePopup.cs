using UnityEngine;
using UnityEngine.EventSystems;

public interface IDraggablePopup : IDragHandler, IEndDragHandler
{
    void IDragHandler.OnDrag(PointerEventData data)
    {
        if (this is not UIPopup popup || popup.RectTransform == null)
            return;

        float scaleFactor = Managers.UI.ScaleFactor;
        Vector2 nextPosition = popup.RectTransform.anchoredPosition + (data.delta / scaleFactor);
        nextPosition = ClampWithMargin(popup.RectTransform, nextPosition);
        popup.RectTransform.anchoredPosition = nextPosition;
    }

    void IEndDragHandler.OnEndDrag(PointerEventData data) { }

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
