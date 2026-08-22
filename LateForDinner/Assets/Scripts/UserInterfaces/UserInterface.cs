using Cysharp.Text;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class UserInterface : MonoBehaviour, IPoolable
{
    private Dictionary<Type, UnityEngine.Object[]> _views = new Dictionary<Type, UnityEngine.Object[]>();
    private Dictionary<string, CancellationTokenSource> _tokens = new Dictionary<string, CancellationTokenSource>();
    private Vector2 _initialAnchoredPosition;
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
    {
        var _ = RectTransform;
        _views.Clear();
        _initialAnchoredPosition = RectTransform.anchoredPosition;
    }

    public virtual void Get()
        => RectTransform.anchoredPosition = _initialAnchoredPosition;

    public virtual void Refresh() { }

    public virtual void Release()
        => CancelAll();

    public virtual void Close()
        => Managers.UI.Close(this);

    protected void Bind<T>(Type type) where T : UnityEngine.Object
    {
        Array values = Enum.GetValues(type);
        UnityEngine.Object[] newView = new UnityEngine.Object[values.Length];
        _views.Add(typeof(T), newView);

        for (int index = 0; index < values.Length; index++)
        {
            string childName = ZString.Concat(values.GetValue(index));
            newView[index] = typeof(T) == typeof(GameObject)
            ? gameObject.FindChildAssert<Transform>(childName, true)
            : gameObject.FindChildAssert<T>(childName, true);
        }
    }

    protected T Get<T, TEnum>(TEnum element) where T : UnityEngine.Object where TEnum : Enum
    {
        if (!_views.TryGetValue(typeof(T), out var newView))
            return null;

        int index = Convert.ToInt32(element);

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
    protected void BindInputField(Type type)
        => Bind<TMP_InputField>(type);
    protected void BindButton(Type type)
        => Bind<Button>(type);
    protected void BindToggle(Type type)
        => Bind<Toggle>(type);
    protected void BindScrollRect(Type type)
        => Bind<ScrollRect>(type);
    protected void BindScrollbar(Type type)
        => Bind<Scrollbar>(type);
    protected void BindDropdown(Type type)
        => Bind<Dropdown>(type);
    protected void BindPanel(Type type)
        => Bind<CanvasGroup>(type);

    protected GameObject GetObject<TEnum>(TEnum element) where TEnum : Enum
        => Get<GameObject, TEnum>(element);
    protected Image GetImage<TEnum>(TEnum element) where TEnum : Enum
        => Get<Image, TEnum>(element);
    protected TMP_Text GetText<TEnum>(TEnum element) where TEnum : Enum
        => Get<TMP_Text, TEnum>(element);
    protected TMP_InputField GetInputField<TEnum>(TEnum element) where TEnum : Enum
        => Get<TMP_InputField, TEnum>(element);
    protected Button GetButton<TEnum>(TEnum element) where TEnum : Enum
        => Get<Button, TEnum>(element);
    protected Toggle GetToggle<TEnum>(TEnum element) where TEnum : Enum
        => Get<Toggle, TEnum>(element);
    protected ScrollRect GetScrollRect<TEnum>(TEnum element) where TEnum : Enum
        => Get<ScrollRect, TEnum>(element);
    protected Scrollbar GetScrollbar<TEnum>(TEnum element) where TEnum : Enum
        => Get<Scrollbar, TEnum>(element);
    protected Dropdown GetDropdown<TEnum>(TEnum element) where TEnum : Enum
        => Get<Dropdown, TEnum>(element);
    protected CanvasGroup GetPanel<TEnum>(TEnum element) where TEnum : Enum
        => Get<CanvasGroup, TEnum>(element);

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

    protected void CancelToken(string key)
    {
        if (!_tokens.TryGetValue(key, out var cts))
            return;

        cts?.Cancel();
        cts?.Dispose();
        _tokens.Remove(key);
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
}
