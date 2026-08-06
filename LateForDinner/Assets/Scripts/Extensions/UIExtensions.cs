using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class UIExtensions
{
    public static void BindModel<T>(this ReactiveProperty<T> model, Action<T> action, Component component)
        => model.Subscribe(action).AddTo(component);

    public static void BindView(this UIBehaviour view, Action<PointerEventData> action, ViewEvent type, Component component, ReactiveProperty<ButtonState> prop = null)
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
        observable.Where(_ => Disable(prop)).Subscribe(action).AddTo(component);
    }

    private static Observable<PointerEventData> Return(ViewEvent type)
        => Observable.Empty<PointerEventData>();

    private static bool Disable(ReactiveProperty<ButtonState> prop)
    {
        if (prop == null)
            return true;

        return prop.Value != ButtonState.Disable;
    }

    public static void BindViewAsButton(this UIBehaviour view, Action<PointerEventData> action, ViewEvent type, Component component, ReactiveProperty<ButtonState> prop, Action onReset = null)
    {
        Action onAction = onReset ?? (() => prop.Value = ButtonState.Normal);
        view.BindView(_ => prop.Value = ButtonState.Highlight, ViewEvent.Enter, component, prop);
        view.BindView(_ => prop.Value = ButtonState.Press, ViewEvent.Press, component, prop);
        view.BindView(_ => onAction(), ViewEvent.Release, component, prop);
        view.BindView(_ => onAction(), ViewEvent.Exit, component, prop);
        view.BindView(action, type, component, prop);
    }

    public static void BindState(this Image targetImage, ReadOnlyReactiveProperty<ButtonState> prop, string atlas, Component component)
    {
        if (targetImage == null)
            return;

        prop.Subscribe(state =>
        {
            string name = state switch
            {
                ButtonState.New => Define.Sprite.Button_New,
                ButtonState.Highlight => Define.Sprite.Button_Highlight,
                ButtonState.Press => Define.Sprite.Button_Press,
                ButtonState.Disable => Define.Sprite.Button_Disable,
                _ => Define.Sprite.Button_Normal
            };
            targetImage.sprite = Managers.Resource.GetSprite(atlas, name);
        }).AddTo(component);
    }

    public static void BindStateAsArrow(this Image targetImage, ReadOnlyReactiveProperty<ButtonState> prop, string atlas, Component component)
    {
        if (targetImage == null)
            return;

        prop.Subscribe(state =>
        {
            string name = state switch
            {
                ButtonState.Highlight => Define.Sprite.Button_Arrow_Highlight,
                ButtonState.Press => Define.Sprite.Button_Arrow_Press,
                ButtonState.Disable => Define.Sprite.Button_Arrow_Disable,
                _ => Define.Sprite.Button_Arrow_Normal
            };

            targetImage.sprite = Managers.Resource.GetSprite(atlas, name);
        }).AddTo(component);
    }

    public static async UniTask Lock(this UniTask task)
        => await Managers.UI.LockAsync(task);

    public static string ToSpriteAsMealTime(this MealTime mealTime)
    {
        return mealTime switch
        {
            MealTime.Lunch => Define.Sprite.MealTime_Lunch,
            MealTime.Dinner => Define.Sprite.MealTime_Dinner,
            _ => Define.Sprite.MealTime_Breakfast
        };
    }

    public static async UniTask Release<T>(this UniTask<T> task) where T : UserInterface
    {
        var user = await task;

        if (user != null)
            user.Release();
    }

    public static async UniTask Release(this UniTask task)
        => await task;
}
