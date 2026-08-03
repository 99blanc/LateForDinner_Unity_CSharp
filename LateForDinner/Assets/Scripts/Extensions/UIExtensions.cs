using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class UIExtensions
{
    private static Observable<PointerEventData> Return(ViewEvent type)
        => Observable.Empty<PointerEventData>();

    public static string ToSpriteAsMealTime(this MealTime mealTime)
    {
        return mealTime switch
        {
            MealTime.Lunch => Define.Sprite.MealTime_Lunch,
            MealTime.Dinner => Define.Sprite.MealTime_Dinner,
            _ => Define.Sprite.MealTime_Breakfast
        };
    }

    public static void BindState(this UIBehaviour view, ReactiveProperty<ButtonState> prop, Action onClick, Component component, Action onResetState = null)
    {
        view.BindView(_ => prop.Value = ButtonState.Highlight, ViewEvent.Enter, component, prop);
        view.BindView(_ => prop.Value = ButtonState.Press, ViewEvent.Press, component, prop);
        Action resetAction = onResetState ?? (() => prop.Value = ButtonState.Normal);
        view.BindView(_ => resetAction(), ViewEvent.Release, component, prop);
        view.BindView(_ => resetAction(), ViewEvent.Exit, component, prop);
        view.BindView(_ => onClick?.Invoke(), ViewEvent.LeftClick, component, prop);
    }

    public static void BindButton(this Image targetImage, ReadOnlyReactiveProperty<ButtonState> stateProp, string atlas, Component component)
    {
        // TODO ::: async / await 패턴 더 안전하게 사용
        stateProp.Subscribe(async state =>
        {
            string name = state switch
            {
                ButtonState.New => Define.Sprite.Button_New,
                ButtonState.Highlight => Define.Sprite.Button_Highlight,
                ButtonState.Press => Define.Sprite.Button_Press,
                ButtonState.Disable => Define.Sprite.Button_Disable,
                _ => Define.Sprite.Button_Normal
            };

            Sprite sprite = await Managers.Resource.LoadSpriteAsync(atlas, name);

            if (targetImage != null && name != null)
                targetImage.sprite = sprite;
        }).AddTo(component);
    }

    public static void BindArrowButton(this Image targetImage, ReadOnlyReactiveProperty<ButtonState> stateProp, string atlas, Component component)
    {
        // TODO ::: async / await 패턴 더 안전하게 사용
        stateProp.Subscribe(async state =>
        {
            string name = state switch
            {
                ButtonState.Highlight => Define.Sprite.Button_Arrow_Highlight,
                ButtonState.Press => Define.Sprite.Button_Arrow_Press,
                ButtonState.Disable => Define.Sprite.Button_Arrow_Disable,
                _ => Define.Sprite.Button_Arrow_Normal
            };

            Sprite sprite = await Managers.Resource.LoadSpriteAsync(atlas, name);

            if (targetImage != null && name != null)
                targetImage.sprite = sprite;
        }).AddTo(component);
    }

    public static void BindView(this UIBehaviour view, Action<PointerEventData> action, ViewEvent type, Component component, ReactiveProperty<ButtonState> prop)
    {
        Observable<PointerEventData> observable = type switch
        {
            ViewEvent.Enter => view.OnPointerEnterAsObservable(),
            ViewEvent.Exit => view.OnPointerExitAsObservable(),
            ViewEvent.Press => view.OnPointerDownAsObservable().Where(data => data.button == PointerEventData.InputButton.Left),
            ViewEvent.Release => view.OnPointerUpAsObservable().Where(data => data.button == PointerEventData.InputButton.Left),
            ViewEvent.LeftClick => view.OnPointerClickAsObservable().Where(data => data.button == PointerEventData.InputButton.Left),
            ViewEvent.RightClick => view.OnPointerClickAsObservable().Where(data => data.button == PointerEventData.InputButton.Right),
            ViewEvent.DoubleClick => view.OnPointerClickAsObservable().Chunk(TimeSpan.FromSeconds(0.25f), 2).Where(list => list.Length == 2).Select(list => list[1]),
            _ => Return(type)
        };
        observable.Where(_ => prop.Value != ButtonState.Disable).Subscribe(action).AddTo(component);
    }

    public static void BindModel<T>(this ReactiveProperty<T> model, Action<T> action, Component component)
        => model.Subscribe(action).AddTo(component);
}
