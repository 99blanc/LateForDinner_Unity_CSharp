using R3;
using R3.Triggers;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public static class UIExtensions
{
    private static Observable<PointerEventData> Return(ViewEvent type)
    {
        Log.Error(Localization.Log_UIExtensions_NotImplementedEvent, true, type);
        return Observable.Empty<PointerEventData>();
    }

    public static void BindView(this UIBehaviour view, Action<PointerEventData> action, ViewEvent type, Component component)
    {
        Observable<PointerEventData> observable = type switch
        {
            ViewEvent.Enter => view.OnPointerEnterAsObservable(),
            ViewEvent.Exit => view.OnPointerExitAsObservable(),
            ViewEvent.LeftClick => view.OnPointerClickAsObservable().Where(data => data.button == PointerEventData.InputButton.Left),
            ViewEvent.RightClick => view.OnPointerDownAsObservable().Where(data => data.button == PointerEventData.InputButton.Right),
            ViewEvent.DoubleClick => view.OnPointerClickAsObservable().Chunk(TimeSpan.FromSeconds(0.25f), 2).Where(list => list.Length == 2).Select(list => list[1]),
            _ => Return(type)
        };
        observable.Subscribe(action).AddTo(component);
    }

    public static void BindModel<T>(this ReactiveProperty<T> model, Action<T> action, Component component)
        => model.Subscribe(action).AddTo(component);
}
