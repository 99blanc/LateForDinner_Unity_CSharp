using Cysharp.Text;
using UnityEngine;

public static class UnityExtensions
{
    public static T FindChild<T>(this GameObject gameObject, string name = null, bool recursive = false) where T : Object
    {
        if (!gameObject) 
            return null;

        if (recursive)
        {
            T[] components = gameObject.GetComponentsInChildren<T>(true);

            for (int index = 0; index < components.Length; index++)
            {
                T component = components[index];

                if (string.IsNullOrEmpty(name) || ZString.Equals(name, component.name))
                    return component;
            }
        }
        else
        {
            for (int index = 0; index < gameObject.transform.childCount; index++)
            {
                Transform child = gameObject.transform.GetChild(index);

                if (!string.IsNullOrEmpty(name) && !ZString.Equals(name, child.name))
                    continue;

                if (child.TryGetComponent<T>(out var component))
                    return component;
            }
        }

        return null;
    }

    public static GameObject FindChild(this GameObject gameObject, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(gameObject, name, recursive);

        return transform != null ? transform.gameObject : null;
    }

    public static T FindChildAssert<T>(this GameObject gameObject, string name = null, bool recursive = false) where T : Object
    {
        T target = FindChild<T>(gameObject, name, recursive);
        Log.Error(Localization.Log_UnityExtensions_FindChildFailed, target == null, gameObject != null ? gameObject.name : "Null", string.IsNullOrEmpty(name) ? "Target" : name, typeof(T).Name);

        return target;
    }

    public static GameObject FindChildAssert(this GameObject gameObject, string name = null, bool recursive = false)
    {
        Transform target = FindChild<Transform>(gameObject, name, recursive);
        Log.Error(Localization.Log_UnityExtensions_FindChildFailed, target == null, gameObject != null ? gameObject.name : "Null", string.IsNullOrEmpty(name) ? "Target" : name, nameof(GameObject));

        return target != null ? target.gameObject : null;
    }

    public static T GetComponentAssert<T>(this Component component) where T : Component
    {
        T newComponent = component.GetComponent<T>();
        Log.Error(Localization.Log_UnityExtensions_GetComponentFailed, newComponent == null, component != null ? component.name : "Null", typeof(T).Name);

        return newComponent;
    }

    public static T GetComponentAssert<T>(this GameObject gameObject) where T : Component
        => gameObject.GetComponentAssert<T>();
}
