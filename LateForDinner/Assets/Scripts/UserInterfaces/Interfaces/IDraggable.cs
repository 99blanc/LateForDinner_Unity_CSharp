using UnityEngine.EventSystems;

public interface IDraggable : IDragHandler
{
    void IDragHandler.OnDrag(PointerEventData data)
    {
        if (this is UIPopup popup)
        {
            if (popup.RectTransform == null)
                return;

            float scaleFactor = Managers.UI.ScaleFactor;
            popup.RectTransform.anchoredPosition += data.delta / scaleFactor;
        }
    }
}
