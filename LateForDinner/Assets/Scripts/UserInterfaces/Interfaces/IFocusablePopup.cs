using UnityEngine.EventSystems;

public interface IFocusablePopup : IPointerDownHandler
{
    void IPointerDownHandler.OnPointerDown(PointerEventData data)
    {
        if (this is not UIPopup popup)
            return;
        
        Managers.UI.FocusPopup(popup);
    }
}
