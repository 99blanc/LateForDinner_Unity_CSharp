using Cysharp.Text;
using UnityEngine;

public static class UnityExtensions
{
    public static T FindChild<T>(this GameObject gameObject, string name = null, bool recursive = false) where T : Object
    {
        if (!gameObject)
            return null;

        if (recursive)
            return FindChildRecursive<T>(gameObject, name);

        return FindChildDirect<T>(gameObject, name);
    }

    private static T FindChildRecursive<T>(GameObject gameObject, string name) where T : Object
    {
        var components = gameObject.GetComponentsInChildren<T>(true);

        for (int index = 0; index < components.Length; index++)
        {
            var component = components[index];

            if (string.IsNullOrEmpty(name) || ZString.Equals(name, component.name))
                return component;
        }

        return null;
    }

    private static T FindChildDirect<T>(GameObject gameObject, string name) where T : Object
    {
        var transform = gameObject.transform;

        for (int index = 0; index < transform.childCount; index++)
        {
            var child = transform.GetChild(index);

            if (!string.IsNullOrEmpty(name) && !ZString.Equals(name, child.name))
                continue;

            if (child.TryGetComponent<T>(out var component))
                return component;
        }

        return null;
    }

    public static GameObject FindChild(this GameObject gameObject, string name = null, bool recursive = false)
    {
        var transform = FindChild<Transform>(gameObject, name, recursive);
        return transform != null ? transform.gameObject : null;
    }

    public static T FindChildAssert<T>(this GameObject gameObject, string name = null, bool recursive = false) where T : Object 
        => FindChild<T>(gameObject, name, recursive);

    public static GameObject FindChildAssert(this GameObject gameObject, string name = null, bool recursive = false)
    {
        var target = FindChild<Transform>(gameObject, name, recursive);
        return target != null ? target.gameObject : null;
    }

    public static T GetComponentAssert<T>(this Component component) where T : Component 
        => component.GetComponent<T>();

    public static T GetComponentAssert<T>(this GameObject gameObject) where T : Component 
        => gameObject.GetComponent<T>();
}
