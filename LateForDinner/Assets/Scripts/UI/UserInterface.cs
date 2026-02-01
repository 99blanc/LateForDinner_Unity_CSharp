using Cysharp.Text;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public abstract class UserInterface : MonoBehaviour
{
    private Dictionary<Type, Object[]> views = new();

    public virtual void Init() => views.Clear();

    protected void Bind<T>(Type type) where T : Object
    {
        Array values = Enum.GetValues(type);
        Object[] newView = new Object[values.Length];
        views.Add(typeof(T), newView);

        for (int index = 0; index < values.Length; ++index)
        {
            string childName = ZString.Concat(values.GetValue(index));
            newView[index] = typeof(T) == typeof(GameObject) ? gameObject.FindChild(childName, true) : gameObject.FindChild<T>(childName, true);
        }
    }

    protected T Get<T>(int index) where T : Object
    {
        if (views.TryGetValue(typeof(T), out var newView))
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
}