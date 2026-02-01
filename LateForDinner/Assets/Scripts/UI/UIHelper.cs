using R3;
using R3.Triggers;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Token.EVENT;

public class UIHelper
{
    public static void BindViewEvent(UIBehaviour view, Action<PointerEventData> action, ViewEvent type, Component component)
    {
        Observable<PointerEventData> observable = type switch
        {
            ViewEvent.ENTER => view.OnPointerEnterAsObservable(),
            ViewEvent.EXIT => view.OnPointerExitAsObservable(),
            ViewEvent.LEFT_CLICK => view.OnPointerClickAsObservable().Where(data => data.button == PointerEventData.InputButton.Left),
            ViewEvent.RIGHT_CLICK => view.OnPointerDownAsObservable().Where(data => data.button == PointerEventData.InputButton.Right),
            ViewEvent.LEFT_DOUBLE_CLICK => view.OnPointerClickAsObservable().Where(data => data.button == PointerEventData.InputButton.Left).Chunk(TimeSpan.FromSeconds(Define.Physics.TAP_INTERVAL), 2).Where(list => list.Length == 2).Select(list => list[1]),
            _ => throw new()
        };
        observable.Subscribe(action).AddTo(component);
    }

    public static void BindModelEvent<T>(ReactiveProperty<T> model, Action<T> action, Component component)
    {
        model.Subscribe(action).AddTo(component);
    }
}
