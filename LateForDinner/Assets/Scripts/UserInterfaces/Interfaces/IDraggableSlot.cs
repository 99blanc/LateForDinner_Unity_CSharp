using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IDraggableSlot<TTarget> : IDraggableSlotVariant, IBeginDragHandler, IDragHandler, IEndDragHandler where TTarget : Component
{
    private static readonly ConditionalWeakTable<IDraggableSlot<TTarget>, SlotDragState> _dragValues = new ConditionalWeakTable<IDraggableSlot<TTarget>, SlotDragState>();
    private class SlotDragState
    {
        public int SlotIndex = -1;
        public bool IsDragging = false;
        public UIGhostImagePopup GhostImage = null;
        public CanvasGroup CanvasGroup = null;
    }
    public int SlotIndex
    {
        get => _dragValues.GetOrCreateValue(this).SlotIndex;
        set => _dragValues.GetOrCreateValue(this).SlotIndex = value;
    }

    Sprite DragSprite { get; }
    SlotArea CurrentSlotArea { get; }

    void OnDropItem(TTarget targetSlot);

    void OnDropOutside() { }

    void IBeginDragHandler.OnBeginDrag(PointerEventData data)
    {
        if (this is not Component component || data.button != PointerEventData.InputButton.Left)
            return;

        var sprite = DragSprite;
        if (sprite == null)
            return;

        var state = _dragValues.GetOrCreateValue(this);

        if (state.CanvasGroup == null)
            state.CanvasGroup = component.GetComponentAssert<CanvasGroup>();

        if (state.CanvasGroup != null)
        {
            state.CanvasGroup.alpha = 0.5f;
            state.CanvasGroup.blocksRaycasts = false;
        }

        state.IsDragging = true;
        var ghostPopup = Managers.UI.OpenPopup<UIGhostImagePopup>();

        if (ghostPopup != null)
        {
            state.GhostImage = ghostPopup;
            ghostPopup.SetItemImage(sprite);
            ghostPopup.RectTransform.position = data.position;
        }
    }

    void IDragHandler.OnDrag(PointerEventData data)
    {
        if (!_dragValues.TryGetValue(this, out var state) || !state.IsDragging)
            return;

        if (state.GhostImage != null)
            state.GhostImage.RectTransform.position = data.position;
    }

    void IEndDragHandler.OnEndDrag(PointerEventData data)
    {
        if (!_dragValues.TryGetValue(this, out var state) || !state.IsDragging)
            return;

        if (state.CanvasGroup != null)
        {
            state.CanvasGroup.alpha = 1f;
            state.CanvasGroup.blocksRaycasts = true;
        }

        TTarget targetSlot = null;
        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, raycastResults);

        foreach (var result in raycastResults)
        {
            var slot = result.gameObject.GetComponentInParent<TTarget>();

            if (slot != null && slot != (Component)this)
            {
                targetSlot = slot;
                break;
            }
        }

        if (state.GhostImage != null)
        {
            Managers.UI.Close(state.GhostImage);
            state.GhostImage = null;
        }

        if (targetSlot != null)
            OnDropItem(targetSlot);
        else
            OnDropOutside();

        state.IsDragging = false;
    }

    void IDraggableSlotVariant.Reset()
    {
        if (_dragValues.TryGetValue(this, out var state))
        {
            if (state.CanvasGroup != null)
            {
                state.CanvasGroup.alpha = 1f;
                state.CanvasGroup.blocksRaycasts = true;
            }

            if (state.GhostImage != null)
            {
                Managers.UI.Close(state.GhostImage);
                state.GhostImage = null;
            }

            state.IsDragging = false;
            state.SlotIndex = -1;
        }
    }
}
