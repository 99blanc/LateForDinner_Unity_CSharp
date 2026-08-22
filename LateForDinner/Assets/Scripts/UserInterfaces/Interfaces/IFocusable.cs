using UnityEngine.EventSystems;

public interface IFocusable : IPointerDownHandler
{
    void IPointerDownHandler.OnPointerDown(PointerEventData data)
    {
        if (this is UIPopup popup)
            Managers.UI.FocusPopup(popup);
    }
}
