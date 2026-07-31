using Cysharp.Text;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    private Dictionary<Type, UnityEngine.Object[]> _views = new Dictionary<Type, UnityEngine.Object[]>();
    private Dictionary<string, CancellationTokenSource> _tokens = new Dictionary<string, CancellationTokenSource>();
    private RectTransform _rectTransform;
    public RectTransform RectTransform
    {
        get
        {
            if (_rectTransform == null)
                _rectTransform = gameObject.GetComponentAssert<RectTransform>();

            return _rectTransform;
        }
    }

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
        if (!_views.TryGetValue(typeof(T), out var newView))
            return null;

        if (index < 0 || index >= newView.Length)
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

    protected void BindCanvasGroup(Type type)
        => Bind<CanvasGroup>(type);

    protected GameObject GetObject(int index) 
        => Get<GameObject>(index);

    protected Image GetImage(int index) 
        => Get<Image>(index);

    protected TMP_Text GetText(int index) 
        => Get<TMP_Text>(index);

    protected Button GetButton(int index) 
        => Get<Button>(index);

    protected CanvasGroup GetCanvasGroup(int index)
        => Get<CanvasGroup>(index);

    protected CancellationToken GetToken(string key)
    {
        if (_tokens.TryGetValue(key, out var cts))
        {
            cts?.Cancel();
            cts?.Dispose();
        }

        var cancel = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        _tokens[key] = cancel;
        return cancel.Token;
    }

    private void CancelAll()
    {
        foreach (var cts in _tokens.Values)
        {
            cts?.Cancel();
            cts?.Dispose();
        }

        _tokens.Clear();
    }

    protected virtual void OnDisable()
        => CancelAll();

    protected virtual void OnDestroy()
        => CancelAll();
}
