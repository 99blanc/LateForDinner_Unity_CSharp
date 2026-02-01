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

    public static T GetOrAddComponentAssert<T>(this GameObject gameObject) where T : Component
    {
        if (!gameObject.TryGetComponent<T>(out var component))
            component = gameObject.AddComponent<T>();

        Debug.Assert(component is not null);
        return component;
    }

    public static T GetOrAddComponentAssert<T>(this Transform transform) where T : Component
    {
        if (!transform.TryGetComponent<T>(out var component))
            component = transform.AddComponent<T>();

        Debug.Assert(component is not null);
        return component;
    }


    public static T GetComponentAssert<T>(this GameObject gameObject)
    {
        T component = gameObject.GetComponent<T>();
        Debug.Assert(component is not null);
        return component;
    }

    public static T GetComponentAssert<T>(this Transform transform)
    {
        T component = transform.GetComponent<T>();
        Debug.Assert(component is not null);
        return component;
    }

    public static T[] GetComponentsAssert<T>(this GameObject gameObject)
    {
        T[] components = gameObject.GetComponents<T>();
        Debug.Assert(components is not null && components.Length > 0);
        return components;
    }

    public static T[] GetComponentsAssert<T>(this Transform transform)
    {
        T[] components = transform.GetComponents<T>();
        Debug.Assert(components is not null && components.Length > 0);
        return components;
    }
}