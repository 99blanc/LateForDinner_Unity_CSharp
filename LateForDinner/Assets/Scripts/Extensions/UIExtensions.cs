using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public static class UIExtensions
{
    public static void BindModel<T>(this ReactiveProperty<T> model, Action<T> action, IPoolable component)
        => model.Subscribe(action).RegisterToPool(component);

    public static void BindView(this UIBehaviour view, Action<PointerEventData> action, ViewEvent type, IPoolable component, ReactiveProperty<ButtonState> prop = null)
    {
        Observable<PointerEventData> observable = type switch
        {
            ViewEvent.Enter => view.OnPointerEnterAsObservable(),
            ViewEvent.Exit => view.OnPointerExitAsObservable(),
            ViewEvent.Press => view.OnPointerDownAsObservable(),
            ViewEvent.Release => view.OnPointerUpAsObservable(),
            ViewEvent.LeftClick => view.OnPointerClickAsObservable().Where(data => data.button == PointerEventData.InputButton.Left),
            ViewEvent.RightClick => view.OnPointerClickAsObservable().Where(data => data.button == PointerEventData.InputButton.Right),
            ViewEvent.DoubleClick => view.OnPointerClickAsObservable().Chunk(TimeSpan.FromSeconds(0.25f), 2).Where(list => list.Length == 2).Select(list => list[1]),
            _ => Return(type)
        };
        observable.Where(_ => Disable(prop)).Subscribe(action).RegisterToPool(component);
    }

    public static void BindViewForToggle(this UIBehaviour view, Action<PointerEventData> action, ViewEvent type, IPoolable component)
    {
        Observable<PointerEventData> observable = type switch
        {
            ViewEvent.Enter => view.OnPointerEnterAsObservable(),
            ViewEvent.Exit => view.OnPointerExitAsObservable(),
            ViewEvent.Press => view.OnPointerDownAsObservable(),
            ViewEvent.Release => view.OnPointerUpAsObservable(),
            ViewEvent.LeftClick => view.OnPointerClickAsObservable().Where(data => data.button == PointerEventData.InputButton.Left),
            ViewEvent.RightClick => view.OnPointerClickAsObservable().Where(data => data.button == PointerEventData.InputButton.Right),
            ViewEvent.DoubleClick => view.OnPointerClickAsObservable().Chunk(TimeSpan.FromSeconds(Define.Scaler.Threshold), 2).Where(list => list.Length == 2).Select(list => list[1]),
            _ => Return(type)
        };
        observable.Subscribe(action).RegisterToPool(component);
    }

    private static Observable<PointerEventData> Return(ViewEvent type)
        => Observable.Empty<PointerEventData>();

    private static bool Disable(ReactiveProperty<ButtonState> prop)
    {
        if (prop == null)
            return true;

        return prop.Value != ButtonState.Disable;
    }

    public static void BindViewAsButton(this UIBehaviour view, Action<PointerEventData> action, ViewEvent type, IPoolable component, ReactiveProperty<ButtonState> prop, Func<bool> stayCondition = null)
    {
        Action onReset = () =>
        {
            if (stayCondition != null && stayCondition())
                return;

            prop.Value = ButtonState.Normal;
        };
        view.BindView(_ => prop.Value = ButtonState.Highlight, ViewEvent.Enter, component, prop);
        view.BindView(_ => prop.Value = ButtonState.Press, ViewEvent.Press, component, prop);
        view.BindView(_ => onReset(), ViewEvent.Release, component, prop);
        view.BindView(_ => onReset(), ViewEvent.Exit, component, prop);
        view.BindView(action, type, component, prop);
    }

    public static void BindViewAsToggle(this UIBehaviour view, Action<PointerEventData> action, ViewEvent type, IPoolable component, ReactiveProperty<ButtonState> prop, Func<bool> stayCondition = null)
    {
        Action onReset = () =>
        {
            if (stayCondition != null && stayCondition())
            {
                prop.Value = ButtonState.Disable;
                return;
            }
            prop.Value = ButtonState.Normal;
        };
        view.BindView(_ =>
        {
            if (stayCondition != null && stayCondition())
                return;
            prop.Value = ButtonState.Highlight;
        }, ViewEvent.Enter, component, prop);
        view.BindView(_ =>
        {
            if (stayCondition != null && stayCondition())
                return;
            prop.Value = ButtonState.Press;
        }, ViewEvent.Press, component, prop);
        view.BindView(_ => onReset(), ViewEvent.Release, component, prop);
        view.BindView(_ => onReset(), ViewEvent.Exit, component, prop);
        view.BindViewForToggle(action, type, component);
    }

    public static void BindState(this Image targetImage, ReadOnlyReactiveProperty<ButtonState> prop, string atlas, IPoolable component)
    {
        if (targetImage == null)
            return;

        prop.Subscribe(state =>
        {
            string name = state switch
            {
                ButtonState.Normal => Define.Sprite.Button_Normal,
                ButtonState.New => Define.Sprite.Button_New,
                ButtonState.Highlight => Define.Sprite.Button_Highlight,
                ButtonState.Press => Define.Sprite.Button_Press,
                ButtonState.Disable => Define.Sprite.Button_Disable,
                _ => Define.Sprite.Button_Normal
            };
            targetImage.sprite = Managers.Resource.GetSprite(atlas, name);
        }).RegisterToPool(component);
    }

    public static void BindStateAsArrow(this Image targetImage, ReadOnlyReactiveProperty<ButtonState> prop, string atlas, IPoolable component)
    {
        if (targetImage == null)
            return;

        prop.Subscribe(state =>
        {
            string name = state switch
            {
                ButtonState.Normal => Define.Sprite.Button_Arrow_Normal,
                ButtonState.Highlight => Define.Sprite.Button_Arrow_Highlight,
                ButtonState.Press => Define.Sprite.Button_Arrow_Press,
                ButtonState.Disable => Define.Sprite.Button_Arrow_Disable,
                _ => Define.Sprite.Button_Arrow_Normal
            };

            targetImage.sprite = Managers.Resource.GetSprite(atlas, name);
        }).RegisterToPool(component);
    }

    public static void BindScrollbar(this Scrollbar scrollbar, Action<float> action, IPoolable component)
    {
        if (scrollbar == null)
            return;

        action(scrollbar.value);
        scrollbar.OnValueChangedAsObservable().Subscribe(action).RegisterToPool(component);
    }

    public static void BindInputField(this TMP_InputField inputField, Action<string> action, IPoolable component)
    {
        if (inputField == null)
            return;

        action(inputField.text);
        inputField.OnValueChangedAsObservable().Subscribe(action).RegisterToPool(component);
    }

    public static void BindInputEndEdit(this TMP_InputField inputField, Action<string> action, IPoolable component)
    {
        if (inputField == null)
            return;

        inputField.OnEndEditAsObservable().Subscribe(action).RegisterToPool(component);
    }

    public static void BindInputSubmit(this TMP_InputField inputField, Action<string> action, IPoolable component)
    {
        if (inputField is InputField customInput)
        {
            customInput.OnSubmitAction = action;
            Disposable.Create(() => customInput.OnSubmitAction = null).RegisterToPool(component);
        }
    }

    public static void SetVisual(this Image boxImage, Image toggleImage = null, Scrollbar scrollbar = null, bool isEnabled = true)
    {
        Color targetColor = isEnabled ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);

        if (boxImage != null)
            boxImage.color = targetColor;

        if (toggleImage != null)
            toggleImage.color = targetColor;

        if (scrollbar != null)
        {
            scrollbar.interactable = isEnabled;

            if (scrollbar.TryGetComponent<Image>(out var barImage))
                barImage.color = targetColor;

            if (scrollbar.targetGraphic is Graphic bgGraphic)
                bgGraphic.color = targetColor;

            if (scrollbar.handleRect != null && scrollbar.handleRect.TryGetComponent<Image>(out var handleImage))
                handleImage.color = targetColor;
        }
    }
}
