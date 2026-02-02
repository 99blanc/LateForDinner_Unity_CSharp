using R3;
using System;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Component = UnityEngine.Component;
using Token.EVENT;

public static class Extensions
{
    public static void InProp(this Prop current, Prop target) => PropHelper.InProp(current, target);

    public static void OutProp(this Prop current, Prop target) => PropHelper.OutProp(current, target);

    public static bool IsOppositeInput(this IAgentControl agent, float inputX, float currentDir) => InputHelper.IsOppositeInput(agent, inputX, currentDir);

    public static void BindInputEvent(this InputAction action, Action<InputAction.CallbackContext> performed, Component component) => InputHelper.BindInputEvent(action, performed, component);

    public static void BindInputEvent(this InputAction action, Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled, Component component) => InputHelper.BindInputEvent(action, performed, canceled, component);

    public static void BindViewEvent(this UIBehaviour view, Action<PointerEventData> action, ViewEvent type, Component component) => UIHelper.BindViewEvent(view, action, type, component);

    public static void BindModelEvent<T>(this ReactiveProperty<T> model, Action<T> action, Component component) => UIHelper.BindModelEvent(model, action, component);
}
