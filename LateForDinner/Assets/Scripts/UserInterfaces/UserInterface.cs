using Cysharp.Text;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    private Dictionary<Type, UnityEngine.Object[]> _views = new Dictionary<Type, UnityEngine.Object[]>();

    public virtual void Init() 
        => _views.Clear();

    protected void Bind<T>(Type type) where T : UnityEngine.Object
    {
        Array values = Enum.GetValues(type);
        UnityEngine.Object[] newView = new UnityEngine.Object[values.Length];
        _views.Add(typeof(T), newView);

        for (int index = 0; index < values.Length; index++)
        {
            string childName = ZString.Concat(values.GetValue(index));
            newView[index] = typeof(T) == typeof(GameObject) ? gameObject.FindChildAssert<Transform>(childName, true) : gameObject.FindChildAssert<T>(childName, true);
        }
    }

    protected T Get<T>(int index) where T : UnityEngine.Object
    {
        bool isNotFound = !_views.TryGetValue(typeof(T), out var newView);
        Log.Error(Localization.Log_UserInterface_BindingNotFound, isNotFound, typeof(T).Name);

        if (isNotFound)
            return null;

        bool isOutOfRange = index < 0 || index >= newView.Length;
        Log.Error(Localization.Log_UserInterface_OutOfRange, isOutOfRange, index, newView.Length);

        if (isOutOfRange)
            return null;

        return newView[index] as T;
    }

    protected void BindObject(Type type) 
        => Bind<GameObject>(type);
      
    protected void BindImage(Type type) 
        => Bind<Image>(type);

    protected void BindText(Type type) 
        => Bind<TMP_Text>(type);

    protected void BindButton(Type type) 
        => Bind<Button>(type);

    protected GameObject GetObject(int index) 
        => Get<GameObject>(index);

    protected Image GetImage(int index) 
        => Get<Image>(index);

    protected TMP_Text GetText(int index) 
        => Get<TMP_Text>(index);

    protected Button GetButton(int index) 
        => Get<Button>(index);
}
