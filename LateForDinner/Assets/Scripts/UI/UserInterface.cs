using Cysharp.Text;
using R3;
using R3.Triggers;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Token.EVENT;

public abstract class UserInterface : MonoBehaviour
{
    private Dictionary<Type, Object[]> views = new();

    public virtual void Init() => views.Clear();

    protected void Bind<T>(Type type) where T : Object
    {
        var values = Enum.GetValues(type);
        var newView = new Object[values.Length];
        views.Add(typeof(T), newView);

        for (int index = 0; index < values.Length; ++index)
        {
            string childName = ZString.Concat(values.GetValue(index));
            newView[index] = typeof(T) == typeof(GameObject) ? gameObject.FindChild(childName, true) : gameObject.FindChild<T>(childName, true);
        }
    }

    protected T Get<T>(int index) where T : Object
    {
        if (views.TryGetValue(typeof(T), out Object[] newView))
        {
            if (index < 0 || index >= newView.Length)
                throw new();

            return newView[index] as T;
        }

        throw new();
    }

    protected void BindObject(Type type) => Bind<GameObject>(type);
    protected void BindImage(Type type) => Bind<Image>(type);
    protected void BindText(Type type) => Bind<TMP_Text>(type);
    protected void BindButton(Type type) => Bind<Button>(type);

    protected GameObject GetObject(int index) => Get<GameObject>(index);
    protected Image GetImage(int index) => Get<Image>(index);
    protected TMP_Text GetText(int index) => Get<TMP_Text>(index);
    protected Button GetButton(int index) => Get<Button>(index);

    public static void BindViewEvent(UIBehaviour view, Action<PointerEventData> action, ViewEvent type, Component component)
    {
        Observable<PointerEventData> observable = type switch
        {
            ViewEvent.ENTER => view.OnPointerEnterAsObservable(),
            ViewEvent.EXIT => view.OnPointerExitAsObservable(),
            ViewEvent.LEFT_CLICK => view.OnPointerClickAsObservable().Where(data => data.button == PointerEventData.InputButton.Left),
            ViewEvent.RIGHT_CLICK => view.OnPointerDownAsObservable().Where(data => data.button == PointerEventData.InputButton.Right),
            ViewEvent.LEFT_DOUBLE_CLICK => view.OnPointerClickAsObservable().Where(data => data.button == PointerEventData.InputButton.Left).Chunk(TimeSpan.FromSeconds(Define.Input.INPUT_DOUBLE_TAP_TIME), 2).Where(list => list.Length == 2).Select(list => list[1]),
            _ => throw new()
        };

        observable.Subscribe(action).AddTo(component);
    }

    public static void BindModelEvent<T>(ReactiveProperty<T> model, Action<T> action, Component component)
    {
        model.Subscribe(action).AddTo(component);
    }
}