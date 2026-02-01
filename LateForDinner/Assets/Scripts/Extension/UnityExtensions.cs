using Cysharp.Text;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

public static class UnityExtensions
{
    public static readonly List<Component> Caches = new(64);

    public static T FindChild<T>(this GameObject gameObject, string name = null, bool recursive = false) where T : Object
    {
        if (!gameObject)
            throw new();

        if (recursive)
        {
            lock (Caches)
            {
                Caches.Clear();
                gameObject.GetComponentsInChildren<T>(true, (List<T>)(object)Caches);

                for (int index = 0; index < Caches.Count; ++index)
                {
                    Component component = Caches[index];

                    if (string.IsNullOrEmpty(name) || ZString.Equals(name, Caches[index].name))
                        return component as T;
                }

                throw new();
            }
        }
        else
        {
            for (int index = 0; index < gameObject.transform.childCount; ++index)
            {
                Transform child = gameObject.transform.GetChild(index);

                if (!string.IsNullOrEmpty(name) && !ZString.Equals(name, child.name))
                    continue;

                if (child.TryGetComponent<T>(out var component))
                    return component;
            }
        }

        throw new();
    }

    public static GameObject FindChild(this GameObject gameObject, string name = null, bool recursive = false) => FindChild<Transform>(gameObject, name, recursive).gameObject;

    public static Transform FindAssert(this Transform transform, string name)
    {
        Transform newTransform = transform.Find(name);
        Debug.Assert(newTransform);
        return newTransform;
    }

    public static T GetOrAddComponentAssert<T>(this Component source) where T : Component
    {
        bool hasComponent = source.TryGetComponent<T>(out var component);

        if (!hasComponent) 
            component = source.gameObject.AddComponent<T>();

        Debug.Assert(component is not null);
        return component;
    }

    public static T GetComponentAssert<T>(this Component source)
    {
        T component = source.GetComponent<T>();
        Debug.Assert(component is not null);
        return component;
    }

    public static T[] GetComponentsAssert<T>(this Component source)
    {
        T[] components = source.GetComponents<T>();
        bool hasComponent = components is not null && components.Length > 0;
        Debug.Assert(hasComponent);
        return components;
    }

    public static T GetOrAddComponentAssert<T>(this GameObject gameObject) where T : Component => gameObject.transform.GetOrAddComponentAssert<T>();

    public static T GetComponentAssert<T>(this GameObject gameObject) => gameObject.transform.GetComponentAssert<T>();

    public static T[] GetComponentsAssert<T>(this GameObject gameObject) => gameObject.transform.GetComponentsAssert<T>();
}