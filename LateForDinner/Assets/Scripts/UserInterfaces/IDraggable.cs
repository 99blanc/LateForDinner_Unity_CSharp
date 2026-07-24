using UnityEngine;
using UnityEngine.EventSystems;

public interface IDraggable : IDragHandler
{
    void IDragHandler.OnDrag(PointerEventData data)
    {
        if (this is UIPopup popup)
        {
            Vector2 factor = popup.Canvas != null ? new Vector2(popup.Canvas.scaleFactor, popup.Canvas.scaleFactor) : Vector2.one;
            popup.RectTransform.anchoredPosition += data.delta / factor;
        }
    }
}
