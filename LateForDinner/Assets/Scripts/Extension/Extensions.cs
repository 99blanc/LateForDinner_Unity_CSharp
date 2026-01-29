using R3;
using System;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Component = UnityEngine.Component;
using Token.EVENT;

public static class Extensions
{
    public static void BindInputEvent(this InputAction action, Action<InputAction.CallbackContext> performed, Component component) => PlayerControl.BindInputEvent(action, performed, component);

    public static void BindInputEvent(this InputAction action, Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled, Component component) => PlayerControl.BindInputEvent(action, performed, canceled, component);

    public static void BindViewEvent(this UIBehaviour view, Action<PointerEventData> action, ViewEvent type, Component component) => UserInterface.BindViewEvent(view, action, type, component);

    public static void BindModelEvent<T>(this ReactiveProperty<T> model, Action<T> action, Component component) => UserInterface.BindModelEvent(model, action, component);
}
